namespace CRUDGen.Lib;

public static class ModelNamesUtils
{
   public static string ConvertModelNameToVariableName(string modelName)
   {
      return modelName[0].ToString().ToLower() + modelName[new Range(1, modelName.Length - 1)];
   }

   public static string[] ConvertModelNameToPermissions(string modelName)
   {
      return
      [
         $"{modelName}.list",
         $"{modelName}.view",
         $"{modelName}.create",
         $"{modelName}.createFromFor",
         $"{modelName}.update",
         $"{modelName}.delete",
      ];
   }
}