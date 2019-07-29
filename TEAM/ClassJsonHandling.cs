using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TEAM
{

    internal class ClassJsonHandling
    {

        /// <summary>
        ///    Method to create a new dummy JSON file in the designated working directory.
        /// </summary>
        /// <param name="fileType"></param>
        internal static void CreateDummyJsonFile(string fileType)
        {
            var jsonVersionExtension = @"_v" + FormBase.GlobalParameters.currentVersionId + ".json";

            JArray outputFileArray = new JArray();
            JObject dummyJsonFile = new JObject();

            if (fileType == FormBase.GlobalParameters.JsonTableMappingFileName) // Table Mapping
            {
                dummyJsonFile = new JObject(
                    new JProperty("tableMappingHash", "NewHash"),
                    new JProperty("versionId", "versionId"),
                    new JProperty("sourceTable", "STG_EXAMPLESYSTEM_EXAMPLETABLE"),
                    new JProperty("targetTable", "HUB_EXAMPLE"),
                    new JProperty("businessKeyDefinition", "EXAMPLE"),
                    new JProperty("drivingKeyDefinition", ""),
                    new JProperty("filterCriteria", ""),
                    new JProperty("processIndicator", "Y")
                );
            } else if (fileType == FormBase.GlobalParameters.JsonModelMetadataFileName) // Physical Model
            {
                dummyJsonFile = new JObject(
                    new JProperty("versionAttributeHash", "NewHash"),
                    new JProperty("versionId", "versionId"),
                    new JProperty("schemaName", "[dbo]"),
                    new JProperty("tableName", "Sample Table"),
                    new JProperty("columnName", "Sample Column"),
                    new JProperty("dataType", "nvarchar"),
                    new JProperty("characterMaximumLength", "100"),
                    new JProperty("numericPrecision", "0"),
                    new JProperty("ordinalPosition", "1"),
                    new JProperty("primaryKeyIndicator", "N"),
                    new JProperty("multiActiveIndicator", "N")
                );
            } else if (fileType == FormBase.GlobalParameters.JsonAttributeMappingFileName) // Attribute Mapping
            {
                dummyJsonFile = new JObject(
                    new JProperty("attributeMappingHash", "NewHash"),
                    new JProperty("versionId", "versionId"),
                    new JProperty("sourceTable", "SOURCE_TABLE"),
                    new JProperty("sourceAttribute", "EXAMPLE_FROM_ATTRIBUTE"),
                    new JProperty("targetTable", "TARGET_TABLE"),
                    new JProperty("targetAttribute", "EXAMPLE_TO_ATTRIBUTE"),
                    new JProperty("transformationRule", "")
                );
            }
            else
            {
                // No action - issue in code
            }

            // Spool to disk
            outputFileArray.Add(dummyJsonFile);
            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);
            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileType + jsonVersionExtension, json);
        }


        /// <summary>
        ///   Create a completely empty file, for the purposes of clearing a version / existing data.
        /// </summary>
        /// <param name="fileType"></param>
        internal static void CreatePlaceholderJsonFile(string fileType)
        {
            var jsonVersionExtension = @"_v" + FormBase.GlobalParameters.currentVersionId + ".json";

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileType + jsonVersionExtension, "");
        }


        /// <summary>
        ///    Create a backup of a given JSON file.
        /// </summary>
        /// <param name="inputFileName"></param>
        internal string BackupJsonFile(string inputFileName)
        {
            string result;
            var shortDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
            var targetFilePathName = FormBase.GlobalParameters.ConfigurationPath + string.Concat("Backup_" + shortDatetime + "_", inputFileName);

            File.Copy(FormBase.GlobalParameters.ConfigurationPath + inputFileName, targetFilePathName);
            result = targetFilePathName;

            return result;
        }

        /// <summary>
        ///   Clear out (remove) an existing Json file, to facilitate overwriting.
        /// </summary>
        internal static void RemoveExistingJsonFile(string inputFileName)
        {
            File.Delete(FormBase.GlobalParameters.ConfigurationPath + inputFileName);
        }

        /// <summary>
        ///   Saves the Business Key Component data set to disk as a JSON file in the default configuration directory
        /// </summary>
        internal static void SaveJsonInterfaceBusinessKeyComponent()
        {

            const string fileName = "interfaceBusinessKeyComponent";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_BUSINESS_KEY_COMPONENT]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());


            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[SOURCE_NAME] ASC, [HUB_NAME] ASC, [BUSINESS_KEY_COMPONENT_ID] ASC, [BUSINESS_KEY_COMPONENT_ORDER] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    sourceId = singleRow[0].ToString(),
                    sourceName = singleRow[1].ToString(),
                    sourceSchemaName = singleRow[2].ToString(),
                    hubId = singleRow[3].ToString(),
                    hubName = singleRow[4].ToString(),
                    businessKeyDefinition = singleRow[5].ToString(),
                    businessKeyComponentId = singleRow[6].ToString(),
                    businessKeyComponentOrder = singleRow[7].ToString(),
                    businessKeyComponentValue = singleRow[8].ToString()
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        /// <summary>
        ///   Saves the Business Key Component Part data set to disk as a JSON file in the default configuration directory
        /// </summary>
        internal static void SaveJsonInterfaceBusinessKeyComponentPart()
        {

            const string fileName = "interfaceBusinessKeyComponentPart";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_BUSINESS_KEY_COMPONENT_PART]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());


            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[SOURCE_NAME] ASC, [HUB_NAME] ASC, [BUSINESS_KEY_COMPONENT_ID] ASC, [BUSINESS_KEY_COMPONENT_ORDER] ASC, [BUSINESS_KEY_COMPONENT_ELEMENT_ID] ASC, [BUSINESS_KEY_COMPONENT_ELEMENT_ORDER] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    sourceId = singleRow[0].ToString(),
                    sourceName = singleRow[1].ToString(),
                    hubId = singleRow[2].ToString(),
                    hubName = singleRow[3].ToString(),
                    businessKeyDefinition = singleRow[4].ToString(),
                    businessKeyComponentId = singleRow[5].ToString(),
                    businessKeyComponentOrder = singleRow[6].ToString(),
                    businessKeyComponentElementId = singleRow[7].ToString(),
                    businessKeyComponentElementOrder = singleRow[8].ToString(),
                    businessKeyComponentElementValue = singleRow[9].ToString(),
                    businessKeyComponentElementType = singleRow[10].ToString(),
                    businessKeyComponentElementAttributeId = singleRow[11].ToString(),
                    businessKeyComponentElementAttributeName = singleRow[12].ToString(),
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        /// <summary>
        ///   Saves the Business Key Component Part data set to disk as a JSON file in the default configuration directory
        /// </summary>
        internal static void SaveJsonInterfaceDrivingKey()
        {

            const string fileName = "interfaceDrivingKey";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_DRIVING_KEY]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());


            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[SATELLITE_ID] ASC, [HUB_ID] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    satelliteId = singleRow[0].ToString(),
                    satelliteName = singleRow[1].ToString(),
                    hubId = singleRow[2].ToString(),
                    hubName = singleRow[3].ToString()
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        internal static void SaveJsonInterfaceHubLinkXref()
        {

            const string fileName = "interfaceHubLinkXref";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_HUB_LINK_XREF]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());


            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[LINK_ID] ASC, [SOURCE_ID] ASC, [HUB_ID] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    linkId = singleRow[0].ToString(),
                    linkName = singleRow[1].ToString(),
                    sourceId = singleRow[2].ToString(),
                    sourceName = singleRow[3].ToString(),
                    sourceSchemaName = singleRow[4].ToString(),
                    hubId = singleRow[5].ToString(),
                    hubName = singleRow[6].ToString(),
                    businessKeyDefinition = singleRow[7].ToString()
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        internal static void SaveJsonInterfacePhysicalModel()
        {

            const string fileName = "interfacePhysicalModel";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_PHYSICAL_MODEL]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());


            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[DATABASE_NAME] ASC, [SCHEMA_NAME] ASC, [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    databaseName = singleRow[0].ToString(),
                    schemaName = singleRow[1].ToString(),
                    tableName = singleRow[2].ToString(),
                    columnName = singleRow[3].ToString(),
                    characterMaximumLength = singleRow[4].ToString(),
                    numericPrecision = singleRow[5].ToString(),
                    ordinalPosition = singleRow[6].ToString(),
                    primaryKeyIndicator = singleRow[7].ToString()
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        internal static void SaveJsonInterfaceSourceHubXref()
        {

            const string fileName = "interfaceSourceHubXref";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_SOURCE_HUB_XREF]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());


            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[SOURCE_ID] ASC, [TARGET_ID] ASC, [TARGET_BUSINESS_KEY_DEFINITION] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    sourceId = singleRow[0].ToString(),
                    sourceSchemaName = singleRow[1].ToString(),
                    sourceName = singleRow[2].ToString(),
                    sourceBusinessKeyDefinition = singleRow[3].ToString(),
                    targetId = singleRow[4].ToString(),
                    targetSchemaName = singleRow[5].ToString(),
                    targetName = singleRow[6].ToString(),
                    targetBusinessKeyDefinition = singleRow[7].ToString(),
                    targetType = singleRow[8].ToString(),
                    filterCriteria = singleRow[9].ToString(),
                    loadVector = singleRow[10].ToString()
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        internal static void SaveJsonInterfaceSourceLinkAttributeXref()
        {
            const string fileName = "interfaceSourceLinkAttributeXref";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_SOURCE_LINK_ATTRIBUTE_XREF]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());

            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[SOURCE_ID] ASC, [LINK_ID] ASC, [SOURCE_ATTRIBUTE_ID] ASC, [LINK_ATTRIBUTE_ID] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    sourceId = singleRow[0].ToString(),
                    sourceName = singleRow[1].ToString(),
                    sourceSchemaName = singleRow[2].ToString(),
                    linkId = singleRow[3].ToString(),
                    linkName = singleRow[4].ToString(),
                    sourceAttributeId = singleRow[5].ToString(),
                    sourceAttributeName = singleRow[6].ToString(),
                    linkAttributeId = singleRow[7].ToString(),
                    linkAttributeName = singleRow[8].ToString()
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        internal static void SaveJsonInterfaceSourceLinkXref()
        {

            const string fileName = "interfaceSourceLinkXref";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_SOURCE_LINK_XREF]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());

            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[SOURCE_ID] ASC, [LINK_ID] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    sourceId = singleRow[0].ToString(),
                    sourceName = singleRow[1].ToString(),
                    linkId = singleRow[2].ToString(),
                    linkName = singleRow[3].ToString(),
                    filterCriteria = singleRow[4].ToString(),
                    loadVector = singleRow[5].ToString()
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        internal static void SaveJsonInterfaceSourceSatelliteAttributeXref()
        {
            const string fileName = "interfaceSourceSatelliteAttributeXref";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_SOURCE_SATELLITE_ATTRIBUTE_XREF]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());

            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[SOURCE_ID] ASC, [SATELLITE_ID] ASC, [SOURCE_ATTRIBUTE_ID] ASC, [SATELLITE_ATTRIBUTE_ID] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    sourceId = singleRow[0].ToString(),
                    sourceName = singleRow[1].ToString(),
                    sourceSchemaName = singleRow[2].ToString(),
                    satelliteId = singleRow[3].ToString(),
                    satelliteName = singleRow[4].ToString(),
                    sourceAttributeId = singleRow[5].ToString(),
                    sourceAttributeName = singleRow[6].ToString(),
                    satelliteAttributeId = singleRow[7].ToString(),
                    satelliteAttributeName = singleRow[8].ToString(),
                    multiActiveKeyIndicator = singleRow[9].ToString()
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

        internal static void SaveJsonInterfaceSourceSatelliteXref()
        {
            const string fileName = "interfaceSourceSatelliteXref";

            // Get the information from the view
            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("SELECT * FROM [interface].[INTERFACE_SOURCE_SATELLITE_XREF]");

            var conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            var inputDataTable = FormBase.GetDataTable(ref conn, sqlStatement.ToString());

            // Make sure the output is sorted to persist in JSON
            inputDataTable.DefaultView.Sort = "[SOURCE_ID] ASC, [TARGET_ID] ASC";

            inputDataTable.TableName = fileName;

            JArray outputFileArray = new JArray();
            foreach (DataRow singleRow in inputDataTable.DefaultView.ToTable().Rows)
            {
                JObject individualRow = JObject.FromObject(new
                {
                    sourceId = singleRow[0].ToString(),
                    sourceSchemaName = singleRow[1].ToString(),
                    sourceName = singleRow[2].ToString(),
                    sourceBusinessKeyDefinition = singleRow[3].ToString(),
                    targetId = singleRow[4].ToString(),
                    targetSchemaName= singleRow[5].ToString(),
                    targetName = singleRow[6].ToString(),
                    targetBusinessKeyDefinition = singleRow[7].ToString(),
                    targetType = singleRow[8].ToString(),
                    surrogateKey = singleRow[9].ToString(),
                    filterCriteria = singleRow[10].ToString(),
                    loadVector = singleRow[11].ToString(),
                });
                outputFileArray.Add(individualRow);
            }

            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileName + FormBase.GlobalParameters.JsonExtension, json);
        }

    }
}