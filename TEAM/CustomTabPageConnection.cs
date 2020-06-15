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
    internal class CustomTabPageConnection : TabPage
    {
        // Startup flag, disabled in constructor. Used to prevent some events from firing twice (creation and value setting).
        internal bool StartUpIndicator = true;

        // In-memory object representing the connection. Is always updated first and then refreshed to the form.
        private TeamConnectionProfile _localConnection;

        private string _connectionFileName = FormBase.GlobalParameters.ConfigurationPath +
                                             FormBase.GlobalParameters.JsonConnectionFileName + '_' +
                                             FormBase.GlobalParameters.WorkingEnvironment +
                                             FormBase.GlobalParameters.JsonExtension;

        // Objects on main Tab Page
        private TextBox _textBoxServer;
        private TextBox _textBoxDatabase;
        private TextBox _textBoxSchema;
        private RadioButton _radioButtonIntegratedSecurity;
        private RadioButton _radioButtonNamedUserSecurity;
        private readonly GroupBox _groupBoxNamedUser;
        private TextBox _textboxUserName;
        private MaskedTextBox _textBoxPassword;
        private readonly TextBox _textBoxConnectionString;
        private TextBox _textBoxConnectionName;
        public TextBox _textBoxConnectionKey;
        private RichTextBox _richTextBoxConnectionNotes;

        /// <summary>
        /// Constructor to instantiate a new Custom Tab Page
        /// </summary>
        public CustomTabPageConnection(object input)
        {
            _localConnection = (TeamConnectionProfile) input;

            var connectionKey = _localConnection.databaseConnectionKey;
            var connectionName = _localConnection.databaseConnectionName;

            //var inputNiceName = Regex.Replace(connectionName, "(\\B[A-Z])", " $1");

            #region Main Tab Page controls

            ToolTip toolTipConnections = new ToolTip();

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

            // Add ConnectionString TextBox
            _textBoxConnectionString = new TextBox();
            localPanel.Controls.Add(_textBoxConnectionString);
            _textBoxConnectionString.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxConnectionString.Location = new Point(6, 187);
            _textBoxConnectionString.Size = new Size(850, 21);
            _textBoxConnectionString.BorderStyle = BorderStyle.None;
            _textBoxConnectionString.BackColor = Color.White;
            _textBoxConnectionString.Name = $"textBoxConnectionString";
            _textBoxConnectionString.ReadOnly = true;
            _textBoxConnectionString.TabStop = false;

            // Add GroupBox for Database content
            var groupBoxDatabase = new GroupBox();
            localPanel.Controls.Add(groupBoxDatabase);
            groupBoxDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxDatabase.Location = new Point(6, 6);
            groupBoxDatabase.Size = new Size(502, 99);
            groupBoxDatabase.Name = $"groupBoxDatabaseName";
            groupBoxDatabase.Text = $"Database";
            groupBoxDatabase.TabStop = false;
            
            // Add Database Label
            var labelDatabase = new Label();
            groupBoxDatabase.Controls.Add(labelDatabase);
            labelDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelDatabase.Location = new Point(6, 19);
            labelDatabase.Size = new Size(160, 13);
            labelDatabase.Name = $"labelDatabaseName";
            labelDatabase.Text = $"Database name";
            labelDatabase.TabStop = false;

            // Add Server Label
            var labelServer = new Label();
            groupBoxDatabase.Controls.Add(labelServer);
            labelServer.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelServer.Location = new Point(6, 44);
            labelServer.Size = new Size(160, 13);
            labelServer.Name = $"labelDatabaseServerName";
            labelServer.Text = $"Database server name";
            labelServer.TabStop = false;

            // Add Schema Label
            var labelSchema = new Label();
            groupBoxDatabase.Controls.Add(labelSchema);
            labelSchema.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelSchema.Location = new Point(6, 70);
            labelSchema.Size = new Size(160, 13);
            labelSchema.Name = $"labelSchema";
            labelSchema.Text = $"Schema";
            labelSchema.TabStop = false;

            // Add Database TextBox
            _textBoxDatabase = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxDatabase);
            _textBoxDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxDatabase.Location = new Point(172, 16);
            _textBoxDatabase.Size = new Size(317, 20);
            _textBoxDatabase.Name = $"textBoxDatabaseName";
            _textBoxDatabase.Text = _localConnection.databaseServer.databaseName;
            _textBoxDatabase.TextChanged += new EventHandler(UpdateConnectionString);
            _textBoxDatabase.TabIndex = 1;

            // Add Server TextBox
            _textBoxServer = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxServer);
            _textBoxServer.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxServer.Location = new Point(172, 41);
            _textBoxServer.Size = new Size(317, 20);
            _textBoxServer.Name = $"textBoxServerName";
            _textBoxServer.Text = _localConnection.databaseServer.serverName;
            _textBoxServer.TextChanged += new EventHandler(UpdateConnectionString);
            _textBoxDatabase.TabIndex = 2;

            // Add Schema TextBox
            _textBoxSchema = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxSchema);
            _textBoxSchema.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxSchema.Location = new Point(172, 67);
            _textBoxSchema.Size = new Size(317, 20);
            _textBoxSchema.Name = $"textBoxSchemaName";
            _textBoxSchema.Text = _localConnection.databaseServer.schemaName;
            _textBoxSchema.TextChanged += new EventHandler(UpdateConnectionString);
            _textBoxSchema.TabIndex = 3;

            // Add GroupBox for Authentication content
            var groupBoxAuthentication = new GroupBox();
            localPanel.Controls.Add(groupBoxAuthentication);
            groupBoxAuthentication.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxAuthentication.Location = new Point(6, 111);
            groupBoxAuthentication.Size = new Size(140, 70);
            groupBoxAuthentication.Name = $"groupBoxAuthentication";
            groupBoxAuthentication.Text = $"Authentication";
            groupBoxAuthentication.TabStop = false;

            // Add RadioButton for Integrated Security
            _radioButtonIntegratedSecurity = new RadioButton();
            groupBoxAuthentication.Controls.Add(_radioButtonIntegratedSecurity);
            _radioButtonIntegratedSecurity.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _radioButtonIntegratedSecurity.Location = new Point(6, 19);
            _radioButtonIntegratedSecurity.Size = new Size(106, 17);
            _radioButtonIntegratedSecurity.Name = $"radioButtonIntegratedSecurity";
            _radioButtonIntegratedSecurity.Text = $"Integrated (SSPI)"; 
            _radioButtonIntegratedSecurity.Checked = _localConnection.databaseServer.integratedSecuritySelectionEvaluation();
            _radioButtonIntegratedSecurity.CheckedChanged += new EventHandler(RadioButtonIntegratedSecurityCheckedChanged);
            _radioButtonIntegratedSecurity.TabIndex = 4;

            // Add RadioButton for Named User
            _radioButtonNamedUserSecurity = new RadioButton();
            groupBoxAuthentication.Controls.Add(_radioButtonNamedUserSecurity);
            _radioButtonNamedUserSecurity.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _radioButtonNamedUserSecurity.Location = new Point(6, 42);
            _radioButtonNamedUserSecurity.Size = new Size(84, 17);
            _radioButtonNamedUserSecurity.Name = $"radioButtonNamedUserSecurity";
            _radioButtonNamedUserSecurity.Text = $"Named User details";
            _radioButtonNamedUserSecurity.Checked = _localConnection.databaseServer.namedUserSecuritySelectionEvaluation();
            _radioButtonNamedUserSecurity.CheckedChanged += new EventHandler(RadioButtonNamedUserCheckedChanged);
            _radioButtonNamedUserSecurity.TabIndex = 5;

            // Add GroupBox for Named User content
            _groupBoxNamedUser = new GroupBox();
            localPanel.Controls.Add(_groupBoxNamedUser);
            _groupBoxNamedUser.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _groupBoxNamedUser.Location = new Point(152, 111);
            _groupBoxNamedUser.Size = new Size(356, 70);
            _groupBoxNamedUser.Name = $"groupBoxNamedUser";
            _groupBoxNamedUser.Text = $"Named User details";
            _groupBoxNamedUser.TabStop = false;

            // Add Username Label
            var labelUserName = new Label();
            _groupBoxNamedUser.Controls.Add(labelUserName);
            labelUserName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelUserName.Location = new Point(6, 19);
            labelUserName.Size = new Size(55, 13);
            labelUserName.Name = $"labelUserName";
            labelUserName.Text = $"Username";
            labelUserName.TabStop = false;

            // Add Password Label
            var labelPassword = new Label();
            _groupBoxNamedUser.Controls.Add(labelPassword);
            labelPassword.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelPassword.Location = new Point(6, 44);
            labelPassword.Size = new Size(53, 13);
            labelPassword.Name = $"labelPassword";
            labelPassword.Text = $"Password";
            labelPassword.TabStop = false;

            // Add Username TextBox
            _textboxUserName = new TextBox();
            _groupBoxNamedUser.Controls.Add(_textboxUserName);
            _textboxUserName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textboxUserName.Location = new Point(67, 16);
            _textboxUserName.Size = new Size(276, 20);
            _textboxUserName.Name = $"textboxUserName";
            _textboxUserName.Text = _localConnection.databaseServer.namedUserName;
            _textboxUserName.TextChanged += UpdateConnectionString;
            _textboxUserName.TabIndex = 6;

            // Add Password TextBox
            _textBoxPassword = new MaskedTextBox();
            _groupBoxNamedUser.Controls.Add(_textBoxPassword);
            _textBoxPassword.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxPassword.Location = new Point(67, 41);
            _textBoxPassword.Size = new Size(276, 20);
            _textBoxPassword.PasswordChar = '*';
            _textBoxPassword.Name = $"textboxUserName";
            _textBoxPassword.Text = _localConnection.databaseServer.namedUserPassword;
            _textBoxPassword.TextChanged += new EventHandler(UpdateConnectionStringWithPassword);
            _textBoxPassword.TabIndex = 7;

            // Add GroupBox for Connection content
            var groupBoxConnection = new GroupBox();
            localPanel.Controls.Add(groupBoxConnection);
            groupBoxConnection.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxConnection.Location = new Point(516, 6);
            groupBoxConnection.Size = new Size(502, 175);
            groupBoxConnection.Name = $"groupBoxConnection";
            groupBoxConnection.Text = $"Connection";
            groupBoxConnection.TabStop = false;

            // Add Connection Key Label
            var labelConnectionKey = new Label();
            groupBoxConnection.Controls.Add(labelConnectionKey);
            //labelConnectionKey.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
            labelConnectionKey.Location = new Point(6, 19);
            labelConnectionKey.Size = new Size(160, 13);
            labelConnectionKey.Name = $"labelConnectionKey";
            labelConnectionKey.Text = $"Connection key";
            //labelConnectionKey.TextChanged += new EventHandler(ManageKeyNameChange);
            labelConnectionKey.TabStop = false;

            // Add Connection Name Label
            var labelConnectionName = new Label();
            groupBoxConnection.Controls.Add(labelConnectionName);
            //labelConnectionName.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
            labelConnectionName.Location = new Point(6, 44);
            labelConnectionName.Size = new Size(160, 13);
            labelConnectionName.Name = $"labelConnectionName";
            labelConnectionName.Text = $"Connection name";
            labelConnectionName.TabStop = false;

            // Add Connection Notes Label
            var labelConnectionNotes = new Label();
            groupBoxConnection.Controls.Add(labelConnectionNotes);
            //labelConnectionNotes.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
            labelConnectionNotes.Location = new Point(6, 70);
            labelConnectionNotes.Size = new Size(160, 13);
            labelConnectionNotes.Name = $"labelConnectionNotes";
            labelConnectionNotes.Text = $"Connection notes";
            labelConnectionNotes.TabStop = false;

            // Add Connection Notes Panel
            var panelConnectionNotes = new Panel();
            groupBoxConnection.Controls.Add(panelConnectionNotes);
            //panelConnectionNotes.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            panelConnectionNotes.Location = new Point(172, 67);
            panelConnectionNotes.Size = new Size(317, 96);
            panelConnectionNotes.Name = $"panelConnectionNotes";
            panelConnectionNotes.BorderStyle = BorderStyle.FixedSingle;
            panelConnectionNotes.TabIndex = 52;


            // Add Connection Key TextBox
            _textBoxConnectionKey = new TextBox();
            groupBoxConnection.Controls.Add(_textBoxConnectionKey);
            //_textBoxConnectionKey.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxConnectionKey.Location = new Point(172, 16);
            _textBoxConnectionKey.Size = new Size(317, 20);
            _textBoxConnectionKey.Name = $"textBoxServerName";
            _textBoxConnectionKey.Text = _localConnection.databaseConnectionKey;
            _textBoxConnectionKey.TextChanged += (UpdateConnectionKey);
            _textBoxConnectionKey.TabIndex = 50;


            // Add Connection Name TextBox
            _textBoxConnectionName = new TextBox();
            groupBoxConnection.Controls.Add(_textBoxConnectionName);
            //_textBoxConnectioName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxConnectionName.Location = new Point(172, 41);
            _textBoxConnectionName.Size = new Size(317, 20);
            _textBoxConnectionName.Name = $"textBoxConnectionName";
            _textBoxConnectionName.Text = _localConnection.databaseConnectionName;
            _textBoxConnectionName.TextChanged += (UpdateConnectionName);
            _textBoxConnectionName.TabIndex = 51;



            // Add Connection Notes RichTextBox
            _richTextBoxConnectionNotes = new RichTextBox();
            _richTextBoxConnectionNotes.TabIndex = 52;
            panelConnectionNotes.Controls.Add(_richTextBoxConnectionNotes);
            _richTextBoxConnectionNotes.Name = $"richTextBoxConnectionNotes";
            _richTextBoxConnectionNotes.BorderStyle = BorderStyle.None;
            _richTextBoxConnectionNotes.Dock = DockStyle.Fill;
            _richTextBoxConnectionNotes.Text = _localConnection.databaseConnectionNotes;
            _richTextBoxConnectionNotes.TextChanged += (UpdateConnectionNotes);
            toolTipConnections.SetToolTip(this._richTextBoxConnectionNotes, "Free format notes to provide additional information about the connection.");


            // Add Save Button
            Button saveButton = new Button();
            localPanel.Controls.Add(saveButton);
            saveButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            saveButton.Location = new Point(6, 555);
            saveButton.Size = new Size(120, 40);
            saveButton.Name = $"saveButton";
            saveButton.Text = $"Save Connection";
            saveButton.Click += (SaveConnection);
            saveButton.TabIndex = 60;

            // Add Delete Button
            Button deleteButton = new Button();
            localPanel.Controls.Add(deleteButton);
            deleteButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            deleteButton.Location = new Point(132, 555);
            deleteButton.Size = new Size(120, 40);
            deleteButton.Name = $"deleteButton";
            deleteButton.Text = $"Delete Connection";
            deleteButton.Click += (DeleteConnection);
            deleteButton.TabIndex = 70;

            #endregion

            #region Constructor Methods

            // Prevention of double hitting of some event handlers
            this.StartUpIndicator = false;

            // Retrieve the values from memory
            bool localSSPI = _localConnection.databaseServer.integratedSecuritySelectionEvaluation();
            bool localNamed = _localConnection.databaseServer.namedUserSecuritySelectionEvaluation();

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true);

            if (_radioButtonIntegratedSecurity.Checked)
            {
                _groupBoxNamedUser.Visible = false;
            }

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
        public event EventHandler<MyConnectionProfileEventArgs> OnDeleteConnection = delegate { };
        public void DeleteConnection(object sender, EventArgs e)
        {
            if (_localConnection.databaseConnectionKey != "New")
            {
                // Remove the entry from the configuration file
                if (!File.Exists(_connectionFileName))
                {
                    File.Create(_connectionFileName).Close();
                }

                // Check if the value already exists in the file
                var jsonKeyLookup = new TeamConnectionProfile();

                TeamConnectionProfile[] jsonArray = JsonConvert.DeserializeObject<TeamConnectionProfile[]>(
                    File.ReadAllText(_connectionFileName));

                // If the Json file already contains values (non-empty) then perform a key lookup.
                if (jsonArray != null)
                {
                    jsonKeyLookup = jsonArray.FirstOrDefault(obj =>
                        obj.connectionInternalId == _localConnection.connectionInternalId);
                }

                // If nothing yet exists in the file, the key lookup is NULL or "" then the record in question does not exist in the Json file.
                // No action needed in this case.
                if (jsonArray == null || jsonKeyLookup == null || jsonKeyLookup.databaseConnectionKey == "")
                {
                    // Do nothing.
                }
                else
                {
                    // Remove the Json segment.
                    var list = jsonArray.ToList();
                    var itemToRemove =
                        list.Single(r => r.connectionInternalId == _localConnection.connectionInternalId);
                    list.Remove(itemToRemove);
                    jsonArray = list.ToArray();

                    UpdateRichTextBoxInformation(
                        $"The connection {_localConnection.databaseConnectionKey} was removed from {_connectionFileName}.\r\n");

                    // Remove the connection from the global dictionary
                    FormBase.ConfigurationSettings.connectionDictionary.Remove(_localConnection.connectionInternalId);
                }

                // Save the updated file to disk.
                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(_connectionFileName, output);
            }

            // The name of the tab page is passed back to the original control (the tab control).
            OnDeleteConnection(this, new MyConnectionProfileEventArgs(_localConnection));

        }

        /// <summary>
        /// Delegate event handler from the 'main' form (Form Manage Configurations) to pass back the name of the tab page to the control (so that it can be deleted from there).
        /// </summary>
        public event EventHandler<MyStringEventArgs> OnSaveConnection = delegate { };

        public void SaveConnection(object sender, EventArgs e)
        {
            if (_localConnection.databaseConnectionKey != "New")
            {
                // Save the connection to global memory (the shared variables across the application).
                // If the connection key (also the dictionary key) already exists, then update the values.
                // If the key does not exist then insert a new row in the connection dictionary.

                if (FormBase.ConfigurationSettings.connectionDictionary.ContainsKey(_localConnection.connectionInternalId))
                {
                    FormBase.ConfigurationSettings.connectionDictionary[_localConnection.connectionInternalId] = _localConnection;
                }
                else
                {
                    FormBase.ConfigurationSettings.connectionDictionary.Add(_localConnection.connectionInternalId, _localConnection);
                }

                // Update the connection on disk

                if (!File.Exists(_connectionFileName))
                {
                    File.Create(_connectionFileName).Close();
                }

                // Check if the value already exists in the file
                var jsonKeyLookup = new TeamConnectionProfile();

                TeamConnectionProfile[] jsonArray = JsonConvert.DeserializeObject<TeamConnectionProfile[]>(File.ReadAllText(_connectionFileName));

                // If the Json file already contains values (non-empty) then perform a key lookup.
                if (jsonArray != null)
                {
                    jsonKeyLookup = jsonArray.FirstOrDefault(obj => obj.connectionInternalId == _localConnection.connectionInternalId);
                }

                // If nothing yet exists int he file, the key lookup is NULL or "" then the record in question does not exist in the Json file and should be added.
                if (jsonArray == null || jsonKeyLookup == null || jsonKeyLookup.databaseConnectionKey == "")
                {
                    //  There was no key in the file for this connection, so it's new.
                    var list = new List<TeamConnectionProfile>();
                    if (jsonArray != null)
                    {
                        list = jsonArray.ToList();
                    }
                    list.Add(_localConnection);
                    jsonArray = list.ToArray();
                }
                else
                {
                    // Update the values in an existing JSON segment
                    jsonKeyLookup.connectionInternalId = _localConnection.connectionInternalId;
                    jsonKeyLookup.databaseConnectionKey = _localConnection.databaseConnectionKey;
                    jsonKeyLookup.databaseConnectionName = _localConnection.databaseConnectionName;
                    jsonKeyLookup.databaseConnectionNotes = _localConnection.databaseConnectionNotes;
                }

                // Save the updated file to disk.
                EnvironmentConfiguration.CreateFileBackup(_connectionFileName);
                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(_connectionFileName, output);

                UpdateRichTextBoxInformation($"The connection {_localConnection.databaseConnectionKey} was saved to {_connectionFileName}. A backup was made in the Backups directory also.\r\n");

                // The name of the tab page is passed back to the original control (the tab control).
                OnSaveConnection(this, new MyStringEventArgs(this.Name));
            }
            else
            {
                UpdateRichTextBoxInformation("Please update the connection information before saving. The 'new' profile is not meant to be saved.\r\n");
            }
        }


        public void UpdateConnectionName(object sender, EventArgs e)
        {
            var localNameObject = (TextBox) sender;

            // Update the name of the tab
            this.Text = localNameObject.Text;
            this.Name = localNameObject.Text;

            // Update the in-memory representation of the connection
            _localConnection.databaseConnectionName = localNameObject.Text;
        }

        public void UpdateConnectionKey(object sender, EventArgs e)
        {
            var localNameObject = (TextBox)sender;

            // Update the in-memory representation of the connection
            _localConnection.databaseConnectionKey = localNameObject.Text;
        }

        public void UpdateConnectionNotes(object sender, EventArgs e)
        {
            var localNameObject = (RichTextBox)sender;

            // Update the in-memory representation of the connection
            _localConnection.databaseConnectionNotes = localNameObject.Text;
        }

        /// <summary>
        /// /// Receive a TextBox control and use the contents to update the in-memory connection object (database, server, schema, username), as well as display the connectionstring value updated with the text box content.s
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateConnectionString(object sender, EventArgs e)
        {
            var localTextBox = (TextBox)sender;

            if (localTextBox.Name == _textBoxDatabase.Name)
            {
                _localConnection.databaseServer.databaseName = localTextBox.Text;
            } 

            if (localTextBox.Name == _textBoxServer.Name)
            {
                _localConnection.databaseServer.serverName = localTextBox.Text;
            }

            if (localTextBox.Name == _textBoxSchema.Name)
            {
                _localConnection.databaseServer.schemaName = localTextBox.Text;
            }

            if (localTextBox.Name == _textboxUserName.Name)
            {
                _localConnection.databaseServer.namedUserName = localTextBox.Text;
            }

            bool localSspi = _localConnection.databaseServer.integratedSecuritySelectionEvaluation();
            bool localNamed = _localConnection.databaseServer.namedUserSecuritySelectionEvaluation();

            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true);
        }



        /// <summary>
        /// Receive a MaskedTextBox control and use the contents to update the in-memory connection object, as well as display the connectionstring value updated with the text box content.s
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateConnectionStringWithPassword(object sender, EventArgs e)
        {
            var localTextBox = (MaskedTextBox)sender;

            _localConnection.databaseServer.namedUserPassword = localTextBox.Text;

            bool localSspi = _localConnection.databaseServer.integratedSecuritySelectionEvaluation();
            bool localNamed = _localConnection.databaseServer.namedUserSecuritySelectionEvaluation();

            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true);
        }

        /// <summary>
        /// Update the form in case the SSPI / integrated security radio button has been clicked/checked.
        /// If the SSPI radiobutton is checked, some named user controls need to be hidden from view.
        /// Also commits the check value (enabled for named user) to memory (connection object update).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RadioButtonIntegratedSecurityCheckedChanged(object sender, EventArgs e)
        {
            _groupBoxNamedUser.Visible = false;

            _localConnection.databaseServer.authenticationType = ServerAuthenticationTypes.SSPI;

            if (_radioButtonIntegratedSecurity.Checked)
            {
                _localConnection.databaseServer.authenticationType = ServerAuthenticationTypes.SSPI;
            }

            // Retrieve the values from memory
            bool localSspi = _localConnection.databaseServer.integratedSecuritySelectionEvaluation();
            bool localNamed = _localConnection.databaseServer.namedUserSecuritySelectionEvaluation();

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true);
        }

        /// <summary>
        /// Make sure the named user controls on the form are visible if the named user radiobutton has been checked.
        /// Also commits the check value (enabled for named user) to memory (connection object update).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RadioButtonNamedUserCheckedChanged(object sender, EventArgs e)
        {
            if (_radioButtonNamedUserSecurity.Checked)
            {
                _groupBoxNamedUser.Visible = true;
                _localConnection.databaseServer.authenticationType = ServerAuthenticationTypes.NamedUser;
            }

            // Retrieve the values from memory
            bool localSSPI = _localConnection.databaseServer.integratedSecuritySelectionEvaluation();
            bool localNamed = _localConnection.databaseServer.namedUserSecuritySelectionEvaluation();

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true);
        }
    }
}
