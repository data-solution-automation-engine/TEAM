using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TEAM
{
    class CustomDataGridViewPhysicalModel : DataGridView 
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
                var integrationTable = row.Cells[4].Value;

                if (integrationTable != null && row.IsNewRow == false)
                {
                   // Backcolour for Integration Layer tables
                    if (Regex.Matches(integrationTable.ToString(), hubIdentifier).Count>0)
                    {
                        this[4, counter].Style.BackColor = Color.CadetBlue;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lsatIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.Yellow;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), satIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.Gold;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lnkIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.Red;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), stgIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.WhiteSmoke;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), psaIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.AntiqueWhite;
                    }

                }

                counter++;
            }
        }
    }
}
