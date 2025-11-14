USE [LabResultsDb]
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleasePT'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleasePT'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleasePT'
GO

/****** Object:  View [dbo].[vwSampleReleasePT]    Script Date: 10/28/2025 11:34:48 AM ******/
DROP VIEW [dbo].[vwSampleReleasePT]
GO

/****** Object:  View [dbo].[vwSampleReleasePT]    Script Date: 10/28/2025 11:34:48 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleReleasePT]
AS
SELECT DISTINCT 
                         dbo.vwParticleType.SampleID, dbo.vwParticleType.Type, dbo.vwParticleType.SortOrder, dbo.vwParticleTypeIF.Value AS IFvalue, 
                         dbo.vwParticleTypeFR.Value AS FRvalue, dbo.vwParticleTypeFE.Value AS FEvalue, dbo.vwParticleTypeDI.Value AS DIValue, 
                         dbo.vwParticleTypeIF.Comments AS IFcomments, dbo.vwParticleTypeFR.Comments AS FRcomments, dbo.vwParticleTypeFE.Comments AS FEcomments, 
                         dbo.vwParticleTypeDI.Comments AS DIcomments
FROM            dbo.vwParticleType LEFT OUTER JOIN
                         dbo.vwParticleTypeIF ON dbo.vwParticleType.SampleID = dbo.vwParticleTypeIF.SampleID AND 
                         dbo.vwParticleType.SortOrder = dbo.vwParticleTypeIF.SortOrder LEFT OUTER JOIN
                         dbo.vwParticleTypeFR ON dbo.vwParticleType.SampleID = dbo.vwParticleTypeFR.SampleID AND 
                         dbo.vwParticleType.SortOrder = dbo.vwParticleTypeFR.SortOrder LEFT OUTER JOIN
                         dbo.vwParticleTypeFE ON dbo.vwParticleType.SampleID = dbo.vwParticleTypeFE.SampleID AND 
                         dbo.vwParticleType.SortOrder = dbo.vwParticleTypeFE.SortOrder LEFT OUTER JOIN
                         dbo.vwParticleTypeDI ON dbo.vwParticleType.SampleID = dbo.vwParticleTypeDI.SampleID AND dbo.vwParticleType.SortOrder = dbo.vwParticleTypeDI.SortOrder
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[20] 2[13] 3) )"
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
         Begin Table = "vwParticleType (dbo)"
            Begin Extent = 
               Top = 8
               Left = 18
               Bottom = 180
               Right = 188
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "vwParticleTypeIF (dbo)"
            Begin Extent = 
               Top = 127
               Left = 231
               Bottom = 292
               Right = 401
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "vwParticleTypeFR (dbo)"
            Begin Extent = 
               Top = 121
               Left = 424
               Bottom = 291
               Right = 594
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "vwParticleTypeFE (dbo)"
            Begin Extent = 
               Top = 124
               Left = 639
               Bottom = 288
               Right = 809
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "vwParticleTypeDI (dbo)"
            Begin Extent = 
               Top = 6
               Left = 847
               Bottom = 172
               Right = 1017
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
      Begin ColumnWidths = 12
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
        ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleasePT'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N' Width = 1500
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleasePT'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleasePT'
GO

