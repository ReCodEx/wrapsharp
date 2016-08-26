using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WrapSharp {
    class WatcherException : Exception {
        public WatcherException(string message) : base(message) { }

        public WatcherException(string message, Exception innerException) : base(message, innerException) { }
    }

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
            try {
                while (true) {
                    // sandboxed application is alive too long, kill it
                    if (options.Time < sandboxer.SandboxStartTime.Elapsed.TotalSeconds) {
                        AppDomain.Unload(sandboxer.domain);
                        break;
                    }

                    // if execution of sandboxed application ended, end too
                    if (!sandboxer.IsRunning) {
                        break;
                    }

                    Thread.Sleep(watcherPeriod);
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
