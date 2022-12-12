using System;
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
        /// Reusable global parameters. Accessed throughout the app.
        /// </summary>
        public static GlobalParameters globalParameters { get; set; } = new GlobalParameters();

        /// <summary>
        /// TEAM configurations (e.g. conventions, prefixes, attribute names).
        /// </summary>
        public static TeamConfiguration TeamConfiguration { get; set; } = new TeamConfiguration();

        #region Metadata objects in memory

        // In-memory representation of the Physical Model Metadata.
        public static TeamPhysicalModel PhysicalModel { get; set; } = new TeamPhysicalModel();

        // In-memory representation of the Attribute Mapping Metadata.
        public static  TeamDataItemMapping AttributeMapping { get; set; } = new TeamDataItemMapping();

        #endregion

        /// <summary>
        /// TEAM working environment collection.
        /// </summary>
        public static TeamEnvironmentCollection TeamEnvironmentCollection { get; set; } = new TeamEnvironmentCollection();

        /// <summary>
        /// Instance of the export configuration for JSON files (options).
        /// </summary>
        public static JsonExportSetting JsonExportSetting { get; set; } = new JsonExportSetting();

        /// <summary>
        /// Instance of the validation settings (validation options).
        /// </summary>
        public static ValidationSetting ValidationSetting { get; set; } = new ValidationSetting();

        // Create the grid views.
        internal static DataGridViewDataObjects _dataGridViewDataObjects;
        internal static DataGridViewDataItems _dataGridViewDataItems;
        internal static DataGridViewPhysicalModel _dataGridViewPhysicalModel;

        internal static EventLog TeamEventLog { get; set; } = new EventLog();

        public class FilterEventArgs : EventArgs
        {
            public bool DoFilter { get; private set; }

            public FilterEventArgs(bool doFilter)
            {
                DoFilter = doFilter;
            }
        }
    }
}
