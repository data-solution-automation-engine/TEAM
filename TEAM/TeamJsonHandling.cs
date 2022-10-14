using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TEAM
{
    internal class TeamJsonHandling
    {
        /// <summary>
        /// Method to create a new dummy Json file in the designated working directory.
        /// </summary>
        /// <param name="fileType"></param>
        internal static void CreateDummyJsonFile(string fileType)
        {
            JArray outputFileArray = new JArray();
            JObject dummyJsonFile = new JObject();

            string outputFileName = "";
            if (fileType == FormBase.GlobalParameters.JsonTableMappingFileName) // Table Mapping
            {
                dummyJsonFile = new JObject(
                    new JProperty("enabledIndicator", true),
                    new JProperty("tableMappingHash", "NewHash"),
                    new JProperty("sourceTable", "STG_EXAMPLESYSTEM_EXAMPLETABLE"),
                    new JProperty("sourceConnection", null),
                    new JProperty("targetTable", "HUB_EXAMPLE"),
                    new JProperty("targetConnectionKey", null),
                    new JProperty("businessKeyDefinition", "EXAMPLE"),
                    new JProperty("drivingKeyDefinition", ""),
                    new JProperty("filterCriteria", "")
                ); 
                
                outputFileName = JsonFileConfiguration.TableMappingJsonFileName();

            } else if (fileType == FormBase.GlobalParameters.JsonModelMetadataFileName) // Physical Model
            {
                dummyJsonFile = new JObject(
                    new JProperty("attributeHash", "NewHash"),
                    new JProperty("databaseName", "Sample database"),
                    new JProperty("schemaName", "[dbo]"),
                    new JProperty("tableName", "Sample Table"),
                    new JProperty("columnName", "Sample Column"),
                    new JProperty("dataType", "nvarchar"),
                    new JProperty("characterLength", "100"),
                    new JProperty("numericPrecision", "0"),
                    new JProperty("numericScale", "0"),
                    new JProperty("ordinalPosition", "1"),
                    new JProperty("primaryKeyIndicator", "N"),
                    new JProperty("multiActiveIndicator", "N")
                );

                outputFileName = JsonFileConfiguration.PhysicalModelJsonFileName();

            } else if (fileType == FormBase.GlobalParameters.JsonAttributeMappingFileName) // Attribute Mapping
            {
                dummyJsonFile = new JObject(
                    new JProperty("attributeMappingHash", "NewHash"),
                    new JProperty("sourceTable", "SOURCE_TABLE"),
                    new JProperty("sourceAttribute", "EXAMPLE_FROM_ATTRIBUTE"),
                    new JProperty("targetTable", "TARGET_TABLE"),
                    new JProperty("targetAttribute", "EXAMPLE_TO_ATTRIBUTE"),
                    new JProperty("notes", "")
                );

                outputFileName = JsonFileConfiguration.AttributeMappingJsonFileName();
            }
            else
            {
                // No action - issue in code
            }

            // Spool to disk
            outputFileArray.Add(dummyJsonFile);
            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);
            
            File.WriteAllText(outputFileName, json);
        }

        /// <summary>
        /// Local class to manage file names and changes thereof.
        /// </summary>
        internal static class JsonFileConfiguration
        {
            /// <summary>
            /// Builds and returns the fully qualified name of the table mapping Json file.
            /// </summary>
            /// <returns></returns>
            public static string TableMappingJsonFileName()
            {
                string localJsonFileName = FormBase.GlobalParameters.MetadataPath + FormBase.GlobalParameters.WorkingEnvironment + "_" + FormBase.GlobalParameters.JsonTableMappingFileName + FormBase.GlobalParameters.JsonExtension;
                return localJsonFileName;
            }

            /// <summary>
            /// Builds and returns the fully qualified name of the attribute mapping Json file.
            /// </summary>
            /// <returns></returns>
            public static string AttributeMappingJsonFileName()
            {
                string localJsonFileName = FormBase.GlobalParameters.MetadataPath + FormBase.GlobalParameters.WorkingEnvironment + "_" + FormBase.GlobalParameters.JsonAttributeMappingFileName + FormBase.GlobalParameters.JsonExtension;
                return localJsonFileName;
            }

            /// <summary>
            /// Builds and returns the fully qualified name of the physical model Json file.
            /// </summary>
            /// <returns></returns>
            public static string PhysicalModelJsonFileName()
            {
                string localJsonFileName = FormBase.GlobalParameters.MetadataPath + FormBase.GlobalParameters.WorkingEnvironment + "_" + FormBase.GlobalParameters.JsonModelMetadataFileName + FormBase.GlobalParameters.JsonExtension;
                return localJsonFileName;
            }

            internal static string newFileTableMapping { get; set; }
            internal static string newFileAttributeMapping { get; set; }
            internal static string newFilePhysicalModel { get; set; }
        }

        /// <summary>
        /// Create a completely empty file.
        /// </summary>
        /// <param name="fileType"></param>
        internal static void CreatePlaceholderJsonFile(string fileType)
        {
            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileType + FormBase.GlobalParameters.JsonExtension, "");
        }


        /// <summary>
        /// Create a backup of a given JSON file.
        /// </summary>
        /// <param name="inputFileName"></param>
        internal string BackupJsonFile(string inputFileName, string inputFilePath)
        {
            var shortDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");

            var targetFilePathName = inputFilePath + string.Concat("Backup_" + shortDatetime + "_", inputFileName);

            File.Copy( inputFilePath+inputFileName, targetFilePathName);

            var result = targetFilePathName;

            return result;
        }

        /// <summary>
        /// Clear out (remove) an existing Json file, to facilitate overwriting.
        /// </summary>
        internal static void RemoveExistingJsonFile(string inputFileName)
        {
            File.Delete(FormBase.GlobalParameters.ConfigurationPath + inputFileName);
        }
    }
}