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

        private TextReader oldIn;
        private TextWriter oldOut;
        private TextWriter oldError;

        public Sandbox() {
            oldIn = Console.In;
            oldOut = Console.Out;
            oldError = Console.Error;
        }

        public void Execute(string assemblyName, string[] parameters,
            TextReader newIn = null, TextWriter newOut = null,
            TextWriter newError = null) {

            if (newIn != null) { Console.SetIn(newIn); }
            if (newOut != null) { Console.SetOut(newOut); }
            if (newError != null) { Console.SetError(newError); }

            MethodInfo method = Assembly.LoadFile(assemblyName).EntryPoint;
            method.Invoke(null, new object[] { parameters });
        }
    }

    class Sandboxer {
        private Options options;
        private Metadata metadata;
        public AppDomain SandboxDomain { get; private set; }
        public Stopwatch SandboxStartTime { get; private set; }
        public bool IsRunning { get; private set; }
        private ManualResetEventSlim waitForExecutionEvent;
        private const string domainName = "WrapSharp Sandbox";

        public Sandboxer(Options options, Metadata metadata) {
            this.options = options;
            this.metadata = metadata;
            SandboxStartTime = new Stopwatch();
            waitForExecutionEvent = new ManualResetEventSlim();
            IsRunning = true;

            if (options.Verbose) {
                Console.WriteLine("> Sandboxer constructed");
            }
        }

        public void WaitForExecution() {
            waitForExecutionEvent.Wait();
        }

        private void CreateAppDomainAndPopulate() {
            Evidence securityInfo = null;

            AppDomainSetup appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = options.WorkingDirectory;

            PermissionSet permissionSet = GetPermissionSet();

            StrongName fullTrustAssembly = typeof(Sandbox).Assembly.Evidence.GetHostEvidence<StrongName>();

            SandboxDomain = AppDomain.CreateDomain(domainName, securityInfo,
                appDomainSetup, permissionSet, fullTrustAssembly);

            // allow monitoring of domains
            AppDomain.MonitoringIsEnabled = true;

            if (options.Verbose) {
                Console.WriteLine("> AppDomain successfully created");
            }
        }

        private PermissionSet GetPermissionSet() {
            PermissionSet permSet = new PermissionSet(PermissionState.None);

            // allow execution within sandbox
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            // allow reflection within sandbox
            permSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));

            // permissions for directories
            permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, options.WorkingDirectory));
            foreach (var dir in options.BoundDirectoriesParsed) {
                foreach (var perm in dir.DirPermissions) {
                    permSet.AddPermission(new FileIOPermission(perm, dir.DirPath));
                }
            }

            return permSet;
        }

        public async void Execute() {
            await Task.Yield();

            try {
                CreateAppDomainAndPopulate();

                ObjectHandle handle = Activator.CreateInstanceFrom(
                    SandboxDomain, typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(Sandbox).FullName);

                Sandbox instance = (Sandbox) handle.Unwrap();

                if (options.Verbose) {
                    Console.WriteLine("> Execution of sandboxed program started");
                }

                // start execution with measuring time
                SandboxStartTime.Start();
                waitForExecutionEvent.Set();
                instance.Execute(Path.Combine(options.WorkingDirectory, options.ProgramName), 
                    options.ProgramArguments.ToArray(),
                    options.newIn, options.newOut, options.newError);

                // update metadata after successful execution
                metadata.Update(SandboxStartTime.Elapsed.TotalSeconds,
                    SandboxDomain.MonitoringTotalProcessorTime.TotalSeconds, SandboxDomain.MonitoringSurvivedMemorySize);

                if (options.Verbose) {
                    Console.WriteLine("> Sandboxed program successfully ended");
                }
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
