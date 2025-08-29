using System.Reflection;

namespace CRUDGen.Commands;

public static class CreateControllerFiles
{
    public static bool WriteControllersFiles(string? modelsAssemblyPath, string? interfacesAssemblyPath,
        string? controllersPath, string? controllersAssemblyPath, string? dtosAssemblyPath,
        string? modelsPath)
    {
        if (string.IsNullOrEmpty(modelsAssemblyPath) || string.IsNullOrEmpty(interfacesAssemblyPath) || 
            string.IsNullOrEmpty(controllersPath) || string.IsNullOrEmpty(controllersAssemblyPath)
            || string.IsNullOrEmpty(dtosAssemblyPath) || string.IsNullOrEmpty(modelsPath))
        {
            Console.WriteLine("Controllers: A path is null or empty\n");
            return false;
        }

        try
        {
            Console.WriteLine("Controllers: Starting job\n");

            string modelsDirName = new DirectoryInfo(modelsPath).Name;

            var assemblyPaths = new List<string>()
            {
                Path.GetFullPath(controllersAssemblyPath),
                Path.GetFullPath(modelsAssemblyPath),
                Path.GetFullPath(interfacesAssemblyPath)
            };

            var resolver = new CustomAssemblyResolver(assemblyPaths);

            Console.WriteLine("Controllers: Loading Assemblies\n");

            Assembly modelsAssembly = resolver.LoadFromAssemblyPath(
                Path.GetFullPath(modelsAssemblyPath));

            Type[] modelsTypes = modelsAssembly.GetTypes();

           Assembly interfacesAssembly = resolver.LoadFromAssemblyPath(
                Path.GetFullPath(interfacesAssemblyPath));

           Assembly dtosAssembly = resolver.LoadFromAssemblyPath(
               Path.GetFullPath(dtosAssemblyPath));

           Type[] dtosTypes = dtosAssembly.GetTypes();

           string? interfacesNamespace = interfacesAssembly.GetTypes()
               .First(e => e is { IsInterface: true, IsPublic: true, Namespace: not null }
                           )
               .Namespace;

           if (string.IsNullOrEmpty(interfacesNamespace))
           {
               Console.WriteLine("Controllers: Interfaces namespace not found");
               return false;
           }

           string? dtosNamespace = dtosTypes.First(e => e is{IsPublic:true,Namespace:not null}
               && e.Name.Contains("Dto"))
               .Namespace;

           if (string.IsNullOrEmpty(dtosNamespace))
           {
               Console.WriteLine("Controllers: DTO namespace not found");
               return false;
           }

           Assembly controllersAssembly = resolver.LoadFromAssemblyPath(
               Path.GetFullPath(controllersAssemblyPath));

           string? controllersNamespace = controllersAssembly.GetTypes()
               .First(e => e is { IsClass: true, IsPublic: true, Namespace: not null })
               .Namespace;

           if (string.IsNullOrEmpty(controllersNamespace))
           {
               Console.WriteLine("Controllers: Controllers namespace not found");
               return false;
           }

           foreach (Type modelType in modelsTypes)
           {
               if (modelType is { IsPublic: true, Namespace: not null, IsClass: true } &&
                   modelType.Namespace.EndsWith(modelsDirName))
               {
                   Console.WriteLine($"Controllers: Writing to entity: {modelType.Name}");

                   string newFile = $"{modelType.Name}Controller.cs";

                   var newPath = Path.Combine(controllersPath, newFile);
                  
                   File.WriteAllText(newPath, Consts.GetControllerString(
                       modelType.Namespace,interfacesNamespace,dtosNamespace,
                       controllersNamespace,modelType.Name));

                   Console.WriteLine("Controllers: Finished writing");
               }
           }
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
}