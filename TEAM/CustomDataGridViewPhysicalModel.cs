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
            var dimIdentifier = "";
            var factIdentifier = "";

            if (Form_Base.ConfigurationSettings.TableNamingLocation == "Prefix")
            {
                hubIdentifier = Form_Base.ConfigurationSettings.HubTablePrefixValue + "_";
                satIdentifier = Form_Base.ConfigurationSettings.SatTablePrefixValue + "_";
                lnkIdentifier = Form_Base.ConfigurationSettings.LinkTablePrefixValue + "_";
                lsatIdentifier = Form_Base.ConfigurationSettings.LsatTablePrefixValue + "_";
                stgIdentifier = Form_Base.ConfigurationSettings.StgTablePrefixValue + "_";
                psaIdentifier = Form_Base.ConfigurationSettings.PsaTablePrefixValue + "_";
                dimIdentifier = "DIM_";
                factIdentifier = "FACT_";
            }
            else
            {
                hubIdentifier = '_' + Form_Base.ConfigurationSettings.HubTablePrefixValue;
                satIdentifier = '_' + Form_Base.ConfigurationSettings.SatTablePrefixValue;
                lnkIdentifier = '_' + Form_Base.ConfigurationSettings.LinkTablePrefixValue;
                lsatIdentifier = '_' + Form_Base.ConfigurationSettings.LsatTablePrefixValue;
                stgIdentifier = '_' + Form_Base.ConfigurationSettings.StgTablePrefixValue;
                psaIdentifier = '_' + Form_Base.ConfigurationSettings.PsaTablePrefixValue;
                dimIdentifier = "_DIM";
                factIdentifier = "_FACT";
            }

            foreach (DataGridViewRow row in Rows)
            {
                var integrationTable = row.Cells[4].Value;

                if (integrationTable != null && row.IsNewRow == false)
                {
                   // Backcolour for Integration Layer tables
                    if (Regex.Matches(integrationTable.ToString(), hubIdentifier).Count>0)
                    {
                        this[4, counter].Style.BackColor = Color.CornflowerBlue;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lsatIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.Gold;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), satIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.Yellow;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lnkIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.OrangeRed;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), psaIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.AntiqueWhite;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), stgIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.WhiteSmoke;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), dimIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.Aqua;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), factIdentifier).Count > 0)
                    {
                        this[4, counter].Style.BackColor = Color.MediumAquamarine;
                    }

                }

                counter++;
            }
        }
    }
}
