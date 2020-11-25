using System;
using System.Diagnostics;
using System.IO;
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
                var validationFile = GlobalParameters.ConfigurationPath + GlobalParameters.ValidationFileName + '_' +
                                     GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension;

                // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class
                if (!File.Exists(validationFile))
                {
                    LocalTeamEnvironmentConfiguration.CreateDummyValidationFile(validationFile);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path)
                LocalTeamEnvironmentConfiguration.LoadValidationFile(validationFile);

                richTextBoxInformation.Text += "The validation file " + validationFile + " has been loaded.";

                // Apply the values to the form
                LocalInitialiseValidationSettings();
            }
            catch (Exception)
            {
                // Do nothing
            }

        }

        /// <summary>
        /// This method will update the validation values on the form
        /// </summary>
        private void LocalInitialiseValidationSettings()
        {
            // Source object existence
            switch (ValidationSettings.SourceObjectExistence)
            {
                case "True":
                    checkBoxSourceObjectExistence.Checked = true;
                    break;
                case "False":
                    checkBoxSourceObjectExistence.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the source object validation values, only true and false are allowed but this was encountered: " + ValidationSettings.SourceObjectExistence + ". Please check the validation file (TEAM_<environment>_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Target object existence
            switch (ValidationSettings.TargetObjectExistence)
            {
                case "True":
                    checkBoxTargetObjectExistence.Checked = true;
                    break;
                case "False":
                    checkBoxTargetObjectExistence.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the target object validation values, only true and false are allowed but this was encountered: " +ValidationSettings.TargetObjectExistence + ". Please check the validation file (TEAM_<environment>_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Source attribute existence
            switch (ValidationSettings.SourceAttributeExistence)
            {
                case "True":
                    checkBoxSourceAttribute.Checked = true;
                    break;
                case "False":
                    checkBoxSourceAttribute.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the source attribute validation values, only true and false are allowed but this was encountered: " + ValidationSettings.SourceAttributeExistence + ". Please check the validation file (TEAM_<environment>_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Target attribute existence
            switch (ValidationSettings.TargetAttributeExistence)
            {
                case "True":
                    checkBoxTargetAttribute.Checked = true;
                    break;
                case "False":
                    checkBoxTargetAttribute.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the target attribute validation values, only true and false are allowed but this was encountered: " + ValidationSettings.TargetAttributeExistence + ". Please check the validation file (TEAM_<environment>_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }


            if (ValidationSettings.SourceBusinessKeyExistence == "True")
            {
                checkBoxSourceBusinessKeyExistence.Checked = true;
            }
            else if (ValidationSettings.SourceBusinessKeyExistence == "False")
            {
                checkBoxSourceBusinessKeyExistence.Checked = false;
            }
            else
            {
                // Raise exception
                MessageBox.Show(
                    "There is something wrong with the business key validation values, only true and false are allowed but this was encountered: " +
                    ValidationSettings.SourceBusinessKeyExistence +
                    ". Please check the validation file (TEAM_<environment>_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("There is something wrong with the logical group validation values, only true and false are allowed but this was encountered: " + ValidationSettings.LogicalGroup + ". Please check the validation file (TEAM_<environment>_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            // Link Key order
            if (ValidationSettings.LinkKeyOrder == "True")
            {
                checkBoxLinkKeyOrder.Checked = true;
            }
            else if (ValidationSettings.LinkKeyOrder == "False")
            {
                checkBoxLinkKeyOrder.Checked = false;
            }
            else
            {
                // Raise exception
                MessageBox.Show("There is something wrong with the Link key order validation values, only true and false are allowed but this was encountered: " + ValidationSettings.LinkKeyOrder + ". Please check the validation file (TEAM_<environment>_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Business Key Syntax
            if (ValidationSettings.BusinessKeySyntax == "True")
            {
                checkBoxBusinessKeySyntaxValidation.Checked = true;
            }
            else if (ValidationSettings.BusinessKeySyntax == "False")
            {
                checkBoxBusinessKeySyntaxValidation.Checked = false;
            }
            else
            {
                // Raise exception
                MessageBox.Show("There is something wrong with the business key syntax validation values, only true and false are allowed but this was encountered: " + ValidationSettings.BusinessKeySyntax + ". Please check the validation file (TEAM_<environment>_validation.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }

        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Validation File",
                Filter = @"Text files|*.txt",
                InitialDirectory = @"" + GlobalParameters.ConfigurationPath + ""
            };

            if (theDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                var myStream = theDialog.OpenFile();

                using (myStream)
                {
                    richTextBoxInformation.Clear();
                    var chosenFile = theDialog.FileName;
                    
                    // Load from disk into memory
                    LocalTeamEnvironmentConfiguration.LoadValidationFile(chosenFile);

                    // Update values on form
                    LocalInitialiseValidationSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message, "An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Save validation settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                // Source object existence check
                var stringSourceObjectExistence = "";
                if (checkBoxSourceObjectExistence.Checked)
                {
                    stringSourceObjectExistence = "True";
                }
                else
                {
                    stringSourceObjectExistence = "False";
                }
                ValidationSettings.SourceObjectExistence = stringSourceObjectExistence;


                // Target object existence check
                var stringtargetObjectExistence = "";
                if (checkBoxTargetObjectExistence.Checked)
                {
                    stringtargetObjectExistence = "True";
                }
                else
                {
                    stringtargetObjectExistence = "False";
                }
                ValidationSettings.TargetObjectExistence = stringtargetObjectExistence;


                // Source business key existence check
                var stringBusinessKeyExistence = "";
                if (checkBoxSourceBusinessKeyExistence.Checked)
                {
                    stringBusinessKeyExistence = "True";
                }
                else
                {
                    stringBusinessKeyExistence = "False";
                }
                ValidationSettings.SourceBusinessKeyExistence = stringBusinessKeyExistence;


                // Source attribute existence check
                var stringSourceAttributeExistence = "";
                if (checkBoxSourceAttribute.Checked)
                {
                    stringSourceAttributeExistence = "True";
                }
                else
                {
                    stringSourceAttributeExistence = "False";
                }
                ValidationSettings.SourceAttributeExistence = stringSourceAttributeExistence;


                // Target attribute existence check
                var stringTargetAttributeExistence = "";
                if (checkBoxTargetAttribute.Checked)
                {
                    stringTargetAttributeExistence = "True";
                }
                else
                {
                    stringTargetAttributeExistence = "False";
                }
                ValidationSettings.TargetAttributeExistence = stringTargetAttributeExistence;


                // Logical Group Validation
                var stringLogicalGroup = "";
                if (checkBoxLogicalGroup.Checked)
                {
                    stringLogicalGroup = "True";
                }
                else
                {
                    stringLogicalGroup = "False";
                }
                ValidationSettings.LogicalGroup = stringLogicalGroup;


                // Link Key Order Validation
                var stringLinkKeyOrder = "";
                if (checkBoxLinkKeyOrder.Checked)
                {
                    stringLinkKeyOrder = "True";
                }
                else
                {
                    stringLinkKeyOrder = "False";
                }
                ValidationSettings.LinkKeyOrder = stringLinkKeyOrder;


                // Business key syntax check
                var businessKeySyntax = "";
                if (checkBoxBusinessKeySyntaxValidation.Checked)
                {
                    businessKeySyntax = "True";
                }
                else
                {
                    businessKeySyntax = "False";
                }
                ValidationSettings.BusinessKeySyntax = businessKeySyntax;


                // Write to disk
                LocalTeamEnvironmentConfiguration.SaveValidationFile();

                richTextBoxInformation.Text = "The values have been successfully saved.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not write values to memory and disk. Original error: " + ex.Message, "An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openConfigurationDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(GlobalParameters.ConfigurationPath);
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
                Process.Start(GlobalParameters.OutputPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the configuration directory. The error message is: " + ex;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
