# AssemblyNamespaceChanger
Console utility to search/replace namespaces in .NET assemblies.

## Usage
```
  -i, --input      Required. Input assembly path.

  -o, --output     Output assembly path. If not specified, '.Modified' will be
                   added to the input assembly name.

  -r, --regexps    Required. Array of regexp search and replace patterns. First
                   consequential one is the search pattern, second is the
                   replacement pattern. Separated by semicolon (:)
```

## Usage example
```
LostPolygon.AssemblyNamespaceChanger --input InputAssembly.dll --output Output/OutputAssembly.dll --regexps ^Foo:Bar.Foo:^Namespace1.Test:Namespace2.Whatever
```
