using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("LubeTechQualification")]
public class LubeTechQualification
{
    [Column("employeeID")]
    [StringLength(5)]
    public string EmployeeId { get; set; } = string.Empty;

    [Column("testStandID")]
    public short? TestStandId { get; set; }

    [Column("testStand")]
    [StringLength(50)]
    public string? TestStand { get; set; }

    [Column("qualificationLevel")]
    [StringLength(10)]
    public string? QualificationLevel { get; set; }

    // Note: Navigation property removed because this entity is keyless
    // Use raw SQL queries to join with LubeTech when needed
}

public enum QualificationLevel
{
    TRAIN,
    Q_QAG, // Q/QAG
    MicrE
}

public static class QualificationLevelExtensions
{
    public static QualificationLevel ParseQualificationLevel(string? level)
    {
        return level switch
        {
            "Q/QAG" => QualificationLevel.Q_QAG,
            "TRAIN" => QualificationLevel.TRAIN,
            "MicrE" => QualificationLevel.MicrE,
            _ => QualificationLevel.TRAIN
        };
    }

    public static string ToDisplayString(this QualificationLevel level)
    {
        return level switch
        {
            QualificationLevel.Q_QAG => "Q/QAG",
            QualificationLevel.TRAIN => "TRAIN",
            QualificationLevel.MicrE => "MicrE",
            _ => "TRAIN"
        };
    }
}