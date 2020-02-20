using Newtonsoft.Json;

namespace Stratis.Bitcoin.Features.Wallet.Models
{
    /// <summary>
    /// Class containing details of a signature message.
    /// </summary>
    public class SignMessageResult
    {
        [JsonProperty(PropertyName = "signedAddress")]
        public string SignedAddress { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}
