using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.IO;

namespace TEAM
{
    public partial class FormMain : FormBase
    {
        internal bool RevalidateFlag = true;

        Form_Alert _alertEventLog;

        public FormMain()
        {
            InitializeComponent();

            // Set the version of the build for everything
            const string versionNumberForTeamApplication = "v1.6.2";
            Text = "TEAM - Taxonomy for ETL Automation Metadata " + versionNumberForTeamApplication;

            GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The TEAM root path is {GlobalParameters.RootPath}."));
            GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The TEAM script path is {GlobalParameters.ScriptPath}."));

            richTextBoxInformation.AppendText("Initialising the application.\r\n\r\n");

            // Root paths (mandatory TEAM directories)
            // Make sure the application and custom location directories exist as per the start-up default.
            try
            {
                LocalTeamEnvironmentConfiguration.InitialiseEnvironmentPaths();
            }
            catch (Exception ex)
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error was encountered while creating the standard TEAM paths: \r\n\r\n{ex}"));
            }

            #region Load the root path configuration settings (user defined paths and working environment)

            // Load the root file, to be able to locate the (customisable) configuration file.
            // This file contains the configuration directory, the output directory and the working environment.
            string rootPathFileName = GlobalParameters.CorePath + GlobalParameters.PathFileName + GlobalParameters.FileExtension;
            try
            {
                LocalTeamEnvironmentConfiguration.LoadRootPathFile(rootPathFileName, GlobalParameters.ConfigurationPath, GlobalParameters.OutputPath);
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"The core configuration file {rootPathFileName} has been loaded."));
            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"The core configuration file {rootPathFileName} could not be loaded. Is there a Configuration directory in the TEAM installation location?"));
            }

            // Environments file
            string environmentFile = GlobalParameters.CorePath + GlobalParameters.JsonEnvironmentFileName + GlobalParameters.JsonExtension;
            try
            {
                TeamEnvironmentCollection.LoadTeamEnvironmentCollection(environmentFile);
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"The environment file {environmentFile} has been loaded."));
            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"The environment file {environmentFile} could not be loaded. Does the file exists in the designated (root) location?"));
            }

            #endregion

            #region Check if user configured paths exists (now that they have been loaded from the root file), and create dummy Configuration and Validation files if necessary
            // Configuration Path
            try
            {
                FileHandling.InitialisePath(GlobalParameters.ConfigurationPath);
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The user defined configuration path {GlobalParameters.ConfigurationPath} is available."));
            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
            }

            // Output Path
            try
            {
                FileHandling.InitialisePath(GlobalParameters.OutputPath);
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The user defined output path {GlobalParameters.OutputPath} is available."));
            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
            }

            // Create a dummy configuration file if it does not exist.
            var configurationFileName =
                GlobalParameters.ConfigurationPath +
                GlobalParameters.ConfigFileName + '_' +
                GlobalParameters.WorkingEnvironment +
                GlobalParameters.FileExtension;

            try
            {
                if (!File.Exists(configurationFileName))
                {
                    TeamConfigurationSettings.CreateDummyEnvironmentConfigurationFile(configurationFileName); 
                    GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new configuration file {configurationFileName} was created."));
                }
                else
                {
                    GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The existing configuration file {configurationFileName} was detected."));
                }
            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered creating or detecting the configuration paths for {configurationFileName}."));
            }

            // Create a default validation file if the file does not exist as expected.
            var validationFileName =
                GlobalParameters.ConfigurationPath +
                GlobalParameters.ValidationFileName + '_' +
                GlobalParameters.WorkingEnvironment +
                GlobalParameters.FileExtension;

            try
            {
                LocalTeamEnvironmentConfiguration.CreateDummyValidationFile(validationFileName);

            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered creating or detecting the configuration paths for {validationFileName}."));
            }

            // Create a default json configuration file if the file does not exist as expected.
            var jsonConfigurationFileName =
                GlobalParameters.ConfigurationPath +
                GlobalParameters.JsonExportConfigurationFileName + '_' +
                GlobalParameters.WorkingEnvironment +
                GlobalParameters.FileExtension;

            try
            {
                LocalTeamEnvironmentConfiguration.CreateDummyJsonExtractConfigurationFile(jsonConfigurationFileName);

            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered creating or detecting the configuration paths for {jsonConfigurationFileName}."));
            }
            #endregion

            // Load the connections file for the respective environment.
            var connectionFileName =
                GlobalParameters.ConfigurationPath +
                GlobalParameters.JsonConnectionFileName + '_' +
                GlobalParameters.WorkingEnvironment +
                GlobalParameters.JsonExtension;

            TeamConfigurationSettings.ConnectionDictionary = TeamConnectionFile.LoadConnectionFile(connectionFileName);

            #region Load configuration file
            // Load the available configuration file into memory.
            var configurationFile = GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension;
            try
            {
                // Load the configuration file.
                TeamConfigurationSettings.LoadTeamConfigurationFile(configurationFile);

                // Retrieve the events into the main event log.
                GlobalParameters.TeamEventLog.MergeEventLog(TeamConfigurationSettings.ConfigurationSettingsEventLog);
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The user configuration settings ({configurationFile}) have been loaded."));
            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered loading the user configuration file ({configurationFile})."));
            }
            #endregion

            //FormBase.EnvironmentVersion.GetEnvironmentVersionsFromFile(Path.GetDirectoryName(configurationFile));

            EnvironmentVersion.LoadVersionList(GlobalParameters.CorePath+GlobalParameters.VersionFileName+GlobalParameters.JsonExtension);

            // Load the pattern definition file.
            try
            {
                GlobalParameters.PatternDefinitionList = LoadPatternDefinition.DeserializeLoadPatternDefinition(GlobalParameters.LoadPatternPath + GlobalParameters.LoadPatternDefinitionFile);
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, "The pattern definition file was loaded successfully."));
            }
            catch
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, "An issue was encountered loading the pattern definition file."));
            }


            // Report the events (including errors) back to the user
            int errorCounter = 0;
            foreach (Event individualEvent in GlobalParameters.TeamEventLog)
            {
                if (individualEvent.eventCode == (int) EventTypes.Error)
                {
                    errorCounter++;
                }
            }

            richTextBoxInformation.AppendText($"{errorCounter} error(s) have been found at startup.\r\n\r\n");

            TestConnections();

            //Startup information
            richTextBoxInformation.AppendText("\r\nApplication initialised - the Taxonomy of ETL Automation Metadata (TEAM). \r\n");
            richTextBoxInformation.AppendText("Welcome to version " + versionNumberForTeamApplication + ".\r\n\r\n");

            labelWorkingEnvironment.Text = GlobalParameters.WorkingEnvironment; //+"("+GlobalParameters.WorkingEnvironmentInternalId+")";
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

            // There is no metadata object available (set)
            if (TeamConfigurationSettings.MetadataConnection is null)
            {
                DisableMenu();
                return;
            }

            var connOmd = new SqlConnection { ConnectionString = TeamConfigurationSettings.MetadataConnection.CreateSqlServerConnectionString(false) };

            if (connOmd.ConnectionString != "Server=<>;Initial Catalog=<Metadata>;user id=sa; password=<>")
                try
                {
                    connOmd.Open();
                }
                catch
                {
                    richTextBoxInformation.AppendText(
                        "There was an issue establishing a database connection to the Metadata Repository Database. Can you verify the connection information in the 'configuration' menu option? \r\n");
                    DisableMenu();
                    return;
                }
            else
            {
                richTextBoxInformation.AppendText(
                    "Metadata Repository Connection wasn't defined yet. Please set the connection information in the 'configuration' menu option? \r\n");
                DisableMenu();
                return;
            }

            EnableMenu();
            richTextBoxInformation.AppendText("Database connections have been successfully validated.\r\n");

            try
            {

                DisplayMaxVersion();
                DisplayCurrentVersionFromRepository(connOmd);
                DisplayRepositoryVersion(connOmd);
                openMetadataFormToolStripMenuItem.Enabled = true;

                labelMetadataSave.Text = TeamConfigurationSettings.MetadataRepositoryType.ToString();
            }
            catch
            {
                richTextBoxInformation.AppendText(
                    "There was an issue while reading Metadata Database. The Database is missing tables.\r\n");
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



        internal void DisplayMaxVersion()
        {
            var selectedVersion = EnvironmentVersion.GetMaxVersionForEnvironment(GlobalParameters.WorkingEnvironment);

            //var versionMajorMinor = GetVersion(selectedVersion, connOmd);
            var majorVersion = selectedVersion.Item2;
            var minorVersion = selectedVersion.Item3;

            labelVersion.Text = majorVersion + "." + minorVersion;
        }

        /// <summary>
        /// Retrieves the currently activated version from the repository. This is set after the 'activation' process in 'Manage Metadata' has been completed successfully.
        /// </summary>
        /// <param name="connOmd"></param>
        internal void DisplayCurrentVersionFromRepository(SqlConnection connOmd)
        {
            var sqlStatementForCurrentVersion = new StringBuilder();
            sqlStatementForCurrentVersion.AppendLine("SELECT [VERSION_NAME], [ACTIVATION_DATETIME] FROM [MD_MODEL_METADATA]");

            try
            {
                var versionList = Utility.GetDataTable(ref connOmd, sqlStatementForCurrentVersion.ToString());

                if (versionList != null && versionList.Rows.Count > 0)
                {
                    foreach (DataRow versionNameRow in versionList.Rows)
                    {
                        var versionName = (string) versionNameRow["VERSION_NAME"];
                        labelActiveVersion.Text = versionName;

                        var versionDate = (DateTime)versionNameRow["ACTIVATION_DATETIME"];
                        labelActiveVersionDateTime.Text = versionDate.ToString(CultureInfo.InvariantCulture);
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

            var versionList = Utility.GetDataTable(ref connOmd, sqlStatementForCurrentVersion.ToString());

            try
            {
                if (versionList != null && versionList.Rows.Count > 0)
                {
                    foreach (DataRow versionNameRow in versionList.Rows)
                    {
                        labelRepositoryVersion.Text = (string)versionNameRow["REPOSITORY_VERSION"];
                        var versionDate = (DateTime) versionNameRow["REPOSITORY_UPDATE_DATETIME"];
                        labelRepositoryCreationDate.Text = versionDate.ToString(CultureInfo.InvariantCulture);
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

        /// <summary>
        /// Local function to update the main form details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateEnvironment(object sender, MyWorkingEnvironmentEventArgs e)
        {
            var localEnvironment = e.Value;
            var localTextForLabel = localEnvironment.environmentKey;

            if (labelWorkingEnvironment.InvokeRequired)
            {
                labelWorkingEnvironment.BeginInvoke((MethodInvoker)delegate { labelWorkingEnvironment.Text = localTextForLabel; });
            }
            else
            {
               labelWorkingEnvironment.Text = localTextForLabel;
            }

            LocalTeamEnvironmentConfiguration.InitialiseEnvironmentPaths();
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
                _myConfigurationForm.OnUpdateEnvironment += UpdateEnvironment;
                Application.Run(_myConfigurationForm);
            }
            else
            {
                if (_myConfigurationForm.InvokeRequired)
                {
                    // Thread Error
                    _myConfigurationForm.Invoke((MethodInvoker)delegate { _myConfigurationForm.Close(); });
                    _myConfigurationForm.FormClosed += CloseConfigurationForm;
                    _myConfigurationForm.OnUpdateEnvironment += UpdateEnvironment;

                    _myConfigurationForm = new FormManageConfiguration(this);
                    Application.Run(_myConfigurationForm);
                }
                else
                {
                    // No invoke required - same thread
                    _myConfigurationForm.FormClosed += CloseConfigurationForm; 
                    _myConfigurationForm.OnUpdateEnvironment += UpdateEnvironment;
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

        private void backgroundWorkerEventLog_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            var localEventLog = GlobalParameters.TeamEventLog;

            // Handle multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                backgroundWorkerEventLog.ReportProgress(0);
                _alertEventLog.SetTextLogging("Event Log.\r\n\r\n");

                try
                {
                    foreach (var individualEvent in localEventLog)
                    {
                        _alertEventLog.SetTextLogging($"{individualEvent.eventTime} - {(EventTypes) individualEvent.eventCode}: {individualEvent.eventDescription}\r\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An issue occurred displaying the event log. The error message is: " + ex, "An issue has occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                backgroundWorkerEventLog.ReportProgress(100);
            }
        }

        private void backgroundWorkerEventLog_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _alertEventLog.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertEventLog.ProgressValue = e.ProgressPercentage;
        }

        private void backgroundWorkerEventLog_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // Do nothing
            }
            else if (e.Error != null)
            {
                // Do nothing
            }
            else
            {
                // Do nothing
            }
        }

        private void displayEventLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerEventLog.IsBusy != true)
            {
                // create a new instance of the alert form
                _alertEventLog = new Form_Alert();
                _alertEventLog.Text = "Event Log";
                _alertEventLog.ShowLogButton(false);
                _alertEventLog.ShowCancelButton(false);
                _alertEventLog.ShowProgressBar(false);
                _alertEventLog.ShowProgressLabel(false);
                _alertEventLog.Show();
                
                // Start the asynchronous operation.
                backgroundWorkerEventLog.RunWorkerAsync();
            }
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start(ExtensionMethod.GetDefaultBrowserPath(),
                "http://roelantvos.com/blog/team/");
        }
    }
}
