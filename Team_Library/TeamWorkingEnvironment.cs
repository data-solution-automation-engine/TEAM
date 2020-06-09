using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TEAM
{

    public class TeamWorkingEnvironment
    {
        public string environmentName { get; set; }
        public string environmentKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string environmentNotes { get; set; }
    }
}
