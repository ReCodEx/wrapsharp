using System;
using CommandLine;

namespace WrapSharp {
    class Program {
        static void Main(string[] args) {
            try {
                Options options = new Options();
                Metadata metadata = new Metadata();

                if (Parser.Default.ParseArgumentsStrict(args, options)) {
                    options.Check();

                    Sandboxer sandboxer = new Sandboxer(options, metadata);
                    Watcher watcher = new Watcher(options, sandboxer, metadata);

                    sandboxer.Execute();
                    watcher.Run();

                    metadata.SaveIfFileDefined(options.MetaFile, options.Verbose);
                }
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }
}
