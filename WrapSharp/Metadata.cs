using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WrapSharp {
    enum StatusCode { OK, RE, TO, XX }

    class Metadata {
        public ulong Memory { get; set; }
        public double Time { get; set; }
        public double WallTime { get; set; }
        public int ExitCode { get; set; }
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
                default:
                return "XX";
            }
        }

        public Metadata() {
            Status = StatusCode.OK;
        }

        public void SaveIfFileDefined(string file) {
            if (file == null || file.Length == 0) {
                return;
            }

            using (StreamWriter writer = new StreamWriter(file)) {
                writer.WriteLine("mem:" + Memory);
                writer.WriteLine("time:" + Time);
                writer.WriteLine("wall-time:" + WallTime);
                writer.WriteLine("exitcode:" + ExitCode);
                writer.WriteLine("message:" + Message);
                writer.WriteLine("status:" + StatusCodeToString(Status));
            }
        }
    }
}
