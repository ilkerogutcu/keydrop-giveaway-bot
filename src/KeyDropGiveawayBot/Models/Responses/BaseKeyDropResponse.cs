namespace KeyDropGiveawayBot.Models.Responses;

public class BaseKeyDropResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string? Message { get; set; }
}