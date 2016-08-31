using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace WrapSharp {
    class Options {
        [Option('l', "meta", DefaultValue = "", HelpText = "Meta data")]
        public string MetaFile { get; set; }

        [Option('m', "mem", DefaultValue = long.MaxValue, HelpText = "Memory")]
        public long Memory { get; set; }

        [Option('t', "time", DefaultValue = double.MaxValue, HelpText = "Time")]
        public double CpuTime { get; set; }

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

        [Option('d', "working-dir", DefaultValue = "", HelpText = "WorkingDirectory, has to be absolute path", Required = true)]
        public string WorkingDirectory { get; set; }

        [OptionList('b', "bound-dir", Separator = ',', DefaultValue = null, HelpText = "Bound directories which sandbox can access (or else), has to be absolute path")]
        public IList<string> BoundDirectories { get; set; }

        [Option('v', "verbose", HelpText = "Verbose")]
        public bool Verbose { get; set; }

        [ValueList(typeof(List<string>))]
        public IList<string> UnboundedValues { get; set; }

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage() {
            return HelpText.AutoBuild(this);
        }

        public string ProgramName { get; private set; }
        public List<string> ProgramArguments { get; private set; }
        public List<BoundDirectory> BoundDirectoriesParsed { get; private set; }

        public Options() {
            ProgramName = "";
            ProgramArguments = new List<string>();
            BoundDirectoriesParsed = new List<BoundDirectory>();
        }

        public void Check() {
            if (UnboundedValues.Count == 0) {
                throw new Exception("Name of executed program not given");
            }
            ProgramName = UnboundedValues[0];

            // arguments which will be handed over to execution binary
            for (int i = 1; i < UnboundedValues.Count; ++i) {
                ProgramArguments.Add(UnboundedValues[i]);
            }

            // bound directories which will be visible inside sandbox
            if (BoundDirectories != null) {
                foreach (var dir in BoundDirectories) {
                    BoundDirectoriesParsed.Add(new BoundDirectory(dir));
                }
            }

            // TODO: isnt double.MaxValue too high, what if we define a bit less limits (0-3600 or sth else)

            if (CpuTime < 0) {
                CpuTime = double.MaxValue;
            }
            if (WallTime < 0) {
                WallTime = double.MaxValue;
            }
            if (ExtraTime < 0) {
                ExtraTime = double.MaxValue;
            }
            if (Memory < 0) {
                Memory = long.MaxValue;
            }

            if (Verbose) {
                Console.WriteLine("> Commandline options successfully checked");
            }
        }
    }
}
