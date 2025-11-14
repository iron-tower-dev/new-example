-- Seed particle analysis data
USE LabResultsDb;
GO

-- Clear existing data
DELETE FROM ParticleSubTypeDefinition;
DELETE FROM ParticleSubTypeCategoryDefinition;
DELETE FROM ParticleTypeDefinition;
GO

-- Seed ParticleTypeDefinition
INSERT INTO ParticleTypeDefinition (Type, Description, Image1, Image2, Active, SortOrder) VALUES
('Rubbing Wear (platlet)', 'Free metal particles with smooth surfaces. (hints: a] -- any shape with exception of curved or curled are rubbing wear particles. b] -- less than 15 micron )', 'rwp1.jpg', 'rwp2.jpg', '1', 0),
('Rubbing Wear', 'Flat platlets with similar length and width dimensions (hints: a] -- thin, usually with a major dimension to thickness ration of 5:1 to 10:1 or more. b] -- any shape with exception of curved or curled are considered rubbing wear particles. The small curved/curled particles found are considered to be abrasive particles.  c] -- less than 15 micron in size )', 'rw1.jpg', 'rw2.jpg', '1', 1),
('Black Oxide', 'Finely divided black clusters which may have small dots of blue or orange color. 800x magnification required to identify these particles', 'bo1.jpg', 'bo2.jpg', '1', 2),
('Dark Metallo-Oxide', 'Partially oxidized ferrous particles with color changes due to high heat at time of generation', 'dmo1.jpg', 'dmo2.jpg', '1', 3),
('Abrasive Wear', 'Long thin particles which in some cases are wire-like particles in the form of loops or spirals', 'aw1.jpg', 'aw2.jpg', '1', 4),
('Rework', 'Large very thin free metal particles often measured in the range of 20-50 micron in the major dimension (hints: a] -- these particles often have holes and have been known as laminar or fatigue particles.  B] -- particles are larger than 15 micron with a major dimension to thickness ratio of greater than 30:1 )', 're1.jpg', 're2.jpg', '1', 5),
('Severe Wear Particles', 'Free metal particles which have surface striations and straight edges. (hints: a] -- major dimension is greater than 15 micron.  b] shape factor is between 5:1 to 30:1 c] -- a red oxide form exists which appears grey in reflected white light but is dull/reddish in white transmitted light)', 'swp1.jpg', 'swp2.jpg', '1', 6),
('Chunks', 'Metal particles larger than 5 micron with a shape factor (major dimension to thickness) of less than 5:1', 'ch1.jpg', 'ch2.jpg', '1', 7),
('Spheres', 'Round and small free metal particles. (hint: particles less than 5 micron in size are likely due to a fatigue failure.  Larger spheres are likely a contaminant)', 'sp1.jpg', 'sp2.jpg', '1', 8);

PRINT 'Inserted particle type definitions.';
GO

-- Seed ParticleSubTypeCategoryDefinition
INSERT INTO ParticleSubTypeCategoryDefinition (Description, Active, SortOrder) VALUES
('Severity', '1', 7),
('Heat', '1', 0),
('Concentration', '1', 1),
('Size, Ave', '1', 2),
('Size, Max', '1', 3),
('Color', '1', 4),
('Texture', '1', 5),
('Composition', '1', 6);

PRINT 'Inserted particle sub-type categories.';
GO

-- Seed ParticleSubTypeDefinition
INSERT INTO ParticleSubTypeDefinition (ParticleSubTypeCategoryID, Value, Description, Active, SortOrder) VALUES
-- Severity
(1, 1, '1', '1', 0),
(1, 2, '2', '1', 1),
(1, 3, '3', '1', 2),
(1, 4, '4', '1', 3),
-- Heat
(2, 1, 'NA', '1', 0),
(2, 2, 'Blue', '1', 1),
(2, 3, 'Straw', '1', 2),
(2, 4, 'Purple', '1', 3),
(2, 5, 'No Change', '1', 4),
(2, 6, 'Melted', '1', 5),
(2, 7, 'Charred', '1', 6),
-- Concentration
(3, 1, 'Few', '1', 0),
(3, 2, 'Moderate', '1', 1),
(3, 3, 'Many', '1', 2),
(3, 4, 'Heavy', '1', 3),
-- Size, Ave
(4, 1, 'Fine, <5μm', '1', 0),
(4, 2, 'Small, 5 to 15μm', '1', 1),
(4, 3, 'Medium, 15 to 40μm', '1', 2),
(4, 4, 'Large, 40 to 100μm', '1', 3),
-- Size, Max
(5, 1, 'Fine, <5μm', '1', 0),
(5, 2, 'Small, 5 to 15μm', '1', 1),
(5, 3, 'Medium, 15 to 40μm', '1', 2),
(5, 4, 'Large, 40 to 100μm', '1', 3),
(5, 5, 'Very Large, >100μm', '1', 4),
-- Color
(6, 1, 'Copper', '1', 0),
(6, 2, 'Dark Metallic', '1', 1),
(6, 3, 'Temper', '1', 2),
(6, 4, 'Red Oxide', '1', 3),
-- Texture
(7, 1, 'Smooth', '1', 0),
(7, 2, 'Rough', '1', 1),
(7, 3, 'Striated', '1', 2),
-- Composition
(8, 1, 'Ferrous', '1', 0),
(8, 2, 'Non-Ferrous', '1', 1),
(8, 3, 'Alloy', '1', 2);

PRINT 'Inserted particle sub-type definitions.';
GO

PRINT 'Particle analysis data seeding completed successfully!';

-- Show summary
SELECT 'Particle Types' as TableName, COUNT(*) as RecordCount FROM ParticleTypeDefinition
UNION ALL
SELECT 'Sub-Type Categories', COUNT(*) FROM ParticleSubTypeCategoryDefinition  
UNION ALL
SELECT 'Sub-Type Definitions', COUNT(*) FROM ParticleSubTypeDefinition;
GO