using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TEAM
{
    /// <summary>
    /// Derived Custom Connection TabPage inherited from the TabPage class.
    /// </summary>
    internal class CustomTabPageEnvironment : TabPage
    {
        // Startup flag, disabled in constructor. Used to prevent some events from firing twice (creation and value setting).
        internal bool StartUpIndicator = true;

        // In-memory object representing the connection. Is always updated first and then refreshed to the form.
        private TeamWorkingEnvironment _localEnvironment;

        private string _environmentFileName = FormBase.GlobalParameters.RootPath +
                                              FormBase.GlobalParameters.JsonEnvironmentFileName + 
                                              FormBase.GlobalParameters.JsonExtension;

        // Objects on main Tab Page
        private TextBox _textBoxEnvironmentName;
        public TextBox _textBoxEnvironmentKey;
        private RichTextBox _richTextBoxNotes;

        /// <summary>
        /// Constructor to instantiate a new Custom Tab Page, using a TeamWorkingEnvironment as input.
        /// </summary>
        public CustomTabPageEnvironment(object input)
        {
            _localEnvironment = (TeamWorkingEnvironment) input;

            var connectionKey = _localEnvironment.environmentKey;
            var connectionName = _localEnvironment.environmentName;

            //var inputNiceName = Regex.Replace(connectionName, "(\\B[A-Z])", " $1");

            #region Main Tab Page controls

            ToolTip toolTipEnvironment = new ToolTip();

            // Base properties of the custom tab page
            Name = $"{connectionKey}";
            Text = connectionName;
            BackColor = Color.Transparent;
            BorderStyle = BorderStyle.None;
            UseVisualStyleBackColor = true;
            Size = new Size(1330, 601);
            AutoSizeMode = AutoSizeMode.GrowOnly;
            AutoSize = true;

            // Add Panel to facilitate docking
            var localPanel = new Panel();
            Controls.Add(localPanel);
            localPanel.Dock = DockStyle.Fill;
            localPanel.AutoSize = true;
            localPanel.TabStop = false;

            // Add GroupBox for Environment content
            var groupBoxEnvironment = new GroupBox();
            localPanel.Controls.Add(groupBoxEnvironment);
            groupBoxEnvironment.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxEnvironment.Location = new Point(6, 6);
            groupBoxEnvironment.Size = new Size(502, 175);
            groupBoxEnvironment.Name = $"groupBoxEnvironment";
            groupBoxEnvironment.Text = $"Working environment";
            groupBoxEnvironment.TabStop = false;

            // Add Environment Key Label
            var labelEnvironmentKey = new Label();
            groupBoxEnvironment.Controls.Add(labelEnvironmentKey);
            labelEnvironmentKey.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelEnvironmentKey.Location = new Point(6, 19);
            labelEnvironmentKey.Size = new Size(160, 13);
            labelEnvironmentKey.Name = $"labelEnvironmentKey";
            labelEnvironmentKey.Text = $"Environment key";
            labelEnvironmentKey.TabStop = false;

            // Add Environment Name Label
            var labelEnvironmentName = new Label();
            groupBoxEnvironment.Controls.Add(labelEnvironmentName);
            labelEnvironmentName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelEnvironmentName.Location = new Point(6, 44);
            labelEnvironmentName.Size = new Size(160, 13);
            labelEnvironmentName.Name = $"labelEnvironmentName";
            labelEnvironmentName.Text = $"Environment name";
            labelEnvironmentName.TabStop = false;

            // Add Environment Notes Label
            var labelEnvironmentNotes = new Label();
            groupBoxEnvironment.Controls.Add(labelEnvironmentNotes);
            labelEnvironmentNotes.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelEnvironmentNotes.Location = new Point(6, 70);
            labelEnvironmentNotes.Size = new Size(160, 13);
            labelEnvironmentNotes.Name = $"labelEnvironmentNotes";
            labelEnvironmentNotes.Text = $"Environment notes";
            labelEnvironmentNotes.TabStop = false;

            // Add Environment Key TextBox
            _textBoxEnvironmentKey = new TextBox();
            groupBoxEnvironment.Controls.Add(_textBoxEnvironmentKey);
            _textBoxEnvironmentKey.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxEnvironmentKey.Location = new Point(172, 16);
            _textBoxEnvironmentKey.Size = new Size(317, 20);
            _textBoxEnvironmentKey.Name = $"_textBoxEnvironmentKey";
            _textBoxEnvironmentKey.TextChanged += (UpdateKey);
            _textBoxEnvironmentKey.Text = _localEnvironment.environmentKey;
            _textBoxEnvironmentKey.TabIndex = 1;

            // Add Environment Name TextBox
            _textBoxEnvironmentName = new TextBox();
            groupBoxEnvironment.Controls.Add(_textBoxEnvironmentName);
            _textBoxEnvironmentName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxEnvironmentName.Location = new Point(172, 41);
            _textBoxEnvironmentName.Size = new Size(317, 20);
            _textBoxEnvironmentName.Name = $"_textBoxEnvironmentName";
            _textBoxEnvironmentName.TextChanged += (UpdateName);
            _textBoxEnvironmentName.Text = _localEnvironment.environmentName;
            _textBoxEnvironmentKey.TabIndex = 2;

            // Add Notes Panel
            var panelNotes = new Panel();
            groupBoxEnvironment.Controls.Add(panelNotes);
            //panelConnectionNotes.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            panelNotes.Location = new Point(172, 67);
            panelNotes.Size = new Size(317, 96);
            panelNotes.Name = $"panelNotes";
            panelNotes.BorderStyle = BorderStyle.FixedSingle;
            panelNotes.TabIndex = 52;

            // Add Connection Notes RichTextBox
            _richTextBoxNotes = new RichTextBox();
            _richTextBoxNotes.TabIndex = 52;
            panelNotes.Controls.Add(_richTextBoxNotes);
            _richTextBoxNotes.Name = $"richTextBoxConnectionNotes";
            _richTextBoxNotes.BorderStyle = BorderStyle.None;
            _richTextBoxNotes.Dock = DockStyle.Fill;
            _richTextBoxNotes.Text = _localEnvironment.environmentNotes;
            _richTextBoxNotes.TextChanged += (UpdateNotes);
            toolTipEnvironment.SetToolTip(this._richTextBoxNotes,
                "Free format notes to provide additional information about the environment.");


            // Add Save Button
            Button saveButton = new Button();
            localPanel.Controls.Add(saveButton);
            saveButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            saveButton.Location = new Point(6, 555);
            saveButton.Size = new Size(120, 40);
            saveButton.Name = $"saveButton";
            saveButton.Text = $"Save Environment";
            saveButton.Click += (SaveEnvironment);
            saveButton.TabIndex = 60;

            // Add Delete Button
            Button deleteButton = new Button();
            localPanel.Controls.Add(deleteButton);
            deleteButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            deleteButton.Location = new Point(132, 555);
            deleteButton.Size = new Size(120, 40);
            deleteButton.Name = $"deleteButton";
            deleteButton.Text = $"Delete Environment";
            deleteButton.Click += DeleteEnvironment;
            deleteButton.TabIndex = 70;

            #endregion

            #region Constructor Methods

            // Prevention of double hitting of some event handlers
            this.StartUpIndicator = false;

            // Retrieve the values from memory
            //bool localSSPI = _localEnvironment.databaseServer.integratedSecuritySelectionEvaluation();
            //bool localNamed = _localEnvironment.databaseServer.namedUserSecuritySelectionEvaluation();

            // Display the connection string results
            //_textBoxConnectionString.Text = _localEnvironment.CreateConnectionString(true, localSSPI, localNamed);

            #endregion
        }

        /// <summary>
        /// Delegate event handler from the 'main' form (Form Manage Configurations) to pass back information to be updated on the main textbox. E.g. status updates.
        /// </summary>
        public event EventHandler<MyStringEventArgs> OnChangeMainText = delegate { };

        public void UpdateRichTextBoxInformation(string inputText)
        {
            OnChangeMainText(this, new MyStringEventArgs(inputText));
        }

        /// <summary>
        /// Delegate event handler from the 'main' form (Form Manage Configurations) to pass back the name of the tab page to the control (so that it can be deleted from there).
        /// </summary>
        public event EventHandler<MyWorkingEnvironmentEventArgs> OnDeleteEnvironment = delegate { };

        public void DeleteEnvironment(object sender, EventArgs e)
        {
            if (_localEnvironment.environmentKey != "New")
            {
                // Remove the entry from the configuration file
                if (!File.Exists(_environmentFileName))
                {
                    File.Create(_environmentFileName).Close();
                }

                // Check if the value already exists in the file
                var jsonKeyLookup = new TeamWorkingEnvironment();

                TeamWorkingEnvironment[] jsonArray = JsonConvert.DeserializeObject<TeamWorkingEnvironment[]>(File.ReadAllText(_environmentFileName));

                // If the Json file already contains values (non-empty) then perform a key lookup.
                if (jsonArray != null)
                {
                    jsonKeyLookup = jsonArray.FirstOrDefault(obj => obj.environmentInternalId == _localEnvironment.environmentInternalId);
                }

                // If nothing yet exists in the file, the key lookup is NULL or "" then the record in question does not exist in the Json file.
                // No action needed in this case.
                if (jsonArray == null || jsonKeyLookup == null || jsonKeyLookup.environmentKey == "")
                {
                    // Do nothing.
                }
                else
                {
                    // Remove the Json segment.
                    var list = jsonArray.ToList();
                    var itemToRemove =
                        list.Single(r => r.environmentInternalId == _localEnvironment.environmentInternalId);
                    list.Remove(itemToRemove);
                    jsonArray = list.ToArray();

                    UpdateRichTextBoxInformation(
                        $"The environment {_localEnvironment.environmentKey} was removed from {_environmentFileName}.\r\n");

                    // Remove the value from the global parameter dictionary.
                    FormBase.TeamConfigurationSettings.EnvironmentDictionary.Remove(_localEnvironment.environmentInternalId);
                }

                // Save the updated file to disk.
                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(_environmentFileName, output);
            }

            // The name of the tab page is passed back to the original control (the tab control).
            OnDeleteEnvironment(this, new MyWorkingEnvironmentEventArgs(_localEnvironment));

        }


        /// <summary>
        /// Delegate event handler from the 'main' form (Form Manage Configurations) to pass back the name of the tab page to the control (so that it can be deleted from there).
        /// </summary>
        public event EventHandler<MyStringEventArgs> OnSaveEnvironment = delegate { };

        public void SaveEnvironment(object sender, EventArgs e)
        {
            if (_localEnvironment.environmentKey != "New")
            {
                // Save the connection to global memory (the shared variables across the application).
                // If the connection key (also the dictionary key) already exists, then update the values.
                // If the key does not exist then insert a new row in the connection dictionary.

                if (FormBase.TeamConfigurationSettings.EnvironmentDictionary.ContainsKey(_localEnvironment.environmentInternalId))
                {
                    FormBase.TeamConfigurationSettings.EnvironmentDictionary[_localEnvironment.environmentInternalId] = _localEnvironment;
                }
                else
                {
                    FormBase.TeamConfigurationSettings.EnvironmentDictionary.Add(_localEnvironment.environmentInternalId, _localEnvironment);
                }

                // Update the environment on disk
                if (!File.Exists(_environmentFileName))
                {
                    File.Create(_environmentFileName).Close();
                }

                // Check if the value already exists in the file
                var jsonKeyLookup = new TeamWorkingEnvironment();

                TeamWorkingEnvironment[] jsonArray = JsonConvert.DeserializeObject<TeamWorkingEnvironment[]>(File.ReadAllText(_environmentFileName));

                // If the Json file already contains values (non-empty) then perform a key lookup.
                if (jsonArray != null)
                {
                    jsonKeyLookup = jsonArray.FirstOrDefault(obj => obj.environmentInternalId == _localEnvironment.environmentInternalId);
                }

                // If nothing yet exists in the file, the key lookup is NULL or "" then the record in question does not exist in the Json file and should be added.
                if (jsonArray == null || jsonKeyLookup == null || jsonKeyLookup.environmentInternalId == "")
                {
                    //  There was no key in the file for this connection, so it's new.
                    var list = new List<TeamWorkingEnvironment>();
                    if (jsonArray != null)
                    {
                        list = jsonArray.ToList();
                    }

                    list.Add(_localEnvironment);
                    jsonArray = list.ToArray();
                }
                else
                {
                    // Update the values in an existing JSON segment
                    jsonKeyLookup.environmentInternalId = _localEnvironment.environmentInternalId;
                    jsonKeyLookup.environmentKey = _localEnvironment.environmentKey;
                    jsonKeyLookup.environmentName = _localEnvironment.environmentName;
                    jsonKeyLookup.environmentNotes = _localEnvironment.environmentNotes;
                }

                // Save the updated file to disk.
                EnvironmentConfiguration.CreateFileBackup(_environmentFileName);
                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(_environmentFileName, output);

                UpdateRichTextBoxInformation($"The environment {_localEnvironment.environmentKey} was saved to {_environmentFileName}. A backup was made in the Backups directory also.\r\n");


                // The name of the tab page is passed back to the original control (the tab control).
                OnSaveEnvironment(this, new MyStringEventArgs(this.Name));
            }
            else
            {
                UpdateRichTextBoxInformation("Please update the environment information before saving. The 'new' profile is not meant to be saved.\r\n");
            }
        }


        public void UpdateName(object sender, EventArgs e)
        {
            var localNameObject = (TextBox) sender;

            // Update the name of the tab
            this.Text = localNameObject.Text;
            this.Name = localNameObject.Text;

            // Update the in-memory representation of the connection
            _localEnvironment.environmentName = localNameObject.Text;
        }

        public void UpdateKey(object sender, EventArgs e)
        {
            var localNameObject = (TextBox) sender;

            // Update the in-memory representation of the connection
            _localEnvironment.environmentKey = localNameObject.Text;
        }

        public void UpdateNotes(object sender, EventArgs e)
        {
            var localNameObject = (RichTextBox) sender;

            // Update the in-memory representation of the connection
            _localEnvironment.environmentNotes = localNameObject.Text;
        }

    }
}
