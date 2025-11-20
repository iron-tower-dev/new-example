#!/bin/bash
# Generate SQL INSERT statements from UsedLubeSamples.csv

CSV_FILE="db-seeding/UsedLubeSamples.csv"
OUTPUT_FILE="LabResultsApi/Scripts/UsedLubeSamplesInserts.sql"

echo "-- Generated INSERT statements for UsedLubeSamples" > "$OUTPUT_FILE"
echo "-- Generated from: $CSV_FILE" >> "$OUTPUT_FILE"
echo "-- Generated on: $(date)" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "USE [LabResultsDb]" >> "$OUTPUT_FILE"
echo "GO" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "-- Clear existing data" >> "$OUTPUT_FILE"
echo "DELETE FROM UsedLubeSamples;" >> "$OUTPUT_FILE"
echo "GO" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "-- Insert data" >> "$OUTPUT_FILE"

# Skip header and process each line
tail -n +2 "$CSV_FILE" | while IFS=',' read -r ID tagNumber component location lubeType woNumber trackingNumber warehouseId batchNumber classItem sampleDate receivedOn sampledBy status cmptSelectFlag newUsedFlag entryId validateId testPricesId pricingPackageId evaluation siteId results_review_date results_avail_date results_reviewId storeSource schedule returnedDate; do
    
    # Replace NULL with SQL NULL
    [ "$tagNumber" = "NULL" ] && tagNumber="NULL" || tagNumber="'$tagNumber'"
    [ "$component" = "NULL" ] && component="NULL" || component="'$component'"
    [ "$location" = "NULL" ] && location="NULL" || location="'$location'"
    [ "$lubeType" = "NULL" ] && lubeType="NULL" || lubeType="'$lubeType'"
    [ "$woNumber" = "NULL" ] && woNumber="NULL" || woNumber="'$woNumber'"
    [ "$trackingNumber" = "NULL" ] && trackingNumber="NULL" || trackingNumber="'$trackingNumber'"
    [ "$warehouseId" = "NULL" ] && warehouseId="NULL" || warehouseId="'$warehouseId'"
    [ "$batchNumber" = "NULL" ] && batchNumber="NULL" || batchNumber="'$batchNumber'"
    [ "$classItem" = "NULL" ] && classItem="NULL" || classItem="'$classItem'"
    [ "$sampleDate" = "NULL" ] && sampleDate="NULL" || sampleDate="'$sampleDate'"
    [ "$receivedOn" = "NULL" ] && receivedOn="NULL" || receivedOn="'$receivedOn'"
    [ "$sampledBy" = "NULL" ] && sampledBy="NULL" || sampledBy="'$sampledBy'"
    [ "$status" = "NULL" ] && status="NULL" || status="$status"
    [ "$cmptSelectFlag" = "NULL" ] && cmptSelectFlag="NULL" || cmptSelectFlag="$cmptSelectFlag"
    [ "$newUsedFlag" = "NULL" ] && newUsedFlag="NULL" || newUsedFlag="$newUsedFlag"
    [ "$entryId" = "NULL" ] && entryId="NULL" || entryId="'$entryId'"
    [ "$validateId" = "NULL" ] && validateId="NULL" || validateId="'$validateId'"
    [ "$testPricesId" = "NULL" ] && testPricesId="NULL" || testPricesId="$testPricesId"
    [ "$pricingPackageId" = "NULL" ] && pricingPackageId="NULL" || pricingPackageId="$pricingPackageId"
    [ "$evaluation" = "NULL" ] && evaluation="NULL" || evaluation="$evaluation"
    [ "$siteId" = "NULL" ] && siteId="NULL" || siteId="$siteId"
    [ "$results_review_date" = "NULL" ] && results_review_date="NULL" || results_review_date="'$results_review_date'"
    [ "$results_avail_date" = "NULL" ] && results_avail_date="NULL" || results_avail_date="'$results_avail_date'"
    [ "$results_reviewId" = "NULL" ] && results_reviewId="NULL" || results_reviewId="'$results_reviewId'"
    [ "$storeSource" = "NULL" ] && storeSource="NULL" || storeSource="'$storeSource'"
    [ "$schedule" = "NULL" ] && schedule="NULL" || schedule="'$schedule'"
    [ "$returnedDate" = "NULL" ] && returnedDate="NULL" || returnedDate="'$returnedDate'"
    
    echo "INSERT INTO UsedLubeSamples (ID, tagNumber, component, location, lubeType, woNumber, trackingNumber, warehouseId, batchNumber, classItem, sampleDate, receivedOn, sampledBy, status, cmptSelectFlag, newUsedFlag, entryId, validateId, testPricesId, pricingPackageId, evaluation, siteId, results_review_date, results_avail_date, results_reviewId, storeSource, schedule, returnedDate) VALUES ($ID, $tagNumber, $component, $location, $lubeType, $woNumber, $trackingNumber, $warehouseId, $batchNumber, $classItem, $sampleDate, $receivedOn, $sampledBy, $status, $cmptSelectFlag, $newUsedFlag, $entryId, $validateId, $testPricesId, $pricingPackageId, $evaluation, $siteId, $results_review_date, $results_avail_date, $results_reviewId, $storeSource, $schedule, $returnedDate);" >> "$OUTPUT_FILE"
    
    ((count++))
    if [ $((count % 100)) -eq 0 ]; then
        echo "GO" >> "$OUTPUT_FILE"
        echo "Processed $count records..."
    fi
done

echo "GO" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "-- Verify data" >> "$OUTPUT_FILE"
echo "SELECT COUNT(*) as TotalRecords FROM UsedLubeSamples;" >> "$OUTPUT_FILE"
echo "GO" >> "$OUTPUT_FILE"

echo "Generated $OUTPUT_FILE with $count INSERT statements"
echo "Run with: sqlcmd -S localhost,1433 -U sa -P 'LabResults123!' -d LabResultsDb -i $OUTPUT_FILE"
