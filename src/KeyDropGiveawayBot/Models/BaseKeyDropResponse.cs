namespace KeyDropGiveawayBot.Models;

public class BaseKeyDropResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public class PaginatedKeyDropResponse<T> : BaseKeyDropResponse<List<T>>
{
    public Pagination? Pagination { get; set; }
}

public class Pagination
{
    public int? ItemsCount { get; set; }
    public int? ItemsPerPage { get; set; }
    public int? CurrentPage { get; set; }
}