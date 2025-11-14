using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("LubeTechList")]
public class LubeTech
{
    [Key]
    [Column("employeeID")]
    [StringLength(5)]
    public string EmployeeId { get; set; } = string.Empty;

    [Column("lastName")]
    [StringLength(22)]
    public string? LastName { get; set; }

    [Column("firstName")]
    [StringLength(14)]
    public string? FirstName { get; set; }

    [Column("MI")]
    [StringLength(1)]
    public string? MiddleInitial { get; set; }

    [Column("qualificationPassword")]
    [StringLength(8)]
    public string? QualificationPassword { get; set; }

    // Note: Navigation property removed because LubeTechQualification is keyless
    // Use raw SQL queries to get qualifications for this employee

    // Computed properties
    [NotMapped]
    public string FullName => $"{FirstName} {MiddleInitial} {LastName}".Trim().Replace("  ", " ");
}