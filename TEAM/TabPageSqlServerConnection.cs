﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using TEAM_Library;
using static TEAM.FormBase;

namespace TEAM
{
    /// <summary>
    /// Derived Custom Connection TabPage inherited from the TabPage class.
    /// </summary>
    internal class TabPageSqlServerConnection : TabPage
    {
        // Startup flag, disabled in constructor. Used to prevent some events from firing twice (creation and value setting).
        internal bool StartUpIndicator = true;

        // In-memory object representing the connection. Is always updated first and then refreshed to the form.
        private readonly TeamConnection _localConnection;

        private readonly string _connectionFileName = globalParameters.ConfigurationPath + globalParameters.JsonConnectionFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.JsonExtension;

        // Objects on main Tab Page.
        private readonly TextBox _textBoxServer;
        private readonly TextBox _textBoxPortNumber;
        private readonly TextBox _textBoxDatabase;
        private readonly TextBox _textBoxSchema;
        private readonly RadioButton _radioButtonIntegratedSecurity;
        private readonly RadioButton _radioButtonNamedUserSecurity;
        private readonly RadioButton _radioButtonUniversalMfa;

        private readonly GroupBox _groupBoxNamedUser;
        private readonly TextBox _textBoxUserName;
        private readonly MaskedTextBox _textBoxPassword;

        private readonly GroupBox _groupBoxMfa;
        private readonly TextBox _textBoxMfaUserName;

        private readonly TextBox _textBoxConnectionString;

        private readonly TextBox _textBoxConnectionName;
        public TextBox _textBoxConnectionKey;
        private readonly RichTextBox _richTextBoxConnectionNotes;
        private readonly RadioButton _radioButtonDatabaseCatalog;
        private readonly RadioButton _radioButtonDatabaseCustom;

        // Objects related to custom queries
        private readonly GroupBox _groupBoxDatabaseCustom;
        private readonly RichTextBox _richTextBoxCustomQuery;

        /// <summary>
        /// Constructor to instantiate a new Custom Tab Page.
        /// </summary>
        public TabPageSqlServerConnection(object input)
        {
            _localConnection = (TeamConnection) input;

            #region Main Tab Page controls

            ToolTip toolTipConnections = new ToolTip();
            toolTipConnections.AutoPopDelay = 3000;

            // Base properties of the custom tab page
            Name = $"{_localConnection.ConnectionKey}";
            Text = _localConnection.ConnectionName;
            BackColor = Color.Transparent;
            BorderStyle = BorderStyle.None;
            UseVisualStyleBackColor = true;
            Size = new Size(1330, 601);
            AutoSizeMode = AutoSizeMode.GrowOnly;
            AutoSize = true;
            AutoScroll = true;

            // Add Panel to facilitate docking
            var localPanel = new Panel();
            Controls.Add(localPanel);
            localPanel.Dock = DockStyle.Fill;
            localPanel.AutoSize = true;
            localPanel.TabStop = false;

            #region Database connection controls

            // GroupBox for Database content
            var groupBoxDatabase = new GroupBox();
            localPanel.Controls.Add(groupBoxDatabase);
            groupBoxDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxDatabase.Location = new Point(6, 6);
            groupBoxDatabase.Size = new Size(535, 124);
            groupBoxDatabase.Name = "groupBoxDatabaseName";
            groupBoxDatabase.Text = @"Database";
            groupBoxDatabase.TabStop = false;
            
            // Database Label
            var labelDatabase = new Label();
            groupBoxDatabase.Controls.Add(labelDatabase);
            labelDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelDatabase.Location = new Point(6, 19);
            labelDatabase.Size = new Size(160, 13);
            labelDatabase.Name = "labelDatabaseName";
            labelDatabase.Text = @"Database name";
            labelDatabase.TabStop = false;

            // Server Label
            var labelServer = new Label();
            groupBoxDatabase.Controls.Add(labelServer);
            labelServer.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelServer.Location = new Point(6, 44);
            labelServer.Size = new Size(160, 13);
            labelServer.Name = "labelDatabaseServerName";
            labelServer.Text = @"Database server name";
            labelServer.TabStop = false;

            // Add Port Label
            var labelPortNumber = new Label();
            groupBoxDatabase.Controls.Add(labelPortNumber);
            labelPortNumber.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelPortNumber.Location = new Point(6, 69);
            labelPortNumber.Size = new Size(160, 13);
            labelPortNumber.Name = "labelPortNumber";
            labelPortNumber.Text = @"Database server port number";
            labelPortNumber.TabStop = false;

            // Add Schema Label
            var labelSchema = new Label();
            groupBoxDatabase.Controls.Add(labelSchema);
            labelSchema.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelSchema.Location = new Point(6, 94);
            labelSchema.Size = new Size(160, 13);
            labelSchema.Name = "labelSchema";
            labelSchema.Text = @"Schema";
            labelSchema.TabStop = false;

            // Add Database TextBox
            _textBoxDatabase = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxDatabase);
            _textBoxDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxDatabase.Location = new Point(172, 16);
            _textBoxDatabase.Size = new Size(355, 20);
            _textBoxDatabase.Name = "textBoxDatabaseName";
            _textBoxDatabase.Text = _localConnection.DatabaseServer.DatabaseName;
            _textBoxDatabase.TextChanged += UpdateConnectionString;
            _textBoxDatabase.TabIndex = 1;

            // Add Server TextBox
            _textBoxServer = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxServer);
            _textBoxServer.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxServer.Location = new Point(172, 41);
            _textBoxServer.Size = new Size(355, 20);
            _textBoxServer.Name = "textBoxServerName";
            _textBoxServer.Text = _localConnection.DatabaseServer.ServerName;
            _textBoxServer.TextChanged +=UpdateConnectionString;
            _textBoxServer.TabIndex = 2;

            // Add Port Number TextBox
            _textBoxPortNumber = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxPortNumber);
            _textBoxPortNumber.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxPortNumber.Location = new Point(172, 69);
            _textBoxPortNumber.Size = new Size(355, 20);
            _textBoxPortNumber.Name = "textBoxPortNumber";
            _textBoxPortNumber.Text = _localConnection.DatabaseServer.PortNumber;
            _textBoxPortNumber.TextChanged += UpdateConnectionString;
            _textBoxPortNumber.TabIndex = 3;
            toolTipConnections.SetToolTip(this._textBoxPortNumber, "Optional port number that can be used to connect to the database server.");

            // Add Schema TextBox
            _textBoxSchema = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxSchema);
            _textBoxSchema.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxSchema.Location = new Point(172, 94);
            _textBoxSchema.Size = new Size(355, 20);
            _textBoxSchema.Name = "textBoxSchemaName";
            _textBoxSchema.Text = _localConnection.DatabaseServer.SchemaName;
            _textBoxSchema.TextChanged += UpdateConnectionString;
            _textBoxSchema.TabIndex = 4;

            // Add GroupBox for Authentication content
            var groupBoxAuthentication = new GroupBox();
            localPanel.Controls.Add(groupBoxAuthentication);
            groupBoxAuthentication.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxAuthentication.Location = new Point(6, 136);
            groupBoxAuthentication.Size = new Size(140, 93);
            groupBoxAuthentication.Name = "groupBoxAuthentication";
            groupBoxAuthentication.Text = @"Authentication";
            groupBoxAuthentication.TabStop = false;

            // Add RadioButton for Integrated Security
            _radioButtonIntegratedSecurity = new RadioButton();
            groupBoxAuthentication.Controls.Add(_radioButtonIntegratedSecurity);
            _radioButtonIntegratedSecurity.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _radioButtonIntegratedSecurity.Location = new Point(6, 19);
            _radioButtonIntegratedSecurity.Size = new Size(106, 17);
            _radioButtonIntegratedSecurity.Name = "radioButtonIntegratedSecurity";
            _radioButtonIntegratedSecurity.Text = @"Integrated (SSPI)"; 
            _radioButtonIntegratedSecurity.Checked = _localConnection.DatabaseServer.IsSSPI();
            _radioButtonIntegratedSecurity.CheckedChanged += RadioButtonIntegratedSecurityCheckedChanged;
            _radioButtonIntegratedSecurity.TabIndex = 5;

            // Add RadioButton for Named User
            _radioButtonNamedUserSecurity = new RadioButton();
            groupBoxAuthentication.Controls.Add(_radioButtonNamedUserSecurity);
            _radioButtonNamedUserSecurity.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _radioButtonNamedUserSecurity.Location = new Point(6, 42);
            _radioButtonNamedUserSecurity.Size = new Size(84, 17);
            _radioButtonNamedUserSecurity.Name = "radioButtonNamedUserSecurity";
            _radioButtonNamedUserSecurity.Text = @"Named User";
            _radioButtonNamedUserSecurity.Checked = _localConnection.DatabaseServer.IsNamedUser();
            _radioButtonNamedUserSecurity.CheckedChanged += RadioButtonNamedUserCheckedChanged;
            _radioButtonNamedUserSecurity.TabIndex = 6;

            // Add RadioButton for Universal with MFA
            _radioButtonUniversalMfa = new RadioButton();
            groupBoxAuthentication.Controls.Add(_radioButtonUniversalMfa);
            _radioButtonUniversalMfa.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _radioButtonUniversalMfa.Location = new Point(6, 65);
            _radioButtonUniversalMfa.Size = new Size(100, 17);
            _radioButtonUniversalMfa.Name = "radioButtonUniversalMfa";
            _radioButtonUniversalMfa.Text = @"Universal MFA";
            _radioButtonUniversalMfa.Checked = _localConnection.DatabaseServer.IsMfa();
            _radioButtonUniversalMfa.CheckedChanged += RadioButtonMfaCheckedChanged;
            _radioButtonUniversalMfa.TabIndex = 7;

            // Add GroupBox for Named User content
            _groupBoxNamedUser = new GroupBox();
            localPanel.Controls.Add(_groupBoxNamedUser);
            _groupBoxNamedUser.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _groupBoxNamedUser.Location = new Point(152, 136);
            _groupBoxNamedUser.Size = new Size(389, 93);
            _groupBoxNamedUser.Name = "groupBoxNamedUser";
            _groupBoxNamedUser.Text = @"Named User details";
            _groupBoxNamedUser.TabStop = false;

            // Add Username Label
            var labelUserName = new Label();
            _groupBoxNamedUser.Controls.Add(labelUserName);
            labelUserName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelUserName.Location = new Point(6, 19);
            labelUserName.Size = new Size(55, 13);
            labelUserName.Name = "labelUserName";
            labelUserName.Text = @"User name";
            labelUserName.TabStop = false;

            // Add Password Label
            var labelPassword = new Label();
            _groupBoxNamedUser.Controls.Add(labelPassword);
            labelPassword.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelPassword.Location = new Point(6, 44);
            labelPassword.Size = new Size(53, 13);
            labelPassword.Name = "labelPassword";
            labelPassword.Text = @"Password";
            labelPassword.TabStop = false;

            // Add Username TextBox
            _textBoxUserName = new TextBox();
            _groupBoxNamedUser.Controls.Add(_textBoxUserName);
            _textBoxUserName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxUserName.Location = new Point(67, 16);
            _textBoxUserName.Size = new Size(312, 20);
            _textBoxUserName.Name = "textboxUserName";
            _textBoxUserName.Text = _localConnection.DatabaseServer.NamedUserName;
            _textBoxUserName.TextChanged += UpdateConnectionString;
            _textBoxUserName.TabIndex = 7;

            // Add Password TextBox
            _textBoxPassword = new MaskedTextBox();
            _groupBoxNamedUser.Controls.Add(_textBoxPassword);
            _textBoxPassword.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxPassword.Location = new Point(67, 41);
            _textBoxPassword.Size = new Size(312, 20);
            _textBoxPassword.PasswordChar = '*';
            _textBoxPassword.Name = "textboxUserPassword";
            _textBoxPassword.Text = _localConnection.DatabaseServer.NamedUserPassword;
            _textBoxPassword.TextChanged += UpdateConnectionStringWithPassword;
            _textBoxPassword.TabIndex = 8;

            // ConnectionString TextBox
            _textBoxConnectionString = new TextBox();
            localPanel.Controls.Add(_textBoxConnectionString);
            _textBoxConnectionString.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxConnectionString.Location = new Point(6, 245);
            _textBoxConnectionString.Size = new Size(850, 21);
            _textBoxConnectionString.BorderStyle = BorderStyle.None;
            _textBoxConnectionString.BackColor = Color.Snow;
            _textBoxConnectionString.Name = "textBoxConnectionString";
            _textBoxConnectionString.ReadOnly = true;
            _textBoxConnectionString.TabStop = false;

            #endregion

            #region MFA panel

            // Add GroupBox for Named User content
            _groupBoxMfa = new GroupBox();
            localPanel.Controls.Add(_groupBoxMfa);
            _groupBoxMfa.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _groupBoxMfa.Location = new Point(152, 136);
            _groupBoxMfa.Size = new Size(389, 93);
            _groupBoxMfa.Name = "groupBoxMfa";
            _groupBoxMfa.Text = @"Multi-Factor Authentication details";
            _groupBoxMfa.TabStop = false;

            // Add Username Label
            var labelMfaUserName = new Label();
            _groupBoxMfa.Controls.Add(labelMfaUserName);
            labelMfaUserName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelMfaUserName.Location = new Point(6, 19);
            labelMfaUserName.Size = new Size(55, 13);
            labelMfaUserName.Name = "labelMfaUserName";
            labelMfaUserName.Text = @"User name";
            labelMfaUserName.TabStop = false;

            _textBoxMfaUserName = new TextBox();
            _groupBoxMfa.Controls.Add(_textBoxMfaUserName);
            _textBoxMfaUserName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxMfaUserName.Location = new Point(67, 16);
            _textBoxMfaUserName.Size = new Size(312, 20);
            _textBoxMfaUserName.Name = "textboxMfaUserName";
            _textBoxMfaUserName.Text = _localConnection.DatabaseServer.MultiFactorAuthenticationUser ?? System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            _textBoxMfaUserName.TextChanged += UpdateConnectionString;
            _textBoxMfaUserName.TabIndex = 9;

            #endregion

            #region Custom Query

            // Add GroupBox for Custom Query content
            _groupBoxDatabaseCustom = new GroupBox();
            localPanel.Controls.Add(_groupBoxDatabaseCustom);
            _groupBoxDatabaseCustom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _groupBoxDatabaseCustom.Location = new Point(6, (_textBoxConnectionString.Location.Y+18));
            _groupBoxDatabaseCustom.Size = new Size(1313, 285);
            _groupBoxDatabaseCustom.Name = "groupBoxCustomQueryConnection";
            _groupBoxDatabaseCustom.Text = @"Custom Query";
            _groupBoxDatabaseCustom.TabStop = false;

            // Add Custom Query Panel
            var panelCustomQuery = new Panel();
            _groupBoxDatabaseCustom.Controls.Add(panelCustomQuery);
            panelCustomQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            panelCustomQuery.Location = new Point(8, 16);
            panelCustomQuery.Size = new Size(1296, 260);
            panelCustomQuery.Name = "panelCustomQuery";
            panelCustomQuery.BorderStyle = BorderStyle.FixedSingle;

            // Add Custom Query RichTextBox
            _richTextBoxCustomQuery = new RichTextBox();
            panelCustomQuery.Controls.Add(_richTextBoxCustomQuery);
            _richTextBoxCustomQuery.Dock = DockStyle.Fill;
            _richTextBoxCustomQuery.Name = "richTextBoxCustomQuery";
            _richTextBoxCustomQuery.BorderStyle = BorderStyle.None;
            _richTextBoxCustomQuery.Text = _localConnection.ConnectionCustomQuery;
            _richTextBoxCustomQuery.TextChanged += UpdateCustomQuery;
            toolTipConnections.SetToolTip(_richTextBoxCustomQuery, "Free format query to fetch physical model (snapshot) metadata, in case a custom query is used instead of direct catalog access.");

            #endregion

            #region Connection generic controls

            // Add GroupBox for Connection content
            var groupBoxConnection = new GroupBox();
            localPanel.Controls.Add(groupBoxConnection);
            groupBoxConnection.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxConnection.Location = new Point(550, 6);
            groupBoxConnection.Size = new Size(535, 223);
            groupBoxConnection.Name = "groupBoxConnection";
            groupBoxConnection.Text = @"Connection";
            groupBoxConnection.TabStop = false;

            // Add Connection Key Label
            var labelConnectionKey = new Label();
            groupBoxConnection.Controls.Add(labelConnectionKey);
            labelConnectionKey.Location = new Point(6, 19);
            labelConnectionKey.Size = new Size(160, 13);
            labelConnectionKey.Name = "labelConnectionKey";
            labelConnectionKey.Text = @"Connection key";
            labelConnectionKey.TabStop = false;

            // Add Connection Name Label
            var labelConnectionName = new Label();
            groupBoxConnection.Controls.Add(labelConnectionName);
            labelConnectionName.Location = new Point(6, 44);
            labelConnectionName.Size = new Size(160, 13);
            labelConnectionName.Name = "labelConnectionName";
            labelConnectionName.Text = @"Connection name";
            labelConnectionName.TabStop = false;

            // Add Connection Type Label
            var labelConnectionType = new Label();
            groupBoxConnection.Controls.Add(labelConnectionType);
            labelConnectionType.Location = new Point(6, 69);
            labelConnectionType.Size = new Size(160, 13);
            labelConnectionType.Name = "labelConnectionType";
            labelConnectionType.Text = @"Connection type";
            labelConnectionType.TabStop = false;

            // Add Connection Notes Label
            var labelConnectionNotes = new Label();
            groupBoxConnection.Controls.Add(labelConnectionNotes);
            labelConnectionNotes.Location = new Point(6, 119);
            labelConnectionNotes.Size = new Size(160, 13);
            labelConnectionNotes.Name = "labelConnectionNotes";
            labelConnectionNotes.Text = @"Connection notes";
            labelConnectionNotes.TabStop = false;

            // Add Connection Key TextBox
            _textBoxConnectionKey = new TextBox();
            groupBoxConnection.Controls.Add(_textBoxConnectionKey);
            _textBoxConnectionKey.Location = new Point(172, 16);
            _textBoxConnectionKey.Size = new Size(355, 20);
            _textBoxConnectionKey.Name = "textBoxServerName";
            _textBoxConnectionKey.Text = _localConnection.ConnectionKey;
            _textBoxConnectionKey.TextChanged += UpdateConnectionKey;
            _textBoxConnectionKey.TabIndex = 50;
            toolTipConnections.SetToolTip(this._textBoxConnectionKey, "The Connection Key is meant to be a short and easily understood reference for the connection that can be used within TEAM.");

            // Add Connection Name TextBox
            _textBoxConnectionName = new TextBox();
            groupBoxConnection.Controls.Add(_textBoxConnectionName);
            _textBoxConnectionName.Location = new Point(172, 41);
            _textBoxConnectionName.Size = new Size(355, 20);
            _textBoxConnectionName.Name = "textBoxConnectionName";
            _textBoxConnectionName.Text = _localConnection.ConnectionName;
            _textBoxConnectionName.TextChanged += UpdateConnectionName;
            _textBoxConnectionName.TabIndex = 51;

            // Add Connection Type Radiobutton for Database
            _radioButtonDatabaseCatalog = new RadioButton();
            groupBoxConnection.Controls.Add(_radioButtonDatabaseCatalog);
            _radioButtonDatabaseCatalog.Location = new Point(172, 66);
            _radioButtonDatabaseCatalog.Name = "radioButtonDatabaseCatalog";
            _radioButtonDatabaseCatalog.Text = CatalogConnectionTypes.Catalog.ToString();
            _radioButtonDatabaseCatalog.CheckedChanged += UpdateConnectionTypeControls;
            _radioButtonDatabaseCatalog.TabIndex = 52;

            // Add Connection Type Radiobutton for File
            _radioButtonDatabaseCustom = new RadioButton();
            groupBoxConnection.Controls.Add(_radioButtonDatabaseCustom);
            _radioButtonDatabaseCustom.Location = new Point(172, 88);
            _radioButtonDatabaseCustom.Name = "radioButtonCustom";
            _radioButtonDatabaseCustom.Text = CatalogConnectionTypes.Custom.ToString();
            _radioButtonDatabaseCustom.CheckedChanged += UpdateConnectionTypeControls;
            _radioButtonDatabaseCustom.TabIndex = 53;

            SetConnectionTypesRadioButton();

            // Add Connection Notes Panel
            var panelConnectionNotes = new Panel();
            groupBoxConnection.Controls.Add(panelConnectionNotes);
            panelConnectionNotes.Location = new Point(172, 119);
            panelConnectionNotes.Size = new Size(354, 95);
            panelConnectionNotes.Name = "panelConnectionNotes";
            panelConnectionNotes.BorderStyle = BorderStyle.FixedSingle;

            // Add Connection Notes RichTextBox
            _richTextBoxConnectionNotes = new RichTextBox();
            _richTextBoxConnectionNotes.TabIndex = 54;
            panelConnectionNotes.Controls.Add(_richTextBoxConnectionNotes);
            _richTextBoxConnectionNotes.Name = "richTextBoxConnectionNotes";
            _richTextBoxConnectionNotes.BorderStyle = BorderStyle.None;
            _richTextBoxConnectionNotes.Dock = DockStyle.Fill;
            _richTextBoxConnectionNotes.Text = _localConnection.ConnectionNotes;
            _richTextBoxConnectionNotes.TextChanged += UpdateConnectionNotes;
            toolTipConnections.SetToolTip(_richTextBoxConnectionNotes, "Free format notes to provide additional information about the connection.");

            #endregion

            // Save Button
            Button saveButton = new Button();
            localPanel.Controls.Add(saveButton);
            saveButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            saveButton.Location = new Point(5, 555);
            saveButton.Size = new Size(120, 40);
            saveButton.Name = "saveButton";
            saveButton.Text = @"Save Connection";
            saveButton.Click += SaveConnection;
            saveButton.TabIndex = 60;

            // Delete Button
            Button deleteButton = new Button();
            localPanel.Controls.Add(deleteButton);
            deleteButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            deleteButton.Location = new Point(131, 555);
            deleteButton.Size = new Size(120, 40);
            deleteButton.Name = "deleteButton";
            deleteButton.Text = @"Delete Connection";
            deleteButton.Click += DeleteConnection;
            deleteButton.TabIndex = 70;

            // Test Button
            Button testButton = new Button();
            localPanel.Controls.Add(testButton);
            testButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            testButton.Location = new Point(257, 555);
            testButton.Size = new Size(120, 40);
            testButton.Name = "testButton";
            testButton.Text = @"Test Connection";
            testButton.Click += TestConnection;
            testButton.TabIndex = 80;

            #endregion

            #region Constructor Methods

            // Prevention of double hitting of some event handlers
            StartUpIndicator = false;

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateSqlServerConnectionString(true);

            if (_radioButtonIntegratedSecurity.Checked)
            {
                _groupBoxNamedUser.Visible = false;
                _groupBoxMfa.Visible = false;
            }
            else if (_radioButtonNamedUserSecurity.Checked)
            {
                _groupBoxMfa.Visible = false;
            }
            else if (_radioButtonUniversalMfa.Checked)
            {
                _groupBoxNamedUser.Visible = false;
            }

            #endregion
        }

        public sealed override bool AutoSize
        {
            get { return base.AutoSize; }
            set { base.AutoSize = value; }
        }

        public sealed override AutoSizeMode AutoSizeMode
        {
            get { return base.AutoSizeMode; }
            set { base.AutoSizeMode = value; }
        }

        public sealed override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// Delegate event handler from the 'main' form (Form Manage Configurations) to pass back information to be updated on the main text box. E.g. status updates.
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
            if (_localConnection.ConnectionKey != "New")
            {
                // Remove the entry from the configuration file.
                if (!File.Exists(_connectionFileName))
                {
                    File.Create(_connectionFileName).Close();
                }

                // Check if the value already exists in the file
                var jsonKeyLookup = new TeamConnection();

                TeamConnection[] jsonArray = JsonConvert.DeserializeObject<TeamConnection[]>(File.ReadAllText(_connectionFileName));

                // If the Json file already contains values (non-empty) then perform a key lookup.
                if (jsonArray != null)
                {
                    jsonKeyLookup = jsonArray.FirstOrDefault(obj =>
                        obj.ConnectionInternalId == _localConnection.ConnectionInternalId);
                }

                // If nothing yet exists in the file, the key lookup is NULL or "" then the record in question does not exist in the Json file.
                // No action needed in this case.
                if (jsonArray == null || jsonKeyLookup == null || jsonKeyLookup.ConnectionKey == "")
                {
                    // Do nothing.
                }
                else
                {
                    // Remove the Json segment.
                    var list = jsonArray.ToList();
                    var itemToRemove = list.Single(r => r.ConnectionInternalId == _localConnection.ConnectionInternalId);
                    list.Remove(itemToRemove);
                    jsonArray = list.ToArray();

                    UpdateRichTextBoxInformation($"The connection {_localConnection.ConnectionKey} was removed from {_connectionFileName}.\r\n");

                    // Remove the connection from the global dictionary
                    FormBase.TeamConfiguration.ConnectionDictionary.Remove(_localConnection.ConnectionInternalId);
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
            if (_localConnection.ConnectionKey != "New")
            {
                // Save the connection to global memory (the shared variables across the application).
                // If the connection key (also the dictionary key) already exists, then update the values.
                // If the key does not exist then insert a new row in the connection dictionary.

                if (FormBase.TeamConfiguration.ConnectionDictionary.ContainsKey(_localConnection.ConnectionInternalId))
                {
                    FormBase.TeamConfiguration.ConnectionDictionary[_localConnection.ConnectionInternalId] = _localConnection;
                }
                else
                {
                    FormBase.TeamConfiguration.ConnectionDictionary.Add(_localConnection.ConnectionInternalId, _localConnection);
                }

                // Update the connection on disk

                if (!File.Exists(_connectionFileName))
                {
                    File.Create(_connectionFileName).Close();
                }

                // Check if the value already exists in the file.
                var jsonKeyLookup = new TeamConnection();

                TeamConnection[] jsonArray = JsonConvert.DeserializeObject<TeamConnection[]>(File.ReadAllText(_connectionFileName));

                // If the Json file already contains values (non-empty) then perform a key lookup.
                if (jsonArray != null)
                {
                    jsonKeyLookup = jsonArray.FirstOrDefault(obj => obj.ConnectionInternalId == _localConnection.ConnectionInternalId);
                }

                // If nothing yet exists in the file, the key lookup is NULL or "" then the record in question does not exist in the Json file and should be added.
                if (jsonArray == null || jsonKeyLookup == null || jsonKeyLookup.ConnectionKey == "")
                {
                    //  There was no key in the file for this connection, so it's new.
                    var list = new List<TeamConnection>();
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
                    jsonKeyLookup.ConnectionInternalId = _localConnection.ConnectionInternalId;
                    jsonKeyLookup.ConnectionName = _localConnection.ConnectionName;
                    jsonKeyLookup.ConnectionKey = _localConnection.ConnectionKey;
                    jsonKeyLookup.CatalogConnectionType = GetSelectedConnectionTypeRadioButtonFromForm();
                    jsonKeyLookup.ConnectionNotes = _localConnection.ConnectionNotes;
                    jsonKeyLookup.DatabaseServer = _localConnection.DatabaseServer;
                    jsonKeyLookup.ConnectionCustomQuery = _localConnection.ConnectionCustomQuery;
                }

                try
                {
                    // Save the updated file to disk.
                    FileHandling.CreateFileBackup(_connectionFileName, globalParameters.BackupPath);
                    string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);

                    File.WriteAllText(_connectionFileName, output);

                    UpdateRichTextBoxInformation($"The connection {_localConnection.ConnectionKey} was saved to {_connectionFileName}. A backup was made in the Backups directory also.\r\n");
                }
                catch (Exception ex)
                {
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error occurred: {ex.Message}"));
                }

                // The name of the tab page is passed back to the original control (the tab control).
                OnSaveConnection(this, new MyStringEventArgs(Name));
            }
            else
            {
                UpdateRichTextBoxInformation("Please update the connection information before saving. The 'new' profile is not meant to be saved.\r\n");
            }
        }

        /// <summary>
        /// See if the connection can connect to the intended target environment.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TestConnection(object sender, EventArgs e)
        {
            UpdateRichTextBoxInformation("Validating the database connection.\r\n");

            var connectionString = _localConnection.CreateSqlServerConnectionString(false);

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    UpdateRichTextBoxInformation("The database connection could be successfully established.\r\n");
                }
                catch (Exception exception)
                {
                    UpdateRichTextBoxInformation($"The database connection could not be established. The error message is {exception.Message} \r\n");
                }
            }

        }

        // Retrieve a single value on which RadioButton has been checked.
        public void SetConnectionTypesRadioButton()
        {
            if (_localConnection.CatalogConnectionType == CatalogConnectionTypes.Catalog)
            {
                _radioButtonDatabaseCatalog.Checked = true;
            }
            else
            {
                _radioButtonDatabaseCustom.Checked = true;
            }
        }

        /// <summary>
        /// Retrieve a single value on which RadioButton has been checked.
        /// </summary>
        /// <returns>ConnectionType enumerator (TeamConnection)</returns>
        public CatalogConnectionTypes GetSelectedConnectionTypeRadioButtonFromForm()
        {
            var localConnectionType = CatalogConnectionTypes.Catalog;

            if (_radioButtonDatabaseCustom.Checked)
            {
                localConnectionType = CatalogConnectionTypes.Custom;
            }

            return localConnectionType;
        }

        public void UpdateConnectionName(object sender, EventArgs e)
        {
            var localNameObject = (TextBox) sender;

            // Update the name of the tab
            Text = localNameObject.Text;
            Name = localNameObject.Text;

            // Update the in-memory representation of the connection.
            _localConnection.ConnectionName = localNameObject.Text;
        }

        public void UpdateConnectionKey(object sender, EventArgs e)
        {
            var localNameObject = (TextBox)sender;

            // Update the in-memory representation of the connection.
            _localConnection.ConnectionKey = localNameObject.Text;
        }

        /// <summary>
        /// Add/update (set) the notes to the in-memory object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateConnectionNotes(object sender, EventArgs e)
        {
            var localNameObject = (RichTextBox)sender;

            // Update the in-memory representation of the connection
            _localConnection.ConnectionNotes = localNameObject.Text;
        }

        public void UpdateCustomQuery(object sender, EventArgs e)
        {
            var localNameObject = (RichTextBox)sender;

            // Update the in-memory representation of the connection
            _localConnection.ConnectionCustomQuery = localNameObject.Text;
        }

        /// <summary>
        /// /// Receive a TextBox control and use the contents to update the in-memory connection object (database, server, schema, username), as well as display the connection string value updated with the text box content.s
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateConnectionString(object sender, EventArgs e)
        {
            var localTextBox = (TextBox)sender;

            if (localTextBox.Name == _textBoxDatabase.Name)
            {
                _localConnection.DatabaseServer.DatabaseName = localTextBox.Text;
            } 

            if (localTextBox.Name == _textBoxServer.Name)
            {
                _localConnection.DatabaseServer.ServerName = localTextBox.Text;
            }

            if (localTextBox.Name == _textBoxPortNumber.Name)
            {
                _localConnection.DatabaseServer.PortNumber = localTextBox.Text;
            }

            if (localTextBox.Name == _textBoxSchema.Name)
            {
                _localConnection.DatabaseServer.SchemaName = localTextBox.Text;
            }

            if (localTextBox.Name == _textBoxUserName.Name)
            {
                _localConnection.DatabaseServer.NamedUserName = localTextBox.Text;
            }

            #region MFA

            if (localTextBox.Name == _textBoxMfaUserName.Name)
            {
                _localConnection.DatabaseServer.MultiFactorAuthenticationUser = localTextBox.Text;
            }

            #endregion

            _textBoxConnectionString.Text = _localConnection.CreateSqlServerConnectionString(true);
        }



        /// <summary>
        /// Receive a MaskedTextBox control and use the contents to update the in-memory connection object, as well as display the connection string value updated with the text box content.s
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateConnectionStringWithPassword(object sender, EventArgs e)
        {
            var localTextBox = (MaskedTextBox)sender;

            _localConnection.DatabaseServer.NamedUserPassword = localTextBox.Text;

            _textBoxConnectionString.Text = _localConnection.CreateSqlServerConnectionString(true);
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
            _groupBoxMfa.Visible = false;

            _localConnection.DatabaseServer.AuthenticationType = ServerAuthenticationTypes.SSPI;

            if (_radioButtonIntegratedSecurity.Checked)
            {
                _localConnection.DatabaseServer.AuthenticationType = ServerAuthenticationTypes.SSPI;
            }

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateSqlServerConnectionString(true);
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
                _groupBoxMfa.Visible = false;
                _localConnection.DatabaseServer.AuthenticationType = ServerAuthenticationTypes.NamedUser;
            }

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateSqlServerConnectionString(true);
        }

        /// <summary>
        /// Make sure the MFA details on the form are visible if the MFA radiobutton has been checked.
        /// Also commits the check value (enabled for named user) to memory (connection object update).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RadioButtonMfaCheckedChanged(object sender, EventArgs e)
        {
            if (_radioButtonUniversalMfa.Checked)
            {
                _groupBoxMfa.Visible = true;
                _groupBoxNamedUser.Visible = false;
                _localConnection.DatabaseServer.AuthenticationType = ServerAuthenticationTypes.MFA;
                _localConnection.DatabaseServer.MultiFactorAuthenticationUser = _textBoxMfaUserName.Text;
            }

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateSqlServerConnectionString(true);
        }

        public void UpdateConnectionTypeControls(object sender, EventArgs e)
        {
            // Show or hide form controls
            if (sender != null)
            {
                if (((RadioButton) sender).Name == _radioButtonDatabaseCustom.Name && _radioButtonDatabaseCustom.Checked)
                {
                    // Commit the setting to memory
                    _localConnection.CatalogConnectionType = CatalogConnectionTypes.Custom;

                    // Database connection controls
                    //_groupBoxNamedUser.Enabled = false;
                    //_groupBoxNamedUser.Visible = false;

                    //_groupBoxDatabase.Enabled = false;
                    //_groupBoxDatabase.Visible = false;

                    //_groupBoxAuthentication.Enabled = false;
                    //_groupBoxAuthentication.Visible = false;

                    //_textBoxConnectionString.Enabled = false;
                    //_textBoxConnectionString.Visible = false;

                    // File connection controls
                    _groupBoxDatabaseCustom.Enabled = true;
                    _groupBoxDatabaseCustom.Visible = true;
                }

                if (((RadioButton) sender).Name == _radioButtonDatabaseCatalog.Name && _radioButtonDatabaseCatalog.Checked)
                {
                    // Commit the setting to memory
                    _localConnection.CatalogConnectionType = CatalogConnectionTypes.Catalog;

                    // Database connection controls
                    //_groupBoxNamedUser.Enabled = true;
                    //_groupBoxNamedUser.Visible = true;

                    //_groupBoxDatabase.Enabled = true;
                    //_groupBoxDatabase.Visible = true;

                    //_groupBoxAuthentication.Enabled = true;
                    //_groupBoxAuthentication.Visible = true;

                    //_textBoxConnectionString.Enabled = true;
                    //_textBoxConnectionString.Visible = true;

                    // File connection controls
                    _groupBoxDatabaseCustom.Enabled = false;
                    _groupBoxDatabaseCustom.Visible = false;
                }
            }
        }
    }
}
