using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using LostPolygon.AssemblyNamespaceChanger;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyNamespaceChanger.Tests {
    [TestFixture]
    class RenameTests {
        [Test]
        public void Rename() {
            string inputAsmPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestAssembly.dll");
            string outputAsmPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestAssembly.Output.dll");

            // sanity
            AssemblyDefinition input = AssemblyDefinition.ReadAssembly(inputAsmPath);
            input.AssertClass("Neat.Cool.Awesome");
            input.AssertNoClass("Test.Cool.Awesome");
            input.AssertClass("Nice.Foo");
            input.AssertClass("Nice.Foo/NestedBar");

            Parser commandLineParser =
                new Parser(settings => {
                    settings.IgnoreUnknownArguments = false;
                    settings.CaseSensitive = false;
                    settings.HelpWriter = null;
                });


            ProcessStartInfo info = new ProcessStartInfo(
                Path.Combine(TestContext.CurrentContext.TestDirectory, "LostPolygon.AssemblyNamespaceChanger.exe")
                );
            info.Arguments =
                commandLineParser.FormatCommandLine(
                    new CommandLineOptions {
                        InputAssemblyPath = inputAsmPath,
                        OutputAssemblyPath = outputAsmPath,
                        ReplaceAssemblyReferences = true,
                        Regexps = new []{ "^Neat", "Test", "^System.Web", "Foo.System.Web" }
                    });;

            Process process = Process.Start(info);
            process.WaitForExit();
            Assert.AreEqual(0, process.ExitCode);

            AssemblyDefinition output = AssemblyDefinition.ReadAssembly(outputAsmPath);
            output.AssertNoClass("Neat.Cool.Awesome");
            output.AssertClass("Test.Cool.Awesome");
            output.AssertClass("Nice.Foo");
            output.AssertClass("Nice.Foo/NestedBar");

            output.AssertNoAssemblyReference("System.Web");
            output.AssertAssemblyReference("Foo.System.Web");
            output.AssertNoTypeReference("System.Web.ModelBinding.CookieAttribute");
            output.AssertTypeReference("Foo.System.Web.ModelBinding.CookieAttribute");
        }
    }
}
