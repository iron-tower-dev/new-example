using LabResultsApi.Data;
using LabResultsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LabResultsApi.Tests.TestData;

public static class TestDataSeeder
{
    public static void SeedTestData(LabDbContext context)
    {
        context.Database.EnsureCreated();

        // Clear existing data
        ClearTestData(context);

        // Seed samples
        SeedSamples(context);

        // Seed tests
        SeedTests(context);

        // Seed equipment
        SeedEquipment(context);

        // Seed lookup data
        SeedLookupData(context);

        // Seed test readings
        SeedTestReadings(context);

        context.SaveChanges();
    }

    public static void ClearTestData(LabDbContext context)
    {
        // Clear in reverse dependency order using Entity Framework
        try
        {
            // Remove all entities using Entity Framework
            context.TestReadings.RemoveRange(context.TestReadings);
            context.NasLookup.RemoveRange(context.NasLookup);
            context.NlgiLookup.RemoveRange(context.NlgiLookup);
            context.Equipment.RemoveRange(context.Equipment);
            context.Tests.RemoveRange(context.Tests);
            context.UsedLubeSamples.RemoveRange(context.UsedLubeSamples);
            context.SaveChanges();
        }
        catch (Exception)
        {
            // Ignore errors for tables that don't exist in test database
        }
    }

    private static void SeedSamples(LabDbContext context)
    {
        var samples = new List<Sample>
        {
            new Sample
            {
                Id = 1,
                TagNumber = "TEST001",
                Component = "Engine",
                Location = "Main Plant",
                LubeType = "Engine Oil",
                SampleDate = DateTime.Now.AddDays(-1),
                Status = "Pending",
                QualityClass = "Q"
            },
            new Sample
            {
                Id = 2,
                TagNumber = "TEST002",
                Component = "Gearbox",
                Location = "Secondary Plant",
                LubeType = "Gear Oil",
                SampleDate = DateTime.Now.AddDays(-2),
                Status = "In Progress",
                QualityClass = "QAG"
            },
            new Sample
            {
                Id = 3,
                TagNumber = "GREASE001",
                Component = "Bearing",
                Location = "Main Plant",
                LubeType = "Lithium Grease",
                SampleDate = DateTime.Now.AddDays(-3),
                Status = "Pending",
                QualityClass = "Q"
            },
            new Sample
            {
                Id = 4,
                TagNumber = "HYDRAULIC001",
                Component = "Hydraulic System",
                Location = "Secondary Plant",
                LubeType = "Hydraulic Oil",
                SampleDate = DateTime.Now.AddDays(-4),
                Status = "In Progress",
                QualityClass = "TRAIN"
            }
        };

        context.UsedLubeSamples.AddRange(samples);
    }

    private static void SeedTests(LabDbContext context)
    {
        var tests = new List<Test>
        {
            new Test { TestId = 1, TestName = "TAN", TestDescription = "Total Acid Number", Active = true },
            new Test { TestId = 2, TestName = "Viscosity @ 40°C", TestDescription = "Kinematic Viscosity at 40°C", Active = true },
            new Test { TestId = 3, TestName = "Viscosity @ 100°C", TestDescription = "Kinematic Viscosity at 100°C", Active = true },
            new Test { TestId = 4, TestName = "Water-KF", TestDescription = "Water Content by Karl Fischer", Active = true },
            new Test { TestId = 5, TestName = "TBN", TestDescription = "Total Base Number", Active = true },
            new Test { TestId = 6, TestName = "Flash Point", TestDescription = "Flash Point Temperature", Active = true },
            new Test { TestId = 7, TestName = "Emission Spectroscopy", TestDescription = "Elemental Analysis", Active = true },
            new Test { TestId = 8, TestName = "Particle Count", TestDescription = "Particle Count Analysis", Active = true },
            new Test { TestId = 9, TestName = "Grease Penetration", TestDescription = "Grease Penetration Worked", Active = true },
            new Test { TestId = 10, TestName = "Grease Dropping Point", TestDescription = "Grease Dropping Point", Active = true },
            new Test { TestId = 11, TestName = "Inspect Filter", TestDescription = "Filter Inspection Analysis", Active = true },
            new Test { TestId = 12, TestName = "Ferrography", TestDescription = "Ferrographic Analysis", Active = true }
        };

        context.Tests.AddRange(tests);
    }

    private static void SeedEquipment(LabDbContext context)
    {
        var equipment = new List<Equipment>
        {
            new Equipment
            {
                Id = 1,
                EquipName = "Digital Thermometer #1",
                EquipType = "Thermometer",
                Val1 = 1.0,
                DueDate = DateTime.Now.AddDays(335),
                Exclude = false
            },
            new Equipment
            {
                Id = 2,
                EquipName = "Digital Thermometer #2",
                EquipType = "Thermometer",
                Val1 = 1.0,
                DueDate = DateTime.Now.AddDays(320),
                Exclude = false
            },
            new Equipment
            {
                Id = 3,
                EquipName = "Digital Thermometer #3 - OVERDUE",
                EquipType = "Thermometer",
                Val1 = 1.0,
                DueDate = DateTime.Now.AddDays(-35),
                Exclude = false
            },
            new Equipment
            {
                Id = 4,
                EquipName = "Digital Stopwatch #1",
                EquipType = "Stopwatch",
                Val1 = 1.0,
                DueDate = DateTime.Now.AddDays(305),
                Exclude = false
            },
            new Equipment
            {
                Id = 5,
                EquipName = "Viscometer Tube #1",
                EquipType = "Viscometer Tube",
                Val1 = 0.1234,
                DueDate = DateTime.Now.AddDays(275),
                Exclude = false
            },
            new Equipment
            {
                Id = 6,
                EquipName = "Low Range Tube",
                EquipType = "Viscometer Tube",
                Val1 = 0.0123,
                DueDate = DateTime.Now.AddDays(245),
                Exclude = false
            },
            new Equipment
            {
                Id = 7,
                EquipName = "High Range Tube",
                EquipType = "Viscometer Tube",
                Val1 = 1.2345,
                DueDate = DateTime.Now.AddDays(215),
                Exclude = false
            },
            new Equipment
            {
                Id = 8,
                EquipName = "Digital Barometer #1",
                EquipType = "Barometer",
                Val1 = 1.0,
                DueDate = DateTime.Now.AddDays(290),
                Exclude = false
            }
        };

        context.Equipment.AddRange(equipment);
    }

    private static void SeedLookupData(LabDbContext context)
    {
        // Seed NAS Lookup data
        var nasLookups = new List<NasLookup>
        {
            new NasLookup { Id = 1, Channel = 1, ValLo = 32, ValHi = 63, NAS = 6 },
            new NasLookup { Id = 2, Channel = 1, ValLo = 64, ValHi = 129, NAS = 7 },
            new NasLookup { Id = 3, Channel = 1, ValLo = 130, ValHi = 249, NAS = 8 },
            new NasLookup { Id = 4, Channel = 1, ValLo = 250, ValHi = 499, NAS = 9 },
            new NasLookup { Id = 5, Channel = 1, ValLo = 500, ValHi = 999, NAS = 10 }
        };

        context.NasLookup.AddRange(nasLookups);

        // Seed NLGI Lookup data
        var nlgiLookups = new List<NlgiLookup>
        {
            new NlgiLookup { ID = 1, LowerValue = 445, UpperValue = 475, NLGIValue = "000" },
            new NlgiLookup { ID = 2, LowerValue = 400, UpperValue = 430, NLGIValue = "00" },
            new NlgiLookup { ID = 3, LowerValue = 355, UpperValue = 385, NLGIValue = "0" },
            new NlgiLookup { ID = 4, LowerValue = 310, UpperValue = 340, NLGIValue = "1" },
            new NlgiLookup { ID = 5, LowerValue = 265, UpperValue = 295, NLGIValue = "2" },
            new NlgiLookup { ID = 6, LowerValue = 220, UpperValue = 250, NLGIValue = "3" },
            new NlgiLookup { ID = 7, LowerValue = 175, UpperValue = 205, NLGIValue = "4" },
            new NlgiLookup { ID = 8, LowerValue = 130, UpperValue = 160, NLGIValue = "5" },
            new NlgiLookup { ID = 9, LowerValue = 85, UpperValue = 115, NLGIValue = "6" }
        };

        context.NlgiLookup.AddRange(nlgiLookups);

        // Skip particle type definitions for now to focus on core functionality
    }

    private static void SeedTestReadings(LabDbContext context)
    {
        // Skip seeding keyless entities in tests to avoid Entity Framework tracking issues
        // TestReadings is a keyless entity and cannot be tracked by Entity Framework
        // In real scenarios, this data would be inserted via raw SQL or stored procedures
    }

    public static void SeedEmissionSpectroscopyData(LabDbContext context)
    {
        // Skip seeding keyless entities in tests to avoid Entity Framework tracking issues
        // EmSpectro is a keyless entity and cannot be tracked by Entity Framework
        // In real scenarios, this data would be inserted via raw SQL or stored procedures
    }
}