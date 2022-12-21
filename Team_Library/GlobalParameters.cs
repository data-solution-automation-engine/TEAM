using System.Windows.Forms;

namespace TEAM_Library
{ 
    public class GlobalParameters
    {
        // TEAM core path parameters
        public string RootPath { get; } = Application.StartupPath + @"\";
        public string ConfigurationPath { get; set; } = Application.StartupPath + @"\" + @"Configuration\";
        public string MetadataPath { get; set; } = Application.StartupPath + @"\" + @"Metadata\";

        public string PhysicalModelDirectory { get; set; } = @"PhysicalModel\";
        public string CorePath { get; } = Application.StartupPath + @"\" + @"Core\";

        public string SchemaPath { get; } = Application.StartupPath + @"\" + @"Schema\";

        public string BackupPath { get; } = Application.StartupPath + @"\" + @"Backup\";

        public string ModelLayerPath { get; } = Application.StartupPath + @"\" + @"ModelLayers\";
        public string ScriptPath { get; set; } = Application.StartupPath + @"\" + @"Scripts\";
        public string FilesPath { get; set; } = Application.StartupPath + @"\" + @"Files\";
        public string ConfigFileName { get; set; } = "TEAM_configuration";
        public string PathFileName { get; set; } = "TEAM_Path_configuration";
        public string ValidationFileName { get; set; } = "TEAM_validation";
        public string JsonExportConfigurationFileName { get; set; } = "TEAM_jsonconfiguration";
        public string FileExtension { get; set; } = ".txt";
        public string ActiveEnvironmentInternalId { get; set; } = Utility.CreateMd5(new[] { "Development" }, "%$@");
        public string ActiveEnvironmentKey { get; set; }

        // JSON file name parameters
        public string JsonSchemaForDataWarehouseAutomationFileName { get; } = "interfaceDataWarehouseAutomationMetadata.json";
        public static string JsonTableMappingFileName { get; } = "TEAM_Table_Mapping";
        public string JsonAttributeMappingFileName { get; } = "TEAM_Attribute_Mapping";
        public string JsonModelMetadataFileName { get; } = "TEAM_Model_Metadata";
        public string JsonConnectionFileName { get; } = "TEAM_connections";
        public string JsonEnvironmentFileName { get; } = "TEAM_environments";
        public string JsonExtension { get; } = ".json";

        /// <summary>
        /// Extension method to infer the target path for a given string value (should be a target data object name).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetMetadataFilePath(string fileName)
        {
            return MetadataPath + fileName + ".json";
        }
    }
}
