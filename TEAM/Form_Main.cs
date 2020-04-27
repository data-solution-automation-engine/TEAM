using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;
using System.Drawing;
using System.Data;
using System.Globalization;

namespace TEAM
{
    public partial class FormMain : FormBase
    {
        internal bool RevalidateFlag = true;
        public FormMain()
        {
            InitializeComponent();

            // Instantiate the logging
            EventLog eventLog = new EventLog();

            // Set the version of the build for everything
            const string versionNumberForTeamApplication = "v1.6.0.0";
            Text = "TEAM - Taxonomy for ETL Automation Metadata " + versionNumberForTeamApplication;

            richTextBoxInformation.AppendText("Initialising the application.\r\n\r\n");

            // Make sure the application and custom location directories exist
            try
            {
                EnvironmentConfiguration.InitialiseRootPath();
                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, "... The TEAM directories are available and initialised.\r\n"));
            }
            catch
            {
                eventLog.Add(Event.CreateNewEvent(EventTypes.Error, "The directories required to operate TEAM are not available and can not be created. Do you have administrative priviliges in the installation directory to create these additional directories?"));
            }

            // Set the root path, to be able to locate the customisable configuration file
            try
            {
                EnvironmentConfiguration.LoadRootPathFile();
                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, "... The core configuration file has been loaded.\r\n"));
            }
            catch 
            {
                eventLog.Add(Event.CreateNewEvent(EventTypes.Error, "The core configuration file could not be loaded. Is there a Configuration directory in the TEAM installation location?"));
            }

            // Make sure the configuration file is in memory
            try
            {
                EnvironmentConfiguration.InitialiseConfigurationPath();
                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, "... The user configuration paths are available.\r\n"));
            }
            catch
            {
                eventLog.Add(Event.CreateNewEvent(EventTypes.Error, "An issue was encountered creating or detecting the configuration paths."));
            }

            // Load the available configuration file
            try
            {
                EnvironmentConfiguration.LoadConfigurationFile(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigfileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, "... The user configuration settings (file) have been loaded.\r\n"));
            }
            catch
            {
                eventLog.Add(Event.CreateNewEvent(EventTypes.Error, "An issue was encountered loading the user configuration file."));
            }

            // Load the pattern definition file
            try
            {
                ConfigurationSettings.patternDefinitionList = LoadPatternDefinition.DeserializeLoadPatternDefinition(GlobalParameters.RootPath + @"..\LoadPatterns\" + GlobalParameters.LoadPatternDefinitionFile);
                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, "... The pattern definition file was loaded successfully.\r\n"));
            }
            catch 
            {
                eventLog.Add(Event.CreateNewEvent(EventTypes.Error, "An issue was encountered loading the pattern definition file."));
            }


            // Report the events (including errors) back to the user
            int errorCounter = 0;
            foreach (Event individualEvent in eventLog)
            {
                if (individualEvent.eventCode == (int)EventTypes.Error)
                {
                    errorCounter++;
                }

                richTextBoxInformation.AppendText(individualEvent.eventDescription);
            }

            richTextBoxInformation.AppendText($"\r\n{errorCounter} error(s) have been found at startup.\r\n\r\n");


            TestConnections();

            //Startup information
            richTextBoxInformation.AppendText("\r\nApplication initialised - the Taxonomy of ETL Automation Metadata (TEAM). \r\n");
            richTextBoxInformation.AppendText("Welcome to version " + versionNumberForTeamApplication + ".\r\n\r\n");

            labelWorkingEnvironment.Text = "The working environment is: " + GlobalParameters.WorkingEnvironment;
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public void TestConnections()
        {
            if (RevalidateFlag == false)
                return;
            RevalidateFlag = false;
            //MessageBox.Show("Validating Connections");
            richTextBoxInformation.AppendText("Validating database connections.\r\n");
            var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };
            var connStg = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringStg };
            var connPsa = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringHstg };

            if (connOmd.ConnectionString != "Server=<>;Initial Catalog=<Metadata>;user id=sa; password=<>")
                try
                {
                    connOmd.Open();
                }
                catch
                {
                    richTextBoxInformation.AppendText("There was an issue establishing a database connection to the Metadata Repository Database. Can you verify the connection information in the 'configuration' menu option? \r\n");
                    DisableMenu();
                    return;
                }
            else
            { 
                richTextBoxInformation.AppendText("Metadata Repository Connection wasn't defined yet. Please set the connection information in the 'configuration' menu option? \r\n");
                DisableMenu();
                return;
            }
            

            if (connStg.ConnectionString != "Server=<>;Initial Catalog=<Staging_Area>;user id=sa; password=<>")
                try
                {
                    connStg.Open();
                    connStg.Close();
                    connStg.Dispose();
                }
                catch
                {
                    richTextBoxInformation.AppendText("There was an issue establishing a database connection to the Staging Area Database. Can you verify the connection information in the 'configuration' menu option? \r\n");
                    DisableMenu();
                    return;
                }
            else
            {
                richTextBoxInformation.AppendText("Staging Area connection wasn't defined yet. Please set the connection information in the 'configuration' menu option? \r\n");
                DisableMenu();
                return;
            }
            if (connStg.ConnectionString != "Server=<>;Initial Catalog=<Persistent_Staging_Area>;user id=sa; password=<>")
                try
                {
                    connPsa.Open();
                    connPsa.Close();
                    connPsa.Dispose();
                }
                catch
                {
                    richTextBoxInformation.AppendText("There was an issue establishing a database connection to the Persistent Staging Area (PSA) Database. Can you verify the connection information in the 'configuration' menu option? \r\n");
                    DisableMenu();
                    return;
                }
            else
            { 
                richTextBoxInformation.AppendText("Persistent Staging Area (PSA) connection wasn't defined yet. Please set the connection information in the 'configuration' menu option? \r\n");
                DisableMenu();
                return;
            }
            EnableMenu();
            richTextBoxInformation.AppendText("Database connections have been successfully validated.\r\n");

            try
            {
                DisplayMaxVersion(connOmd);
                DisplayCurrentVersion(connOmd);
                DisplayRepositoryVersion(connOmd);
                openMetadataFormToolStripMenuItem.Enabled = true;

                labelMetadataRepository.Text = "Repository type in configuration is set to: " +
                                               ConfigurationSettings.MetadataRepositoryType;
            }
            catch
            {
                richTextBoxInformation.AppendText("There was an issue while reading Metadata Database. The Database is missing tables  \r\n");
                openMetadataFormToolStripMenuItem.Enabled = false;
                RevalidateFlag = true;
            }
            finally
            {
                connOmd.Close();
                connOmd.Dispose();
            }

        }

        public void DisableMenu()
        {
            metadataToolStripMenuItem.Enabled = false;
        }
        public void EnableMenu()
        {
            metadataToolStripMenuItem.Enabled = true;
        }

        internal void DisplayMaxVersion(SqlConnection connOmd)
        {
            var selectedVersion = GetMaxVersionId(connOmd);

            var versionMajorMinor = GetVersion(selectedVersion, connOmd);
            var majorVersion = versionMajorMinor.Key;
            var minorVersion = versionMajorMinor.Value;

            labelVersion.Text = majorVersion + "." + minorVersion;
        }

        internal void DisplayCurrentVersion(SqlConnection connOmd)
        {
            var sqlStatementForCurrentVersion = new StringBuilder();
            sqlStatementForCurrentVersion.AppendLine("SELECT [VERSION_NAME] FROM [MD_MODEL_METADATA]");

            try
            {
                var versionList = GetDataTable(ref connOmd, sqlStatementForCurrentVersion.ToString());

                if (versionList != null && versionList.Rows.Count > 0)
                {
                    foreach (DataRow versionNameRow in versionList.Rows)
                    {
                        var versionName = (string) versionNameRow["VERSION_NAME"];
                        labelActiveVersion.Text = versionName;
                    }
                }

            }
            catch (Exception)
            {
                labelActiveVersion.Text = "There has been an error displaying the active version";
            }
        }


        internal void DisplayRepositoryVersion(SqlConnection connOmd)
        {
            var sqlStatementForCurrentVersion = new StringBuilder();
            sqlStatementForCurrentVersion.AppendLine("SELECT [REPOSITORY_VERSION],[REPOSITORY_UPDATE_DATETIME] FROM [MD_REPOSITORY_VERSION]");

            var versionList = GetDataTable(ref connOmd, sqlStatementForCurrentVersion.ToString());

            try
            {
                if (versionList != null && versionList.Rows.Count > 0)
                {
                    foreach (DataRow versionNameRow in versionList.Rows)
                    {
                        var versionName = (string) versionNameRow["REPOSITORY_VERSION"];
                        var versionDate = (DateTime) versionNameRow["REPOSITORY_UPDATE_DATETIME"];
                        labelRepositoryVersion.Text = versionName;
                        labelRepositoryDate.Text = versionDate.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception)
            {
                // THROW EXCEPTION
            }
        }

        private void CheckKeyword(string word, Color color, int startIndex)
        {
            if (richTextBoxInformation.Text.Contains(word))
            {
                int index = -1;
                int selectStart = richTextBoxInformation.SelectionStart;

                while ((index = richTextBoxInformation.Text.IndexOf(word, (index + 1), StringComparison.Ordinal)) != -1)
                {
                    richTextBoxInformation.Select((index + startIndex), word.Length);
                    richTextBoxInformation.SelectionColor = color;
                    richTextBoxInformation.Select(selectStart, 0);
                    richTextBoxInformation.SelectionColor = Color.Black;
                }
            }
        }

        private void openOutputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(GlobalParameters.OutputPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the output directory. The error message is: "+ex;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



        private void openMetadataFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (metadataToolStripMenuItem.Enabled == false)
                return;
            var t = new Thread(ThreadProcMetadata);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcAbout);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }


        #region From Close Delegates
        private void CloseMetadataForm(object sender, FormClosedEventArgs e)
        {
            _myMetadataForm = null;
        }

        private void CloseRepositoryForm(object sender, FormClosedEventArgs e)
        {
            _myRepositoryForm = null;
        }

        private void CloseAboutForm(object sender, FormClosedEventArgs e)
        {
            _myAboutForm = null;
        }

        private void CloseConfigurationForm(object sender, FormClosedEventArgs e)
        {
            _myConfigurationForm = null;
        }

        private void ClosePatternForm(object sender, FormClosedEventArgs e)
        {
            _myPatternForm = null;
        }
        #endregion

        #region Form Threads
        private FormManagePattern _myPatternForm;
        [STAThread]
        public void ThreadProcPattern()
        {
            if (_myPatternForm == null)
            {
                _myPatternForm = new FormManagePattern(this);
                Application.Run(_myPatternForm);
            }
            else
            {
                if (_myPatternForm.InvokeRequired)
                {
                    // Thread Error
                    _myPatternForm.Invoke((MethodInvoker)delegate { _myPatternForm.Close(); });
                    _myPatternForm.FormClosed += ClosePatternForm;

                    _myPatternForm = new FormManagePattern(this);
                    Application.Run(_myPatternForm);
                }
                else
                {
                    // No invoke required - same thread
                    _myPatternForm.FormClosed += ClosePatternForm;
                    _myPatternForm = new FormManagePattern(this);

                    Application.Run(_myPatternForm);
                }
            }
        }

        private FormManageConfiguration _myConfigurationForm;
        [STAThread]
        public void ThreadProcConfiguration()
        {
            if (_myConfigurationForm == null)
            {
                _myConfigurationForm = new FormManageConfiguration(this);
                Application.Run(_myConfigurationForm);
            }
            else
            {
                if (_myConfigurationForm.InvokeRequired)
                {
                    // Thread Error
                    _myConfigurationForm.Invoke((MethodInvoker)delegate { _myConfigurationForm.Close(); });
                    _myConfigurationForm.FormClosed += CloseConfigurationForm;

                    _myConfigurationForm = new FormManageConfiguration(this);
                    Application.Run(_myConfigurationForm);
                }
                else
                {
                    // No invoke required - same thread
                    _myConfigurationForm.FormClosed += CloseConfigurationForm;
                    _myConfigurationForm = new FormManageConfiguration(this);

                    Application.Run(_myConfigurationForm);
                }
            }
        }

        // Form_Manage_Metadata form
        private FormManageMetadata _myMetadataForm;
        [STAThread]
        public void ThreadProcMetadata()
        {
            if (_myMetadataForm == null)
            {
                _myMetadataForm = new FormManageMetadata(this);
                Application.Run(_myMetadataForm);
            }
            else
            {
                if (_myMetadataForm.InvokeRequired)
                {
                    // Thread Error
                    _myMetadataForm.Invoke((MethodInvoker)delegate { _myMetadataForm.Close(); });
                    _myMetadataForm.FormClosed += CloseMetadataForm;

                    _myMetadataForm = new FormManageMetadata(this);
                    Application.Run(_myMetadataForm);
                }
                else
                {
                    try
                    {
                        // No invoke required - same thread
                        _myMetadataForm.FormClosed += CloseMetadataForm;
                        _myMetadataForm = new FormManageMetadata(this);

                        Application.Run(_myMetadataForm);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Cannot close the form that is open. The reported reason is: " + ex);
                    }
                }
            }
        }

        private FormManageRepository _myRepositoryForm;
        [STAThread]
        public void ThreadProcRepository()
        {
            if (_myRepositoryForm == null)
            {
                _myRepositoryForm = new FormManageRepository();
                _myRepositoryForm.Show();

                Application.Run();
            }
            else
            {
                if (_myRepositoryForm.InvokeRequired)
                {
                    // Thread Error
                    _myRepositoryForm.Invoke((MethodInvoker)delegate { _myRepositoryForm.Close(); });
                    _myRepositoryForm.FormClosed += CloseMetadataForm;

                    _myRepositoryForm = new FormManageRepository();
                    _myRepositoryForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myRepositoryForm.FormClosed += CloseRepositoryForm;

                    _myRepositoryForm = new FormManageRepository();
                    _myRepositoryForm.Show();
                    Application.Run();
                }
            }
        }

        private FormAbout _myAboutForm;
        public void ThreadProcAbout()
        {
            if (_myAboutForm == null)
            {
                _myAboutForm = new FormAbout(this);
                _myAboutForm.Show();

                Application.Run();
            }

            else
            {
                if (_myAboutForm.InvokeRequired)
                {
                    // Thread Error
                    _myAboutForm.Invoke((MethodInvoker)delegate { _myAboutForm.Close(); });
                    _myAboutForm.FormClosed += CloseAboutForm;

                    _myAboutForm = new FormAbout(this);
                    _myAboutForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myAboutForm.FormClosed += CloseAboutForm;

                    _myAboutForm = new FormAbout(this);
                    _myAboutForm.Show();
                    Application.Run();
                }
            }
        }
        #endregion


        private void richTextBoxInformation_TextChanged(object sender, EventArgs e)
        {
            CheckKeyword("Issues occurred", Color.Red, 0);
            CheckKeyword("The statement was executed successfully.", Color.GreenYellow, 0);
        }

        private void generalSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcConfiguration);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void richTextBoxInformation_Enter(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            TestConnections();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void createRebuildRepositoryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcRepository);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void patternDefinitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcPattern);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}
