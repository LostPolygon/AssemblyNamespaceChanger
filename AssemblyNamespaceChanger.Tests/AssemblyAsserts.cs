using System.Linq;
using Mono.Cecil;
using NUnit.Framework;

// Ported from https://github.com/BrokenEvent/ILStrip
namespace AssemblyNamespaceChanger.Tests {
    static class AssemblyAsserts {
        public static void AssertClass(this AssemblyDefinition def, string className) {
            Assert.IsNotNull(FindType(def, className), "Class " + className + " not found");
        }

        public static void AssertNoClass(this AssemblyDefinition def, string className) {
            Assert.IsNull(FindType(def, className), "Class " + className + " found");
        }

        public static void AssertAssemblyReference(this AssemblyDefinition def, string assemblyReferenceName) {
            Assert.IsNotNull(FindAssemblyReference(def, assemblyReferenceName), "Reference to assembly " + assemblyReferenceName + " not found");
        }

        public static void AssertNoAssemblyReference(this AssemblyDefinition def, string assemblyReferenceName) {
            Assert.IsNull(FindAssemblyReference(def, assemblyReferenceName), "Reference to assembly " + assemblyReferenceName + " found");
        }

        public static void AssertTypeReference(this AssemblyDefinition def, string typeReferenceName) {
            Assert.IsNotNull(FindTypeReference(def, typeReferenceName), "Reference to type " + typeReferenceName + " not found");
        }

        public static void AssertNoTypeReference(this AssemblyDefinition def, string typeReferenceName) {
            Assert.IsNull(FindTypeReference(def, typeReferenceName), "Reference to type " + typeReferenceName + " found");
        }

        private static TypeReference FindTypeReference(AssemblyDefinition def, string typeReferenceFullName) {
            def.MainModule.TryGetTypeReference(typeReferenceFullName, out TypeReference typeReference);
            return typeReference;
        }

        private static AssemblyNameReference FindAssemblyReference(AssemblyDefinition def, string assemblyReferenceName) {
            return def.MainModule.AssemblyReferences.FirstOrDefault(reference => reference.Name == assemblyReferenceName);
        }

        private static TypeDefinition FindType(AssemblyDefinition def, string className) {
            foreach (TypeDefinition type in def.MainModule.Types) {
                if (type.FullName == className)
                    return type;

                foreach (TypeDefinition nestedType in type.NestedTypes)
                    if (nestedType.FullName == className)
                        return nestedType;
            }

            return null;
        }
    }
}
