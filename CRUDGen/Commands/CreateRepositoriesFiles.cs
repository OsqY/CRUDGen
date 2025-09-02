using System.Reflection;
using System.Text;
using CRUDGen.Tts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRUDGen.Commands;

public static class CreateRepositoriesFiles
{
    public static bool CreateRepositories(string? dbContextAssemblyPath, string? repositoriesPath,
        string? modelsPath, string? modelsAssemblyPath, bool overrideFiles)
    {
        if (string.IsNullOrEmpty(dbContextAssemblyPath) || string.IsNullOrEmpty(modelsAssemblyPath) ||
            string.IsNullOrEmpty(repositoriesPath) || string.IsNullOrEmpty(modelsPath))
            return false;

        try
        {
            Console.WriteLine("Starting Repository job...");
            Directory.CreateDirectory(repositoriesPath);

            var modelFolderName = new DirectoryInfo(modelsPath).Name;
            var repositoryNameSpace = new DirectoryInfo(repositoriesPath).Name;

            Console.WriteLine("Loading assemblies");
            var assemblyPaths = new List<string>()
            {
                Path.GetFullPath(modelsAssemblyPath),
                Path.GetFullPath(dbContextAssemblyPath)
            };
            
            var resolver = new CustomAssemblyResolver(assemblyPaths);
            Assembly externalAssembly = resolver.LoadFromAssemblyPath(Path.GetFullPath(modelsAssemblyPath));
            Type[] allTypes = externalAssembly.GetTypes();
            
            Assembly dbContextAssembly = resolver.LoadFromAssemblyPath(Path.GetFullPath(dbContextAssemblyPath));
            Type[] dbContextTypes = dbContextAssembly.GetTypes();
            
            Console.WriteLine("Assemblies loaded");
            string dbContextNameSpace = "";

            int matchedTypes = 0;
            bool foundIRepository = false;
            Console.WriteLine("Checking types");

            string modelsNamespace = "";

            foreach (Type type in allTypes)
            {
                if (type is { IsInterface: true, IsPublic: true, Namespace: not null }
                    && type.Name != "IRepository" && !foundIRepository)
                {
                    Console.WriteLine("found IRepository doesn't exist");
                    foundIRepository = true;
                    var fileName = "IRepository.cs";
                    var filePath = Path.Combine(repositoriesPath, fileName);

                    
                    foreach (Type dbContext in dbContextTypes)
                    {
                        if (dbContext is { IsClass: true, IsPublic:true,Namespace:not null } &&
                            (dbContext.IsSubclassOf(typeof(DbContext)) ||
                             dbContext.IsSubclassOf(typeof(IdentityDbContext)))
                           )
                        {
                            Console.WriteLine("found DbContext");
                            dbContextNameSpace = dbContext.Namespace;
                        }
                    }

                    if (string.IsNullOrEmpty(dbContextNameSpace))
                    {
                        Console.WriteLine("No namespace found");
                        return false;
                    }
                    
                    var dbContextNamespace = dbContextNameSpace;
                    Console.WriteLine("Writing IRepository");
                    
                    File.WriteAllText(filePath,
                        Consts.GetGenericRepositoryString(repositoryNameSpace,
                            dbContextNameSpace));
                }

                if (type is { IsClass: true, IsPublic: true, Namespace: not null } &&
                    type.Namespace.EndsWith(modelFolderName))
                {
                    Console.WriteLine("Writing implementations");
                    var entityName = type.Name;
                    var entityRepositoryPath = Path.Combine(repositoriesPath, entityName, 
                        $"{entityName}Repository.cs");

                    if (Path.Exists(entityRepositoryPath) && !overrideFiles)
                        continue;

                    modelsNamespace = type.Namespace;

                    Directory.CreateDirectory(Path.Combine(repositoriesPath, entityName));

                    var temp = new RepositoryFile();

                    temp.Session = new Dictionary<string, object>
                    {
                        { "EntityName", entityName },
                        { "ModelNamespace", modelsNamespace },
                        { "RepositoryNamespace", repositoryNameSpace},
                    };
                        
                    temp.Initialize();

                    File.WriteAllText(entityRepositoryPath,temp.TransformText());
                
                    Console.WriteLine("Finished writing");
                    
                }
                
            }
            return true;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}