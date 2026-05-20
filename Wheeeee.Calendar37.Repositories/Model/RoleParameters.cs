using System.Text.Json.Serialization;
using Wheeeee.Calendar37.Core.Enums;

namespace Wheeeee.Calendar37.Repositories.Model
{
    public class RoleParameters
    {
        [JsonPropertyName("seesOthers")] public SeesOthers SeesOthers { get; set; }
        [JsonPropertyName("canEditOthers")] public CanEditOthers CanEditOthers { get; set; }
    }
}
