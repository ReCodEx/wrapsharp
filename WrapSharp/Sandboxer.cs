using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

namespace WrapSharp {
    class Sandbox : MarshalByRefObject {
        public void Execute(string assemblyName, string[] parameters) {
            MethodInfo method = Assembly.LoadFile(assemblyName).EntryPoint;
            method.Invoke(null, new object[] { parameters });
        }
    }

    class Sandboxer {
        private Options options;
        private Metadata metadata;
        public AppDomain domain { get; private set; }
        public Stopwatch SandboxStartTime { get; private set; }
        public bool IsRunning { get; private set; }
        public ManualResetEventSlim WaitForExecutionEvent { get; private set; }
        private const string domainName = "WrapSharp Sandbox";

        public Sandboxer(Options options, Metadata metadata) {
            this.options = options;
            this.metadata = metadata;
            SandboxStartTime = new Stopwatch();
            WaitForExecutionEvent = new ManualResetEventSlim();
            IsRunning = true;
        }

        private void CreateAppDomainAndPopulate() {
            Evidence securityInfo = null;

            AppDomainSetup appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = options.WorkingDirectory;

            PermissionSet permissionSet = GetPermissionSet();

            StrongName fullTrustAssembly = typeof(Sandbox).Assembly.Evidence.GetHostEvidence<StrongName>();

            domain = AppDomain.CreateDomain(domainName, securityInfo,
                appDomainSetup, permissionSet, fullTrustAssembly);

            // allow monitoring of domains
            AppDomain.MonitoringIsEnabled = true;
        }

        private PermissionSet GetPermissionSet() {
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, options.WorkingDirectory));
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            return permSet;
        }

        public async void Execute() {
            await Task.Yield();

            try {
                CreateAppDomainAndPopulate();

                ObjectHandle handle = Activator.CreateInstanceFrom(
                    domain, typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(Sandbox).FullName);

                Sandbox instance = (Sandbox) handle.Unwrap();

                // start execution with measuring time
                SandboxStartTime.Start();
                WaitForExecutionEvent.Set();
                instance.Execute(Path.Combine(options.WorkingDirectory, options.ProgramName), options.ProgramArguments.ToArray());
            } catch (AppDomainUnloadedException) {
                // nothing to do here, watcher unloaded AppDomain
            } catch (Exception e) {
                Console.WriteLine(e.Message);

                metadata.Status = StatusCode.XX;
                metadata.ExceptionType = e.GetType().Name;
                metadata.Message = e.Message;
            } finally {
                // do not remember to set running state of sandboxer to false
                IsRunning = false;
            }
        }
    }
}
