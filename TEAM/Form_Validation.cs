using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TEAM
{
    public partial class FormManageValidation : FormBase
    {
        private readonly FormManageMetadata _myParent;
        public FormManageValidation(FormManageMetadata parent)
        {            
            _myParent = parent;
            InitializeComponent();
        }

        private void LocalInitialiseConnections(string chosenFile)
        {
            var configList = new Dictionary<string, string>();
            var fs = new FileStream(chosenFile, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);

            try
            {
                string textline;
                while ((textline = sr.ReadLine()) != null)
                {
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1)
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();

                var connectionStringOmd = configList["connectionStringMetadata"];
                connectionStringOmd = connectionStringOmd.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringSource = configList["connectionStringSource"];
                connectionStringSource = connectionStringSource.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringStg = configList["connectionStringStaging"];
                connectionStringStg = connectionStringStg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringHstg = configList["connectionStringPersistentStaging"];
                connectionStringHstg = connectionStringHstg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringInt = configList["connectionStringIntegration"];
                connectionStringInt = connectionStringInt.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringPres = configList["connectionStringPresentation"];
                connectionStringPres = connectionStringPres.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                //textBoxOutputPath.Text = GlobalParameters.OutputPath;
                //textBoxIntegrationConnection.Text = connectionStringInt;
                //textBoxPSAConnection.Text = connectionStringHstg;
                //textBoxSourceConnection.Text = connectionStringSource;
                //textBoxStagingConnection.Text = connectionStringStg;
                //textBoxMetadataConnection.Text = connectionStringOmd;
                //textBoxPresentationConnection.Text = connectionStringPres;

                //textBoxHubTablePrefix.Text = configList["HubTablePrefix"];
                //textBoxSatPrefix.Text = configList["SatTablePrefix"];
                //textBoxLinkTablePrefix.Text = configList["LinkTablePrefix"];
                //textBoxLinkSatPrefix.Text = configList["LinkSatTablePrefix"];
                //textBoxDWHKeyIdentifier.Text = configList["KeyIdentifier"];
                //textBoxSchemaName.Text = configList["SchemaName"];
                //textBoxEventDateTime.Text = configList["EventDateTimeStamp"];
                //textBoxLDST.Text = configList["LoadDateTimeStamp"];
                //textBoxExpiryDateTimeName.Text = configList["ExpiryDateTimeStamp"];
                //textBoxChangeDataCaptureIndicator.Text = configList["ChangeDataIndicator"];
                //textBoxRecordSource.Text = configList["RecordSourceAttribute"];
                //textBoxETLProcessID.Text = configList["ETLProcessID"];
                //textBoxETLUpdateProcessID.Text = configList["ETLUpdateProcessID"];
                //textBoxSourcePrefix.Text = configList["SourceSystemPrefix"];
                //textBoxStagingAreaPrefix.Text = configList["StagingAreaPrefix"];
                //textBoxPSAPrefix.Text = configList["PersistentStagingAreaPrefix"];
                //textBoxSourceRowId.Text = configList["RowID"];
                //textBoxSourceDatabase.Text = configList["SourceDatabase"];
                //textBoxStagingDatabase.Text = configList["StagingDatabase"];
                //textBoxPSADatabase.Text = configList["PersistentStagingDatabase"];
                //textBoxIntegrationDatabase.Text = configList["IntegrationDatabase"];
                //textBoxPresentationDatabase.Text = configList["PresentationDatabase"];
                //textBoxRecordChecksum.Text = configList["RecordChecksum"];
                //textBoxCurrentRecordAttributeName.Text = configList["CurrentRecordAttribute"];
                //textBoxAlternativeRecordSource.Text = configList["AlternativeRecordSource"];
                //textBoxHubAlternativeLDTSAttribute.Text = configList["AlternativeHubLDTS"];
                //textBoxSatelliteAlternativeLDTSAttribute.Text = configList["AlternativeSatelliteLDTS"];
                //textBoxLogicalDeleteAttributeName.Text = configList["LogicalDeleteAttribute"];
                //textBoxLinkedServer.Text = configList["LinkedServerName"];

                ////Checkbox setting based on loaded configuration
                //CheckBox myConfigurationCheckBox;

                //if (configList["AlternativeRecordSourceFunction"] == "False")
                //{
                //    myConfigurationCheckBox = checkBoxAlternativeRecordSource;
                //    myConfigurationCheckBox.Checked = false;
                //    textBoxAlternativeRecordSource.Enabled = false;
                //}
                //else
                //{
                //    myConfigurationCheckBox = checkBoxAlternativeRecordSource;
                //    myConfigurationCheckBox.Checked = true;
                //}

                //if (configList["AlternativeHubLDTSFunction"] == "False")
                //{
                //    myConfigurationCheckBox = checkBoxAlternativeHubLDTS;
                //    myConfigurationCheckBox.Checked = false;
                //    textBoxHubAlternativeLDTSAttribute.Enabled = false;
                //}
                //else
                //{
                //    myConfigurationCheckBox = checkBoxAlternativeHubLDTS;
                //    myConfigurationCheckBox.Checked = true;
                //}

                //if (configList["AlternativeSatelliteLDTSFunction"] == "False")
                //{
                //    myConfigurationCheckBox = checkBoxAlternativeSatLDTS;
                //    myConfigurationCheckBox.Checked = false;
                //    textBoxSatelliteAlternativeLDTSAttribute.Enabled = false;
                //}
                //else
                //{
                //    myConfigurationCheckBox = checkBoxAlternativeSatLDTS;
                //    myConfigurationCheckBox.Checked = true;
                //}


                ////Radiobutton setting for prefix / suffix 
                //RadioButton myTableRadioButton;

                //if (configList["TableNamingLocation"] == "Prefix")
                //{
                //    myTableRadioButton = tablePrefixRadiobutton;
                //    myTableRadioButton.Checked = true;
                //}
                //else
                //{
                //    myTableRadioButton = tableSuffixRadiobutton;
                //    myTableRadioButton.Checked = true;
                //}

                ////Radiobutton settings for on key location
                //RadioButton myKeyRadioButton;

                //if (configList["KeyNamingLocation"] == "Prefix")
                //{
                //    myKeyRadioButton = keyPrefixRadiobutton;
                //    myKeyRadioButton.Checked = true;
                //}
                //else
                //{
                //    myKeyRadioButton = keySuffixRadiobutton;
                //    myKeyRadioButton.Checked = true;
                //}

                ////Radiobutton settings for PSA Natural Key determination
                //RadioButton myPsaBusinessKeyLocation;

                //if (configList["PSAKeyLocation"] == "PrimaryKey")
                //{
                //    myPsaBusinessKeyLocation = radioButtonPSABusinessKeyPK;
                //    myPsaBusinessKeyLocation.Checked = true;
                //}
                //else
                //{
                //    myPsaBusinessKeyLocation = radioButtonPSABusinessKeyIndex;
                //    myPsaBusinessKeyLocation.Checked = true;
                //}

                ////Radiobutton settings for repository type
                //RadioButton myMetadatarepositoryType;

                //if (configList["metadataRepositoryType"] == "JSON")
                //{
                //    myMetadatarepositoryType = radioButtonJSON;
                //    myMetadatarepositoryType.Checked = true;
                //}
                //else
                //{
                //    myMetadatarepositoryType = radioButtonSQLServer;
                //    myMetadatarepositoryType.Checked = true;
                //}

                richTextBoxInformation.AppendText("The default values were loaded. \r\n\r\n");
                richTextBoxInformation.AppendText(@"The file " + chosenFile + " was uploaded successfully. \r\n\r\n");
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("\r\n\r\nAn error occured while interpreting the configuration file. The original error is: '" + ex.Message + "'");
            }
        }

        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var configurationPath = Application.StartupPath + @"\Configuration\";
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Configuration File",
                Filter = @"Text files|*.txt",
                InitialDirectory = @"" + configurationPath + ""
            };

            if (theDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                var myStream = theDialog.OpenFile();

                using (myStream)
                {
                    richTextBoxInformation.Clear();
                    var chosenFile = theDialog.FileName;
                    LocalInitialiseConnections(chosenFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message, "An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
