using System;
using System.Collections.Generic;
using System.Configuration;
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
    }
}
