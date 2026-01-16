namespace Shared.MongoDB.Dto;

public class PagingObject<T>    
{
    public List<T>? Data { get; set; }
    public long TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
