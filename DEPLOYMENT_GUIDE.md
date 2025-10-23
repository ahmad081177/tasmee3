# Azure Deployment Guide - Quran Listening App

## Quick Deployment Checklist

### ✅ Pre-Deployment (Completed)
- [x] Azure SQL serverless support implemented
- [x] EF Core retry policy configured
- [x] UI loading indicators added
- [x] Environment-specific configuration files created
- [x] Build successful

### 📋 Azure App Service Configuration

#### 1. Application Settings (Environment Variables)
Set these in Azure Portal → App Service → Configuration → Application settings:

| Name | Value |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |

#### 2. Connection Strings
Set in Azure Portal → App Service → Configuration → Connection strings:

| Name | Value | Type |
|------|-------|------|
| `DefaultConnection` | `Server=tcp:YOUR_SERVER.database.windows.net,1433;Database=QuranListeningDb;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` | SQL Azure |

**Replace:**
- `YOUR_SERVER` with your Azure SQL server name
- `YOUR_USERNAME` with your database username
- `YOUR_PASSWORD` with your database password

#### 3. General Settings
- **Platform**: 64-bit
- **Always On**: Enable (if not using free tier)
- **ARR Affinity**: Enable (required for Blazor Server)

---

## Local Development vs Production

### Local Development (appsettings.Development.json)
```json
{
  "DatabaseSettings": {
    "IsAzureSql": false,
    "EnableRetryOnFailure": false,
    "CommandTimeoutSeconds": 30
  }
}
```
- Uses local SQL Server `(localdb)\\mssqllocaldb`
- No retry policy (faster development)
- 30-second timeout

### Production (appsettings.Production.json)
```json
{
  "DatabaseSettings": {
    "IsAzureSql": true,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "MaxRetryDelaySeconds": 2,
    "CommandTimeoutSeconds": 60
  }
}
```
- Uses Azure SQL Database
- Retry policy enabled for cold starts
- 60-second timeout for wake-up scenarios

---

## Database Migration to Azure

### Option 1: Azure Portal (Recommended)
1. Go to Azure Portal → SQL Database → Query editor
2. Copy schema from local database
3. Run create scripts
4. Use Azure Data Studio to migrate data

### Option 2: EF Core Migrations
```powershell
# Set production connection string temporarily
$env:ConnectionStrings__DefaultConnection="Server=tcp:yourserver.database.windows.net,1433;Database=QuranListeningDb;..."

# Apply migrations
cd src/QuranListeningApp.Web
dotnet ef database update --project ../QuranListeningApp.Infrastructure
```

### Option 3: BACPAC Export/Import
1. Export local database as BACPAC
2. Import to Azure SQL Database via Azure Portal

---

## Deployment Methods

### Method 1: Visual Studio Publish
1. Right-click `QuranListeningApp.Web` project
2. Select **Publish**
3. Choose **Azure** → **Azure App Service (Windows)**
4. Select your subscription and app service
5. Click **Publish**

### Method 2: Azure CLI
```bash
# Login to Azure
az login

# Build and publish
cd src/QuranListeningApp.Web
dotnet publish -c Release -o ./publish

# Deploy to App Service
az webapp deployment source config-zip \
  --resource-group YOUR_RESOURCE_GROUP \
  --name YOUR_APP_NAME \
  --src ./publish.zip
```

### Method 3: GitHub Actions (CI/CD)
See `.github/workflows/azure-deploy.yml` (if configured)

---

## Post-Deployment Verification

### 1. Check Application Logs
Azure Portal → App Service → Log stream

### 2. Test Login
- Navigate to `https://your-app.azurewebsites.net`
- Login with default admin credentials
- Verify school logo and name display

### 3. Test Database Connection
- Access admin dashboard
- Try creating a listening session
- Verify data saves to Azure SQL

### 4. Monitor Cold Start
- Stop Azure SQL database (or wait for auto-pause)
- Access application
- Verify loading indicator shows during wake-up
- Confirm page loads successfully after 3-5 seconds

---

## Azure SQL Serverless Configuration

### Recommended Settings (Free Tier)
- **Compute Tier**: Serverless
- **Auto-pause delay**: 1 hour
- **Min vCores**: 0.5
- **Max vCores**: 1
- **Max data size**: 2 GB (free tier limit)

### Cost Optimization
- Auto-pause during idle periods (60-70% cost savings)
- Pay only for compute used when active
- Free tier: No charge if under 32 GB-hours/month

### Performance Expectations
- **Cold start**: 1-3 seconds (handled by retry policy)
- **Warm state**: < 100ms response time
- **Auto-pause**: After 60 minutes of inactivity

---

## Troubleshooting

### Issue: "Database unavailable" errors
**Solution**: Already handled by retry policy. If persistent:
1. Check Azure SQL firewall rules (allow Azure services)
2. Verify connection string is correct
3. Check database is not paused manually

### Issue: Long loading times (> 5 seconds)
**Possible causes**:
1. Database cold start (normal, wait for retry policy)
2. Network latency (check App Service region matches SQL region)
3. DTU exhaustion (monitor in Azure Portal)

### Issue: Loading indicator not showing
**Check**:
1. Browser console for JavaScript errors
2. Component is included in layout files
3. CSS is loading properly

### Issue: Configuration not loading
**Verify**:
1. `ASPNETCORE_ENVIRONMENT` is set to `Production`
2. `appsettings.Production.json` is included in publish
3. Restart App Service after config changes

---

## Monitoring & Maintenance

### Application Insights (Recommended)
Enable in Azure Portal → App Service → Application Insights
- Track database retry attempts
- Monitor response times
- Detect errors and exceptions
- Analyze user behavior

### Database Monitoring
Azure Portal → SQL Database → Metrics
- DTU usage (keep under free tier limit)
- Active connections
- Query performance
- Storage usage

### Regular Tasks
- [ ] Monitor database size (stay within 2 GB free tier)
- [ ] Review Application Insights weekly
- [ ] Check for failed login attempts
- [ ] Backup database monthly (Azure automatic backups available)

---

## Security Checklist

### Before Going Live
- [ ] Change default admin password
- [ ] Enable HTTPS only (force SSL)
- [ ] Configure custom domain (optional)
- [ ] Enable Azure AD authentication (optional, future)
- [ ] Set up firewall rules for SQL Database
- [ ] Review and minimize SQL user permissions
- [ ] Enable Azure DDoS protection (if available)
- [ ] Configure CORS policies if needed

### Connection String Security
✅ **DO**: Store in Azure App Service Configuration (encrypted)
❌ **DON'T**: Hardcode in appsettings.json (committed to git)

---

## Rollback Plan

### If Deployment Fails
1. Azure Portal → App Service → Deployment slots
2. Swap back to previous deployment
3. Or: Re-deploy previous version from Visual Studio

### Database Rollback
1. Use Azure SQL automatic backups (point-in-time restore)
2. Or: Restore from manual backup
3. Update migrations if schema changed

---

## Support & Resources

### Documentation
- [Azure SQL Serverless](https://docs.microsoft.com/en-us/azure/azure-sql/database/serverless-tier-overview)
- [EF Core Retry Policy](https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency)
- [Blazor Server Deployment](https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server)

### Cost Calculator
[Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/)

### Performance Benchmarks
- Expected users: 50-200 concurrent
- Database size: < 2 GB (free tier)
- Monthly cost estimate: $0-5 with serverless auto-pause

---

**Last Updated**: October 23, 2025
**Application Version**: 1.0
**Azure SQL Support**: ✅ Fully Implemented
