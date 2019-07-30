using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TEAM
{
    class CustomDataGridViewTable : DataGridView 
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            ColourGridView();
        }

        private void ColourGridView()
        {
            var counter = 0;

            var hubIdentifier = "";
            var satIdentifier = "";
            var lnkIdentifier = "";
            var lsatIdentifier = "";
            var stgIdentifier = "";
            var psaIdentifier = "";

            if (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix")
            {
                hubIdentifier = FormBase.ConfigurationSettings.HubTablePrefixValue + "_";
                satIdentifier = FormBase.ConfigurationSettings.SatTablePrefixValue + "_";
                lnkIdentifier = FormBase.ConfigurationSettings.LinkTablePrefixValue + "_";
                lsatIdentifier = FormBase.ConfigurationSettings.LsatTablePrefixValue + "_";
                stgIdentifier = FormBase.ConfigurationSettings.StgTablePrefixValue + "_";
                psaIdentifier = FormBase.ConfigurationSettings.PsaTablePrefixValue + "_";
            }
            else
            {
                hubIdentifier = '_' + FormBase.ConfigurationSettings.HubTablePrefixValue;
                satIdentifier = '_' + FormBase.ConfigurationSettings.SatTablePrefixValue;
                lnkIdentifier = '_' + FormBase.ConfigurationSettings.LinkTablePrefixValue;
                lsatIdentifier = '_' + FormBase.ConfigurationSettings.LsatTablePrefixValue;
                stgIdentifier = '_' + FormBase.ConfigurationSettings.StgTablePrefixValue;
                psaIdentifier = '_' + FormBase.ConfigurationSettings.PsaTablePrefixValue;
            }

            foreach (DataGridViewRow row in Rows)
            {
               // var genericTable = row.Cells[0].Value;
                var integrationTable = row.Cells[3].Value;
                var businessKeySyntax = row.Cells[4].Value;

                if (integrationTable != null && businessKeySyntax != null && row.IsNewRow == false)
                {
                   // Backcolour for Integration Layer tables
                    if (Regex.Matches(integrationTable.ToString(), hubIdentifier).Count>0)
                    {
                        this[3, counter].Style.BackColor = Color.CadetBlue;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = System.Drawing.Color.LightGray;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lsatIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Yellow;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), satIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Gold;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lnkIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Red;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), stgIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.NavajoWhite;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), psaIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.AntiqueWhite;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }

                    //Syntax highlighting for code
                    if (businessKeySyntax.ToString().Contains("CONCATENATE") || businessKeySyntax.ToString().Contains("COMPOSITE"))
                    {      
                        this[4, counter].Style.ForeColor = Color.DarkBlue;
                        this[4, counter].Style.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
                    }
                }

                counter++;
            }
        }
    }
}
