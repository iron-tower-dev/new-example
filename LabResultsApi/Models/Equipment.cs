using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("M_And_T_Equip")]
public class Equipment
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("EquipType")]
    [MaxLength(30)]
    public string EquipType { get; set; } = string.Empty;

    [Column("EquipName")]
    [MaxLength(30)]
    public string? EquipName { get; set; }

    [Column("exclude")]
    public bool? Exclude { get; set; }

    [Column("testID")]
    public short? TestId { get; set; }

    [Column("DueDate")]
    public DateTime? DueDate { get; set; }

    [Column("Comments")]
    [MaxLength(250)]
    public string? Comments { get; set; }

    [Column("Val1")]
    public double? Val1 { get; set; }

    [Column("Val2")]
    public double? Val2 { get; set; }

    [Column("Val3")]
    public double? Val3 { get; set; }

    [Column("Val4")]
    public double? Val4 { get; set; }
}