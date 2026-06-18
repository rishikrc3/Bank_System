using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public class GBPResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, double> Rates { get; set; }
    }
}
