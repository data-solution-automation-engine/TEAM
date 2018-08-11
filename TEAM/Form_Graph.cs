using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Text;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Layout.Hierarchic;
using yWorks.Layout.Orthogonal;
using yWorks.Graph.Styles;
using yWorks.Layout.Organic;
using yWorks.Layout.Radial;
using yWorks.Layout.Tree;
using yWorks.Graph.LabelModels;

namespace TEAM
{
    public partial class FormManageGraph : FormBase
    {
        public FormManageGraph(FormMain parent) : base(parent)
        {
            InitializeComponent();
            RegisterToolStripButtonCommands();

            // Configures default styles for newly created graph elements
            SetDefaultStyles();

            // Enables and configures
            EnableFolding();

            // Configures interaction
            ConfigureInteraction();

            // Set form controls (zoom in, out etc.)
            UpdateViewport();



            SetDefaultLabelParameters();

            // Generate Graph
            MetadataGraph();

            // Make sure the layout is handled
            MorphLayout("Organic");
        }

        private IGraph Graph
        {
            get { return graphControl.Graph; }
        }

        private INode CreateGroupNodes(string groupNodeLabel, INode[] childNodes)
        {
            //Creates a group node that encloses the given child nodes
            INode groupNode = Graph.GroupNodes(childNodes);

            // Creates a label for the group node
            Graph.AddLabel(groupNode, groupNodeLabel);

            // Adjusts the layout of the group nodes
            Graph.AdjustGroupNodeLayout(groupNode);
            return groupNode;
        }

        private void SetDefaultStyles()
        {
            // Sets the default style for nodes
            INodeStyle defaultNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidBrush(Color.LightGray) };

            // Sets this style as the default for all nodes that don't have another
            // style assigned explicitly
            Graph.NodeDefaults.Style = defaultNodeStyle;

            // Sets the default style for edges:
            // Creates an edge style that will apply a gray pen with thickness 1
            // to the entire line using PolyLineEdgeStyle,
            // which draws a polyline determined by the edge's control points (bends)
            var defaultEdgeStyle = new PolylineEdgeStyle { Pen = Pens.Gray };

            // Sets the source and target arrows on the edge style instance
            // (Actually: no source arrow)
            // Note that IEdgeStyle itself does not have these properties
            // Also note that by default there are no arrows
            defaultEdgeStyle.TargetArrow = Arrows.Default;


            // Sets the defined edge style as the default for all edges that don't have
            Graph.EdgeDefaults.Style = defaultEdgeStyle;    

            // Sets the default style for labels
            ILabelStyle defaultEdgeLabelStyle = new DefaultLabelStyle { Font = new Font("Segoe UI", 9), TextBrush = Brushes.Black };
            ILabelStyle defaultNodeLabelStyle = new DefaultLabelStyle { Font = new Font("Segoe UI", 9, FontStyle.Bold), TextBrush = Brushes.Black };

            // Sets the defined style as the default for both edge and node labels:
            Graph.EdgeDefaults.Labels.Style = defaultEdgeLabelStyle;
            Graph.NodeDefaults.Labels.Style = defaultNodeLabelStyle;

            // Sets the default size explicitly (Width, Height) 
            Graph.NodeDefaults.Size = new SizeD(400, 40);


            // GROUP NODES
            // PanelNodeStyle is a style especially suited to group nodes
            // Creates a panel with a light blue background
            Color groupNodeColor = Color.FromArgb(255, 214, 229, 248);
            var groupNodeDefaults = Graph.GroupNodeDefaults;
            groupNodeDefaults.Style = new PanelNodeStyle
            {
                Color = groupNodeColor,
                // Specifies insets that provide space for a label at the top
                // For a solution how to determine these insets automatically, please
                // see the yEd.NET demo application.
                Insets = new InsetsD(5, 18, 5, 5),
                LabelInsetsColor = groupNodeColor,
            };

            // Sets a label style with right-aligned text
            groupNodeDefaults.Labels.Style = new DefaultLabelStyle
            {
                StringFormat = { Alignment = StringAlignment.Far }
            };

            // Places the label at the top inside of the panel.
            // For PanelNodeStyle, InteriorStretchLabelModel is usually the most appropriate label model
            groupNodeDefaults.Labels.LayoutParameter = InteriorStretchLabelModel.North;

            // Sets the default size explicitly (Width, Height) 
            Graph.GroupNodeDefaults.Size = new SizeD(100, 100);
        }


        private FoldingManager _manager;

        // Enables folding - changes the GraphControl's graph to a managed view that provides the actual collapse/expand state.
        private void EnableFolding()
        {
            // Creates the folding manager and sets its master graph to the single graph there is now
            _manager = new FoldingManager(Graph);

            // Creates a managed view from the master graph and replaces the existing graph view with a managed view
            graphControl.Graph = _manager.CreateFoldingView().Graph;
            WrapGroupNodeStyles();
        }

        // Changes the default style for group nodes (the ones that can be folded)
        private void WrapGroupNodeStyles()
        {
            IFoldingView foldingView = Graph.GetFoldingView();
            if (foldingView != null)
            {
                //Wrap the style with CollapsibleNodeStyleDecorator
                foldingView.Graph.GroupNodeDefaults.Style =
                  new CollapsibleNodeStyleDecorator(foldingView.Graph.GroupNodeDefaults.Style);
            }
        }

        private void ConfigureInteraction()
        {
            // Creates a new GraphEditorInputMode instance and registers it as the main
            // input mode for the graphControl
            graphControl.InputMode = new GraphEditorInputMode();
        }

        private void MorphLayout(string layoutParameter)
        {
            if (layoutParameter == "Radial")
            {
                graphControl.MorphLayout(new RadialLayout(), TimeSpan.FromSeconds(1), null);
            }
            else if (layoutParameter == "Orthogonal")
            {
                graphControl.MorphLayout(new OrthogonalLayout(), TimeSpan.FromSeconds(1), null);
            }
            else if (layoutParameter == "Hierarchic")
            {
                graphControl.MorphLayout(new HierarchicLayout(), TimeSpan.FromSeconds(1), null);
            }
            else if (layoutParameter == "Organic")
            {
                graphControl.MorphLayout(new OrganicLayout(), TimeSpan.FromSeconds(1), null);
            }
            else if (layoutParameter == "Tree")
            {
                graphControl.MorphLayout(new TreeLayout(), TimeSpan.FromSeconds(1), null);
            }
            else // Default
            {
                //No layout
            }
        }

        class EdgeCollection
        {
            public string SourceNode { get; set; }
            public string TargetNode { get; set; }
            public string BusinessKeyDefinition { get; set;}
        }

        private void UpdateViewport()
        {            
            graphControl.FitGraphBounds();
        }

        private void SetDefaultLabelParameters()
        {
            // For node labels, the default is a label position at the node center. This code below sets the default explicitly.
            Graph.NodeDefaults.Labels.LayoutParameter = InteriorLabelModel.Center;

            // For edge labels the default is a label that is rotated to match the associated edge segment
            // We'll start by creating a model that is similar to the default:
            EdgeSegmentLabelModel edgeSegmentLabelModel = new EdgeSegmentLabelModel();
            // However, by default, the rotated label is centered on the edge path.
            // Let's move the label off of the path:
            edgeSegmentLabelModel.Distance = 5;
            // Finally, we can set this label model as the default for edge labels using a location at the center of the first segment
            Graph.EdgeDefaults.Labels.LayoutParameter = edgeSegmentLabelModel.CreateParameterFromSource(0, 0.5, EdgeSides.RightOfEdge);

        }

        private void MetadataGraph()
        {
            var configurationSettings = new ConfigurationSettings();
            var connOmd = new SqlConnection { ConnectionString = configurationSettings.ConnectionStringOmd };

            var systemList = new List<string>(); // To create the groups (per system)
            var nodeDictionary = new Dictionary<string, string>(); // To create the nodes and add them to a parent node (group)
            var edgeDictionary = new Dictionary<string, EdgeCollection>(); // To create the edges / mappings from source to target

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


            // Make sure the metadata is loaded into the 'systems', 'nodes' and 'edges' dictionaries
            foreach (DataRow row in tableMetadataDataTable.Rows)
            {
                //Create the list of systems (containers)
                var systemName = row["STAGING_AREA_TABLE"].ToString().Split('_')[1];
                if (!systemList.Contains(systemName))
                {
                    systemList.Add(systemName);
                }


                // Add the Integration Layer nodes
                if (!nodeDictionary.ContainsKey((string)row["INTEGRATION_AREA_TABLE"]))
                {
                    nodeDictionary.Add((string)row["INTEGRATION_AREA_TABLE"], systemName);
                }

                //Add the Staging Layer nodes
                if (!nodeDictionary.ContainsKey((string)row["STAGING_AREA_TABLE"]))
                {
                    nodeDictionary.Add((string)row["STAGING_AREA_TABLE"], systemName);
                }

                // Add the edge to the custom dictionary
                //if (edgeDictionary.All((e => e.Value.TargetNode != "")))
                if (!edgeDictionary.ContainsKey((string)row["TABLE_MAPPING_HASH"]))
                {
                    edgeDictionary.Add((string)row["TABLE_MAPPING_HASH"], new EdgeCollection { SourceNode = (string)row["STAGING_AREA_TABLE"], TargetNode = (string)row["INTEGRATION_AREA_TABLE"], BusinessKeyDefinition = (string)row["BUSINESS_KEY_ATTRIBUTE"] });
                }
            }

            // Create the group nodes (systems)
            foreach (string system in systemList)
            {
                INode groupNode = Graph.GroupNodes();

                // Creates a label for the group node
                Graph.AddLabel(groupNode, system);
                Graph.AdjustGroupNodeLayout(groupNode);
            }

            // Add the nodes to the graph
            foreach (var nodeDict in nodeDictionary)
            {
                // Creates a node in the graph
                var tempNode = Graph.CreateNode(new PointD(50, 50));

                // Add the label (name) to the node
                Graph.AddLabel(tempNode, nodeDict.Key);

                // Creates a port in the center of the node layout
                Graph.AddPort(tempNode, FreeNodePortLocationModel.NodeCenterAnchored);

                // Override the default style
                INodeStyle hubNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidBrush(Color.LightBlue) };
                INodeStyle satNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidBrush(Color.LightYellow) };
                INodeStyle lnkNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidBrush(Color.Red) };
                if (nodeDict.Key.StartsWith("HUB_"))
                {
                    Graph.SetStyle(tempNode, hubNodeStyle);
                }
                if (nodeDict.Key.StartsWith("SAT_") || nodeDict.Key.StartsWith("LSAT_"))
                {
                    Graph.SetStyle(tempNode, satNodeStyle);
                }
                if (nodeDict.Key.StartsWith("LNK_"))
                {
                    Graph.SetStyle(tempNode, lnkNodeStyle);
                }
            }

            // Create a dictionary of all the nodes in the graph
            var graphNodeDictionary = new Dictionary<string, INode>();
            foreach (INode i in Graph.Nodes)
            {
                if (!graphNodeDictionary.ContainsKey(i.Labels[0].Text))
                {
                    graphNodeDictionary.Add(i.Labels[0].Text, i);
                }
            }

            // Make sure the nodes are placed in the appropriate group nodes (system nodes)
            foreach (var node in graphNodeDictionary)
            {
                //Check whether current node isn't a system node
                if (!Graph.IsGroupNode(node.Value))
                {
                    //Check whether parent node is known
                    if (nodeDictionary.ContainsKey(node.Key))
                    {
                        // Retrieve the parent for the given Node
                        INode parentNode;
                        graphNodeDictionary.TryGetValue(nodeDictionary[node.Key], out parentNode);

                        // Set parent node
                        Graph.SetParent(node.Value, parentNode);
                    }
                }
            }

            // Add the edges to the graph
            foreach (var edgeValue in edgeDictionary.Values)
            {
                INode sourceValue;
                INode targetValue;

                graphNodeDictionary.TryGetValue(edgeValue.SourceNode, out sourceValue);
                graphNodeDictionary.TryGetValue(edgeValue.TargetNode, out targetValue);

                var newEdge = Graph.CreateEdge(sourceValue, targetValue);
                // Add the label to the Edge
                Graph.AddLabel(newEdge, edgeValue.BusinessKeyDefinition);
            }





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

            if (businessConceptsRelationships.Rows.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("In development - businessConceptsRelationships datatable is empty.");
            }
            else
            {
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
            }


            // Separate routine for attribute nodes, with some additional logic to allow for 'duplicate' nodes e.g. source and target attribute names
            foreach (DataRow row in satelliteAttributes.Rows)
            {
                var sourceNodeLabel = (string)row["ATTRIBUTE_NAME_FROM"];
                var sourceNode = "staging_" + sourceNodeLabel;
                var targetNodeLabel = (string)row["ATTRIBUTE_NAME_TO"];
                var targetNode = "dwh_" + targetNodeLabel;

                //// Add source tables to Node List
                //if (!nodeList.Contains(sourceNode))
                //{
                //    nodeList.Add(sourceNode);
                //}

                //// Add target tables to Node List
                //if (!nodeList.Contains(targetNode))
                //{
                //    nodeList.Add(targetNode);
                //}

            }

            //Adding the CBC nodes (Hubs and Links)
            foreach (string node in segmentNodeList)
            {
                string segmentName = node.Remove(0, 4).ToLower();
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                textInfo.ToTitleCase(segmentName);

          }

   


            // Separate routine to create STG/ATT and SAT/ATT relationships
            foreach (DataRow row in satelliteAttributes.Rows)
            {
                var sourceNodeSat = (string)row["SATELLITE_TABLE_NAME"];
                var targetNodeSat = "dwh_" + (string)row["ATTRIBUTE_NAME_TO"];
                var sourceNodeStg = (string)row["STAGING_AREA_TABLE_NAME"];
                var targetNodeStg = "staging_" + (string)row["ATTRIBUTE_NAME_FROM"];

                // This is adding the attributes to the tables

            }

            //Add Data Vault objects to segment
            foreach (var node in segmentNodeList)
            {
                var segmentName = node.Remove(0, 4).ToLower();
                var textInfo = new CultureInfo("en-US", false).TextInfo;
                textInfo.ToTitleCase(segmentName);
                // <Link Source="Renewal_Membership" Target="LNK_RENEWAL_MEMBERSHIP" Category="Contains" />
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
                textInfo.ToTitleCase(segmentName);

                //Map the Satellite to the Hub and CBC

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
                textInfo.ToTitleCase(segmentName);

                //Map the Satellite to the Link and CBC
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
                textInfoFrom.ToTitleCase(segmentNameFrom);

                var segmentNameTo = modelRelationshipsLink.Remove(0, 4).ToLower();
                var textInfoTo = new CultureInfo("en-US", false).TextInfo;
                textInfoTo.ToTitleCase(segmentNameTo);

            }
        }


        private void RegisterToolStripButtonCommands()
        {
            // Register commands to buttons using the convinence extension method
            zoomInButton.SetCommand(Commands.IncreaseZoom, graphControl);
            zoomOutButton.SetCommand(Commands.DecreaseZoom, graphControl);
            fitContentButton.SetCommand(Commands.FitContent, graphControl);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void radialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MorphLayout("Radial");
        }

        private void orthogonalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MorphLayout("Orthogonal");
        }

        private void organicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MorphLayout("Organic");
        }

        private void hierarchicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MorphLayout("Hierarchic");
        }
    }
}
