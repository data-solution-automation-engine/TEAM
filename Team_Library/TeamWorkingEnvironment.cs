using Newtonsoft.Json;

namespace TEAM
{

    public class TeamWorkingEnvironment
    {
        public string environmentInternalId { get; set; }
        public string environmentName { get; set; }
        public string environmentKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string environmentNotes { get; set; }
    }
}
