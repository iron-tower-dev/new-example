namespace LabResultsApi.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Area { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string Remark { get; set; } = string.Empty;
}

public class CacheStatusDto
{
    public Dictionary<string, CacheInfo> CacheEntries { get; set; } = new();
}

public class CacheInfo
{
    public bool IsLoaded { get; set; }
    public DateTime? LastRefreshed { get; set; }
    public int ItemCount { get; set; }
    public TimeSpan? ExpiresIn { get; set; }
}