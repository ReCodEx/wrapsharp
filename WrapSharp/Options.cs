using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace WrapSharp {
    class Options {
        [Option('M', "meta", DefaultValue = "", HelpText = "Meta data")]
        public string MetaFile { get; set; }

        [Option('m', "mem", DefaultValue = ulong.MaxValue, HelpText = "Memory")]
        public ulong Memory { get; set; }

        [Option('t', "time", DefaultValue = double.MaxValue, HelpText = "Time")]
        public double Time { get; set; }

        [Option('w', "wall-time", DefaultValue = double.MaxValue, HelpText = "WallTime")]
        public double WallTime { get; set; }

        [Option('x', "extra-time", DefaultValue = double.MaxValue, HelpText = "ExtraTime")]
        public double ExtraTime { get; set; }

        [Option('i', "stdin", DefaultValue = "", HelpText = "Stdin")]
        public string Stdin { get; set; }

        [Option('o', "stdout", DefaultValue = "", HelpText = "Stdout")]
        public string Stdout { get; set; }

        [Option('r', "stderr", DefaultValue = "", HelpText = "Stderr")]
        public string Stderr { get; set; }

        [Option('d', "working-dir", DefaultValue = "", HelpText = "WorkingDirectory", Required = true)]
        public string WorkingDirectory { get; set; }

        [Option('b', "bound-dir", DefaultValue = null, HelpText = "Bound directories which sandbox can access (or else)")]
        public IList<string> BoundDirectories { get; set; }

        [Option('v', "verbose", HelpText = "Verbose")]
        public bool Verbose { get; set; }

        [ValueList(typeof(List<string>))]
        public IList<string> UnboundedValues { get; set; }

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage() {
            return HelpText.AutoBuild(this);
        }

        public string ProgramName { get; set; }
        public List<string> ProgramArguments { get; set; }

        public void Check() {
            if (UnboundedValues.Count == 0) {
                throw new Exception("Name of executed program not given");
            } else {
                ProgramName = UnboundedValues[0];
                ProgramArguments = new List<string>();

                for (int i = 1; i < UnboundedValues.Count; ++i) {
                    ProgramArguments.Add(UnboundedValues[i]);
                }
            }

            // TODO: check directories, program name if exists
            // TODO: isnt double.MaxValue too high, what if we define a bit less limits (0-3600 or sth else)

            if (Time < 0) {
                Time = double.MaxValue;
            }
            if (WallTime < 0) {
                WallTime = double.MaxValue;
            }
            if (ExtraTime < 0) {
                ExtraTime = double.MaxValue;
            }
        }
    }
}
