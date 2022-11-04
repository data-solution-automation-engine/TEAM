﻿using System;
using System.Collections.Generic;
using System.IO;
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
        /// Perform a safe lookup on the collection to retrieve the environment details by its key.
        /// </summary>
        /// <param name="environmentKey"></param>
        /// <returns></returns>
        public TeamEnvironment GetEnvironmentById(string environmentKey)
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
                    localEnvironment = EnvironmentDictionary[environmentKey];
                    
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment '{localEnvironment.environmentName}' was retrieved using key {localEnvironment.environmentKey}."));
                }
                catch (Exception ex)
                {
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered while trying to lookup the environment for key {localEnvironment.environmentKey}. The exception message is '{ex.Message}'"));

                }
            }

            return localEnvironment;
        }
    }
}