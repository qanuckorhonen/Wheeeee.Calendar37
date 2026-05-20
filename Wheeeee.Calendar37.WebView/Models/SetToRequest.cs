using Newtonsoft.Json;

namespace Wheeeee.Calendar37.WebView.Models
{
    public class SetToRequest
    {
        [JsonProperty(PropertyName = "setTo")] public string SetTo { get; set; }
        [JsonProperty(PropertyName = "personID")] public int PersonID { get; set; }
        [JsonProperty(PropertyName = "dateID")] public int DateID { get; set; }
        [JsonProperty(PropertyName = "orchestraID")] public int OrchestraID { get; set; }
        [JsonProperty(PropertyName = "index")] public int Index { get; internal set; }
    }
}
