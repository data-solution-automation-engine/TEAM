using System;
using System.IO;

namespace TEAM
{
    internal class TeamJsonHandling
    {
        /// <summary>
        /// Local class to manage file names and changes thereof.
        /// </summary>
        internal static class JsonFileConfiguration
        {
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
                return localJsonFileName; }

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