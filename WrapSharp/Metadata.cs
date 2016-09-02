using System;
using System.IO;
using System.Text;

namespace WrapSharp {
    enum StatusCode { OK, RE, TO, ME, XX }

    class Metadata {
        public long Memory { get; set; }
        public double CpuTime { get; set; }
        public double WallTime { get; set; }
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public StatusCode Status { get; set; }

        private static string StatusCodeToString(StatusCode status)
        {
            switch (status) {
                case StatusCode.OK:
                return "OK";
                case StatusCode.RE:
                return "RE";
                case StatusCode.TO:
                return "TO";
                case StatusCode.ME:
                return "ME";
                default:
                return "XX";
            }
        }

        public Metadata() {
            Status = StatusCode.OK;
        }

        public void Update(double elapsed, double cpuElapsed, long memory) {
            WallTime = elapsed;
            CpuTime = cpuElapsed;

            if (memory > Memory) {
                Memory = memory;
            }
        }

        public void SaveIfFileDefined(string file, bool copyToOut = false) {
            if ((file == null || file.Length == 0) && copyToOut == false) {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("mem: " + Memory);
            sb.AppendLine("time: " + CpuTime);
            sb.AppendLine("wall-time: " + WallTime);
            sb.AppendLine("exception: " + ExceptionType);
            sb.AppendLine("message: " + Message);
            sb.AppendLine("status: " + StatusCodeToString(Status));

            if (file != null && file.Length != 0) {
                using (StreamWriter writer = new StreamWriter(file)) {
                    writer.Write(sb);
                }
            }

            if (copyToOut) {
                Console.WriteLine(">>> Metadata log <<<");
                Console.Write(sb);
                Console.WriteLine(">>> Metadata log <<<");
            }
        }
    }
}
