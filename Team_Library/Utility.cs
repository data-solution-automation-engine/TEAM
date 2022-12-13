using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;


namespace TEAM_Library
{
    public static class Utility
    {
        /// <summary>
        /// Extension method to be able to use StringComparison for non Case Sensitive comparisons using string.Contains.
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="checkString"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static bool Contains(this string inputString, string checkString, StringComparison stringComparison)
        {
            return inputString?.IndexOf(checkString, stringComparison) >= 0;
        }

        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }

        public static string GetDefaultBrowserPath()
        {
            string urlAssociation = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http";
            string browserPathKey = @"$BROWSER$\shell\open\command";

            string browserPath = "";

            try
            {
                //Read default browser path from userChoiceLKey
                var userChoiceKey = Registry.CurrentUser.OpenSubKey(urlAssociation + @"\UserChoice", false);

                //If user choice was not found, try machine default
                if (userChoiceKey == null)
                {
                    //Read default browser path from Win XP registry key
                    var browserKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                    //If browser path wasn’t found, try Win Vista (and newer) registry key
                    if (browserKey == null)
                    {
                        browserKey =
                        Registry.CurrentUser.OpenSubKey(
                        urlAssociation, false);
                    }
                    var path = CleanBrowserPath(browserKey.GetValue(null) as string);
                    browserKey.Close();
                    return path;
                }
                else
                {
                    // user defined browser choice was found
                    string progId = (userChoiceKey.GetValue("ProgId").ToString());
                    userChoiceKey.Close();

                    // now look up the path of the executable
                    string concreteBrowserKey = browserPathKey.Replace("$BROWSER$", progId);
                    var kp = Registry.ClassesRoot.OpenSubKey(concreteBrowserKey, false);
                    browserPath = CleanBrowserPath(kp.GetValue(null) as string);
                    kp.Close();
                    return browserPath;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string CleanBrowserPath(string p)
        {
            string[] url = p.Split('"');
            string clean = url[1];
            return clean;
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        /// <summary>
        /// Evaluates if the given string value (cell value in most cases) can be considered a Data Query, as opposed to a Data Object or Data Item.
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        public static Boolean IsDataQuery(this string stringValue)
        {
            bool isDataQuery = stringValue == null || (stringValue.StartsWith("`") && stringValue.EndsWith("`"));

            return isDataQuery;
        }

        /// <summary>
        /// Assert if a given Team Data Object is a Data Vault Hub entity.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static Boolean IsDataVaultHub(this string dataObjectName, TeamConfiguration teamConfiguration)
        {
            bool isDataVaultHub = (teamConfiguration.TableNamingLocation == "Prefix" && dataObjectName.StartsWith(teamConfiguration.HubTablePrefixValue)) ||
                                  (teamConfiguration.TableNamingLocation == "Suffix" && dataObjectName.EndsWith(teamConfiguration.HubTablePrefixValue));

            return isDataVaultHub;
        }

        /// <summary>
        /// Assert if a given Team Data Object is a Data Vault Satellite entity.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static Boolean IsDataVaultSatellite(this string dataObjectName, TeamConfiguration teamConfiguration)
        {
            bool isDataVaultHub = (teamConfiguration.TableNamingLocation == "Prefix" && dataObjectName.StartsWith(teamConfiguration.SatTablePrefixValue)) ||
                                  (teamConfiguration.TableNamingLocation == "Suffix" && dataObjectName.EndsWith(teamConfiguration.SatTablePrefixValue));

            return isDataVaultHub;
        }

        /// <summary>
        /// Assert if a given Team Data Object is a Data Vault Link-Satellite entity.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static Boolean IsDataVaultLinkSatellite(this string dataObjectName, TeamConfiguration teamConfiguration)
        {
            bool isDataVaultHub = (teamConfiguration.TableNamingLocation == "Prefix" && dataObjectName.StartsWith(teamConfiguration.LsatTablePrefixValue)) ||
                                  (teamConfiguration.TableNamingLocation == "Suffix" && dataObjectName.EndsWith(teamConfiguration.LsatTablePrefixValue));

            return isDataVaultHub;
        }

        /// <summary>
        /// Assert if a given Team Data Object is a Data Vault Link entity.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static Boolean IsDataVaultLink(this string dataObjectName, TeamConfiguration teamConfiguration)
        {
            bool isDataVaultHub = (teamConfiguration.TableNamingLocation == "Prefix" && dataObjectName.StartsWith(teamConfiguration.LinkTablePrefixValue)) ||
                                  (teamConfiguration.TableNamingLocation == "Suffix" && dataObjectName.EndsWith(teamConfiguration.LinkTablePrefixValue));

            return isDataVaultHub;
        }

        /// <summary>
        /// Assert if a given Team Data Object is a PSA entity.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static Boolean IsPsa(this string dataObjectName, TeamConfiguration teamConfiguration)
        {
            bool isDataVaultHub = (teamConfiguration.TableNamingLocation == "Prefix" && dataObjectName.StartsWith(teamConfiguration.PsaTablePrefixValue)) ||
                                  (teamConfiguration.TableNamingLocation == "Suffix" && dataObjectName.EndsWith(teamConfiguration.PsaTablePrefixValue));

            return isDataVaultHub;
        }

        /// <summary>
        /// Receives a comma-separated string and turns it into an array without spaces.
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        public static string[] SplitLabelIntoArray(string labels)
        {
            string[] SplitLabelArray = { "default" };
            if (labels != null)
            {
                SplitLabelArray = labels.Replace(" ", "").Split(',');
            }

            return SplitLabelArray;
        }

        public static string SandingElement { get; set; } = "^@#%7!";

        /// <summary>
        /// Generate a MD5 hash based on the string input and sanding element.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sandingElement"></param>
        /// <returns></returns>
        public static string CreateMd5(string[] input, string sandingElement)
        {
            string hashingString = "";
            // Make sure the values in the string array are appropriately sanded
            foreach (string inputElement in input)
            {
                hashingString = hashingString + inputElement+ sandingElement;
            }

            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(hashingString);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        
        /// <summary>
        /// Generate a random number value.
        /// </summary>
        /// <param name="maxNumber"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int maxNumber)
        {
            if (maxNumber < 1)
                throw new Exception("The maxNumber value should be greater than 1");
            var b = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(b);
            var seed = (b[0] & 0x7f) << 24 | b[1] << 16 | b[2] << 8 | b[3];
            var r = new Random(seed);
            return r.Next(1, maxNumber);
        }

        /// <summary>
        /// Generate random date value.
        /// </summary>
        /// <param name="startYear"></param>
        /// <returns></returns>
        public static DateTime GetRandomDate(int startYear = 1995)
        {
            var start = new DateTime(startYear, 1, 1);
            var range = (DateTime.Today - start).Days;
            var b = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(b);
            var seed = (b[0] & 0x7f) << 24 | b[1] << 16 | b[2] << 8 | b[3];
            return start.AddDays(new Random(seed).Next(1, range)).AddSeconds(new Random(seed).Next(1, 86400));
        }

        /// <summary>
        /// Generate a random string value.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            var array = new[]
            {
                "0","2","3","4","5","6","8","9",
                "a","b","c","d","e","f","g","h","j","k","m","n","p","q","r","s","t","u","v","w","x","y","z",
                "A","B","C","D","E","F","G","H","J","K","L","M","N","P","R","S","T","U","V","W","X","Y","Z"
            };
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++) sb.Append(array[GetRandomNumber(53)]);
            return sb.ToString();
        }

        /// <summary>
        /// Populate a data table by loading from a Sql Server database table.
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="sqlQuery"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(ref SqlConnection sqlConnection, string sqlQuery)
        {
            // Pass the connection to a command object
            var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);
            var sqlDataAdapter = new SqlDataAdapter { SelectCommand = sqlCommand };

            var dataTable = new DataTable();

            // Adds or refreshes rows in the DataSet to match those in the data source
            try
            {
                sqlDataAdapter.Fill(dataTable);
            }

            catch (Exception)
            {
                //  MessageBox.Show(@"SQL error: " + exception.Message + "\r\n\r\n The executed query was: " + sql + "\r\n\r\n The connection used was " + sqlConnection.ConnectionString, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return dataTable;

        }

        /// <summary>
        /// Convert a list of objects into a data table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            DataTable table = new DataTable();

            try
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                foreach (PropertyDescriptor prop in properties)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    table.Rows.Add(row);
                }
            }
            catch (Exception)
            {
                // IGNORE
            }
            return table;
        }

        /// <summary>
        /// Returns the default type (e.g. null value, empty or default) for a given input object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T); // returns the default value for the type
            }
            else
            {
                return (T)obj;
            }
        }

        public static bool In<T>(this T val, params T[] values) where T : struct
        {
            return values.Contains(val);
        }
    }
}
