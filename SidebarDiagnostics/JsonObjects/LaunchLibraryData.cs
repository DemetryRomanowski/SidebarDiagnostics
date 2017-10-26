using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SidebarDiagnostics.JsonObjects
{
    public partial class LaunchLib
    {
        [JsonProperty("launches")]
        public List<Launch> Launches { get; set; }

        [JsonProperty("count")]
        public Int64 Count { get; set; }

        [JsonProperty("offset")]
        public Int64 Offset { get; set; }

        [JsonProperty("total")]
        public Int64 Total { get; set; }
    }

    public partial class Launch
    {
        [JsonProperty("name")]
        public String Name { get; set; }

        [JsonProperty("tbddate")]
        public Int64 Tbddate { get; set; }

        [JsonProperty("id")]
        public Int64 Id { get; set; }

        [JsonProperty("net")]
        public String Net { get; set; }

        [JsonProperty("tbdtime")]
        public Int64 Tbdtime { get; set; }
    }

    public partial class LaunchLib
    {
        public static LaunchLib FromJson(String json) => JsonConvert.DeserializeObject<LaunchLib>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static String ToJson(this LaunchLib self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}