# Azure Setup Guide - Tasmee3 Application

## Your Azure Configuration

### Database Details
- **Server**: tasmee3dbserver.database.windows.net
- **Database**: tasmee3db
- **Admin User**: tasmee3admin
- **Password**: ⚠️ DO NOT commit to git - Set via Azure Portal only

---

## Deployment Steps

### Step 1: Configure Azure App Service Connection String

#### Option A: Azure Portal (Recommended)
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your App Service
3. Go to **Configuration** → **Connection strings**
4. Click **+ New connection string**
5. Enter:
   - **Name**: `DefaultConnection`
   - **Value**: 
     ```
     Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YOUR_ACTUAL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
     ```
   - **Type**: `SQLAzure`
6. Click **OK**
7. Click **Save** at the top

#### Option B: Azure CLI
```bash
az webapp config connection-string set \
  --resource-group YOUR_RESOURCE_GROUP \
  --name YOUR_APP_NAME \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YOUR_ACTUAL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

---

### Step 2: Set Environment to Production

#### Azure Portal
1. Go to **Configuration** → **Application settings**
2. Click **+ New application setting**
3. Enter:
   - **Name**: `ASPNETCORE_ENVIRONMENT`
   - **Value**: `Production`
4. Click **OK**
5. Click **Save**

#### Azure CLI
```bash
az webapp config appsettings set \
  --resource-group YOUR_RESOURCE_GROUP \
  --name YOUR_APP_NAME \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

---

### Step 3: Migrate Database to Azure

You have 3 options:

#### Option A: Use EF Core Migrations from Local Machine
```powershell
# Navigate to Web project
cd src\QuranListeningApp.Web

# Set connection string temporarily (replace YOUR_PASSWORD)
$env:ConnectionStrings__DefaultConnection="Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Apply migrations
dotnet ef database update --project ..\QuranListeningApp.Infrastructure

# Clear the environment variable
Remove-Item Env:\ConnectionStrings__DefaultConnection
```

#### Option B: Azure Data Studio (GUI)
1. Install [Azure Data Studio](https://aka.ms/azuredatastudio)
2. Connect to your local database
3. Right-click database → **Schema Compare**
4. Set target to Azure SQL (tasmee3dbserver.database.windows.net)
5. Generate and run schema script
6. Use **Data Compare** or **Import/Export** for data

#### Option C: SQL Server Management Studio (SSMS)
1. Connect to local database `(localdb)\mssqllocaldb`
2. Right-click `QuranListeningDb` → Tasks → **Generate Scripts**
3. Select all objects
4. Advanced options: Schema and Data
5. Save script
6. Connect to Azure SQL: `tasmee3dbserver.database.windows.net`
7. Run script on `tasmee3db`

---

### Step 4: Configure Azure SQL Firewall

#### Add Your IP Address
1. Azure Portal → SQL Database → `tasmee3db`
2. Click **Set server firewall** (top toolbar)
3. Click **Add client IP** (adds your current IP)
4. Add rule for Azure services:
   - **Rule name**: `AllowAzureServices`
   - **Start IP**: `0.0.0.0`
   - **End IP**: `0.0.0.0`
5. Click **Save**

#### Azure CLI
```bash
# Add your current IP
az sql server firewall-rule create \
  --resource-group YOUR_RESOURCE_GROUP \
  --server tasmee3dbserver \
  --name MyIP \
  --start-ip-address YOUR_IP \
  --end-ip-address YOUR_IP

# Allow Azure services
az sql server firewall-rule create \
  --resource-group YOUR_RESOURCE_GROUP \
  --server tasmee3dbserver \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

---

### Step 5: Configure Azure SQL for Serverless (Cost Optimization)

#### Azure Portal
1. Go to SQL Database → `tasmee3db`
2. Click **Compute + storage**
3. Change to:
   - **Compute tier**: Serverless
   - **Auto-pause delay**: 60 minutes
   - **Min vCores**: 0.5
   - **Max vCores**: 1
   - **Max data size**: 2 GB
4. Click **Apply**

#### Cost Estimate
- **Idle (auto-paused)**: $0.00/hour
- **Active**: ~$0.15/hour (only when database is active)
- **Monthly (with auto-pause)**: $5-15 depending on usage

---

### Step 6: Test Connection Locally (Optional)

Before deploying, test Azure SQL connection from your local machine:

```powershell
# Create test connection file
$testConnection = @"
Server=tcp:tasmee3dbserver.database.windows.net,1433;
Initial Catalog=tasmee3db;
Persist Security Info=False;
User ID=tasmee3admin;
Password=YOUR_ACTUAL_PASSWORD;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
"@

# Test with appsettings override
cd src\QuranListeningApp.Web

# Temporarily update appsettings.Development.json with Azure connection
# Run: dotnet run
# Login and test
# Then revert appsettings.Development.json
```

---

### Step 7: Deploy Application

#### Visual Studio 2022
1. Right-click `QuranListeningApp.Web` project
2. Select **Publish**
3. Choose **Azure** → **Azure App Service (Windows)**
4. Sign in to your Azure account
5. Select your subscription
6. Select or create App Service
7. Click **Finish**
8. Click **Publish** button

#### VS Code / Command Line
```powershell
# Build in Release mode
cd src
dotnet publish QuranListeningApp.Web -c Release -o ../publish

# Create deployment package
cd ../publish
Compress-Archive -Path * -DestinationPath ../deploy.zip -Force

# Deploy using Azure CLI
az webapp deployment source config-zip `
  --resource-group YOUR_RESOURCE_GROUP `
  --name YOUR_APP_NAME `
  --src ../deploy.zip
```

---

### Step 8: Verify Deployment

#### Check Application
1. Navigate to `https://YOUR_APP_NAME.azurewebsites.net`
2. Login with default credentials:
   - Username: `admin@system.local`
   - Password: `Admin123!`
3. Verify:
   - ✅ School logo displays
   - ✅ School name displays
   - ✅ Can navigate to different pages
   - ✅ Can create a listening session

#### Test Cold Start
1. Go to Azure Portal → SQL Database → `tasmee3db`
2. Click **Pause** (if available)
3. Access your application
4. You should see:
   - Loading indicator appears: "جاري الاتصال بقاعدة البيانات..."
   - Page loads after 3-5 seconds
   - No error messages

---

## Security Best Practices

### ✅ DO
- Store password in Azure App Service Configuration only
- Use Azure Key Vault for production secrets (future enhancement)
- Enable SSL/HTTPS only
- Change default admin password immediately after first login
- Set up Azure AD authentication (future)
- Regularly review SQL firewall rules
- Enable Azure SQL auditing

### ❌ DON'T
- Commit passwords to git
- Hardcode connection strings in appsettings.json
- Use default admin credentials in production
- Allow all IPs in SQL firewall (use specific ranges)
- Share database credentials via email/chat

---

## Troubleshooting

### Issue: Cannot connect to Azure SQL from local machine
**Solution**:
1. Check firewall rules include your IP
2. Verify credentials are correct
3. Ensure `Encrypt=True` in connection string
4. Check if your network blocks port 1433

### Issue: Application can't connect to database after deployment
**Solution**:
1. Verify connection string is set in App Service Configuration
2. Check `ASPNETCORE_ENVIRONMENT=Production`
3. Ensure firewall allows Azure services (0.0.0.0)
4. Restart App Service

### Issue: "Login failed for user 'tasmee3admin'"
**Solution**:
1. Verify password is correct
2. Check user exists in Azure SQL
3. Ensure user has `db_owner` role on `tasmee3db`

### Issue: Database always shows as "cold" even after recent use
**Solution**:
1. Auto-pause delay might be too short
2. Increase to 60-120 minutes in database settings
3. Or disable auto-pause if constant availability is needed

---

## Monitoring

### Application Insights (Recommended)
1. Azure Portal → App Service → Application Insights
2. Click **Turn on Application Insights**
3. Create new resource or select existing
4. Monitor:
   - Response times
   - Failed requests
   - Database retry attempts
   - User sessions

### Database Metrics
1. Azure Portal → SQL Database → Metrics
2. Monitor:
   - DTU percentage (keep under 80%)
   - Storage usage (free tier: max 2 GB)
   - Active connections
   - Failed connections

### Set Up Alerts
1. Azure Portal → SQL Database → Alerts
2. Create alerts for:
   - DTU > 80%
   - Storage > 1.8 GB (90% of free tier)
   - Failed connections > 5
   - Database paused/resumed

---

## Backup & Recovery

### Azure SQL Automatic Backups
- **Full backup**: Weekly
- **Differential backup**: Every 12-24 hours
- **Transaction log backup**: Every 5-10 minutes
- **Retention**: 7 days (free tier)

### Manual Backup
```bash
# Export to BACPAC
az sql db export \
  --resource-group YOUR_RESOURCE_GROUP \
  --server tasmee3dbserver \
  --name tasmee3db \
  --admin-user tasmee3admin \
  --admin-password YOUR_PASSWORD \
  --storage-key-type StorageAccessKey \
  --storage-key YOUR_STORAGE_KEY \
  --storage-uri https://YOUR_STORAGE.blob.core.windows.net/backups/tasmee3db.bacpac
```

### Point-in-Time Restore
1. Azure Portal → SQL Database → Restore
2. Select restore point (up to 7 days back)
3. Create new database or overwrite existing

---

## Performance Optimization

### Connection String Optimization
Your connection string is already optimized with:
- ✅ `Connection Timeout=30` (adequate for cold starts)
- ✅ `Encrypt=True` (security)
- ✅ `MultipleActiveResultSets=False` (recommended for Blazor Server)

### Application Settings
Already configured in code:
- ✅ Retry policy: 3 attempts
- ✅ Retry delay: 2 seconds
- ✅ Command timeout: 60 seconds (production)

### Database Indexes
Run this after deploying schema:
```sql
-- Optimize user lookups
CREATE INDEX IX_Users_Email ON Users(Email);

-- Optimize session queries
CREATE INDEX IX_ListeningSessions_StudentId_Date ON ListeningSessions(StudentId, Date DESC);
CREATE INDEX IX_ListeningSessions_TeacherId_Date ON ListeningSessions(TeacherId, Date DESC);

-- Optimize surah lookups
CREATE INDEX IX_SurahReferences_Number ON SurahReferences(Number);
```

---

## Next Steps After Deployment

1. ✅ Deploy application
2. ✅ Run database migrations
3. ✅ Test login and functionality
4. ✅ Test cold start scenario
5. ⏳ Change default admin password
6. ⏳ Upload school logo
7. ⏳ Configure school name
8. ⏳ Set pledge text
9. ⏳ Create teacher accounts
10. ⏳ Create student accounts
11. ⏳ Enable Application Insights
12. ⏳ Set up database alerts
13. ⏳ Configure custom domain (optional)

---

## Quick Reference

### Your Azure Resources
| Resource | Name |
|----------|------|
| SQL Server | `tasmee3dbserver.database.windows.net` |
| Database | `tasmee3db` |
| Admin User | `tasmee3admin` |
| App Service | `YOUR_APP_NAME` (you'll create this) |

### Important Files (Local)
| File | Purpose |
|------|---------|
| `appsettings.Development.json` | Local SQL config |
| `appsettings.Production.json` | Azure SQL config (without password) |
| `DEPLOYMENT_GUIDE.md` | General deployment guide |
| `AZURE_SETUP.md` | This file - Your specific setup |

### Environment Variables to Set in Azure
| Name | Value |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### Connection String to Set in Azure
| Name | Type | Value |
|------|------|-------|
| `DefaultConnection` | `SQLAzure` | `Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` |

---

**Created**: October 23, 2025  
**Status**: Ready for deployment  
**Security Level**: ⚠️ DO NOT commit database password to git

