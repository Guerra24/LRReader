using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Models.Main
{
    public class ServerInfo
    {
        public string name { get; set; }
        public string motd { get; set; }
        public string version { get; set; }
        public string version_name { get; set; }
        [JsonConverter(typeof(BoolConverter))]
        public bool has_password { get; set; }
        [JsonConverter(typeof(BoolConverter))]
        public bool debug_mode { get; set; }
        [JsonConverter(typeof(BoolConverter))]
        public bool nofun_mode { get; set; }
        public string archives_per_page { get; set; }
        [JsonConverter(typeof(BoolConverter))]
        public bool server_resizes_images { get; set; }
    }


}
