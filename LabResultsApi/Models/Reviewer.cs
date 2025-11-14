using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("ReviewerList")]
public class Reviewer
{
    [Key]
    [Column("employeeID")]
    [StringLength(5)]
    public string EmployeeId { get; set; } = string.Empty;

    [Column("lastName")]
    [StringLength(22)]
    public string LastName { get; set; } = string.Empty;

    [Column("firstName")]
    [StringLength(14)]
    public string FirstName { get; set; } = string.Empty;

    [Column("MI")]
    [StringLength(1)]
    public string? MiddleInitial { get; set; }

    [Column("reviewerPassword")]
    [StringLength(8)]
    public string? ReviewerPassword { get; set; }

    [Column("level")]
    public short Level { get; set; } = 1;

    // Computed properties
    [NotMapped]
    public string FullName => $"{FirstName} {MiddleInitial} {LastName}".Trim().Replace("  ", " ");
}