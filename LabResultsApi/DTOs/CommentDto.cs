using LabResultsApi.Models;

namespace LabResultsApi.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Area { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string Remark { get; set; } = string.Empty;

    public static CommentDto ToDto(Comment entity)
    {
        return new CommentDto
        {
            Id = entity.Id,
            Area = entity.Area,
            Type = entity.Type,
            Remark = entity.Remark
        };
    }

    public static Comment ToEntity(CommentDto dto)
    {
        return new Comment
        {
            Id = dto.Id,
            Area = dto.Area,
            Type = dto.Type,
            Remark = dto.Remark
        };
    }
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