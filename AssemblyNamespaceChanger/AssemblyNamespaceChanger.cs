using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using log4net;
using Mono.Cecil;

namespace LostPolygon.AssemblyNamespaceChanger {
    class AssemblyNamespaceChanger {
        private static readonly ILog Log = LogManager.GetLogger("AssemblyNamespaceChanger");

        private readonly string[] _args;
        private CommandLineOptions _commandLineOptions;
        private ParserResult<CommandLineOptions> _commandLineParserResult;

        public static void Run(string[] args) {
            AssemblyNamespaceChanger instance = new AssemblyNamespaceChanger(args);
            instance.Run();
        }

        public AssemblyNamespaceChanger(string[] args) {
            _args = args;
        }

        private void Run() {
            Parser commandLineParser =
                new Parser(settings => {
                    settings.IgnoreUnknownArguments = false;
                    settings.CaseSensitive = false;
                    settings.HelpWriter = null;
                });

            _commandLineParserResult = commandLineParser.ParseArguments<CommandLineOptions>(_args);
            _commandLineParserResult
                .WithParsed(options => {
                    _commandLineOptions = options;
                    ExecuteOperation();
                })
                .WithNotParsed(errors => {
                    Console.WriteLine(GetHelpText());
                    Environment.ExitCode = 1;
                });
        }

        private void ExecuteOperation() {
            List<(Regex pattern, string replacement)> replacementPatterns = new List<(Regex pattern, string replacement)>();

            string[] regexpOptions = _commandLineOptions.Regexps.ToArray();
            for (int i = 0; i < regexpOptions.Length; i += 2) {
                string regexpOption = regexpOptions[i];
                string regexpReplacementOption = regexpOptions[i + 1];

                Regex regex = new Regex(regexpOption, RegexOptions.Singleline);
                replacementPatterns.Add((regex, regexpReplacementOption));
            }

            Log.Info($"Reading assembly from {_commandLineOptions.InputAssemblyPath}");
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(_commandLineOptions.InputAssemblyPath);

            if (_commandLineOptions.ReplaceAssemblyName) {
                string originalName = assembly.Name.Name;
                foreach ((Regex pattern, string replacement) replacementPattern in replacementPatterns) {
                    assembly.Name.Name =
                        replacementPattern.pattern.Replace(assembly.Name.Name, replacementPattern.replacement);
                }

                if (originalName != assembly.Name.Name) {
                    Log.Info("Assembly name modified");
                }
            }

            Log.Info("Modifying types");

            TypeDefinition[] types = assembly.MainModule.GetTypes().ToArray();
            int modifiedTypes = 0;
            foreach (TypeDefinition type in types) {
                string originalNamespace = type.Namespace;
                foreach ((Regex pattern, string replacement) replacementPattern in replacementPatterns) {
                    type.Namespace =
                        replacementPattern.pattern.Replace(type.Namespace, replacementPattern.replacement);
                }

                if (originalNamespace != type.Namespace) {
                    modifiedTypes++;
                }
            }

            Log.Info($"Modified {modifiedTypes} type(s)");

            Log.Info("Modifying type references");

            int modifiedTypeReferences = 0;
            foreach (TypeReference typeReference in assembly.MainModule.GetTypeReferences()) {
                string originalNamespace = typeReference.Namespace;
                foreach ((Regex pattern, string replacement) replacementPattern in replacementPatterns) {
                    typeReference.Namespace =
                        replacementPattern.pattern.Replace(typeReference.Namespace, replacementPattern.replacement);
                }

                if (originalNamespace != typeReference.Namespace) {
                    modifiedTypeReferences++;
                }
            }

            Log.Info($"Modified {modifiedTypeReferences} type reference(s)");

            if (_commandLineOptions.ReplaceAssemblyReferences) {
                Log.Info("Modifying assembly references");

                int modifiedReferences = 0;
                foreach (AssemblyNameReference assemblyReference in assembly.MainModule.AssemblyReferences) {
                    string originalName = assemblyReference.Name;
                    foreach ((Regex pattern, string replacement) replacementPattern in replacementPatterns) {
                        assemblyReference.Name =
                            replacementPattern.pattern.Replace(assemblyReference.Name, replacementPattern.replacement);
                    }

                    if (originalName != assemblyReference.Name) {
                        modifiedReferences++;
                    }
                }

                Log.Info($"Modified {modifiedReferences} assembly reference(s)");
            }

            string outputPath;
            if (!String.IsNullOrWhiteSpace(_commandLineOptions.OutputAssemblyPath)) {
                outputPath = _commandLineOptions.OutputAssemblyPath;
            } else {
                outputPath =
                    Path.Combine(
                        Path.GetDirectoryName(_commandLineOptions.InputAssemblyPath) ?? "",
                        Path.GetFileNameWithoutExtension(_commandLineOptions.InputAssemblyPath) +
                        ".Modified" +
                        Path.GetExtension(_commandLineOptions.InputAssemblyPath)
                    );
            }

            Log.Info($"Writing assembly to {outputPath}");
            assembly.Write(outputPath);
        }

        private HelpText GetHelpText() {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string assemblyConfiguration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
            string assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            HelpText helpText =
                _commandLineParserResult.Tag == ParserResultType.NotParsed ? HelpText.AutoBuild(_commandLineParserResult) : HelpText.AutoBuild(_commandLineParserResult, text => text, example => example);
            helpText.AddEnumValuesToHelpText = true;
            helpText.AddDashesToOption = true;
            helpText.Heading =
                $"{assembly.GetName().Name} v{assemblyVersion} ({assemblyConfiguration}";
            helpText.Copyright = "© Lost Polygon";
            return helpText;
        }
    }
}
