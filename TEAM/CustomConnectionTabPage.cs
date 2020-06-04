using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TEAM
{
    /// <summary>
    /// Derived Custom Connection TabPage inherited from the TabPage class.
    /// </summary>
    internal class CustomTabPage : TabPage
    {
        // Startup flag, disabled in constructor. Used to prevent some events from firing twice (creation and value setting).
        internal bool StartUpIndicator = true;

        // In-memory object representing the connection. Is always updated first and then refreshed to the form.
        private TeamConnectionProfile _localConnection;

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

        /// <summary>
        /// Constructor to instantiate a new Custom Tab Page
        /// </summary>
        public CustomTabPage(object input)
        {
            _localConnection = (TeamConnectionProfile) input;

            var connectionKey = _localConnection.databaseConnectionKey;
            var connectionName = _localConnection.databaseConnectionName;

            var inputNiceName = Regex.Replace(connectionName, "(\\B[A-Z])", " $1");

            #region Main Tab Page controls
            // Base properties of the custom tab page
            Name = $"{connectionKey}";
            Text = inputNiceName;
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

            // Add ConnectionString TextBox
            _textBoxConnectionString = new TextBox();
            localPanel.Controls.Add(_textBoxConnectionString);
            _textBoxConnectionString.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxConnectionString.Location = new Point(6, 187);
            _textBoxConnectionString.Size = new Size(502, 21);
            _textBoxConnectionString.BorderStyle = BorderStyle.None;
            _textBoxConnectionString.Name = $"textBoxConnectionString";

            // Add GroupBox for Database content
            var groupBoxDatabase = new GroupBox();
            localPanel.Controls.Add(groupBoxDatabase);
            groupBoxDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxDatabase.Location = new Point(6, 6);
            groupBoxDatabase.Size = new Size(502, 99);
            groupBoxDatabase.Name = $"groupBoxDatabaseName";
            groupBoxDatabase.Text = $"Database";
            
            // Add Database Label
            var labelDatabase = new Label();
            groupBoxDatabase.Controls.Add(labelDatabase);
            labelDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelDatabase.Location = new Point(6, 19);
            labelDatabase.Size = new Size(160, 13);
            labelDatabase.Name = $"labelDatabaseName";
            labelDatabase.Text = $"Database name";
            
            // Add Server Label
            var labelServer = new Label();
            groupBoxDatabase.Controls.Add(labelServer);
            labelServer.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelServer.Location = new Point(6, 44);
            labelServer.Size = new Size(160, 13);
            labelServer.Name = $"labelDatabaseServerName";
            labelServer.Text = $"Database server name";

            // Add Schema Label
            var labelSchema = new Label();
            groupBoxDatabase.Controls.Add(labelSchema);
            labelSchema.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelSchema.Location = new Point(6, 70);
            labelSchema.Size = new Size(160, 13);
            labelSchema.Name = $"labelSchema";
            labelSchema.Text = $"Schema";

            // Add Database TextBox
            _textBoxDatabase = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxDatabase);
            _textBoxDatabase.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxDatabase.Location = new Point(172, 16);
            _textBoxDatabase.Size = new Size(317, 20);
            _textBoxDatabase.Name = $"textBoxDatabaseName";
            _textBoxDatabase.Text = _localConnection.databaseServer.databaseName;
            _textBoxDatabase.TextChanged += new EventHandler(UpdateConnectionString);

            // Add Server TextBox
            _textBoxServer = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxServer);
            _textBoxServer.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxServer.Location = new Point(172, 41);
            _textBoxServer.Size = new Size(317, 20);
            _textBoxServer.Name = $"textBoxServerName";
            _textBoxServer.Text = _localConnection.databaseServer.serverName;
            _textBoxServer.TextChanged += new EventHandler(UpdateConnectionString);

            // Add Schema TextBox
            _textBoxSchema = new TextBox();
            groupBoxDatabase.Controls.Add(_textBoxSchema);
            _textBoxSchema.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textBoxSchema.Location = new Point(172, 67);
            _textBoxSchema.Size = new Size(317, 20);
            _textBoxSchema.Name = $"textBoxSchemaName";
            _textBoxSchema.Text = _localConnection.databaseServer.schemaName;
            _textBoxSchema.TextChanged += new EventHandler(UpdateConnectionString);


            // Add GroupBox for Named User content
            _groupBoxNamedUser = new GroupBox();
            localPanel.Controls.Add(_groupBoxNamedUser);
            _groupBoxNamedUser.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _groupBoxNamedUser.Location = new Point(152, 111);
            _groupBoxNamedUser.Size = new Size(356, 70);
            _groupBoxNamedUser.Name = $"groupBoxNamedUser";
            _groupBoxNamedUser.Text = $"Named User details";

            // Add Username Label
            var labelUserName = new Label();
            _groupBoxNamedUser.Controls.Add(labelUserName);
            labelUserName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelUserName.Location = new Point(6, 19);
            labelUserName.Size = new Size(55, 13);
            labelUserName.Name = $"labelUserName";
            labelUserName.Text = $"Username";
            
            // Add Password Label
            var labelPassword = new Label();
            _groupBoxNamedUser.Controls.Add(labelPassword);
            labelPassword.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            labelPassword.Location = new Point(6, 44);
            labelPassword.Size = new Size(53, 13);
            labelPassword.Name = $"labelPassword";
            labelPassword.Text = $"Password";

            // Add Username TextBox
            _textboxUserName = new TextBox();
            _groupBoxNamedUser.Controls.Add(_textboxUserName);
            _textboxUserName.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            _textboxUserName.Location = new Point(67, 16);
            _textboxUserName.Size = new Size(276, 20);
            _textboxUserName.Name = $"textboxUserName";
            _textboxUserName.Text = _localConnection.databaseServer.namedUserName;
            _textboxUserName.TextChanged += new EventHandler(UpdateConnectionString);

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

            // Add GroupBox for Authentication content
            var groupBoxAuthentication = new GroupBox();
            localPanel.Controls.Add(groupBoxAuthentication);
            groupBoxAuthentication.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            groupBoxAuthentication.Location = new Point(6, 111);
            groupBoxAuthentication.Size = new Size(140, 70);
            groupBoxAuthentication.Name = $"groupBoxAuthentication";
            groupBoxAuthentication.Text = $"Authentication";

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

            #endregion

            #region Constructor Methods

            // Prevention of double hitting of some event handlers
            this.StartUpIndicator = false;

            // Retrieve the values from memory
            bool localSSPI = _localConnection.databaseServer.integratedSecuritySelectionEvaluation();
            bool localNamed = _localConnection.databaseServer.namedUserSecuritySelectionEvaluation();

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true, localSSPI, localNamed);

            #endregion
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

            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true, localSspi, localNamed);
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

            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true, localSspi, localNamed);
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
            if (_radioButtonNamedUserSecurity.Checked == false)
            {
                _groupBoxNamedUser.Visible = false;

                _localConnection.databaseServer.authenticationType = ServerAuthenticationTypes.SSPI;
            }

            if (_radioButtonIntegratedSecurity.Checked)
            {
                FormBase.ConfigurationSettings.MetadataNamed = "False";
                FormBase.ConfigurationSettings.MetadataSspi = "True";
                _localConnection.databaseServer.authenticationType = ServerAuthenticationTypes.SSPI;
            }

            // Retrieve the values from memory
            bool localSspi = _localConnection.databaseServer.integratedSecuritySelectionEvaluation();
            bool localNamed = _localConnection.databaseServer.namedUserSecuritySelectionEvaluation();

            // Display the connection string results
            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true, localSspi, localNamed);
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
            _textBoxConnectionString.Text = _localConnection.CreateConnectionString(true, localSSPI, localNamed);
        }
    }
}
