namespace LabResultsApi.DTOs;

public class ParticleSubTypeCategoryDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; }
    public int SortOrder { get; set; }
    public List<ParticleSubTypeDto> SubTypes { get; set; } = new();
}

public class ParticleSubTypeDto
{
    public int ParticleSubTypeCategoryId { get; set; }
    public int Value { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; }
    public int SortOrder { get; set; }
}