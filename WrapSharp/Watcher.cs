using System;
using System.Threading;

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

            if (options.Verbose) {
                Console.WriteLine("> Watcher constructed");
            }
        }

        private void KillSandboxAndFillMeta(double elapsed, double cpuElapsed, long memory, string message, StatusCode status) {
            AppDomain.Unload(sandboxer.SandboxDomain);

            Console.Error.WriteLine("Sandbox killed: " + message);

            metadata.Update(elapsed, cpuElapsed, memory);
            metadata.Message = message;
            metadata.Status = status;
        }

        public void Run() {
            // wait for domain to create and run
            sandboxer.WaitForExecution();

            if (options.Verbose) {
                Console.WriteLine("> Watcher started monitoring sandboxed program");
            }

            try {
                double time = options.WallTime + options.ExtraTime;

                while (true) {
                    double elapsed = sandboxer.SandboxStartTime.Elapsed.TotalSeconds;
                    double cpuElapsed = sandboxer.SandboxDomain.MonitoringTotalProcessorTime.TotalSeconds;
                    long memory = sandboxer.SandboxDomain.MonitoringSurvivedMemorySize;

                    // sandboxed application is alive too long, kill it
                    if (time < elapsed) {
                        KillSandboxAndFillMeta(elapsed, cpuElapsed, memory, "WallTime exceeded", StatusCode.TO);
                        break;
                    }

                    // now do the same checking on cpu time
                    if (options.CpuTime < cpuElapsed) {
                        KillSandboxAndFillMeta(elapsed, cpuElapsed, memory, "CpuTime exceeded", StatusCode.TO);
                        break;
                    }

                    // and of course check memory
                    if (options.Memory < memory) {
                        KillSandboxAndFillMeta(elapsed, cpuElapsed, memory, "Memory exdeeded", StatusCode.ME);
                        break;
                    }

                    // if execution of sandboxed application ended, end too
                    if (!sandboxer.IsRunning) {
                        // Sandboxer should update all values in metadata
                        break;
                    }

                    metadata.Update(elapsed, cpuElapsed, memory);

                    Thread.Sleep(watcherPeriod);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);

                metadata.Status = StatusCode.XX;
                metadata.ExceptionType = e.GetType().Name;
                metadata.Message = e.Message;
            }

            if (options.Verbose) {
                Console.WriteLine("> Watcher ended monitoring");
            }
        }
    }
}
