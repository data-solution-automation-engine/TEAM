using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.IO;
using TEAM_Library;
using System.Collections.Generic;
using System.Linq;

namespace TEAM
{
    public partial class FormMain : FormBase
    {
        internal bool RevalidateFlag = true;

        Form_Alert _alertEventLog;

        List<Thread> threads = new List<Thread>();

        public FormMain()
        {
            AutoScaleMode = AutoScaleMode.Dpi;

            this.Font = SystemFonts.IconTitleFont;

            this.Location = Screen.PrimaryScreen.WorkingArea.Location;
            this.StartPosition = FormStartPosition.Manual;

            InitializeComponent();

            // Set the version of the build for everything
            const string versionNumberForTeamApplication = "v1.6.14";
            Text = $@"Taxonomy for ETL Automation Metadata {versionNumberForTeamApplication}";

            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The TEAM root path is {globalParameters.RootPath}."));
            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The TEAM script path is {globalParameters.ScriptPath}."));

            // Root paths (mandatory TEAM directories)
            // Make sure the application and custom location directories exist as per the start-up default.
            try
            {
                LocalTeamEnvironmentConfiguration.InitialiseEnvironmentPaths();
            }
            catch (Exception exception)
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error was encountered while creating the standard TEAM paths: \r\n\r\n{exception.Message}"));
            }

            #region Load the root path configuration settings (user defined paths and working environment)

            // Load the root file, to be able to locate the other configuration files.
            // This file contains the configuration directory, the output directory and the working environment.
            string rootPathFileName = globalParameters.CorePath + globalParameters.PathFileName + globalParameters.FileExtension;

            try
            {
                LocalTeamEnvironmentConfiguration.LoadRootPathFile(rootPathFileName, globalParameters.CorePath);
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The core configuration file {rootPathFileName} has been loaded."));
            }
            catch
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The core configuration file {rootPathFileName} could not be loaded. Is there a Configuration directory in the TEAM installation location?"));
            }

            #endregion

            #region Environment file

            // Environments file
            string environmentFile = globalParameters.CorePath + globalParameters.JsonEnvironmentFileName + globalParameters.JsonExtension;

            try
            {
                TeamEnvironmentCollection.LoadTeamEnvironmentCollection(environmentFile);
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment file {environmentFile} has been loaded."));
            }
            catch
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The environment file {environmentFile} could not be loaded. Does the file exists in the designated (root) location?"));
            }

            // Set the paths of the active environment
            var activeEnvironment = TeamEnvironmentCollection.GetEnvironmentById(globalParameters.ActiveEnvironmentInternalId);
            globalParameters.ActiveEnvironmentKey = activeEnvironment.environmentKey;
            globalParameters.ConfigurationPath = activeEnvironment.configurationPath;
            globalParameters.MetadataPath = activeEnvironment.metadataPath;

            labelWorkingEnvironment.Text = globalParameters.ActiveEnvironmentKey;

            #endregion

            #region Check if user configured paths exists (now that they have been loaded from the root file)

            // Configuration Path
            FileHandling.InitialisePath(globalParameters.ConfigurationPath, TeamPathTypes.ConfigurationPath, TeamEventLog);
            // Metadata Path
            FileHandling.InitialisePath(globalParameters.MetadataPath, TeamPathTypes.MetadataPath, TeamEventLog);

            #endregion

            #region Validation file

            // Create a default validation file if the file does not exist as expected.
            var validationFileName = $"{globalParameters.ConfigurationPath}{globalParameters.ValidationFileName}{'_'}{globalParameters.ActiveEnvironmentKey}{globalParameters.FileExtension}";

            try
            {
                ValidationSetting.CreateDummyValidationFile(validationFileName);

            }
            catch
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered creating or detecting the configuration paths for {validationFileName}."));
            }

            #endregion

            #region JSON export configuration file
            // Create a default json configuration file if the file does not exist as expected.
            var jsonConfigurationFileName = $"{globalParameters.ConfigurationPath}{globalParameters.JsonExportConfigurationFileName}{'_'}{globalParameters.ActiveEnvironmentKey}{globalParameters.FileExtension}";

            try
            {
                JsonExportSetting.CreateDummyJsonConfigurationFile(jsonConfigurationFileName, TeamEventLog);

            }
            catch
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered creating or detecting the configuration paths for {jsonConfigurationFileName}."));
            }

            #endregion

            #region Connection file

            // Load the connections file for the respective environment.
            var connectionFileName = $"{globalParameters.ConfigurationPath}{globalParameters.JsonConnectionFileName}{'_'}{globalParameters.ActiveEnvironmentKey}{globalParameters.JsonExtension}";

            TeamConfiguration.ConnectionDictionary = TeamConnectionFile.LoadConnectionFile(connectionFileName, TeamEventLog);

            #endregion

            #region Team configuration file

            // Create a dummy configuration file if it does not exist.
            var configurationFile = $"{globalParameters.ConfigurationPath}{globalParameters.ConfigFileName}{'_'}{globalParameters.ActiveEnvironmentKey}{globalParameters.FileExtension}";

            try
            {
                if (!File.Exists(configurationFile))
                {
                    TeamConfiguration.CreateDummyTeamConfigurationFile(configurationFile);
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new configuration file {configurationFile} was created."));
                }
                else
                {
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The existing configuration file {configurationFile} was detected."));
                }
            }
            catch
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered creating or detecting the configuration paths for {configurationFile}."));
            }

            try
            {
                // Load the configuration file.
                TeamConfiguration.LoadTeamConfigurationFile(configurationFile);
                // Retrieve the events into the main event log.
                TeamEventLog.MergeEventLog(TeamConfiguration.ConfigurationSettingsEventLog);
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The user configuration settings ({configurationFile}) have been loaded."));

            }
            catch
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered loading the user configuration file ({configurationFile})."));
            }

            #endregion

            // Notify the user of any errors that were detected.
            var errors = TeamEventLog.ReportErrors(TeamEventLog);

            if (errors > 0)
            {
                richTextBoxInformation.AppendText($"{errors} error(s) have been found at startup. Please check the Event Log in the menu.\r\n\r\n");
            }

            openMetadataFormToolStripMenuItem.Enabled = true;

            //Startup information
            richTextBoxInformation.AppendText($"\r\nWelcome to the Taxonomy of ETL Automation Metadata (TEAM) - version {versionNumberForTeamApplication}.\r\n\r\n");
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openMetadataFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var thread = threads.FirstOrDefault(x => x.Name == "metadataThread");

            if (thread != null)
            {
                richTextBoxInformation.Text += "\r\nA metadata form is being opened, please wait for the operation to complete.";
            }
            else
            {
                richTextBoxInformation.Text += "\r\nOpening the metadata form - please wait, this may take up to a minute for large sets.";
            }

            var t = new Thread(ThreadProcMetadata);
            t.Name = "metadataThread";
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            threads.Add(t);
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

            var thread = threads.FirstOrDefault(x => x.Name == "metadataThread");
            if (thread != null)
            {
                threads.Remove(thread);
            }
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
        private void UpdateEnvironmentLabel(object sender, MyWorkingEnvironmentEventArgs e)
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

        #endregion

        #region Form Threads

        private FormManageConfiguration _myConfigurationForm;
        [STAThread]
        public void ThreadProcConfiguration()
        {
            if (_myConfigurationForm == null)
            {
                _myConfigurationForm = new FormManageConfiguration(this);
                _myConfigurationForm.OnUpdateEnvironment += UpdateEnvironmentLabel;
                Application.Run(_myConfigurationForm);
            }
            else
            {
                if (_myConfigurationForm.InvokeRequired)
                {
                    // Thread Error
                    _myConfigurationForm.Invoke((MethodInvoker)delegate { _myConfigurationForm.Close(); });
                    _myConfigurationForm.FormClosed += CloseConfigurationForm;
                    _myConfigurationForm.OnUpdateEnvironment += UpdateEnvironmentLabel;

                    _myConfigurationForm = new FormManageConfiguration(this);
                    Application.Run(_myConfigurationForm);
                }
                else
                {
                    // No invoke required - same thread
                    _myConfigurationForm.FormClosed += CloseConfigurationForm;
                    _myConfigurationForm.OnUpdateEnvironment += UpdateEnvironmentLabel;
                    _myConfigurationForm = new FormManageConfiguration(this);

                    Application.Run(_myConfigurationForm);
                }
            }
        }

        // Form_Manage_Metadata form
        private FormManageMetadata _myMetadataForm = new FormManageMetadata();

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
                    _myMetadataForm.Invoke((MethodInvoker)delegate { _myMetadataForm.BringToFront(); });
                    _myMetadataForm.Invoke((MethodInvoker)delegate { _myMetadataForm.Focus(); });
                    _myMetadataForm.Invoke((MethodInvoker)delegate { _myMetadataForm.Activate(); });
                }
                else
                {
                    try
                    {
                        // No invoke required - same thread.
                        _myMetadataForm.FormClosed += CloseMetadataForm;
                        _myMetadataForm = new FormManageMetadata(this);

                        Application.Run(_myMetadataForm);
                    }
                    catch (Exception exception)
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, exception.Message));
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
                _myAboutForm = new FormAbout();
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

                    _myAboutForm = new FormAbout();
                    _myAboutForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myAboutForm.FormClosed += CloseAboutForm;

                    _myAboutForm = new FormAbout();
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

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void backgroundWorkerEventLog_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            var localEventLog = TeamEventLog;

            // Handle multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                backgroundWorkerEventLog.ReportProgress(0);

                try
                {
                    foreach (var individualEvent in localEventLog)
                    {
                        _alertEventLog.SetTextLogging($"{individualEvent.eventTime} - {(EventTypes)individualEvent.eventCode}: {individualEvent.eventDescription}\r\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"An issue occurred displaying the event log. The error message is: {ex.Message}", @"An issue has occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                _alertEventLog.Text = @"Event Log";
                _alertEventLog.ShowLogButton(false);
                _alertEventLog.ShowCancelButton(false);
                _alertEventLog.ShowProgressBar(false);
                _alertEventLog.ShowProgressLabel(false);
                _alertEventLog.Show();

                // Start the asynchronous operation.
                backgroundWorkerEventLog.RunWorkerAsync();
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Utility.GetDefaultBrowserPath(), "https://github.com/data-solution-automation-engine/TEAM");
        }

        private void deployMetadataExamplesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcRepository);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void openMetadataDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                try
                {
                    var psi = new ProcessStartInfo() { FileName = globalParameters.MetadataPath, UseShellExecute = true };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.Text = $@"An error has occurred while attempting to open the metadata directory. The error message is: {ex.Message}.";
                }
            }
        }

        private void openCoreDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                try
                {
                    var psi = new ProcessStartInfo() { FileName = globalParameters.CorePath, UseShellExecute = true };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.Text = $@"An error has occurred while attempting to open the directory. The error message is: {ex.Message}.";
                }
            }
        }
    }
}
