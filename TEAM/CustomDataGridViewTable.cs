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
            var dimIdentifier = "";
            var factIdentifier = "";

            if (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix")
            {
                hubIdentifier = FormBase.ConfigurationSettings.HubTablePrefixValue + "_";
                satIdentifier = FormBase.ConfigurationSettings.SatTablePrefixValue + "_";
                lnkIdentifier = FormBase.ConfigurationSettings.LinkTablePrefixValue + "_";
                lsatIdentifier = FormBase.ConfigurationSettings.LsatTablePrefixValue + "_";
                stgIdentifier = FormBase.ConfigurationSettings.StgTablePrefixValue + "_";
                psaIdentifier = FormBase.ConfigurationSettings.PsaTablePrefixValue + "_";
                dimIdentifier = "DIM_";
                factIdentifier = "FACT_";
            }
            else
            {
                hubIdentifier = '_' + FormBase.ConfigurationSettings.HubTablePrefixValue;
                satIdentifier = '_' + FormBase.ConfigurationSettings.SatTablePrefixValue;
                lnkIdentifier = '_' + FormBase.ConfigurationSettings.LinkTablePrefixValue;
                lsatIdentifier = '_' + FormBase.ConfigurationSettings.LsatTablePrefixValue;
                stgIdentifier = '_' + FormBase.ConfigurationSettings.StgTablePrefixValue;
                psaIdentifier = '_' + FormBase.ConfigurationSettings.PsaTablePrefixValue;
                dimIdentifier = "_DIM";
                factIdentifier = "_FACT";
            }

            foreach (DataGridViewRow row in Rows)
            {
               // var genericTable = row.Cells[0].Value;
                var targetTable = row.Cells[3].Value;
                var businessKeySyntax = row.Cells[4].Value;

                if (targetTable != null && businessKeySyntax != null && row.IsNewRow == false)
                {
                   // Backcolour for Integration Layer tables
                    if (Regex.Matches(targetTable.ToString(), hubIdentifier).Count>0)
                    {
                        this[3, counter].Style.BackColor = Color.CornflowerBlue;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = System.Drawing.Color.LightGray;
                    }
                    else if (Regex.Matches(targetTable.ToString(), lsatIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Gold;
                    }
                    else if (Regex.Matches(targetTable.ToString(), satIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Yellow;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }
                    else if (Regex.Matches(targetTable.ToString(), lnkIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.OrangeRed;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }
                    else if (Regex.Matches(targetTable.ToString(), psaIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.AntiqueWhite;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }
                    else if (Regex.Matches(targetTable.ToString(), stgIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.WhiteSmoke;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }
                    else if (Regex.Matches(targetTable.ToString(), dimIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Aqua;
                        row.Cells[5].ReadOnly = true;
                        row.Cells[5].Style.BackColor = Color.LightGray;
                    }
                    else if (Regex.Matches(targetTable.ToString(), factIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.MediumAquamarine;
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
