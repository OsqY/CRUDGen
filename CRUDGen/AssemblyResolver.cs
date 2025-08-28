using System.Reflection;
using System.Runtime.Loader;

internal class CustomAssemblyResolver : AssemblyLoadContext
{
    private readonly Dictionary<string, string> _assemblyPaths;

    public CustomAssemblyResolver(IEnumerable<string> pluginPaths)
    {
        _assemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in pluginPaths)
        {
            var assemblyName = AssemblyName.GetAssemblyName(path).Name;
            if (assemblyName != null)
            {
                _assemblyPaths[assemblyName] = path;
            }
        }

        Resolving += OnResolving;
    }

    private Assembly? OnResolving(AssemblyLoadContext context, AssemblyName name)
    {
        if (name.Name != null && _assemblyPaths.TryGetValue(name.Name, out var path))
        {
            return LoadFromAssemblyPath(path);
        }

        return null;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name != null && _assemblyPaths.TryGetValue(assemblyName.Name, out var path))
        {
            return LoadFromAssemblyPath(path);
        }
        return null;
    }
}