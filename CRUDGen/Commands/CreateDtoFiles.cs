using System.Reflection;
using System.Text;

namespace CRUDGen.Commands;

public static class CreateDtoFiles
{
    public static bool CreateDtoFilesFromAssembly(string? modelsPath, string? assemblyPath, string? toDirectory,
        bool overrideFiles)
    {
        if (string.IsNullOrEmpty(modelsPath) || string.IsNullOrEmpty(assemblyPath) || string.IsNullOrEmpty(toDirectory))
        {
            Console.WriteLine("Null or empty paths, error.");
            return false;
        }

        try
        {
            Console.WriteLine("Starting DTO process");
            Directory.CreateDirectory(toDirectory);

            var modelFolderName = new DirectoryInfo(modelsPath).Name;
            var dtoNamespace = new DirectoryInfo(toDirectory).Name;
            
            Console.WriteLine($"Searching classes with: '{modelFolderName}'");

            Assembly externalAssembly = Assembly.LoadFile(Path.GetFullPath(assemblyPath));
            Type[] allTypes = externalAssembly.GetTypes();

            Console.WriteLine($"Found {allTypes.Length} types.");
            int matchedTypes = 0;

            foreach (Type aClass in allTypes)
            {

                if (aClass is { IsClass: true, IsPublic: true, Namespace: not null } &&
                    aClass.Namespace.EndsWith(modelFolderName))
                {
                    matchedTypes++;
                    Console.WriteLine($"Class to process: {aClass.Name}");
                    var fileName = aClass.Name + "Dto.cs";
                    var filePath = Path.Combine(toDirectory, fileName);

                    if (Path.Exists(filePath) && !overrideFiles)
                        continue;
                    
                    var properties = aClass.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead && p.CanWrite);

                    var sb = new StringBuilder();
                    var sbCollections = new StringBuilder();
                    sbCollections.Append(", ");
                    foreach (var prop in properties)
                    {
                        if (prop.PropertyType != typeof(string) &&
                            typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType))
                        {
                            Type? genericType = prop.PropertyType.GetGenericArguments().FirstOrDefault();

                            if (genericType != null)
                            {
                                sbCollections.Append($"IEnumerable<{genericType.Name}> {genericType.Name}s, ");
                            }
                            else
                            {
                                sbCollections.Append(GetFriendlyTypeName(prop.PropertyType));
                            }
                            
                        }
                        else
                        {
                            string propTypeName = GetFriendlyTypeName(prop.PropertyType);
                            sb.Append($"{propTypeName} {prop.Name}, ");
                        }
                        
                    }

                    if (sb.Length > 0) sb.Length -= 2;
                    sbCollections.Length -= 2;

                    string fileContent = $"namespace {dtoNamespace};\n\npublic record Create{aClass.Name}Dto({sb.ToString()});\n";
                    fileContent += $"public record Update{aClass.Name}Dto({sb.ToString()});\n";
                    fileContent += $"public record Response{aClass.Name}Dto({sb.ToString()}{sbCollections.ToString()});\n";
                    fileContent += $"public record {aClass.Name}Dto({sb.ToString()});\n";
                    Console.WriteLine("DTO: Class processed\n");
                    File.WriteAllText(filePath, fileContent);
                }
            }

            if (matchedTypes == 0)
            {
                Console.WriteLine("Any classes found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception {ex.Message}");
            return false;
        }

        return true;
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return $"{GetFriendlyTypeName(Nullable.GetUnderlyingType(type)!)}?";
        }
        
        return type.Name
            .Replace("String", "string").Replace("Int32", "int")
            .Replace("Boolean", "bool").Replace("Decimal", "decimal");
    }
}