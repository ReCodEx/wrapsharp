using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using System.IO;

namespace WrapSharp {
    class Options {
        [Option('l', "meta", DefaultValue = "", HelpText = "Meta data")]
        public string MetaFile { get; set; }

        [Option('m', "mem", HelpText = "Memory", Required = true)]
        public long Memory { get; set; }

        [Option('t', "cpu-time", HelpText = "CpuTime", Required = true)]
        public double CpuTime { get; set; }

        [Option('w', "wall-time", HelpText = "WallTime", Required = true)]
        public double WallTime { get; set; }

        [Option('x', "extra-time", HelpText = "ExtraTime", Required = true)]
        public double ExtraTime { get; set; }

        [Option('i', "stdin", DefaultValue = "", HelpText = "Stdin")]
        public string Stdin { get; set; }

        [Option('o', "stdout", DefaultValue = "", HelpText = "Stdout")]
        public string Stdout { get; set; }

        [Option('r', "stderr", DefaultValue = "", HelpText = "Stderr")]
        public string Stderr { get; set; }

        [Option('d', "working-dir", HelpText = "WorkingDirectory, has to be absolute path", Required = true)]
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

            if (CpuTime < 0) {
                CpuTime = 0;
            }
            if (WallTime < 0) {
                WallTime = 0;
            }
            if (ExtraTime < 0) {
                ExtraTime = 0;
            }
            if (Memory < 0) {
                Memory = 0;
            }

            if (Verbose) {
                Console.WriteLine("> Commandline options successfully checked");
            }
        }
    }
}
