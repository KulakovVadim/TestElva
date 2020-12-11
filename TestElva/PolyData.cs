using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TestElva
{
    class PolyData<T>
    {
        public PolyData() { }
        public PolyData(int place_id, string license)
        {
            this.place_id = place_id;
            this.licence = license;
        }

        public int place_id { get; set; }
        public string licence { get; set; }
        public string osm_type { get; set; }
        public int osm_id { get; set; }
        public string[] boundingbox { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
        [JsonPropertyName("class")]
        public string Class { get; set; }
        public string type { get; set; }
        public double importance { get; set; }
        public string icon { get; set; }
        public T geojson { get; set; }
    }
}
