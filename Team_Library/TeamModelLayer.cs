using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TEAM_Library
{
    public class TeamModelLayer
    {
        /// <summary>
        /// Name of the layer, used for identification and must be unique.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public string DefaultName => "MyNewLayer";

        [JsonIgnore]
        public string DefaultDescription => "Click on the tab to create a new layer.\r\n\r\nLayers are saved against the active environment.";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TeamModelLayer()
        {
            Name = DefaultName;
            Description = DefaultDescription;
        }

        public TeamModelLayer(string layerName)
        {
            Name = layerName;
        }
    }
}
