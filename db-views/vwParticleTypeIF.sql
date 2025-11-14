USE [LabResultsDb]
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwParticleTypeIF'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwParticleTypeIF'
GO

/****** Object:  View [dbo].[vwParticleTypeIF]    Script Date: 10/28/2025 11:30:58 AM ******/
DROP VIEW [dbo].[vwParticleTypeIF]
GO

/****** Object:  View [dbo].[vwParticleTypeIF]    Script Date: 10/28/2025 11:30:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwParticleTypeIF]
AS
SELECT        TOP (100) PERCENT dbo.ParticleType.SampleID, dbo.ParticleType.testID, dbo.ParticleTypeDefinition.Type, dbo.ParticleType.Comments, 
                         dbo.ParticleTypeDefinition.SortOrder, dbo.ParticleSubType.Value
FROM            dbo.ParticleType LEFT OUTER JOIN
                         dbo.ParticleSubType ON dbo.ParticleType.SampleID = dbo.ParticleSubType.SampleID AND dbo.ParticleType.testID = dbo.ParticleSubType.testID AND 
                         dbo.ParticleType.ParticleTypeDefinitionID = dbo.ParticleSubType.ParticleTypeDefinitionID RIGHT OUTER JOIN
                         dbo.ParticleTypeDefinition ON dbo.ParticleType.ParticleTypeDefinitionID = dbo.ParticleTypeDefinition.ID
WHERE        (dbo.ParticleTypeDefinition.Active = N'1') AND (dbo.ParticleSubType.ParticleSubTypeCategoryID = 1) AND (dbo.ParticleType.testID = 120) OR
                         (dbo.ParticleTypeDefinition.Active = N'1') AND (dbo.ParticleSubType.ParticleSubTypeCategoryID IS NULL) AND (dbo.ParticleType.testID = 120)
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "ParticleType (dbo)"
            Begin Extent = 
               Top = 8
               Left = 49
               Bottom = 154
               Right = 266
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ParticleSubType (dbo)"
            Begin Extent = 
               Top = 7
               Left = 519
               Bottom = 151
               Right = 752
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ParticleTypeDefinition (dbo)"
            Begin Extent = 
               Top = 115
               Left = 297
               Bottom = 293
               Right = 467
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwParticleTypeIF'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwParticleTypeIF'
GO

