namespace LabResultsApi.DTOs;

public class ParticleSubTypeCategoryDefinitionDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; }
    public int SortOrder { get; set; }
    public List<ParticleSubTypeDefinitionDto> SubTypes { get; set; } = new();
}

public class ParticleSubTypeDefinitionDto
{
    public int ParticleSubTypeCategoryId { get; set; }
    public int Value { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; }
    public int SortOrder { get; set; }
}