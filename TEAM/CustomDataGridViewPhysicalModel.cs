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

            var hubIdentifier = "HUB_";
            var satIdentifier = "SAT_";
            var lnkIdentifier = "LNK_";
            var lsatIdentifier = "LSAT_";

            ColourGridView(hubIdentifier, satIdentifier, lnkIdentifier, lsatIdentifier);
        }

        private void ColourGridView(string hubIdentifier, string satIdentifier, string lnkIdentifier, string lsatIdentifier)
        {
            var counter = 0;

            foreach (DataGridViewRow row in Rows)
            {
                var integrationTable = row.Cells[3].Value;

                if (integrationTable != null && row.IsNewRow == false)
                {
                   // Backcolour for Integration Layer tables
                    if (Regex.Matches(integrationTable.ToString(), hubIdentifier).Count>0)
                    {
                        this[3, counter].Style.BackColor = Color.CadetBlue;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lsatIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Yellow;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), satIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Gold;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lnkIdentifier).Count > 0)
                    {
                        this[3, counter].Style.BackColor = Color.Red;
                    }

                }

                counter++;
            }
        }
    }
}
