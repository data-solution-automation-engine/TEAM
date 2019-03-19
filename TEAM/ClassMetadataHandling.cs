using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEAM
{
    internal class ClassMetadataHandling
    {
        internal static string GetTableType(string tableName)
        {
            string localType ="";

            // Remove schema, if there
            //tableName = tableName.Substring(tableName.IndexOf(']') + 2);

            if (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix") // I.e. HUB_CUSTOMER
            {
                if (tableName.StartsWith(FormBase.ConfigurationSettings.SatTablePrefixValue))
                {
                    localType = "Satellite";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.HubTablePrefixValue))
                {
                    localType = "Hub";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.LinkTablePrefixValue))
                {
                    localType = "Link";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.LsatPrefixValue))
                {
                    localType = "Link-Satellite";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.StgTablePrefixValue))
                {
                    localType = "Staging Area";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.PsaTablePrefixValue))
                {
                    localType = "Persistent Staging Area";
                }
                else if (tableName.StartsWith("DIM_") && tableName.StartsWith("FACT_"))
                {
                    localType = "Presentation";
                }
                else 
                {
                    localType = "Derived";
                }
            }
            else if (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix") // I.e. CUSTOMER_HUB
            {
                if (tableName.EndsWith(FormBase.ConfigurationSettings.SatTablePrefixValue))
                {
                    localType = "Satellite";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.HubTablePrefixValue))
                {
                    localType = "Hub";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.LinkTablePrefixValue))
                {
                    localType = "Link";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.LsatPrefixValue))
                {
                    localType = "Link-Satellite";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.StgTablePrefixValue))
                {
                    localType = "Staging Area";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.PsaTablePrefixValue))
                {
                    localType = "Persistent Staging Area";
                }
                else if (tableName.EndsWith("DIM_") && tableName.EndsWith("FACT_"))
                {
                    localType = "Presentation";
                }
                else
                {
                    localType = "Derived";
                }
            }
            else
            {
                localType = "The table type cannot be defined because of an unknown prefix/suffix: "+ FormBase.ConfigurationSettings.TableNamingLocation;
            }


            return localType;
        }

        internal static string GetDatabaseForArea(string tableType)
        {
            string localDatabase = "";

            if (new string[] {"Hub", "Satellite", "Link", "Link-Satellite", "Derived"}.Contains(tableType))
            {
                localDatabase = FormBase.ConfigurationSettings.IntegrationDatabaseName;
            }
            else if (tableType == "Staging Area")
            {
                localDatabase = FormBase.ConfigurationSettings.StagingDatabaseName;
            }
            else if (tableType == "Persistent Staging Area")
            {
                localDatabase = FormBase.ConfigurationSettings.PsaDatabaseName;
            }
            else if (tableType == "Presentation")
            {
                localDatabase = FormBase.ConfigurationSettings.PresentationDatabaseName;
            }
            else // Return error
            {
                localDatabase = "Unknown - error - the database could not be derived from the object " + tableType;
            }

            return localDatabase;
        }

        internal static string GetArea(string sourceMapping, string targetMapping)
        {
            string localArea = "";
            if (targetMapping.Contains("BDV"))
            {
                localArea = "Derived";
            }
            else
            {
                localArea = "Base";
            }

            return localArea;
        }

        internal static Dictionary<string, string> GetSchema(string tableName)
        {
            Dictionary<string, string> fullyQualifiedTableName = new Dictionary<string, string>();
            string schemaName = "";
            string returnTableName = "";

            if (tableName.Contains('.')) // Split the string
            {
                var splitName = tableName.Split('.').ToList();

                fullyQualifiedTableName.Add(splitName[0], splitName[1]);

            }
            else // Return the default (e.g. [dbo])
            {
                schemaName = FormBase.GlobalParameters.DefaultSchema;
                returnTableName = tableName;
            }

            fullyQualifiedTableName.Add(schemaName, returnTableName);

            return fullyQualifiedTableName;
        }

        internal static string getFullSchemaTable(string tableName)
        {
            var fullyQualifiedSourceName = GetSchema(tableName).FirstOrDefault();

            var returnTableName = fullyQualifiedSourceName.Key + '.' + fullyQualifiedSourceName.Value;

            return returnTableName;
        }

        public static List<string> GetHubTargetBusinessKeyList(string hubTableName, int versionId, string queryMode)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys
            var conn = new SqlConnection();

            if (queryMode == "physical")
            {
                conn = new SqlConnection {ConnectionString = FormBase.ConfigurationSettings.ConnectionStringInt};
            }
            else // Virtual
            {
                conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            }

            try
            {
                conn.Open();
            }
            catch (Exception exception)
            {
               // SetTextDebug("An error has occurred defining the Hub Business Key in the model due to connectivity issues (connection string " + conn.ConnectionString + "). The associated message is " + exception.Message);
            }

            var sqlStatementForHubBusinessKeys = new StringBuilder();

            var keyText = FormBase.ConfigurationSettings.DwhKeyIdentifier;
            var localkeyLength = keyText.Length;
            var localkeySubstring = localkeyLength + 1;

            if (queryMode == "physical")
            {
                // Make sure the live database is hit when the checkbox is ticked
                sqlStatementForHubBusinessKeys.AppendLine("SELECT COLUMN_NAME");
                sqlStatementForHubBusinessKeys.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS");
                sqlStatementForHubBusinessKeys.AppendLine("WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-" + localkeyLength + "," + localkeySubstring + ")!='_" + FormBase.ConfigurationSettings.DwhKeyIdentifier + "'");
                sqlStatementForHubBusinessKeys.AppendLine("AND TABLE_SCHEMA = '" + FormBase.ConfigurationSettings.SchemaName + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND TABLE_NAME= '" + hubTableName + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" + FormBase.ConfigurationSettings.RecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeRecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" +
                                                          FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute + "','" + FormBase.ConfigurationSettings.EtlProcessAttribute + "','" + FormBase.ConfigurationSettings.LoadDateTimeAttribute + "')");
            }
            else
            {
                //Ignore version is not checked, so versioning is used - meaning the business key metadata is sourced from the version history metadata.
                sqlStatementForHubBusinessKeys.AppendLine("SELECT COLUMN_NAME");
                sqlStatementForHubBusinessKeys.AppendLine("FROM MD_VERSION_ATTRIBUTE");
                sqlStatementForHubBusinessKeys.AppendLine("WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-" + localkeyLength + "," + localkeySubstring + ")!='_" + FormBase.ConfigurationSettings.DwhKeyIdentifier + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND TABLE_NAME= '" + hubTableName + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" + FormBase.ConfigurationSettings.RecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeRecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" + FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute + "','" +
                                                          FormBase.ConfigurationSettings.EtlProcessAttribute + "','" + FormBase.ConfigurationSettings.LoadDateTimeAttribute + "')");
                sqlStatementForHubBusinessKeys.AppendLine("  AND VERSION_ID = " + versionId + "");
            }


            var hubKeyList = FormBase.GetDataTable(ref conn, sqlStatementForHubBusinessKeys.ToString());

            if (hubKeyList == null)
            {
                //SetTextDebug("An error has occurred defining the Hub Business Key in the model for " + hubTableName + ". The Business Key was not found when querying the underlying metadata. This can be either that the attribute is missing in the metadata or in the table (depending if versioning is used). If the 'ignore versioning' option is checked, then the metadata will be retrieved directly from the data dictionary. Otherwise the metadata needs to be available in the repository (manage model metadata).");
            }

            var businessKeyList = new List<string>();
            foreach (DataRow row in hubKeyList.Rows)
            {
                if (!businessKeyList.Contains((string)row["COLUMN_NAME"]))
                {
                    businessKeyList.Add((string)row["COLUMN_NAME"]);
                }
            }

            return businessKeyList;
        }
    }
}
