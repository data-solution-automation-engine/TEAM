using System.Collections.Generic;
using System.Windows.Forms;
using TEAM_Library;

namespace TEAM
{
    public partial class FormBase : Form
    {
        protected FormMain MyParent;

        public FormBase()
        {
            InitializeComponent();
        }

        public FormBase(FormMain myParent)
        {
            MyParent = myParent;
            InitializeComponent();
        }
        
        /// <summary>
        /// TEAM configurations (e.g. conventions, prefixes, attribute names).
        /// </summary>
        public static TeamConfiguration TeamConfiguration { get; set; } = new TeamConfiguration();

        /// <summary>
        /// Return the full TeamConnection object for a given (TeamConnection) connection Id string.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public static TeamConnection GetTeamConnectionByConnectionId(string connectionId)
        {
            if (!TeamConfiguration.ConnectionDictionary.TryGetValue(connectionId, out var teamConnection))
            {
                // The key isn't in the dictionary.
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The connection could not be matched for Connection Id {connectionId}."));
            }

            return teamConnection;
        }

        public static TeamConnection GetTeamConnectionByConnectionKey(string connectionKey)
        {
            TeamConnection returnTeamConnection = new TeamConnection();

            foreach (var teamConnection in TeamConfiguration.ConnectionDictionary)
            {
                if (teamConnection.Value.ConnectionKey == connectionKey)
                {
                    returnTeamConnection = teamConnection.Value;
                }
            }

            return returnTeamConnection;
        }

        #region Metadata objects in memory
        // In-memory representation of the Table Mapping Metadata.
        public static TeamDataObjectMapping TableMapping { get; set; } = new TeamDataObjectMapping();

        // In-memory representation of the Physical Model Metadata.
        public static TeamPhysicalModel PhysicalModel { get; set; } = new TeamPhysicalModel();

        // In-memory representation of the Attribute Mapping Metadata.
        public static  TeamAttributeMapping AttributeMapping { get; set; } = new TeamAttributeMapping();
        #endregion

        /// <summary>
        /// Instance of a Team Version List, basically containing the list of versions for a given active environment.
        /// </summary>
        public static TeamVersionList TeamVersionList { get; set; } = new TeamVersionList();

        /// <summary>
        /// TEAM working environment collection.
        /// </summary>
        public static TeamWorkingEnvironmentCollection TeamEnvironmentCollection { get; set; } = new TeamWorkingEnvironmentCollection();

        /// <summary>
        /// Instance of the export configuration for JSON files (options).
        /// </summary>
        public static JsonExportSetting JsonExportSetting { get; set; } = new JsonExportSetting();

        /// <summary>
        /// Instance of the validation settings (validation options).
        /// </summary>
        public static ValidationSetting ValidationSetting { get; set; } = new ValidationSetting();

        /// <summary>
        /// These parameters are used as global constants throughout the application.
        /// </summary>
        internal static class GlobalParameters
        {
            internal static EventLog TeamEventLog { get; set; } = new EventLog();

            // TEAM core path parameters
            public static string RootPath { get; } = Application.StartupPath + @"\";
            public static string ConfigurationPath { get; set; } = RootPath + @"Configuration\";
            public static string OutputPath { get; set; } = RootPath + @"Output\";
            public static string MetadataPath { get; set; } = RootPath + @"Metadata\";
            public static string CorePath { get; } = RootPath + @"Core\";

            public static string BackupPath { get; } = RootPath + @"Backup\";
            public static string ScriptPath { get; set; } = RootPath + @"Scripts\";
            public static string FilesPath { get; set; } = RootPath + @"Files\";
            internal static string LoadPatternPath { get; set; } = RootPath + @"LoadPatterns\";

            public static string ConfigFileName { get; set; } = "TEAM_configuration";
            public static string PathFileName { get; set; } = "TEAM_Path_configuration";
            public static string ValidationFileName { get; set; } = "TEAM_validation";
            public static string JsonExportConfigurationFileName { get; set; } = "TEAM_jsonconfiguration";
            public static string VersionFileName { get; } = "TEAM_versions";
            public static string FileExtension { get; set; } = ".txt";
            internal static string WorkingEnvironment { get; set; } = "Development";
            internal static string WorkingEnvironmentInternalId { get; set; }

            // JSON file name parameters
            public static string JsonSchemaForDataWarehouseAutomationFileName { get; } = "interfaceDataWarehouseAutomationMetadata.json";
            public static string JsonTableMappingFileName { get; } = "TEAM_Table_Mapping";
            public static string JsonAttributeMappingFileName { get; } = "TEAM_Attribute_Mapping";
            public static string JsonModelMetadataFileName { get; } = "TEAM_Model_Metadata";
            public static string JsonConnectionFileName { get; } = "TEAM_connections";
            public static string JsonEnvironmentFileName { get; } = "TEAM_environments";
            public static string JsonExtension { get;  } = ".json";

            public static string LoadPatternDefinitionFile { get; } = "loadPatternDefinition.json";

            // Version handling
            public static int CurrentVersionId { get; set; } = 0;
            public static int HighestVersionId { get; set; } = 0;

            // File paths
            public static List<LoadPatternDefinition> PatternDefinitionList { get; set; }

            // Environment mode
            public static EnvironmentModes EnvironmentMode { get; set; }
        }
    }
}
