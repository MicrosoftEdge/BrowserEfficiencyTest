using Newtonsoft.Json;

namespace TestingPower
{
    class UserInfo
    {
        [JsonProperty("Domain")]
        public string Domain { get; set; }

        [JsonProperty("UserName")]
        public string UserName { get; set; }

        [JsonProperty("Password")]
        public string PassWord { get; set; }
    }
}
