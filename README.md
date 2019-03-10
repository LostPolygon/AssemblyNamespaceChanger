# AssemblyNamespaceChanger
Console utility to search/replace namespaces in .NET assemblies.

## Usage
```
  -i, --input      Required. Input assembly path.

  -o, --output     Output assembly path. If not specified, '.Modified' will be
                   added to the input assembly name.

  -r, --regexps    Required. Array of regexp search and replace patterns. First
                   consequential one is the search pattern, second is the
                   replacement pattern. Separated by colon (:)

  --replace-references       Whether to apply search/replace to assembly
                             references. Default is off.

  --replace-assembly-name    Whether to apply search/replace to assembly name.
                             Default is off.
```

## Usage example
```
LostPolygon.AssemblyNamespaceChanger --input InputAssembly.dll --output Output/OutputAssembly.dll --regexps ^Foo:Bar.Foo:^Namespace1.Test:Namespace2.Whatever
```
