using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace LostPolygon.AssemblyNamespaceChanger
{
    public class CommandLineOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input assembly path.")]
        public string InputAssemblyPath { get; set; }

        [Option('o', "output", HelpText = "Output assembly path. If not specified, '.Modified' will be added to the input assembly name.")]
        public string OutputAssemblyPath { get; set; }

        [Option('r', "regexps", Separator=':', Required = true,
            HelpText = "Array of regexp search and replace patterns. " +
                "First consequential one is the search pattern, " +
                "second is the replacement pattern. Separated by semicolon (:)")]
        public IEnumerable<string> Regexps { get; set; }

        [Option("replace-references", HelpText = "Whether to apply search/replace to assembly references. Default is off.")]
        public bool ReplaceAssemblyReferences { get; set; }

        [Option("replace-assembly-name", HelpText = "Whether to apply search/replace to assembly name. Default is off.")]
        public bool ReplaceAssemblyName { get; set; }

        [Usage(ApplicationAlias = "LostPolygon.AssemblyNamespaceChanger")]
        public static IEnumerable<Example> Examples {
            get {
                yield return new Example("Normal scenario", new CommandLineOptions {
                    InputAssemblyPath = "InputAssembly.dll",
                    OutputAssemblyPath = "Output/OutputAssembly.dll",
                    Regexps = new []{ "^Foo:Bar.Foo:^Namespace1.Test:Namespace2:Whatever" }
                });
            }
        }
    }
}
