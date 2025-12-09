using LabResultsApi.Models;

namespace LabResultsApi.DTOs;

public class ParticleTypeDefinitionDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image1 { get; set; } = string.Empty;
    public string Image2 { get; set; } = string.Empty;
    public string? Active { get; set; }
    public int? SortOrder { get; set; }

    public static ParticleTypeDefinitionDto ToDto(ParticleTypeDefinition entity)
    {
        return new ParticleTypeDefinitionDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Description = entity.Description,
            Image1 = entity.Image1,
            Image2 = entity.Image2,
            Active = entity.Active,
            SortOrder = entity.SortOrder
        };
    }
}

public class ParticleSubTypeDefinitionDto
{
    public int ParticleSubTypeCategoryId { get; set; }
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Active { get; set; }
    public int? SortOrder { get; set; }

    public static ParticleSubTypeDefinitionDto ToDto(ParticleSubTypeDefinition entity)
    {
        return new ParticleSubTypeDefinitionDto
        {
            ParticleSubTypeCategoryId = entity.ParticleSubTypeCategoryId,
            Value = entity.Value,
            Description = entity.Description,
            Active = entity.Active,
            SortOrder = entity.SortOrder
        };
    }
}

public class ParticleSubTypeCategoryDefinitionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Active { get; set; }
    public int? SortOrder { get; set; }

    public static ParticleSubTypeCategoryDefinitionDto ToDto(ParticleSubTypeCategoryDefinition entity)
    {
        return new ParticleSubTypeCategoryDefinitionDto
        {
            Id = entity.Id,
            Description = entity.Description,
            Active = entity.Active,
            SortOrder = entity.SortOrder
        };
    }
}
