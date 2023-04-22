using Newtonsoft.Json;

namespace KeyDropGiveawayBot.Models;

public class Winner
{
    public int? PrizeId { get; set; }
    [JsonProperty("userdata")] public UserData UserData { get; set; }
}