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
}

public class ParticleSubTypeDefinitionDto
{
    public int ParticleSubTypeCategoryId { get; set; }
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Active { get; set; }
    public int? SortOrder { get; set; }
}

public class ParticleSubTypeCategoryDefinitionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Active { get; set; }
    public int? SortOrder { get; set; }
}