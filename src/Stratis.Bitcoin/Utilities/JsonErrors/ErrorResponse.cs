using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stratis.Bitcoin.Utilities.JsonErrors
{
    public class ErrorResponse
    {
        [JsonProperty(PropertyName = "errors")]
        public List<ErrorModel> Errors { get; set; }
    }

    public class ErrorWalletCreateResponse
    {
        [JsonProperty(PropertyName = "errors")]
        public WalletCreationErrors Errors { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "traceId")]
        public string TraceId { get; set; }
    }

    public class WalletCreationErrors
    {
        [JsonProperty(PropertyName = "password")]
        public List<string> Password { get; set; }

        [JsonProperty(PropertyName = "name")]
        public List<string> Name { get; set; }
    }

    public class ErrorModel
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
