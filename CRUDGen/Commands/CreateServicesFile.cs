using System.Reflection;
using CRUDGen.Tts;

namespace CRUDGen.Commands;

public static class CreateServicesFile
{
    public static bool WriteServicesFile(string? servicesPath, string? servicesAssemblyPath,
        string? entitiesPath, string? entitiesAssemblyPath,
        string? dtoAssemblyPath, string? interfacesPath, bool overrideFiles)
    {
        if (string.IsNullOrEmpty(servicesPath) || string.IsNullOrEmpty(entitiesAssemblyPath) ||
            string.IsNullOrEmpty(dtoAssemblyPath) || string.IsNullOrEmpty(interfacesPath)
            || string.IsNullOrEmpty(entitiesPath) || string.IsNullOrEmpty(servicesAssemblyPath))
        {
            Console.WriteLine("A path is null or empty");
            return false;
        }
        try

        {
            Console.WriteLine("Starting Services Job \n");
            Directory.CreateDirectory(servicesPath);

            var entityDirName = new DirectoryInfo(entitiesPath).Name;

            var assemblyPaths = new List<string>()
            {
                Path.GetFullPath(dtoAssemblyPath),
                Path.GetFullPath(entitiesAssemblyPath),
                Path.GetFullPath(interfacesPath)
            };

            var resolver = new CustomAssemblyResolver(assemblyPaths);

            Console.WriteLine("Service: Loading assemblies\n");

            Assembly repositoriesAssembly = resolver.LoadFromAssemblyPath(
                Path.GetFullPath(interfacesPath));

            Assembly dtoAssembly = resolver.LoadFromAssemblyPath(
                Path.GetFullPath(dtoAssemblyPath));
            
            Assembly entitiesAssembly = resolver.LoadFromAssemblyPath(
                Path.GetFullPath(entitiesAssemblyPath));

            Type[] entityTypes = entitiesAssembly.GetTypes();

            Assembly servicesAssembly = resolver.LoadFromAssemblyPath(
                Path.GetFullPath(servicesAssemblyPath));

            string? dtosNamespace = dtoAssembly.GetTypes()
                .First(e => e is {IsPublic: true, Namespace: not null } && e.Name.Contains("Dto"))
                .Namespace;
            
            if (string.IsNullOrEmpty(dtosNamespace))
            {
                Console.WriteLine("Services(Error): Could not load DTO namespace");
                return false;
            }

            string? servicesNamespace = servicesAssembly.GetTypes()
                .First(e => e is { IsClass: true, IsPublic: true, Namespace: not null })
                .Namespace;
            
            if (string.IsNullOrEmpty(servicesNamespace))
            {
                Console.WriteLine("Services(Error): Could not load Services namespace");
                return false;
            }
            
            Console.WriteLine("Services: Assemblies loaded\n");

            foreach (Type entityType in entityTypes)
            {
                if (entityType is { IsClass: true, IsPublic: true, 
                        Namespace: not null } && entityType.Namespace.EndsWith(entityDirName))
                {
                    Console.WriteLine($"Services: Writing File of -> {entityType.Name}");
                    string? repositoryNamespace = repositoriesAssembly.GetTypes()
                        .First(e => e is { IsClass: true, IsPublic:true,Namespace: not null } && e.Name.Contains(entityType.Name)
                        && e.Name.Contains("Repository"))
                        .Namespace;

                    if (string.IsNullOrEmpty(repositoryNamespace))
                    {
                        Console.WriteLine("Services(Error): Could not load Repository namespace");
                        return false;
                    }
            
                    string entityNamespace = entityType.Namespace;
                    string fileName = $"{entityType.Name}.cs";

                    var file = Path.Combine(servicesPath, fileName);
                    
                    if (Path.Exists(file) && !overrideFiles)
                        continue;

                    var temp = new ServiceFile();

                    temp.DtosNamespace = dtosNamespace;
                    temp.EntitiesNamespace = entityNamespace;
                    temp.EntityName = entityType.Name;
                    temp.ServicesNamespace = servicesNamespace;
                    temp.RepositoryNamespace = repositoryNamespace;
                    
                    File.WriteAllText(file,temp.TransformText());
                    
                    File.WriteAllText(Path.Combine(servicesPath,"IService.cs"),
                        Consts.GetGenericService());
                    
                    Console.WriteLine("Services: File written successfully");
                } 
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        
        return true;
    }
}