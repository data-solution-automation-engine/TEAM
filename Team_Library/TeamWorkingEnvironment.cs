using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TEAM
{
    /// <summary>
    /// An unique working environment (tenant) containing all TEAM settings.
    /// </summary>
    public class TeamWorkingEnvironment
    {
        public string environmentInternalId { get; set; }
        public string environmentName { get; set; }
        public string environmentKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string environmentNotes { get; set; }

    }

    public class TeamWorkingEnvironmentCollection
    {
        public Dictionary<string, TeamWorkingEnvironment> EnvironmentDictionary { get; set; }
        public EventLog EventLog { get; set; }

        public TeamWorkingEnvironmentCollection()
        {
            EnvironmentDictionary = new Dictionary<string, TeamWorkingEnvironment>();
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
                var list = new List<TeamWorkingEnvironment>();

                var developmentEnvironment = new TeamWorkingEnvironment
                {
                    environmentInternalId = Utility.CreateMd5(new[] { "Development" }, "%$@"),
                    environmentKey = "Development",
                    environmentName = "Development environment",
                    environmentNotes = "Environment created as initial / starter environment."
                };

                list.Add(developmentEnvironment);

                var productionEnvironment = new TeamWorkingEnvironment
                {
                    environmentInternalId = Utility.CreateMd5(new[] { "Production" }, "%$@"),
                    environmentKey = "Production",
                    environmentName = "Production environment",
                    environmentNotes = "Environment created as initial / starter environment."
                };

                list.Add(productionEnvironment);

                string output = JsonConvert.SerializeObject(list.ToArray(), Formatting.Indented);
                File.WriteAllText(fileName, output);

                // Commit to memory also.
                var localDictionary = new Dictionary<string, TeamWorkingEnvironment>();

                localDictionary.Add(developmentEnvironment.environmentInternalId, developmentEnvironment);
                localDictionary.Add(productionEnvironment.environmentInternalId, productionEnvironment);

                EnvironmentDictionary = localDictionary;
            }
            else
            {
                EnvironmentDictionary.Clear();
                TeamWorkingEnvironment[] environmentJson = JsonConvert.DeserializeObject<TeamWorkingEnvironment[]>(File.ReadAllText(fileName));

                foreach (var environment in environmentJson)
                {
                    EnvironmentDictionary.Add(environment.environmentInternalId, environment);
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment '{environment.environmentName}'with identifier {environment.environmentInternalId} has been loaded."));
                }
            }
        }

        /// <summary>
        /// Perform a safe lookup on the collection to retrieve the environment details by its key.
        /// </summary>
        /// <param name="environmentKey"></param>
        /// <returns></returns>
        public TeamWorkingEnvironment GetEnvironmentByKey(string environmentKey)
        {
            TeamWorkingEnvironment localEnvironment = new TeamWorkingEnvironment();

            if (environmentKey == null || environmentKey == "")
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The environment key was not provided, so no environment was attempted to be retrieved from the collection."));
            }
            else
            {
                try
                {
                    localEnvironment = EnvironmentDictionary[environmentKey];
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment '{localEnvironment.environmentName}' was retrieved using key {localEnvironment.environmentKey}."));
                }
                catch (Exception ex)
                {
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered while trying to lookup the environment for key {localEnvironment.environmentKey}. The exception message is '{ex}'"));

                }
            }

            return localEnvironment;
        }
    }
}
