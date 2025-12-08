#!/bin/bash

# Script to help verify that C# models match SQL table definitions
# Usage: ./verify-models.sh

echo "=== Model to SQL Schema Verification Helper ==="
echo ""
echo "This script lists the models and their corresponding SQL files"
echo "for manual verification that they match."
echo ""

cd "$(dirname "$0")"

echo "Models in LabResultsApi/Models/:"
echo "================================"
ls -1 LabResultsApi/Models/*.cs | while read model_file; do
    model_name=$(basename "$model_file" .cs)
    echo ""
    echo "Model: $model_name.cs"
    
    # Try to find corresponding SQL file
    # Check for exact match first
    if [ -f "db-tables/${model_name}.sql" ]; then
        echo "  SQL:  db-tables/${model_name}.sql ✓"
    else
        # Check for common name variations
        case $model_name in
            "Sample")
                if [ -f "db-tables/UsedLubeSamples.sql" ]; then
                    echo "  SQL:  db-tables/UsedLubeSamples.sql ✓"
                fi
                ;;
            "Equipment")
                if [ -f "db-tables/M_And_T_Equip.sql" ]; then
                    echo "  SQL:  db-tables/M_And_T_Equip.sql ✓"
                fi
                ;;
            "EmissionSpectroscopy")
                if [ -f "db-tables/EmSpectro.sql" ]; then
                    echo "  SQL:  db-tables/EmSpectro.sql ✓"
                fi
                ;;
            "TestReading")
                if [ -f "db-tables/TestReadings.sql" ]; then
                    echo "  SQL:  db-tables/TestReadings.sql ✓"
                fi
                ;;
            "LubeTech")
                if [ -f "db-tables/LubeTechList.sql" ]; then
                    echo "  SQL:  db-tables/LubeTechList.sql ✓"
                fi
                ;;
            "Reviewer")
                if [ -f "db-tables/ReviewerList.sql" ]; then
                    echo "  SQL:  db-tables/ReviewerList.sql ✓"
                fi
                ;;
            "NasLookup")
                if [ -f "db-tables/NAS_lookup.sql" ]; then
                    echo "  SQL:  db-tables/NAS_lookup.sql ✓"
                fi
                ;;
            "LubeSamplingPoint")
                if [ -f "db-tables/Lube_Sampling_Point.sql" ]; then
                    echo "  SQL:  db-tables/Lube_Sampling_Point.sql ✓"
                fi
                ;;
            "AuditLog"|"HealthCheckModels"|"ValidationModels")
                echo "  SQL:  [Generated/Internal Model - No SQL file]"
                ;;
            *)
                echo "  SQL:  [NOT FOUND] ⚠️"
                ;;
        esac
    fi
done

echo ""
echo "=== Summary ==="
echo "Models with verified SQL files are marked with ✓"
echo "Models without SQL files are marked with ⚠️"
echo ""
echo "To manually verify a model matches its SQL schema:"
echo "  1. Look at db-tables/[TableName].sql"
echo "  2. Look at LabResultsApi/Models/[ModelName].cs"
echo "  3. Ensure all SQL columns are represented as C# properties"
echo "  4. Ensure data types match (int→int, smallint→short, etc.)"
echo "  5. Ensure MaxLength attributes match SQL column lengths"
echo ""
echo "See MODEL_SYNC_SUMMARY.md for detailed data type mappings."
