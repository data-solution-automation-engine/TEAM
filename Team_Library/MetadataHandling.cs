using System.Collections.Generic;
using System.Linq;
using DataWarehouseAutomation;

namespace TEAM_Library
{
    public static class MetadataHandling
    {
        /// <summary>
        /// Definition of the allowed table types. These are used everywhere to derive approach based on conventions.
        /// </summary>
        public enum DataObjectTypes
        {
            Context,
            CoreBusinessConcept,
            NaturalBusinessRelationship,
            NaturalBusinessRelationshipContext,
            NaturalBusinessRelationshipContextDrivingKey,
            StagingArea,
            PersistentStagingArea,
            Derived,
            Presentation,
            Source,
            Unknown
        }

        /// <summary>
        /// This method returns the type (classification) of Data Object as an TableTypes enumerator based on the name and active conventions.
        /// Requires fully qualified name, or at least ignores schemas in the name.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="additionalInformation"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static DataObjectTypes GetDataObjectType(string dataObjectName, string additionalInformation, TeamConfiguration configuration)
        {
            DataObjectTypes localType;

            var presentationLayerLabelArray = Utility.SplitLabelIntoArray(configuration.PresentationLayerLabels);
            var transformationLabelArray = Utility.SplitLabelIntoArray(configuration.TransformationLabels);

            // Remove schema, if one is set
            dataObjectName = GetNonQualifiedTableName(dataObjectName);

            switch (configuration.TableNamingLocation)
            {
                // I.e. HUB_CUSTOMER
                case "Prefix" when dataObjectName.StartsWith(configuration.SatTablePrefixValue):
                    localType = DataObjectTypes.Context;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.HubTablePrefixValue):
                    localType = DataObjectTypes.CoreBusinessConcept;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.LinkTablePrefixValue):
                    localType = DataObjectTypes.NaturalBusinessRelationship;
                    break;
                case "Prefix" when (dataObjectName.StartsWith(configuration.LsatTablePrefixValue) && additionalInformation==""):
                    localType = DataObjectTypes.NaturalBusinessRelationshipContext;
                    break;
                case "Prefix" when (dataObjectName.StartsWith(configuration.LsatTablePrefixValue) && additionalInformation != ""):
                    localType = DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.StgTablePrefixValue):
                    localType = DataObjectTypes.StagingArea;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.PsaTablePrefixValue):
                    localType = DataObjectTypes.PersistentStagingArea;
                    break;
                // Presentation Layer
                case "Prefix" when presentationLayerLabelArray.Any(s => dataObjectName.StartsWith(s)):
                    localType = DataObjectTypes.Presentation;
                    break;
                // Derived or transformation
                case "Prefix" when transformationLabelArray.Any(s => dataObjectName.StartsWith(s)):
                    localType = DataObjectTypes.Derived;
                    break;
                // Source
                case "Prefix":
                    localType = DataObjectTypes.Source;
                    break;
                // Suffix
                // I.e. CUSTOMER_HUB
                case "Suffix" when dataObjectName.EndsWith(configuration.SatTablePrefixValue):
                    localType = DataObjectTypes.Context;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.HubTablePrefixValue):
                    localType = DataObjectTypes.CoreBusinessConcept;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.LinkTablePrefixValue):
                    localType = DataObjectTypes.NaturalBusinessRelationship;
                    break;
                case "Suffix" when (dataObjectName.EndsWith(configuration.LsatTablePrefixValue) && additionalInformation == ""):
                    localType = DataObjectTypes.NaturalBusinessRelationshipContext;
                    break;
                case "Suffix" when (dataObjectName.EndsWith(configuration.LsatTablePrefixValue) && additionalInformation != ""):
                    localType = DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.StgTablePrefixValue):
                    localType = DataObjectTypes.StagingArea;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.PsaTablePrefixValue):
                    localType = DataObjectTypes.PersistentStagingArea;
                    break;
                // Presentation Layer
                case "Suffix" when presentationLayerLabelArray.Any(s => dataObjectName.EndsWith(s)):
                    localType = DataObjectTypes.Presentation;
                    break;
                // Transformation / derived
                case "Suffix" when transformationLabelArray.Any(s => dataObjectName.EndsWith(s)):
                    localType = DataObjectTypes.Derived;
                    break;
                case "Suffix":
                    localType = DataObjectTypes.Source;
                    break;
                default:
                    localType = DataObjectTypes.Unknown;
                    break;
            }
            // Return the table type
            return localType;
        }

        /// <summary>
        ///   Return only the table name (without the schema), in case a fully qualified name is available (including schema etc.).
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetNonQualifiedTableName(string tableName)
        {
            string returnTableName = "";

            if (tableName.Contains('.')) // Split the string, keep the table name (remove the schema prefix)
            {
                var splitName = tableName.Split('.').ToList();
                returnTableName = splitName[1];

            }
            else // Return the default (e.g. just the table name)
            {
                returnTableName = tableName;
            }

            return returnTableName;
        }

        /// <summary>
        /// Separates the schema from the table name (if available), and returns both as individual values in a Dictionary key/value pair (key schema/ value table).
        /// If no schema is defined, the schema property from the data object will be used, otherwise the connection information will be used to determine the schema. If all else fails 'dbo' will set as default.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConnection"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFullyQualifiedDataObjectName(DataObject dataObject, TeamConnection teamConnection)
        {
            Dictionary<string, string> fullyQualifiedTableName = new Dictionary<string, string>();
            string schemaName = "";
            string returnDataObjectName = "";

            if (dataObject.Name.Contains('.')) // Split the string
            {
                var splitName = dataObject.Name.Split('.').ToList();

                schemaName = splitName[0];
                returnDataObjectName = splitName[1];

                //fullyQualifiedTableName.Add(schemaName, returnDataObjectName);

            }
            else if (!dataObject.Name.Contains('.'))
            {
                returnDataObjectName = dataObject.Name;

                var schemaExtension = dataObject.DataObjectConnection?.Extensions?.Where(x => x.Key.Equals("schema")).FirstOrDefault();

                // Keep the enxtension blank if the schema returns NULL.
                if (schemaExtension!= null && schemaExtension.Value == null)
                {
                    schemaExtension = null;
                }

                if (schemaExtension != null)
                {
                    schemaName = schemaExtension.Value;
                }
                else
                {
                    if (teamConnection is null)
                    {
                        schemaName = "dbo";
                    }
                    else
                    {
                        schemaName = teamConnection.DatabaseServer.SchemaName ?? "dbo";
                    }

                    returnDataObjectName = dataObject.Name;
                }
            }

            fullyQualifiedTableName.Add(schemaName, returnDataObjectName);
            return fullyQualifiedTableName;
        }
    }
}
