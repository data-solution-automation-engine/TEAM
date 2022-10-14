using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TEAM_Library;

namespace TEAM
{
    internal class TeamDataGridView : DataGridView
    {
        /// <summary>
        /// Override event for the OnPaint event to apply colour coding.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            ColourGridView();
        }

        /// <summary>
        /// The definition of the Data Grid View for table mappings (DataObject mappings).
        /// </summary>
        public TeamDataGridView()
        {
            AutoGenerateColumns = false;
            ColumnHeadersVisible = true;
            EditMode = DataGridViewEditMode.EditOnEnter;
            
            if (!Controls.ContainsKey(DataObjectMappingGridColumns.Enabled.ToString()))
            {
                DataGridViewCheckBoxColumn enabledIndicator = new DataGridViewCheckBoxColumn();
                enabledIndicator.Name = DataObjectMappingGridColumns.Enabled.ToString();
                enabledIndicator.HeaderText = DataObjectMappingGridColumns.Enabled.ToString();
                enabledIndicator.DataPropertyName = DataObjectMappingGridColumns.Enabled.ToString();
                Columns.Add(enabledIndicator);
            }

            if (!Controls.ContainsKey(DataObjectMappingGridColumns.HashKey.ToString()))
            {
                DataGridViewTextBoxColumn hashKey = new DataGridViewTextBoxColumn();
                hashKey.Name = DataObjectMappingGridColumns.HashKey.ToString();
                hashKey.HeaderText = DataObjectMappingGridColumns.HashKey.ToString();
                hashKey.DataPropertyName = DataObjectMappingGridColumns.HashKey.ToString();
                hashKey.Visible = false;
                Columns.Add(hashKey);
            }

            DataGridViewTextBoxColumn sourceTable = new DataGridViewTextBoxColumn();
            sourceTable.Name = DataObjectMappingGridColumns.SourceDataObject.ToString();
            sourceTable.HeaderText = @"Source Data Object";
            sourceTable.DataPropertyName = DataObjectMappingGridColumns.SourceDataObject.ToString();
            Columns.Add(sourceTable);

            DataGridViewComboBoxColumn sourceConnection = new DataGridViewComboBoxColumn();
            sourceConnection.Name = DataObjectMappingGridColumns.SourceConnection.ToString();
            sourceConnection.HeaderText = @"Source Connection";
            sourceConnection.DataPropertyName = DataObjectMappingGridColumns.SourceConnection.ToString();
            sourceConnection.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            sourceConnection.DataSource = LocalTeamConnection.GetConnections(FormBase.TeamConfiguration.ConnectionDictionary);
            sourceConnection.DisplayMember = "ConnectionKey";
            sourceConnection.ValueMember = "ConnectionId";
            sourceConnection.ValueType = typeof(string);
            Columns.Add(sourceConnection);

            DataGridViewTextBoxColumn targetTable = new DataGridViewTextBoxColumn();
            targetTable.Name = DataObjectMappingGridColumns.TargetDataObject.ToString();
            targetTable.HeaderText = @"Target Data Object";
            targetTable.DataPropertyName = DataObjectMappingGridColumns.TargetDataObject.ToString();
            Columns.Add(targetTable);

            DataGridViewComboBoxColumn targetConnection = new DataGridViewComboBoxColumn();
            targetConnection.Name = DataObjectMappingGridColumns.TargetConnection.ToString();
            targetConnection.HeaderText = @"Target Connection";
            targetConnection.DataPropertyName = DataObjectMappingGridColumns.TargetConnection.ToString();
            targetConnection.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            targetConnection.DataSource = LocalTeamConnection.GetConnections(FormBase.TeamConfiguration.ConnectionDictionary);
            targetConnection.DisplayMember = "ConnectionKey";
            targetConnection.ValueMember = "ConnectionId";
            targetConnection.ValueType = typeof(string);
            Columns.Add(targetConnection);

            DataGridViewTextBoxColumn businessKeyDefinition = new DataGridViewTextBoxColumn();
            businessKeyDefinition.Name = DataObjectMappingGridColumns.BusinessKeyDefinition.ToString();
            businessKeyDefinition.HeaderText = @"Business Key Definition";
            businessKeyDefinition.DataPropertyName = DataObjectMappingGridColumns.BusinessKeyDefinition.ToString();
            Columns.Add(businessKeyDefinition);

            DataGridViewTextBoxColumn drivingKeyDefinition = new DataGridViewTextBoxColumn();
            drivingKeyDefinition.Name = DataObjectMappingGridColumns.DrivingKeyDefinition.ToString();
            drivingKeyDefinition.HeaderText = @"Driving Key Definition";
            drivingKeyDefinition.DataPropertyName = DataObjectMappingGridColumns.DrivingKeyDefinition.ToString();
            Columns.Add(drivingKeyDefinition);

            DataGridViewTextBoxColumn filterCriterion = new DataGridViewTextBoxColumn();
            filterCriterion.Name = DataObjectMappingGridColumns.FilterCriterion.ToString();
            filterCriterion.HeaderText = @"Filter Criterion";
            filterCriterion.DataPropertyName = DataObjectMappingGridColumns.FilterCriterion.ToString();
            Columns.Add(filterCriterion);
        }

        private void ColourGridView()
        {
            var counter = 0;

            var presentationLayerLabelArray = Utility.SplitLabelIntoArray(FormBase.TeamConfiguration.PresentationLayerLabels);
            var transformationLabelArray = Utility.SplitLabelIntoArray(FormBase.TeamConfiguration.TransformationLabels);

            foreach (DataGridViewRow row in Rows)
            {
                if (!row.IsNewRow)
                {
                    // Target info
                    string targetDataObjectName = row.Cells[(int) DataObjectMappingGridColumns.TargetDataObject].Value.ToString();
                    var targetConnectionId = row.Cells[(int) DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                    TeamConnection targetConnection = FormBase.GetTeamConnectionByConnectionId(targetConnectionId);
                    KeyValuePair<string, string> targetDataObjectFullyQualifiedKeyValuePair = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();

                    // Only the name (e.g. without the schema) should be evaluated.
                    string targetDataObjectNonQualifiedName = targetDataObjectFullyQualifiedKeyValuePair.Value;

                    var businessKeySyntax = row.Cells[(int) DataObjectMappingGridColumns.BusinessKeyDefinition].Value;

                    if (targetDataObjectNonQualifiedName != null && businessKeySyntax != null && row.IsNewRow == false)
                    {
                        // Hub
                        if (
                            (FormBase.TeamConfiguration.TableNamingLocation == "Prefix" && targetDataObjectNonQualifiedName.StartsWith(FormBase.TeamConfiguration.HubTablePrefixValue)) || (FormBase.TeamConfiguration.TableNamingLocation == "Suffix" && targetDataObjectNonQualifiedName.EndsWith(FormBase.TeamConfiguration.HubTablePrefixValue))
                        )
                        {
                            this[(int) DataObjectMappingGridColumns.TargetDataObject, counter].Style.BackColor = Color.CornflowerBlue;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].ReadOnly = true;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                        }
                        // Link-Sat
                        else if ((FormBase.TeamConfiguration.TableNamingLocation == "Prefix" && targetDataObjectNonQualifiedName.StartsWith(FormBase.TeamConfiguration.LsatTablePrefixValue)) || (FormBase.TeamConfiguration.TableNamingLocation == "Suffix" && targetDataObjectNonQualifiedName.EndsWith(FormBase.TeamConfiguration.LsatTablePrefixValue)))
                        {
                            this[(int) DataObjectMappingGridColumns.TargetDataObject, counter].Style.BackColor = Color.Gold;
                        }
                        // Context
                        else if (
                            (FormBase.TeamConfiguration.TableNamingLocation == "Prefix" && targetDataObjectNonQualifiedName.StartsWith(FormBase.TeamConfiguration.SatTablePrefixValue)) || (FormBase.TeamConfiguration.TableNamingLocation == "Suffix" && targetDataObjectNonQualifiedName.EndsWith(FormBase.TeamConfiguration.SatTablePrefixValue))
                        )
                        {
                            this[(int) DataObjectMappingGridColumns.TargetDataObject, counter].Style.BackColor = Color.Yellow;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].ReadOnly = true;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].Style.BackColor =
                                Color.LightGray;
                        }
                        // Natural Business Relationship
                        else if (
                            (FormBase.TeamConfiguration.TableNamingLocation == "Prefix" && targetDataObjectNonQualifiedName.StartsWith(FormBase.TeamConfiguration.LinkTablePrefixValue)) || (FormBase.TeamConfiguration.TableNamingLocation == "Suffix" && targetDataObjectNonQualifiedName.EndsWith(FormBase.TeamConfiguration.LinkTablePrefixValue))
                        )
                        {
                            this[(int) DataObjectMappingGridColumns.TargetDataObject, counter].Style.BackColor = Color.OrangeRed;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].ReadOnly = true;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                        }
                        // PSA
                        else if (
                            (FormBase.TeamConfiguration.TableNamingLocation == "Prefix" && targetDataObjectNonQualifiedName.StartsWith(FormBase.TeamConfiguration.PsaTablePrefixValue)) ||
                            (FormBase.TeamConfiguration.TableNamingLocation == "Suffix" && targetDataObjectNonQualifiedName.EndsWith(FormBase.TeamConfiguration.PsaTablePrefixValue))
                        )
                        {
                            this[(int) DataObjectMappingGridColumns.TargetDataObject, counter].Style.BackColor = Color.AntiqueWhite;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].ReadOnly = true;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                        }
                        // Staging
                        else if (
                            (FormBase.TeamConfiguration.TableNamingLocation == "Prefix" && targetDataObjectNonQualifiedName.StartsWith(FormBase.TeamConfiguration.StgTablePrefixValue)) || (FormBase.TeamConfiguration.TableNamingLocation == "Suffix" && targetDataObjectNonQualifiedName.EndsWith(FormBase.TeamConfiguration.StgTablePrefixValue))
                        )
                        {
                            this[(int) DataObjectMappingGridColumns.TargetDataObject, counter].Style.BackColor = Color.WhiteSmoke;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].ReadOnly = true;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                        }
                        // Presentation Layer
                        else if (

                            (FormBase.TeamConfiguration.TableNamingLocation == "Prefix" && presentationLayerLabelArray.Any(s => targetDataObjectNonQualifiedName.StartsWith(s)))
                            ||
                            (FormBase.TeamConfiguration.TableNamingLocation == "Suffix" && presentationLayerLabelArray.Any(s => targetDataObjectNonQualifiedName.EndsWith(s)))
                        )
                        {
                            this[(int) DataObjectMappingGridColumns.TargetDataObject, counter].Style.BackColor = Color.Aquamarine;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].ReadOnly = true;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                        }
                        // Derived objects / transformations
                        else if (

                            (FormBase.TeamConfiguration.TableNamingLocation == "Prefix" &&
                             transformationLabelArray.Any(s => targetDataObjectNonQualifiedName.StartsWith(s)))
                            ||
                            (FormBase.TeamConfiguration.TableNamingLocation == "Suffix" &&
                             transformationLabelArray.Any(s => targetDataObjectNonQualifiedName.EndsWith(s)))
                        )
                        {
                            this[(int) DataObjectMappingGridColumns.TargetDataObject, counter].Style.BackColor = Color.LightGreen;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].ReadOnly = true;
                            row.Cells[(int) DataObjectMappingGridColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                        }
                        else
                        {
                            // Catch
                        }


                        //Syntax highlighting for code.
                        if (businessKeySyntax.ToString().Contains("CONCATENATE") || businessKeySyntax.ToString().Contains("COMPOSITE"))
                        {
                            this[(int) DataObjectMappingGridColumns.BusinessKeyDefinition, counter].Style.ForeColor = Color.DarkBlue;
                            this[(int)DataObjectMappingGridColumns.BusinessKeyDefinition, counter].Style.BackColor = Color.AliceBlue;
                        }

                        //Syntax highlighting for in source data objects.
                        var sourceDataObjectName = row.Cells[(int)DataObjectMappingGridColumns.SourceDataObject].Value.ToString();
                        if (sourceDataObjectName.StartsWith("`"))
                        {
                            this[(int)DataObjectMappingGridColumns.SourceDataObject, counter].Style.BackColor = Color.AliceBlue;

                            if (sourceDataObjectName.EndsWith("`"))
                            {
                                this[(int)DataObjectMappingGridColumns.SourceDataObject, counter].Style.ForeColor = Color.DarkBlue;
                            }
                            else
                            {
                                // Show issue.
                                this[(int)DataObjectMappingGridColumns.SourceDataObject, counter].Style.ForeColor = Color.Red;
                            }
                        }
                    }

                    counter++;
                }
            }
        }
    }
}
