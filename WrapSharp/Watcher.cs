using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WrapSharp {

    class Watcher {
        private Options options;
        private Sandboxer sandboxer;
        private Metadata metadata;
        private const int watcherPeriod = 100; // in milliseconds

        public Watcher(Options options, Sandboxer sandboxer, Metadata metadata) {
            this.options = options;
            this.sandboxer = sandboxer;
            this.metadata = metadata;
        }

        public void Run() {
            // wait for domain to create and run
            sandboxer.WaitForExecutionEvent.Wait();

            try {
                while (true) {
                    double elapsedSeconds = sandboxer.SandboxStartTime.Elapsed.TotalSeconds;
                    double elapsedCpuSeconds = sandboxer.domain.MonitoringTotalProcessorTime.TotalSeconds;
                    long usedMemory = sandboxer.domain.MonitoringSurvivedMemorySize;

                    // fill in metadata in every iteration
                    metadata.WallTime = elapsedSeconds;
                    metadata.CpuTime = elapsedCpuSeconds;
                    metadata.Memory = usedMemory;

                    // sandboxed application is alive too long, kill it
                    if (options.WallTime < elapsedSeconds) {
                        AppDomain.Unload(sandboxer.domain);

                        metadata.Message = "WallTime exceeded";
                        metadata.Status = StatusCode.TO;
                        break;
                    }

                    // now do the same checking on cpu time
                    if (options.CpuTime < elapsedCpuSeconds) {
                        AppDomain.Unload(sandboxer.domain);

                        metadata.Message = "CpuTime exceeded";
                        metadata.Status = StatusCode.TO;
                        break;
                    }

                    // and of course check memory
                    if (options.Memory < usedMemory) {
                        AppDomain.Unload(sandboxer.domain);

                        metadata.Message = "Memory exceeded";
                        metadata.Status = StatusCode.ME;
                        break;
                    }

                    // if execution of sandboxed application ended, end too
                    if (!sandboxer.IsRunning) {
                        metadata.Status = StatusCode.OK;
                        break;
                    }

                    Thread.Sleep(watcherPeriod);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);

                metadata.Status = StatusCode.XX;
                metadata.ExceptionType = e.GetType().Name;
                metadata.Message = e.Message;
            }
        }
    }
}
