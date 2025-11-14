# Quick Start - Database Update

## ✅ Ready to Run!

Your scripts are now configured with the correct settings from your API configuration:

- **Server**: localhost
- **Database**: LabResultsDb  
- **Username**: sa
- **Password**: LabResults123!

## 🚀 Run the Update

### Option 1: Fish Shell (Recommended for you)
```fish
# Test connection first
./test-connection.sh

# Run the database update
./update-database.fish
```

### Option 2: Bash Shell
```bash
# Test connection first
./test-connection.sh

# Run the database update
./update-database.sh
```

## 📊 What Will Happen

The script will:
1. ✅ Connect to LabResultsDb using sa/LabResults123!
2. 🗑️ Drop existing views, procedures, and functions
3. 📋 Create 56+ tables from db-tables/
4. ⚙️ Create 8 functions from db-functions/
5. 🔧 Create 18 stored procedures from db-sp/
6. 👁️ Create 93 views from db-views/

## 🎯 Expected Result

After completion, you'll have:
- Complete database structure matching your SQL files
- Proper test definitions (fixing the duplicate test types issue)
- Clean foundation for manual data seeding

## ⚠️ Current Status

Your database currently has **12 tables**. The update will modify the structure to match your complete schema definition.

---

**Just run**: `./update-database.fish` and you're good to go! 🎉