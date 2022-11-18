using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TEAM_Library
{
    /// <summary>
    /// An unique working environment (tenant) referring to TEAM settings.
    /// </summary>
    public class TeamEnvironment
    {
        public string environmentInternalId { get; set; }
        public string environmentName { get; set; }
        public string environmentKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string configurationPath { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string metadataPath { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string environmentNotes { get; set; }

        public void SaveTeamEnvironment(string fileName)
        {
            // globalParameters.CorePath + globalParameters.JsonEnvironmentFileName + globalParameters.JsonExtension
            // Update the environment on disk
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }

            // Check if the value already exists in the file
            var jsonKeyLookup = new TeamEnvironment();

            TeamEnvironment[] jsonArray = JsonConvert.DeserializeObject<TeamEnvironment[]>(File.ReadAllText(fileName));

            // If the Json file already contains values (non-empty) then perform a key lookup.
            if (jsonArray != null)
            {
                jsonKeyLookup = jsonArray.FirstOrDefault(obj => obj.environmentInternalId == environmentInternalId);
            }

            // If nothing yet exists in the file, the key lookup is NULL or "" then the record in question does not exist in the Json file and should be added.
            if (jsonArray == null || jsonKeyLookup == null || jsonKeyLookup.environmentInternalId == "")
            {
                //  There was no key in the file for this connection, so it's new.
                var list = new List<TeamEnvironment>();
                if (jsonArray != null)
                {
                    list = jsonArray.ToList();
                }

                list.Add(this);
                jsonArray = list.ToArray();
            }
            else
            {
                // Update the values in an existing JSON segment
                jsonKeyLookup.environmentInternalId = environmentInternalId;
                jsonKeyLookup.environmentKey = environmentKey;
                jsonKeyLookup.environmentName = environmentName;
                jsonKeyLookup.configurationPath = configurationPath;
                jsonKeyLookup.metadataPath = metadataPath;
                jsonKeyLookup.environmentNotes = environmentNotes;
            }

            // Save the updated file to disk.
            string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
            File.WriteAllText(fileName, output);
        }
    }

    public class TeamEnvironmentCollection
    {
        public Dictionary<string, TeamEnvironment> EnvironmentDictionary { get; set; }
        public EventLog EventLog { get; set; }

        public TeamEnvironmentCollection()
        {
            EnvironmentDictionary = new Dictionary<string, TeamEnvironment>();
            EventLog = new EventLog();
        }

        /// <summary>
        /// Load a TEAM environment file into memory.
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadTeamEnvironmentCollection(string fileName)
        {
            // Create a new file if it doesn't exist.
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();

                // There was no key in the file for this environment, so it's new.
                // Create two initial environments, development and production.
                var list = new List<TeamEnvironment>();

                var developmentEnvironment = new TeamEnvironment
                {
                    environmentInternalId = Utility.CreateMd5(new[] { "Development" }, "%$@"),
                    environmentKey = "Development",
                    environmentName = "Development environment",
                    configurationPath = Application.StartupPath + @"\Configuration\",
                    metadataPath = Application.StartupPath + @"\Metadata\",
                    environmentNotes = "Environment created as initial / starter environment."
                };

                list.Add(developmentEnvironment);

                var productionEnvironment = new TeamEnvironment
                {
                    environmentInternalId = Utility.CreateMd5(new[] { "Production" }, "%$@"),
                    environmentKey = "Production",
                    environmentName = "Production environment",
                    configurationPath = Application.StartupPath + @"\ConfigurationProduction\",
                    metadataPath = Application.StartupPath + @"\MetadataProduction\",
                    environmentNotes = "Environment created as initial / starter environment."
                };

                list.Add(productionEnvironment);

                string output = JsonConvert.SerializeObject(list.ToArray(), Formatting.Indented);
                File.WriteAllText(fileName, output);

                // Commit to memory also.
                var localDictionary = new Dictionary<string, TeamEnvironment>();

                localDictionary.Add(developmentEnvironment.environmentInternalId, developmentEnvironment);
                localDictionary.Add(productionEnvironment.environmentInternalId, productionEnvironment);

                EnvironmentDictionary = localDictionary;
            }
            // Load the file if it does exist.
            else
            {
                EnvironmentDictionary.Clear();

                TeamEnvironment[] environmentJson = JsonConvert.DeserializeObject<TeamEnvironment[]>(File.ReadAllText(fileName));

                if (environmentJson != null)
                {
                    foreach (var environment in environmentJson)
                    {
                        EnvironmentDictionary.Add(environment.environmentInternalId, environment);
                        EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment '{environment.environmentName}' with identifier '{environment.environmentInternalId}' has been loaded."));
                    }
                }
            }
        }

        /// <summary>
        /// Perform a safe lookup on the collection to retrieve the environment details by its identification value.
        /// </summary>
        /// <param name="environmentId"></param>
        /// <returns></returns>
        public TeamEnvironment GetEnvironmentById(string environmentId)
        {
            TeamEnvironment localEnvironment = new TeamEnvironment();

            if (string.IsNullOrEmpty(environmentId))
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, "The environment id was not provided, so no environment was attempted to be retrieved from the collection."));
            }
            else
            {
                try
                {
                    localEnvironment = EnvironmentDictionary[environmentId];
                    
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment '{localEnvironment.environmentName}' was retrieved using id {environmentId}."));
                }
                catch (Exception ex)
                {
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered while trying to lookup the environment for id {environmentId}. The exception message is '{ex.Message}'"));

                }
            }

            return localEnvironment;
        }

        /// <summary>
        /// Perform a safe lookup on the collection to retrieve the environment details by its key value.
        /// </summary>
        /// <param name="environmentKey"></param>
        /// <returns></returns>
        public TeamEnvironment GetEnvironmentByKey(string environmentKey)
        {
            TeamEnvironment localEnvironment = new TeamEnvironment();

            if (string.IsNullOrEmpty(environmentKey))
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, "The environment key was not provided, so no environment was attempted to be retrieved from the collection."));
            }
            else
            {
                try
                {
                    var selectedKeyValuePair = EnvironmentDictionary.FirstOrDefault(x => x.Value.environmentKey == environmentKey);
                    localEnvironment = selectedKeyValuePair.Value;

                    EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment '{localEnvironment.environmentName}' was retrieved using key {environmentKey}."));
                }
                catch (Exception ex)
                {
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered while trying to lookup the environment for key {environmentKey}. The exception message is '{ex.Message}'"));

                }
            }

            return localEnvironment;
        }
    }
}
