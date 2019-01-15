using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TEAM
{

    internal class JsonHandling
    {
        /// <summary>
        ///    Method to create a new dummy JSON file in the designated working directory.
        /// </summary>
        internal static void CreateDummyJsonFile()
        {
            var jsonVersionExtension = @"_v" + FormBase.GlobalParameters.VersionId + ".json";

            JArray outputFileArray = new JArray();

            JObject dummyJsonTableMappingFile = new JObject(
                new JProperty("tableMappingHash", "NewHash"),
                new JProperty("versionId", "versionId"),
                new JProperty("stagingAreaTable", "STG_EXAMPLESYSTEM_EXAMPLETABLE"),
                new JProperty("integrationAreaTable", "HUB_EXAMPLE"),
                new JProperty("businessKeyDefinition", "EXAMPLE"),
                new JProperty("drivingKeyDefinition", ""),
                new JProperty("filterCriteria", ""),
                new JProperty("generationIndicator", "Y")
            );

            outputFileArray.Add(dummyJsonTableMappingFile);

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.JsonTableMappingFileName + jsonVersionExtension, json);
        }

        internal static void CreatePlaceholderJsonFile()
        {
            var jsonVersionExtension = @"_v" + FormBase.GlobalParameters.VersionId + ".json";

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.JsonTableMappingFileName + jsonVersionExtension, "");
        }


        /// <summary>
        ///    Create a backup of a given JSON file (including path).
        /// </summary>
        /// <param name="inputFileName"></param>
        internal string BackupJsonFile(string inputFileName)
        {
            string result;
            var shortDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
            var targetFilePathName = FormBase.GlobalParameters.ConfigurationPath + string.Concat("Backup_" + shortDatetime + "_", inputFileName);

            File.Copy(FormBase.GlobalParameters.ConfigurationPath + inputFileName, targetFilePathName);
            result = targetFilePathName;

            return result;
        }

        /// <summary>
        ///   Clear out (remove) an existing Json file, to facilitate overwriting.
        /// </summary>
        internal static void RemoveExistingJsonFile(string inputFileName)
        {
            File.Delete(FormBase.GlobalParameters.ConfigurationPath + inputFileName);
        }
    }
}