-- Seed script for Particle Analysis Control Data
-- This script populates the database tables with data from the CSV files
-- Data source: /home/derrick/projects/testing/new-example/db-seeding/*.csv

-- Clear existing data (optional - remove if you want to preserve existing data)
DELETE FROM ParticleSubType;
DELETE FROM ParticleType;
DELETE FROM ParticleSubTypeDefinition;
DELETE FROM ParticleTypeDefinition;
DELETE FROM ParticleSubTypeCategoryDefinition;

-- Insert Particle Sub-Type Categories (from particle-sub-type-category-definition.csv)
SET IDENTITY_INSERT ParticleSubTypeCategoryDefinition ON;
INSERT INTO ParticleSubTypeCategoryDefinition (ID, Description, Active, SortOrder) VALUES
(1, 'Severity', '1', 7),
(2, 'Heat', '1', 0),
(3, 'Concentration', '1', 1),
(4, 'Size, Ave', '1', 2),
(5, 'Size, Max', '1', 3),
(6, 'Color', '1', 4),
(7, 'Texture', '1', 5),
(8, 'Composition', '1', 6);
SET IDENTITY_INSERT ParticleSubTypeCategoryDefinition OFF;

-- Insert Particle Sub-Type Definitions (from particle-sub-type-definition.csv)
INSERT INTO ParticleSubTypeDefinition (ParticleSubTypeCategoryID, Value, Description, Active, SortOrder) VALUES
-- Severity (Category 1)
(1, 1, '1', '1', 0),
(1, 2, '2', '1', 1),
(1, 3, '3', '1', 2),
(1, 4, '4', '1', 3),
-- Heat (Category 2)
(2, 1, 'NA', '1', 0),
(2, 2, 'Blue', '1', 1),
(2, 3, 'Straw', '1', 2),
(2, 4, 'Purple', '1', 3),
(2, 5, 'No Change', '1', 4),
(2, 6, 'Melted', '1', 5),
(2, 7, 'Charred', '1', 6),
-- Concentration (Category 3)
(3, 1, 'Few', '1', 0),
(3, 2, 'Moderate', '1', 1),
(3, 3, 'Many', '1', 2),
(3, 4, 'Heavy', '1', 3),
-- Size, Ave (Category 4)
(4, 1, 'Fine, <5μm', '1', 0),
(4, 2, 'Small, 5 to 15μm', '1', 1),
(4, 3, 'Medium, 15 to 40μm', '1', 2),
(4, 4, 'Large, 40 to 100μm', '1', 3),
(4, 5, 'Huge, >100μm', '1', 4),
-- Size, Max (Category 5)
(5, 1, 'Fine, <5μm', '1', 0),
(5, 2, 'Small, 5 to 15μm', '1', 1),
(5, 3, 'Medium, 15 to 40μm', '1', 2),
(5, 4, 'Large, 40 to 100μm', '1', 3),
(5, 5, 'Huge, >100μm', '1', 4),
-- Color (Category 6)
(6, 1, 'Red', '1', 0),
(6, 2, 'Black', '1', 1),
(6, 3, 'Tempered', '1', 2),
(6, 4, 'Metallic', '1', 3),
(6, 5, 'Straw', '1', 4),
(6, 6, 'Copper', '1', 5),
(6, 7, 'Brass', '1', 6),
(6, 8, 'Other Color', '1', 7),
-- Texture (Category 7)
(7, 1, 'Bright or Reflective', '1', 0),
(7, 2, 'Dull or Oxidized', '1', 1),
(7, 3, 'Pitted', '1', 2),
(7, 4, 'Striated', '1', 3),
(7, 5, 'Smeared', '1', 4),
(7, 6, 'Amorphous', '1', 5),
(7, 7, 'Other Texture', '1', 6),
-- Composition (Category 8)
(8, 1, 'Ferrous Metal', '1', 0),
(8, 2, 'Cupric Metal', '1', 1),
(8, 3, 'Other Metal', '1', 2),
(8, 4, 'Dust', '1', 3),
(8, 5, 'Organic', '1', 4),
(8, 6, 'Sludge', '1', 5),
(8, 7, 'Paint Chips', '1', 6),
(8, 8, 'Other Material', '1', 7);

-- Insert Particle Type Definitions (from particle-type-definition.csv)
SET IDENTITY_INSERT ParticleTypeDefinition ON;
INSERT INTO ParticleTypeDefinition (ID, Type, Description, Image1, Image2, Active, SortOrder) VALUES
(1, 'Rubbing Wear (platlet)', 'Free metal particles with smooth surfaces. (hints: a] -- any shape with exception of curved or curled are rubbing wear particles. b] -- less than 15 micron )', 'rwp1.jpg', 'rwp2.jpg', '1', 0),
(2, 'Rubbing Wear', 'Flat platlets with similar length and width dimensions (hints: a] -- thin, usually with a major dimension to thickness ration of 5:1 to 10:1 or more. b] -- any shape with exception of curved or curled are considered rubbing wear particles. The small curved/curled particles found are considered to be abrasive particles.  c] -- less than 15 micron in size )', 'rw1.jpg', 'rw2.jpg', '1', 1),
(3, 'Black Oxide', 'Finely divided black clusters which may have small dots of blue or orange color. 800x magnification required to identify these particles', 'bo1.jpg', 'bo2.jpg', '1', 2),
(4, 'Dark Metallo-Oxide', 'Partially oxidized ferrous particles with color changes due to high heat at time of generation', 'dmo1.jpg', 'dmo2.jpg', '1', 3),
(5, 'Abrasive Wear', 'Long thin particles which in some cases are wire-like particles in the form of loops or spirals', 'aw1.jpg', 'aw2.jpg', '1', 4),
(6, 'Rework', 'Large very thin free metal particles often measured in the range of 20-50 micron in the major dimension (hints: a] -- these particles often have holes and have been known as laminar or fatigue particles.  B] -- particles are larger than 15 micron with a major dimension to thickness ratio of greater than 30:1 )', 're1.jpg', 're2.jpg', '1', 5),
(7, 'Severe Wear Particles', 'Free metal particles which have surface striations and straight edges. (hints: a] -- major dimension is greater than 15 micron.  b] shape factor is between 5:1 to 30:1 c] -- a red oxide form exists which appears grey in reflected white light but is dull/reddish in white transmitted light)', 'swp1.jpg', 'swp2.jpg', '1', 6),
(8, 'Chunks', 'Metal particles larger than 5 micron with a shape factor (major dimension to thickness) of less than 5:1', 'ch1.jpg', 'ch2.jpg', '1', 7),
(9, 'Spheres', 'Round and small free metal particles. (hint: particles less than 5 micron in size are likely due to a fatigue failure.  Larger spheres are likely a contaminant)', 'sp1.jpg', 'sp2.jpg', '1', 8),
(10, 'Red Oxide (Rust)', 'Particles are a changed form of iron (hint: appear orange in white reflected light)', 'ro1.jpg', 'ro2.jpg', '1', 9),
(11, 'Non Ferrous Metal', 'Free metal particles composed of any metal but iron. (hint: all common non ferrous particles except nickel behave nonmagnetically)', 'nfm1.jpg', 'nfm2.jpg', '1', 10),
(12, 'Corrosive', 'Extremely small partially oxidized particles which result from a corrosive attack (hint: these particles generally collect on the exit end of a ferrography slide)', 'co1.jpg', 'co2.jpg', '1', 11),
(13, 'Non Metallic Crystalline', 'Crystal particles that appear bright in polarized light (hint: they may be single but often appear as agglomerates)', 'nmc1.jpg', 'nmc2.jpg', '1', 12),
(14, 'Non Metallic Amorphous', 'Particles that are transparent and also do not appear bright in polarized light', 'nma1.jpg', 'nma2.jpg', '1', 13),
(15, 'Friction Polymer', 'Particle embedded in an amorphous matrix', 'fp1.jpg', 'fp2.jpg', '1', 14),
(16, 'Fibers', 'Long and thin non metallic particles', 'fi1.jpg', 'fi2.jpg', '1', 15);
SET IDENTITY_INSERT ParticleTypeDefinition OFF;

-- Verify the data was inserted correctly
SELECT 'ParticleSubTypeCategoryDefinition' as TableType, COUNT(*) as RecordCount FROM ParticleSubTypeCategoryDefinition WHERE Active = '1'
UNION ALL
SELECT 'ParticleSubTypeDefinition' as TableType, COUNT(*) as RecordCount FROM ParticleSubTypeDefinition WHERE Active = '1'
UNION ALL
SELECT 'ParticleTypeDefinition' as TableType, COUNT(*) as RecordCount FROM ParticleTypeDefinition WHERE Active = '1';

-- Show the complete data structure for verification
SELECT 
    c.ID as CategoryID,
    c.Description as Category,
    c.SortOrder as CategorySort,
    s.Value,
    s.Description as SubType,
    s.SortOrder as SubTypeSort
FROM ParticleSubTypeCategoryDefinition c
LEFT JOIN ParticleSubTypeDefinition s ON c.ID = s.ParticleSubTypeCategoryID
WHERE c.Active = '1' AND (s.Active = '1' OR s.Active IS NULL)
ORDER BY c.SortOrder, s.SortOrder;

-- Show particle type definitions
SELECT 
    ID,
    Type,
    LEFT(Description, 100) + '...' as Description_Preview,
    Image1,
    Image2,
    Active,
    SortOrder
FROM ParticleTypeDefinition
WHERE Active = '1'
ORDER BY SortOrder;