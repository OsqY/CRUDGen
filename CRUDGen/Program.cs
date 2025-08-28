// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Diagnostics;
using CRUDGen.Commands;

var rootCommand = new RootCommand("CRUD Gen")
{
    new Option<string>("--modelsAssemblyDir"),
    new Option<string>("--dtosDir"),
    new Option<string>("--controllersDir"),
    new Option<string>("--interfacesDir"),
    new Option<string>("--servicesDir"),
    new Option<string>("--modelsDir"),
    new Option<string>("--dbContextAssemblyDir"),
    new Option<string>("--repositoriesDir"),
    new Option<string>("--servicesAssemblyDir"),
    new Option<string>("--dtosAssemblyDir"),
    new Option<string>("--repositoriesAssemblyDir")
};

ParseResult parseResult = rootCommand.Parse(args);
string? modelsDir = parseResult.GetValue<string>("--modelsDir");
string? modelsAssemblyDir = parseResult.GetValue<string>("--modelsAssemblyDir");
string? dtoDir = parseResult.GetValue<string>("--dtosDir");
string? repositoriesDir = parseResult.GetValue<string>("--repositoriesDir");
string? dbContextAssemblyDir = parseResult.GetValue<string>("--dbContextAssemblyDir");
string? servicesAssemblyDir = parseResult.GetValue<string>("--servicesAssemblyDir");
string? servicesDir = parseResult.GetValue<string>("--servicesDir");
string? dtosAssemblyDir = parseResult.GetValue<string>("--dtosAssemblyDir");
string? repositoriesAssemblyDir = parseResult.GetValue<string>("--repositoriesAssemblyDir");

Console.WriteLine($"{modelsDir},{modelsDir},{modelsAssemblyDir},{dtoDir},{dbContextAssemblyDir},{repositoriesDir},{servicesAssemblyDir},{servicesDir}");
CreateDtoFiles.CreateDtoFilesFromAssembly(modelsDir,modelsAssemblyDir,dtoDir);
CreateRepositoriesFiles.CreateRepositories(dbContextAssemblyDir,repositoriesDir,
    modelsDir,modelsAssemblyDir);

ProcessStartInfo psi = new ProcessStartInfo();
psi.FileName = "dotnet";
psi.Arguments = "build";
psi.WorkingDirectory = Directory.GetCurrentDirectory();
Process? process = Process.Start(psi);
if (process != null) await process.WaitForExitAsync();

CreateServicesFile.WriteServicesFile(servicesDir,servicesAssemblyDir,
    modelsDir,modelsAssemblyDir,dtosAssemblyDir,repositoriesAssemblyDir);