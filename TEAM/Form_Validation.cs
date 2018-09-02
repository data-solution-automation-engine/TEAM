using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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



            // Make sure the validation information is available in this form
            try
            {
                var validationFile = ConfigurationSettings.ConfigurationPath + GlobalParameters.ValidationFileName + '_' +
                                     ConfigurationSettings.WorkingEnvironment + GlobalParameters.FileExtension;

                // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class
                if (!File.Exists(validationFile))
                {
                    var newEnvironmentConfiguration = new EnvironmentConfiguration();
                    newEnvironmentConfiguration.CreateDummyValidationConfiguration(validationFile);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path)
                EnvironmentConfiguration.LoadValidationFile(validationFile);

                richTextBoxInformation.Text += "The validation file " + validationFile + " has been loaded.";

                // Apply the values to the form
                LocalInitialiseValidationSettings();
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        ///    This method will update the vaidation values on the form
        /// </summary>
        /// <param name="chosenFile"></param>
        private void LocalInitialiseValidationSettings()
        {
            ////Checkbox setting based on loaded configuration
            if (ValidationSettings.SourceObjectExistence == "True")
            {
                checkBoxSourceObjectExistence.Checked = true;
            }
            else if (ValidationSettings.SourceObjectExistence == "False")
            {
                checkBoxSourceObjectExistence.Checked = false;
            }
            else
            {
                // Raise exception
                MessageBox.Show(
                    "There is something wrong with the validation values, only true and false are allowed but this was encountered: " +
                    ValidationSettings.SourceObjectExistence +
                    ". Please check the validation file (TEAM_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            if (ValidationSettings.TargetObjectExistence == "True")
            {
                checkBoxTargetObjectExistence.Checked = true;
            }
            else if (ValidationSettings.TargetObjectExistence == "False")
            {
                checkBoxTargetObjectExistence.Checked = false;
            }
            else
            {
                // Raise exception
                MessageBox.Show(
                    "There is something wrong with the validation values, only true and false are allowed but this was encountered: " +
                    ValidationSettings.TargetObjectExistence +
                    ". Please check the validation file (TEAM_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            if (ValidationSettings.BusinessKeyExistence == "True")
            {
                checkBoxBusinessKeyExistence.Checked = true;
            }
            else if (ValidationSettings.BusinessKeyExistence == "False")
            {
                checkBoxBusinessKeyExistence.Checked = false;
            }
            else
            {
                // Raise exception
                MessageBox.Show(
                    "There is something wrong with the validation values, only true and false are allowed but this was encountered: " +
                    ValidationSettings.BusinessKeyExistence +
                    ". Please check the validation file (TEAM_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            if (ValidationSettings.LogicalGroup == "True")
            {
                checkBoxLogicalGroup.Checked = true;
            }
            else if (ValidationSettings.LogicalGroup == "False")
            {
                checkBoxLogicalGroup.Checked = false;
            }
            else
            {
                // Raise exception
                MessageBox.Show(
                    "There is something wrong with the validation values, only true and false are allowed but this was encountered: " +
                    ValidationSettings.LogicalGroup +
                    ". Please check the validation file (TEAM_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }

        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var configurationPath = ConfigurationSettings.ConfigurationPath;
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Validation File",
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
                   // PUT LOGIC HERE
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message, "An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void openConfigurationDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(ConfigurationSettings.ConfigurationPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the configuration directory. The error message is: " + ex;
            }
        }

        private void openOutputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(ConfigurationSettings.OutputPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the configuration directory. The error message is: " + ex;
            }
        }
    }
}
