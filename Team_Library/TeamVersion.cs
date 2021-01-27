using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TEAM_Library
{
    // The definition of a TEAM version.
    public class TeamVersion
    {
        public string EnvironmentInternalId { get; set; }
        public string EnvironmentName { get; set; }
        public string VersionInternalId { get; set; }
        public int VersionId { get; set; }
        public string VersionName { get; set; }
        public string VersionNotes { get; set; }
        public int MajorReleaseNumber { get; set; }
        public int MinorReleaseNumber { get; set; }
        public DateTime ModificationDateTime { get; set; }

        /// <summary>
        /// Set the internal id for any new version in the constructor.
        /// </summary>
        public TeamVersion()
        {
            string[] inputHashValue = new string[] { Utility.GetRandomString(100)+DateTime.Now };
            VersionInternalId = Utility.CreateMd5(inputHashValue, Utility.SandingElement);

            ModificationDateTime = DateTime.Now;
        }
    }
    
    //  A list of TEAM versions, containing the collection of versions (models) related to the same project/context/initiative.
    public class TeamVersionList
    {
        public List<TeamVersion> VersionList { get; set; } = new List<TeamVersion>();
        public List<Tuple<string, string, string>> FileEnvironmentVersionList = new List<Tuple<string, string, string>>();
        public List<Tuple<string, string, string>> SelectedFileEnvironmentVersionList = new List<Tuple<string, string, string>>();

        public void LoadVersionList(string fileName)
        {
            if (File.Exists(fileName))
            {
                List<TeamVersion> VersionList = JsonConvert.DeserializeObject<List<TeamVersion>>(File.ReadAllText(fileName));
                this.VersionList = VersionList;
            }
        }

        public void CreateNewVersionListFile(string fileName, string workingEnvironment)
        {
            AddNewVersionToList(workingEnvironment, 0, 0);
            SaveVersionList(fileName);
        }

        /// <summary>
        /// Create a new version and add this to the VersionList.
        /// </summary>
        /// <param name="workingEnvironment"></param>
        /// <param name="majorVersion"></param>
        /// <param name="minorVersion"></param>
        public void AddNewVersionToList(string workingEnvironment, int majorVersion, int minorVersion)
        {
            int newVersionId = 0;
            if (majorVersion == 0 && minorVersion == 0)
            {
                // Don't update the version information, just use the empty / zero values.
            }
            else
            {
                var highestVersion = GetMaxVersionForEnvironment(workingEnvironment);

                newVersionId = highestVersion.Item1;
                newVersionId++;
            }

            var newVersion = new TeamVersion
            {
                VersionId = newVersionId,
                EnvironmentName = workingEnvironment,
                MajorReleaseNumber = majorVersion,
                MinorReleaseNumber = minorVersion
            };

            VersionList.Add(newVersion);
        }

        /// <summary>
        /// Retrieve the most recent (highest) version (as a Tuple with the version ID, the major number and minor number) for a given working environment.
        /// </summary>
        /// <param name="workingEnvironment"></param>
        /// <returns></returns>
        public Tuple<int, int, int> GetMaxVersionForEnvironment(string workingEnvironment)
        {
            /// Tuple setup:
            /// item1 = version id
            /// item2 = major release number
            /// item3 = minor release number
            var returnValue = new Tuple<int, int, int>(0, 0, 0);

            if (VersionList.Count > 0)
            {
                try
                {
                    var highestVersion = VersionList.OrderByDescending(item => item.MajorReleaseNumber)
                        .ThenByDescending(item => item.MinorReleaseNumber)
                        .Where(item => item.EnvironmentName == workingEnvironment).First();

                    if (highestVersion != null)
                    {
                        returnValue = new Tuple<int, int, int>(highestVersion.VersionId, highestVersion.MajorReleaseNumber, highestVersion.MinorReleaseNumber);
                    }

                }
                catch (Exception ex)
                {
                    // TBD
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Retrieve the version details given a specific version ID.
        /// </summary>
        /// <param name="workingEnvironment"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public Tuple<int, int, int> GetMajorMinorForVersionId(string workingEnvironment, int versionId)
        {
            var returnValue = new Tuple<int, int,int>(0, 0, 0);

            if (VersionList.Count > 0 && versionId > 0)
            {
                var selectedVersion = VersionList.Where(item => item.EnvironmentName == workingEnvironment && item.VersionId == versionId).First();

                if (selectedVersion != null)
                {
                    returnValue = new Tuple<int, int, int>(selectedVersion.VersionId, selectedVersion.MajorReleaseNumber, selectedVersion.MinorReleaseNumber);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Return the number of distinct versions for a given environment.
        /// </summary>
        /// <param name="workingEnvironment"></param>
        /// <returns></returns>
        public int GetTotalVersionCount(string workingEnvironment)
        {
            int returnValue = 0;

            if (VersionList.Count > 0)
            {
                returnValue = VersionList.Count(item => item.EnvironmentName == workingEnvironment);
            }

            return returnValue;
        }

        public void SaveVersionList(string fileName)
        {
            // Create a new file if it does not exist.
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }

            // Save the updated file to disk.
            string output = JsonConvert.SerializeObject(VersionList, Formatting.Indented);
            File.WriteAllText(fileName, output);
        }

        /// <summary>
        /// Creates a list of all versions for the identified environments, based on the Json files that exists in a given directory.
        /// Optionally clear the list by providing a boolean ('clearList').
        /// </summary>
        /// <param name="directoryName"></param>
        /// <param name="clearList"></param>
        public void GetEnvironmentVersionsFromFile(string directoryName, bool clearList = false)
        {
            if (clearList)
            {
                FileEnvironmentVersionList.Clear();
            }

            string[] files = Directory.GetFiles(directoryName);

            foreach (string file in files)
            {
                if (file.Contains("_TEAM_Table_Mapping_v"))
                {
                    var environmentName = Path.GetFileName(file).Split('_')[0];
                    var versionNumber = Path.GetFileName(file).Substring(Path.GetFileName(file).LastIndexOf('_') + 1)
                        .Replace("v", "").Replace(".json", "");

                    FileEnvironmentVersionList.Add(new Tuple<string, string, string>(environmentName, versionNumber, Path.GetFileName(file)));
                }
            }
        }

        public void GetVersionsForSelectedEnvironments(string environmentName, bool clearList = false) 
        {
            if (clearList)
            {
                SelectedFileEnvironmentVersionList.Clear();
            }
            List< Tuple<string, string, string> > versionsForSelectedEnvironment = new List<Tuple<string, string, string>>();
            var result = FileEnvironmentVersionList.Where(item => item.Item1 == environmentName );

            foreach (var resultRow in result)
            {
                versionsForSelectedEnvironment.Add(new Tuple<string, string, string>(resultRow.Item1,resultRow.Item2, resultRow.Item3));
            }

            SelectedFileEnvironmentVersionList = versionsForSelectedEnvironment;
        }
    }
}
