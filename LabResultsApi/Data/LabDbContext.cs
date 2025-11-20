using Microsoft.EntityFrameworkCore;
using LabResultsApi.Models;

namespace LabResultsApi.Data;

public class LabDbContext : DbContext
{
    public LabDbContext(DbContextOptions<LabDbContext> options) : base(options)
    {
    }

    // Entities with primary keys
    public DbSet<Sample> UsedLubeSamples { get; set; }
    public DbSet<LubeSamplingPoint> LubeSamplingPoints { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<ParticleTypeDefinition> ParticleTypeDefinitions { get; set; }
    public DbSet<ParticleSubTypeCategoryDefinition> ParticleSubTypeCategoryDefinitions { get; set; }
    public DbSet<ParticleSubTypeDefinition> ParticleSubTypeDefinitions { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<NasLookup> NasLookup { get; set; }
    public DbSet<NlgiLookup> NlgiLookup { get; set; }

    // Authentication entities
    public DbSet<LubeTech> LubeTechs { get; set; }
    public DbSet<LubeTechQualification> LubeTechQualifications { get; set; }
    public DbSet<Reviewer> Reviewers { get; set; }
    
    // Test stand entities
    public DbSet<TestStand> TestStands { get; set; }
    public DbSet<TestStandMapping> TestStandMappings { get; set; }
    
    // Audit trail
    public DbSet<AuditLog> AuditLogs { get; set; }

    // Keyless entities (tables without primary keys)
    public DbSet<TestReading> TestReadings { get; set; }
    public DbSet<EmissionSpectroscopy> EmSpectro { get; set; }
    public DbSet<ParticleType> ParticleTypes { get; set; }
    public DbSet<ParticleSubType> ParticleSubTypes { get; set; }
    public DbSet<InspectFilter> InspectFilters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure authentication entities
        modelBuilder.Entity<LubeTech>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);
            entity.ToTable("LubeTechList");
            // Note: No navigation to LubeTechQualification since it's keyless
        });

        modelBuilder.Entity<LubeTechQualification>(entity =>
        {
            entity.HasNoKey(); // This table doesn't have a primary key
            entity.ToTable("LubeTechQualification");
        });

        modelBuilder.Entity<Reviewer>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);
            entity.ToTable("ReviewerList");
        });

        // Configure test stand entities
        modelBuilder.Entity<TestStand>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("TestStand");
        });

        modelBuilder.Entity<TestStandMapping>(entity =>
        {
            entity.HasKey(e => new { e.TestStandId, e.TestId });
            entity.ToTable("TestStandMapping");
            
            entity.HasOne(e => e.TestStand)
                .WithMany()
                .HasForeignKey(e => e.TestStandId);
                
            entity.HasOne(e => e.Test)
                .WithMany()
                .HasForeignKey(e => e.TestId);
        });

        // Configure keyless entities
        modelBuilder.Entity<TestReading>().HasNoKey();
        modelBuilder.Entity<EmissionSpectroscopy>().HasNoKey();
        modelBuilder.Entity<ParticleType>().HasNoKey();
        modelBuilder.Entity<ParticleSubType>().HasNoKey();
        modelBuilder.Entity<InspectFilter>().HasNoKey();

        // Configure lookup entities with keys
        modelBuilder.Entity<NasLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("NAS_lookup");
        });

        modelBuilder.Entity<NlgiLookup>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("NLGILookup");
        });

        // Configure table mappings (already done via attributes, but can be done here too)
        modelBuilder.Entity<Sample>().ToTable("UsedLubeSamples");
        modelBuilder.Entity<Test>().ToTable("Test");
        modelBuilder.Entity<TestReading>().ToTable("TestReadings");
        modelBuilder.Entity<EmissionSpectroscopy>().ToTable("EmSpectro");

        modelBuilder.Entity<ParticleTypeDefinition>().ToTable("ParticleTypeDefinition");
        modelBuilder.Entity<ParticleSubTypeCategoryDefinition>().ToTable("ParticleSubTypeCategoryDefinition");
        modelBuilder.Entity<ParticleSubTypeDefinition>().ToTable("ParticleSubTypeDefinition");
        modelBuilder.Entity<ParticleType>().ToTable("ParticleType");
        modelBuilder.Entity<ParticleSubType>().ToTable("ParticleSubType");
        modelBuilder.Entity<InspectFilter>().ToTable("InspectFilter");
        modelBuilder.Entity<Comment>().ToTable("Comments");

        // Configure any additional constraints or relationships
        modelBuilder.Entity<Sample>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TagNumber).HasMaxLength(50);
            entity.Property(e => e.Component).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.LubeType).HasMaxLength(50);
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId);
            entity.Property(e => e.TestName).HasMaxLength(100);
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EquipType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.EquipName).HasMaxLength(30);
            entity.Property(e => e.Comments).HasMaxLength(250);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Area).HasMaxLength(4).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(5);
            entity.Property(e => e.Remark).HasMaxLength(80).IsRequired();
        });

        // Configure composite keys for particle-related entities
        modelBuilder.Entity<ParticleSubTypeDefinition>(entity =>
        {
            entity.HasKey(e => new { e.ParticleSubTypeCategoryId, e.Value });
        });
    }
}