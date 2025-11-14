using AutoMapper;
using LabResultsApi.DTOs;
using LabResultsApi.DTOs.Migration;
using LabResultsApi.Models;
using LabResultsApi.Models.Migration;

namespace LabResultsApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Sample mappings
        CreateMap<Sample, SampleDto>();
        CreateMap<SampleDto, Sample>();

        // Test mappings
        CreateMap<Test, TestDto>();
        CreateMap<TestDto, Test>();

        // Equipment mappings
        CreateMap<Equipment, EquipmentDto>();
        CreateMap<EquipmentDto, Equipment>();
        
        CreateMap<Equipment, EquipmentSelectionDto>()
            .ForMember(dest => dest.DisplayText, opt => opt.Ignore())
            .ForMember(dest => dest.IsDueSoon, opt => opt.Ignore())
            .ForMember(dest => dest.IsOverdue, opt => opt.Ignore())
            .ForMember(dest => dest.CalibrationValue, opt => opt.Ignore());

        // EmissionSpectroscopy mappings
        CreateMap<EmissionSpectroscopy, EmissionSpectroscopyDto>()
            .ForMember(dest => dest.ScheduleFerrography, opt => opt.Ignore());
        
        CreateMap<EmissionSpectroscopyCreateDto, EmissionSpectroscopy>()
            .ForMember(dest => dest.TrialDate, opt => opt.Ignore());
        
        CreateMap<EmissionSpectroscopyUpdateDto, EmissionSpectroscopy>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TestId, opt => opt.Ignore())
            .ForMember(dest => dest.TrialNum, opt => opt.Ignore())
            .ForMember(dest => dest.TrialDate, opt => opt.Ignore());

        // Particle Type mappings
        CreateMap<ParticleTypeDefinition, ParticleTypeDefinitionDto>();
        CreateMap<ParticleSubTypeDefinition, ParticleSubTypeDefinitionDto>();
        CreateMap<ParticleSubTypeCategoryDefinition, ParticleSubTypeCategoryDefinitionDto>();

        // Comment mappings
        CreateMap<Comment, CommentDto>();

        // Migration mappings
        CreateMap<MigrationResult, MigrationStatusDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RecentErrors, opt => opt.MapFrom(src => src.Errors.TakeLast(5)))
            .ForMember(dest => dest.CurrentOperation, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedTimeRemaining, opt => opt.MapFrom(src => src.Statistics.EstimatedTimeRemaining));

        CreateMap<MigrationResult, MigrationReportDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.GeneratedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Statistics))
            .ForMember(dest => dest.Recommendations, opt => opt.Ignore());

        CreateMap<MigrationStatistics, MigrationStatisticsDto>();
        CreateMap<MigrationStatistics, MigrationSummaryDto>()
            .ForMember(dest => dest.Success, opt => opt.Ignore())
            .ForMember(dest => dest.OverallProgressPercentage, opt => opt.MapFrom(src => src.ProgressPercentage));

        CreateMap<MigrationError, MigrationErrorDto>()
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level.ToString()));

        CreateMap<SeedingResult, SeedingReportDto>()
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Success))
            .ForMember(dest => dest.TableReports, opt => opt.MapFrom(src => src.TableResults));

        CreateMap<TableSeedingResult, TableSeedingReportDto>();

        CreateMap<Models.Migration.ValidationResult, ValidationReportDto>()
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Success))
            .ForMember(dest => dest.MatchPercentage, opt => opt.MapFrom(src => src.Summary.MatchPercentage))
            .ForMember(dest => dest.QueryReports, opt => opt.MapFrom(src => src.Results));

        CreateMap<QueryComparisonResult, QueryComparisonReportDto>()
            .ForMember(dest => dest.DiscrepancyCount, opt => opt.MapFrom(src => src.Discrepancies.Count))
            .ForMember(dest => dest.PerformanceRatio, opt => opt.MapFrom(src => 
                src.LegacyExecutionTime.TotalMilliseconds > 0 
                    ? src.CurrentExecutionTime.TotalMilliseconds / src.LegacyExecutionTime.TotalMilliseconds 
                    : 0));

        CreateMap<ValidationSummary, ValidationSummaryDto>();

        CreateMap<LabResultsApi.Models.Migration.AuthRemovalResult, AuthRemovalReportDto>()
            .ForMember(dest => dest.RemovedComponentsCount, opt => opt.MapFrom(src => src.RemovedComponents.Count))
            .ForMember(dest => dest.ModifiedFilesCount, opt => opt.MapFrom(src => src.ModifiedFiles.Count))
            .ForMember(dest => dest.BackupFilesCount, opt => opt.MapFrom(src => src.BackupFiles.Count));

        // Service to DTO mappings
        CreateMap<LabResultsApi.Services.BackupResult, LabResultsApi.DTOs.Migration.BackupResult>()
            .ForMember(dest => dest.BackupId, opt => opt.MapFrom(src => src.BackupId.ToString()));
        


        // DTO to Model mappings
        CreateMap<MigrationOptionsDto, MigrationOptions>()
            .ForMember(dest => dest.OperationTimeout, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.OperationTimeoutMinutes)));

        CreateMap<SeedingOptionsDto, SeedingOptions>()
            .ForMember(dest => dest.CommandTimeout, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.CommandTimeoutMinutes)));

        CreateMap<ValidationOptionsDto, ValidationOptions>()
            .ForMember(dest => dest.QueryTimeout, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.QueryTimeoutMinutes)));

        CreateMap<AuthRemovalOptionsDto, AuthRemovalOptions>();
    }
}