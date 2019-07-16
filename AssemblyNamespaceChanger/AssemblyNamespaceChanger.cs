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
using Mono.Cecil.Rocks;

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

            string UpdateName(string name, Action onNameChanged) {
                string originalName = name;
                foreach ((Regex pattern, string replacement) replacementPattern in replacementPatterns) {
                    name =
                        replacementPattern.pattern.Replace(name, replacementPattern.replacement);
                }

                if (name != originalName) {
                    onNameChanged();
                }

                return name;
            }

            Log.Info($"Reading assembly from {_commandLineOptions.InputAssemblyPath}");

            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(_commandLineOptions.InputAssemblyPath);
            TypeDefinition[] types = assembly.MainModule.GetAllTypes().ToArray();
            TypeReference[] typeReferences = assembly.MainModule.GetTypeReferences().ToArray();

            Log.Info("Updating attributes");

            // Just touch all the attributes, this is enough for Mono.Cecil to fix them
            HashSet<CustomAttribute> customAttributes = new HashSet<CustomAttribute>();
            customAttributes.UnionWith(assembly.CustomAttributes);
            customAttributes.UnionWith(assembly.MainModule.GetCustomAttributes());

            foreach (TypeDefinition type in types) {
                customAttributes.UnionWith(type.CustomAttributes);
                type.Events.ToList().ForEach(m => customAttributes.UnionWith(m.CustomAttributes));
                type.Fields.ToList().ForEach(m => customAttributes.UnionWith(m.CustomAttributes));
                type.Interfaces.ToList().ForEach(m => customAttributes.UnionWith(m.CustomAttributes));
                type.Methods.ToList().ForEach(m => customAttributes.UnionWith(m.CustomAttributes));
                type.GenericParameters.ToList().ForEach(m => customAttributes.UnionWith(m.CustomAttributes));
                type.Properties.ToList().ForEach(m => customAttributes.UnionWith(m.CustomAttributes));

                foreach (MethodDefinition method in type.Methods) {
                    method.GenericParameters.ToList().ForEach(m => customAttributes.UnionWith(m.CustomAttributes));
                    method.Parameters.ToList().ForEach(m => customAttributes.UnionWith(m.CustomAttributes));
                }
            }

            int modifiedAttributesParameters = 0;
            foreach (CustomAttribute customAttribute in customAttributes) {
                foreach (CustomAttributeArgument attributeConstructorArgument in customAttribute.ConstructorArguments) {
                    if (!(attributeConstructorArgument.Value is TypeSpecification) && attributeConstructorArgument.Value is TypeReference typeReference) {
                        typeReference.Namespace = UpdateName(typeReference.Namespace, () => modifiedAttributesParameters++);
                    }
                }
            }

            Log.Info($"Modified {modifiedAttributesParameters} attribute parameter(s)");

            Log.Info("Updating assembly name");
            if (_commandLineOptions.ReplaceAssemblyName) {
                assembly.Name.Name = UpdateName(assembly.Name.Name, () => Log.Info("Assembly name modified"));
            }

            Log.Info("Modifying types");

            int modifiedTypes = 0;
            foreach (TypeDefinition type in types) {
                type.Namespace = UpdateName(type.Namespace, () => modifiedTypes++);
            }

            Log.Info($"Modified {modifiedTypes} type(s)");

            Log.Info("Modifying type references");

            int modifiedTypeReferences = 0;
            foreach (TypeReference typeReference in typeReferences) {
                typeReference.Namespace = UpdateName(typeReference.Namespace, () => modifiedTypeReferences++);
            }

            Log.Info($"Modified {modifiedTypeReferences} type reference(s)");

            if (_commandLineOptions.ReplaceAssemblyReferences) {
                Log.Info("Modifying assembly references");

                int modifiedReferences = 0;
                foreach (AssemblyNameReference assemblyReference in assembly.MainModule.AssemblyReferences) {
                    assemblyReference.Name = UpdateName(assemblyReference.Name, () => modifiedReferences++);
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
