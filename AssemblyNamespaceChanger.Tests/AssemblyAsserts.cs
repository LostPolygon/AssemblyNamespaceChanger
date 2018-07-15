using Mono.Cecil;

using NUnit.Framework;

// Ported from https://github.com/BrokenEvent/ILStrip
namespace AssemblyNamespaceChanger.Tests
{
  static class AssemblyAsserts
  {
    private static TypeDefinition FindType(AssemblyDefinition def, string className)
    {
      foreach (TypeDefinition type in def.MainModule.Types)
      {
        if (type.FullName == className)
          return type;

        foreach (TypeDefinition nestedType in type.NestedTypes)
          if (nestedType.FullName == className)
            return nestedType;
      }

      return null;
    }

    public static void AssertClass(this AssemblyDefinition def, string className)
    {
      Assert.IsNotNull(FindType(def, className), "Class " + className + " not found");
    }

    public static void AssertNoClass(this AssemblyDefinition def, string className)
    {
      Assert.IsNull(FindType(def, className), "Class " + className + " found");
    }
  }
}
