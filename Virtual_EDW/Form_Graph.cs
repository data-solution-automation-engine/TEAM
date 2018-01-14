using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using yWorks.Algorithms;
using static TEAM.Form_Base;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Orthogonal;


namespace TEAM
{
    public partial class FormManageGraph : Form_Base
    {
        public FormManageGraph(FormMain parent) : base(parent)
        {
            InitializeComponent();
            RegisterToolStripButtonCommands();

            // Configures interaction
            ConfigureInteraction();

            UpdateViewport();

            MetadataGraph();

            // Make sure the layout is handled
            morphLayout();
        }

        public IGraph Graph
        {
            get { return graphControl.Graph; }
        }


        private void ConfigureInteraction()
        {
            // Creates a new GraphEditorInputMode instance and registers it as the main
            // input mode for the graphControl
            graphControl.InputMode = new GraphEditorInputMode();
        }

        private void morphLayout()
        {
            graphControl.MorphLayout(new HierarchicLayout(), TimeSpan.FromSeconds(1), null);
        }

        class EdgeCollection
        {
            public string SourceNode { get; set; }
            public string TargetNode { get; set; }
        }

        private void UpdateViewport()
        {
            // Uncomment the following line to update the content rectangle 
            // to include all graph elements
            // This should result in correct scrolling behavior:

            //graphControl.UpdateContentRect();

            // Additionally, we can also set the zoom level so that the
            // content rectangle fits exactly into the viewport area:
            // Uncomment this line in addition to UpdateContentRect:
            // Note that this changes the zoom level (i.e. the graph elements will look smaller)

            //graphControl.FitContent();

            // The sequence above is equivalent to just calling:
            graphControl.FitGraphBounds();
        }


        private void MetadataGraph()
        {
            var connOmd = new SqlConnection { ConnectionString = _myParent.textBoxMetadataConnection.Text };

            //Build up the list of nodes
            var nodeList = new List<string>();
            var systemList = new List<string>();
            var edgeList = new Dictionary<string, EdgeCollection>();

            //Get the core list of nodes
            var sqlStatementForTableMetadata = new StringBuilder();
            sqlStatementForTableMetadata.AppendLine("SELECT [TABLE_MAPPING_HASH]");
            sqlStatementForTableMetadata.AppendLine("      ,[VERSION_ID]");
            sqlStatementForTableMetadata.AppendLine("      ,[STAGING_AREA_TABLE]");
            sqlStatementForTableMetadata.AppendLine("      ,[BUSINESS_KEY_ATTRIBUTE]");
            sqlStatementForTableMetadata.AppendLine("      ,[DRIVING_KEY_ATTRIBUTE]");
            sqlStatementForTableMetadata.AppendLine("      ,[INTEGRATION_AREA_TABLE]");
            sqlStatementForTableMetadata.AppendLine("      ,[FILTER_CRITERIA]");
            sqlStatementForTableMetadata.AppendLine("      ,[GENERATE_INDICATOR]");
            sqlStatementForTableMetadata.AppendLine("FROM [MD_TABLE_MAPPING]");

            var tableMetadataDataTable = GetDataTable(ref connOmd, sqlStatementForTableMetadata.ToString());


            // Make sure the metadata is loaded into the 'nodes' and 'edges' dictionaries
            foreach (DataRow row in tableMetadataDataTable.Rows)
            {
                // Add the Integration Layer nodes
                if (!nodeList.Contains((string)row["INTEGRATION_AREA_TABLE"]))
                {
                    nodeList.Add((string)row["INTEGRATION_AREA_TABLE"]);
                }

                // Add the Staging Layer nodes
                if (!nodeList.Contains((string)row["STAGING_AREA_TABLE"]))
                {
                    nodeList.Add((string)row["STAGING_AREA_TABLE"]);
                }

                // Add the edge to the custom dictionary
                edgeList.Add((string)row["TABLE_MAPPING_HASH"], new EdgeCollection { SourceNode = (string)row["STAGING_AREA_TABLE"], TargetNode = (string)row["INTEGRATION_AREA_TABLE"] });
            }



            // Add the nodes to the graph
            foreach (string node in nodeList)
            {
                // Creates a node in the graph
                var tempNode = Graph.CreateNode(new PointD(50, 50));
                Graph.AddLabel(tempNode, node);

                // Creates a port in the center of the node layout
                Graph.AddPort(tempNode, FreeNodePortLocationModel.NodeCenterAnchored);
            }

            // Create a dictionary of all the nodes in the graph
            var nodeDictionary = new Dictionary<string, INode>();
            foreach (INode i in Graph.Nodes)
            {
                if (!nodeDictionary.ContainsKey(i.Labels[0].Text))
                {
                    nodeDictionary.Add(i.Labels[0].Text, i);
                }
            }

            // Add the edges to the graph
            foreach (var edgeValue in edgeList.Values)
            {
                INode sourceValue;
                INode targetValue;

                nodeDictionary.TryGetValue(edgeValue.SourceNode, out sourceValue);
                nodeDictionary.TryGetValue(edgeValue.TargetNode, out targetValue);

                Graph.CreateEdge(sourceValue, targetValue);
            }






            //for (int i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
            //{
            //    DataGridViewRow row = dataGridViewTableMetadata.Rows[i];
            //    string sourceNode = row.Cells[2].Value.ToString();
            //    var systemName = sourceNode.Split('_')[1];
            //    string targetNode = row.Cells[3].Value.ToString();

            //    // Add source tables to Node List
            //    if (!nodeList.Contains(sourceNode))
            //    {
            //        nodeList.Add(sourceNode);
            //    }

            //    // Add target tables to Node List
            //    if (!nodeList.Contains(targetNode))
            //    {
            //        nodeList.Add(targetNode);
            //    }

            //    // Create a system list
            //    if (!systemList.Contains(systemName))
            //    {
            //        systemList.Add(systemName);
            //    }
            //}


            //For later, get the source/target model relationships for Hubs/Sats
            var sqlStatementForHubCategories = new StringBuilder();
            sqlStatementForHubCategories.AppendLine("SELECT ");
            sqlStatementForHubCategories.AppendLine(" [STAGING_AREA_TABLE_ID]");
            sqlStatementForHubCategories.AppendLine(",[STAGING_AREA_TABLE_NAME]");
            sqlStatementForHubCategories.AppendLine(",[STAGING_AREA_SCHEMA_NAME]");
            sqlStatementForHubCategories.AppendLine(",[FILTER_CRITERIA]");
            sqlStatementForHubCategories.AppendLine(",[SATELLITE_TABLE_ID]");
            sqlStatementForHubCategories.AppendLine(",[SATELLITE_TABLE_NAME]");
            sqlStatementForHubCategories.AppendLine(",[SATELLITE_TYPE]");
            sqlStatementForHubCategories.AppendLine(",[HUB_TABLE_ID]");
            sqlStatementForHubCategories.AppendLine(",[HUB_TABLE_NAME]");
            sqlStatementForHubCategories.AppendLine(",[BUSINESS_KEY_DEFINITION]");
            sqlStatementForHubCategories.AppendLine(",[LINK_TABLE_ID]");
            sqlStatementForHubCategories.AppendLine(",[LINK_TABLE_NAME]");
            sqlStatementForHubCategories.AppendLine("FROM [interface].[INTERFACE_STAGING_SATELLITE_XREF]");
            sqlStatementForHubCategories.AppendLine("WHERE SATELLITE_TYPE = 'Normal'");

            var modelRelationshipsHubDataTable = GetDataTable(ref connOmd, sqlStatementForHubCategories.ToString());


            //For later, get the source/target model relationships for Links and Link Satellites
            var sqlStatementForLinkCategories = new StringBuilder();
            sqlStatementForLinkCategories.AppendLine("SELECT ");
            sqlStatementForLinkCategories.AppendLine(" [STAGING_AREA_TABLE_ID]");
            sqlStatementForLinkCategories.AppendLine(",[STAGING_AREA_TABLE_NAME]");
            sqlStatementForLinkCategories.AppendLine(",[STAGING_AREA_SCHEMA_NAME]");
            sqlStatementForLinkCategories.AppendLine(",[FILTER_CRITERIA]");
            sqlStatementForLinkCategories.AppendLine(",[SATELLITE_TABLE_ID]");
            sqlStatementForLinkCategories.AppendLine(",[SATELLITE_TABLE_NAME]");
            sqlStatementForLinkCategories.AppendLine(",[SATELLITE_TYPE]");
            sqlStatementForLinkCategories.AppendLine(",[HUB_TABLE_ID]");
            sqlStatementForLinkCategories.AppendLine(",[HUB_TABLE_NAME]");
            sqlStatementForLinkCategories.AppendLine(",[BUSINESS_KEY_DEFINITION]");
            sqlStatementForLinkCategories.AppendLine(",[LINK_TABLE_ID]");
            sqlStatementForLinkCategories.AppendLine(",[LINK_TABLE_NAME]");
            sqlStatementForLinkCategories.AppendLine("FROM [interface].[INTERFACE_STAGING_SATELLITE_XREF]");
            sqlStatementForLinkCategories.AppendLine("WHERE SATELLITE_TYPE = 'Link Satellite'");

            var modelRelationshipsLinksDataTable = GetDataTable(ref connOmd, sqlStatementForLinkCategories.ToString());


            //Create the relationships between business concepts (Hubs, Links)
            var sqlStatementForRelationships = new StringBuilder();
            sqlStatementForRelationships.AppendLine("SELECT ");
            sqlStatementForRelationships.AppendLine(" [LINK_TABLE_ID]");
            sqlStatementForRelationships.AppendLine(",[LINK_TABLE_NAME]");
            sqlStatementForRelationships.AppendLine(",[STAGING_AREA_TABLE_ID]");
            sqlStatementForRelationships.AppendLine(",[STAGING_AREA_TABLE_NAME]");
            sqlStatementForRelationships.AppendLine(",[STAGING_AREA_SCHEMA_NAME]");
            sqlStatementForRelationships.AppendLine(",[HUB_TABLE_ID]");
            sqlStatementForRelationships.AppendLine(",[HUB_TABLE_NAME]");
            sqlStatementForRelationships.AppendLine(",[BUSINESS_KEY_DEFINITION]");
            sqlStatementForRelationships.AppendLine("FROM [interface].[INTERFACE_HUB_LINK_XREF]");

            var businessConceptsRelationships = GetDataTable(ref connOmd, sqlStatementForRelationships.ToString());


            //Make sure the source-to-target mappings are created for the attributes (STG->SAT)
            var sqlStatementForSatelliteAttributes = new StringBuilder();
            sqlStatementForSatelliteAttributes.AppendLine("SELECT ");
            sqlStatementForSatelliteAttributes.AppendLine(" [STAGING_AREA_TABLE_ID]");
            sqlStatementForSatelliteAttributes.AppendLine(",[STAGING_AREA_TABLE_NAME]");
            sqlStatementForSatelliteAttributes.AppendLine(",[STAGING_AREA_SCHEMA_NAME]");
            sqlStatementForSatelliteAttributes.AppendLine(",[SATELLITE_TABLE_ID]");
            sqlStatementForSatelliteAttributes.AppendLine(",[SATELLITE_TABLE_NAME]");
            sqlStatementForSatelliteAttributes.AppendLine(",[ATTRIBUTE_ID_FROM]");
            sqlStatementForSatelliteAttributes.AppendLine(",[ATTRIBUTE_NAME_FROM]");
            sqlStatementForSatelliteAttributes.AppendLine(",[ATTRIBUTE_ID_TO]");
            sqlStatementForSatelliteAttributes.AppendLine(",[ATTRIBUTE_NAME_TO]");
            sqlStatementForSatelliteAttributes.AppendLine(",[MULTI_ACTIVE_KEY_INDICATOR]");
            sqlStatementForSatelliteAttributes.AppendLine("FROM [interface].[INTERFACE_STAGING_SATELLITE_ATTRIBUTE_XREF]");

            var satelliteAttributes = GetDataTable(ref connOmd, sqlStatementForSatelliteAttributes.ToString());


            //Create a list of segments to create, based on nodes (Hubs and Sats)
            List<string> segmentNodeList = new List<string>();

            foreach (DataRow row in modelRelationshipsHubDataTable.Rows)
            {
                var modelRelationshipsHub = (string)row["HUB_TABLE_NAME"];

                if (!segmentNodeList.Contains(modelRelationshipsHub))
                {
                    segmentNodeList.Add(modelRelationshipsHub);
                }
            }

            // ... and the Links / LSATs
            foreach (DataRow row in modelRelationshipsLinksDataTable.Rows)
            {
                var modelRelationshipsLink = (string)row["LINK_TABLE_NAME"];

                if (!segmentNodeList.Contains(modelRelationshipsLink))
                {
                    segmentNodeList.Add(modelRelationshipsLink);
                }
            }

            // ... and for any orphan Hubs or Links (without Satellites)
            foreach (DataRow row in businessConceptsRelationships.Rows)
            {
                var modelRelationshipsLink = (string)row["LINK_TABLE_NAME"];
                var modelRelationshipsHub = (string)row["HUB_TABLE_NAME"];

                if (!segmentNodeList.Contains(modelRelationshipsLink))
                {
                    segmentNodeList.Add(modelRelationshipsLink);
                }

                if (!segmentNodeList.Contains(modelRelationshipsHub))
                {
                    segmentNodeList.Add(modelRelationshipsHub);
                }
            }

            foreach (string node in segmentNodeList)
            {
                //if (node.Contains("STG_"))
                //{
                //    dgmlExtract.AppendLine("    <Node Id=\"" + node + "\"  Category=\"Source System\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                //}
                //else if (node.Contains("HUB_"))
                //{
                //    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Hub\"  Label=\"" + node + "\" />");
                //}
                //else if (node.Contains("LNK_"))
                //{
                //    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Link\" Label=\"" + node + "\" />");
                //}
                //else if (node.Contains("SAT_") || node.Contains("LSAT_"))
                //{
                //    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Satellite\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                //}
                //else // The others
                //{
                //    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Unknown\" Label=\"" + node + "\" />");
                //}
            }



            //Write the nodes to DGML
            var dgmlExtract = new StringBuilder();


            foreach (string node in nodeList)
            {
               // var tempNode = Graph.CreateNode(new PointD(50, 50));
               // Graph.AddLabel(tempNode, node);
                //if (node.Contains("STG_"))
                //{
                //    dgmlExtract.AppendLine("    <Node Id=\"" + node + "\"  Category=\"Source System\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                //}
                //else if (node.Contains("HUB_"))
                //{
                //    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Hub\"  Label=\"" + node + "\" />");
                //}
                //else if (node.Contains("LNK_"))
                //{
                //    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Link\" Label=\"" + node + "\" />");
                //}
                //else if (node.Contains("SAT_") || node.Contains("LSAT_"))
                //{
                //    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Satellite\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                //}
                //else // The others
                //{
                //    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Unknown\" Label=\"" + node + "\" />");
                //}
            }

            // Separate routine for attribute nodes, with some additional logic to allow for 'duplicate' nodes e.g. source and target attribute names
            foreach (DataRow row in satelliteAttributes.Rows)
            {
                var sourceNodeLabel = (string)row["ATTRIBUTE_NAME_FROM"];
                var sourceNode = "staging_" + sourceNodeLabel;
                var targetNodeLabel = (string)row["ATTRIBUTE_NAME_TO"];
                var targetNode = "dwh_" + targetNodeLabel;

                // Add source tables to Node List
                if (!nodeList.Contains(sourceNode))
                {
                    nodeList.Add(sourceNode);
                }

                // Add target tables to Node List
                if (!nodeList.Contains(targetNode))
                {
                    nodeList.Add(targetNode);
                }

                dgmlExtract.AppendLine("     <Node Id=\"" + sourceNode + "\"  Category=\"Unknown\" Label=\"" + sourceNodeLabel + "\" />");
                dgmlExtract.AppendLine("     <Node Id=\"" + targetNode + "\"  Category=\"Unknown\" Label=\"" + targetNodeLabel + "\" />");
            }




            //Adding the category nodes
            dgmlExtract.AppendLine("    <Node Id=\"Staging Area\" Group=\"Expanded\" Label=\"Staging Area\"/>");
            dgmlExtract.AppendLine("    <Node Id=\"Data Vault\" Group=\"Expanded\" Label=\"Data Vault\"/>");

            //Adding the source system containers as nodes
            foreach (var node in systemList)
            {
                dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Group=\"Expanded\" Category=\"Source System\" Label=\"" + node + "\" />");
            }

            //Adding the CBC nodes (Hubs and Links)
            foreach (string node in segmentNodeList)
            {
                string segmentName = node.Remove(0, 4).ToLower();
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                segmentName = textInfo.ToTitleCase(segmentName);

                dgmlExtract.AppendLine("    <Node Id=\"" + segmentName + "\" Group=\"Expanded\" Label=\"" + segmentName + "\" IsHubContainer=\"True\" />");
            }

            dgmlExtract.AppendLine("  </Nodes>");
            //End of Nodes

            //Edges and containers
            dgmlExtract.AppendLine("  <Links>");

            //for (var i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
            //{
            //    var row = dataGridViewTableMetadata.Rows[i];
            //    var sourceNode = row.Cells[2].Value.ToString();
            //    var targetNode = row.Cells[3].Value.ToString();
            //    var businessKey = row.Cells[4].Value.ToString();

            //    dgmlExtract.AppendLine("    <Link Source=\"" + sourceNode + "\" Target=\"" + targetNode + "\" BusinessKeyDefintion=\"" + businessKey + "\"/>");
            //}

            //Add container groupings (node-based) - adding source system containers to 'staging area'
            foreach (var node in systemList)
            {
                dgmlExtract.AppendLine("     <Link Source=\"Staging Area\" Target=\"" + node + "\" Category=\"Contains\" />");
            }

            //// Adding the staging area table to the source system container
            //for (var i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
            //{
            //    var row = dataGridViewTableMetadata.Rows[i];
            //    var node = row.Cells[2].Value.ToString();
            //    var systemName = node.Split('_')[1];

            //    if (node.Contains("STG_"))
            //    {
            //        dgmlExtract.AppendLine("    <Link Source=\"" + systemName + "\" Target=\"" + node + "\" Category=\"Contains\" />");
            //    }
            //}

            // Separate routine to create STG/ATT and SAT/ATT relationships
            foreach (DataRow row in satelliteAttributes.Rows)
            {
                var sourceNodeSat = (string)row["SATELLITE_TABLE_NAME"];
                var targetNodeSat = "dwh_" + (string)row["ATTRIBUTE_NAME_TO"];
                var sourceNodeStg = (string)row["STAGING_AREA_TABLE_NAME"];
                var targetNodeStg = "staging_" + (string)row["ATTRIBUTE_NAME_FROM"];

                // This is adding the attributes to the tables
                dgmlExtract.AppendLine("    <Link Source=\"" + sourceNodeSat + "\" Target=\"" + targetNodeSat + "\" Category=\"Contains\" />");
                dgmlExtract.AppendLine("    <Link Source=\"" + sourceNodeStg + "\" Target=\"" + targetNodeStg + "\" Category=\"Contains\" />");

                // This is adding the edge between the attributes
                dgmlExtract.AppendLine("    <Link Source=\"" + targetNodeStg + "\" Target=\"" + targetNodeSat + "\" />");
            }

            //Add Data Vault objects to segment
            foreach (var node in segmentNodeList)
            {
                var segmentName = node.Remove(0, 4).ToLower();
                var textInfo = new CultureInfo("en-US", false).TextInfo;
                segmentName = textInfo.ToTitleCase(segmentName);
                // <Link Source="Renewal_Membership" Target="LNK_RENEWAL_MEMBERSHIP" Category="Contains" />
                dgmlExtract.AppendLine("    <Link Source=\"" + segmentName + "\" Target=\"" + node + "\" Category=\"Contains\" />");
                dgmlExtract.AppendLine("    <Link Source=\"Data Vault\" Target=\"" + segmentName + "\" Category=\"Contains\" />");
            }

            //Add groupings to a Hub (CBC), if there is a Satellite
            foreach (DataRow row in modelRelationshipsHubDataTable.Rows)
            {
                if (row["SATELLITE_TABLE_NAME"] == DBNull.Value || row["HUB_TABLE_NAME"] == DBNull.Value)
                    continue;
                var modelRelationshipsHub = (string)row["HUB_TABLE_NAME"];
                var modelRelationshipsSat = (string)row["SATELLITE_TABLE_NAME"];

                var segmentName = modelRelationshipsHub.Remove(0, 4).ToLower();
                var textInfo = new CultureInfo("en-US", false).TextInfo;
                segmentName = textInfo.ToTitleCase(segmentName);

                //Map the Satellite to the Hub and CBC
                dgmlExtract.AppendLine("    <Link Source=\"" + segmentName + "\" Target=\"" +
                                       modelRelationshipsSat + "\" Category=\"Contains\" />");
                dgmlExtract.AppendLine("    <Link Source=\"" + modelRelationshipsHub +
                                       "\" Target=\"" + modelRelationshipsSat + "\" />");
            }

            //Add groupings per Link (CBC), if there is a Satellite
            foreach (DataRow row in modelRelationshipsLinksDataTable.Rows)
            {
                if (row["SATELLITE_TABLE_NAME"] == DBNull.Value || row["LINK_TABLE_NAME"] == DBNull.Value)
                    continue;
                var modelRelationshipsLink = (string)row["LINK_TABLE_NAME"];
                var modelRelationshipsSat = (string)row["SATELLITE_TABLE_NAME"];

                var segmentName = modelRelationshipsLink.Remove(0, 4).ToLower();
                var textInfo = new CultureInfo("en-US", false).TextInfo;
                segmentName = textInfo.ToTitleCase(segmentName);

                //Map the Satellite to the Link and CBC
                dgmlExtract.AppendLine("    <Link Source=\"" + segmentName + "\" Target=\"" + modelRelationshipsSat + "\" Category=\"Contains\" />");
                dgmlExtract.AppendLine("    <Link Source=\"" + modelRelationshipsLink + "\" Target=\"" + modelRelationshipsSat + "\" />");
            }



            //Add the relationships between groupings (core business concepts) - from Hub to Link
            foreach (DataRow row in businessConceptsRelationships.Rows)
            {
                if (row["HUB_TABLE_NAME"] == DBNull.Value || row["LINK_TABLE_NAME"] == DBNull.Value)
                    continue;
                var modelRelationshipsHub = (string)row["HUB_TABLE_NAME"];
                var modelRelationshipsLink = (string)row["LINK_TABLE_NAME"];

                var segmentNameFrom = modelRelationshipsHub.Remove(0, 4).ToLower();
                var textInfoFrom = new CultureInfo("en-US", false).TextInfo;
                segmentNameFrom = textInfoFrom.ToTitleCase(segmentNameFrom);

                var segmentNameTo = modelRelationshipsLink.Remove(0, 4).ToLower();
                var textInfoTo = new CultureInfo("en-US", false).TextInfo;
                segmentNameTo = textInfoTo.ToTitleCase(segmentNameTo);

                dgmlExtract.AppendLine("    <Link Source=\"" + segmentNameFrom + "\" Target=\"" + segmentNameTo + "\" />");
            }

            dgmlExtract.AppendLine("  </Links>");

            //Add containers
            dgmlExtract.AppendLine("  <Categories>");
            dgmlExtract.AppendLine("    <Category Id = \"Source System\" Label = \"Source System\" Background = \"#FFE51400\" IsTag = \"True\" /> ");
            dgmlExtract.AppendLine("    <Category Id = \"Hub\" Label = \"Hub\" IsTag = \"True\" /> ");
            dgmlExtract.AppendLine("    <Category Id = \"Link\" Label = \"Link\" IsTag = \"True\" /> ");
            dgmlExtract.AppendLine("    <Category Id = \"Satellite\" Label = \"Satellite\" IsTag = \"True\" /> ");
            dgmlExtract.AppendLine("  </Categories>");

            //Add styles 
            dgmlExtract.AppendLine("  <Styles >");

            dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Source System\" ValueLabel = \"Has category\" >");
            dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Source System')\" />");
            dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
            dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FF6E6A69\" />");
            dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
            dgmlExtract.AppendLine("    </Style >");

            dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Hub\" ValueLabel = \"Has category\" >");
            dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Hub')\" />");
            dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
            dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FF6495ED\" />");
            dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
            dgmlExtract.AppendLine("    </Style >");

            dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Link\" ValueLabel = \"Has category\" >");
            dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Link')\" />");
            dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
            dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFB22222\" />");
            dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
            dgmlExtract.AppendLine("    </Style >");

            dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Satellite\" ValueLabel = \"Has category\" >");
            dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Satellite')\" />");
            dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
            dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFC0A000\" />");
            dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
            dgmlExtract.AppendLine("    </Style >");

            dgmlExtract.AppendLine("  </Styles >");



            dgmlExtract.AppendLine("</DirectedGraph>");

        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void RegisterToolStripButtonCommands()
        {
            // Register commands to buttons using the convinence extension method
            zoomInButton.SetCommand(Commands.IncreaseZoom, graphControl);
            zoomOutButton.SetCommand(Commands.DecreaseZoom, graphControl);
            fitContentButton.SetCommand(Commands.FitContent, graphControl);
        }


        private void closeToolStripMenuItem_Click_2(object sender, EventArgs e)
        {
            Close();
        }

        private void graphControl_Click(object sender, EventArgs e)
        {

        }
    }
}
