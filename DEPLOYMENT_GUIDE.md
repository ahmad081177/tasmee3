# Quran Listening App - Complete Setup & Deployment Guide

This comprehensive guide covers everything you need to set up the application locally for testing and deploy it to Azure production environment.

## 📋 Table of Contents

1. [Local Development Setup](#-local-development-setup)
2. [Azure Production Deployment](#-azure-production-deployment)
3. [Configuration Reference](#-configuration-reference)
4. [Troubleshooting](#-troubleshooting)
5. [Maintenance](#-maintenance)

---

## 🔧 Local Development Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (2016 or later) or SQL Server Express/LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Step 1: Clone and Setup Repository

```bash
git clone https://github.com/ahmad081177/tasmee3.git
cd tasmee3/src
```

### Step 2: Configure Local Database

#### Option A: SQL Server LocalDB (Recommended for Development)
The application is pre-configured to use LocalDB. No additional setup required.

#### Option B: SQL Server Express/Full Edition
Update `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=QuranListeningDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Replace `YOUR_SERVER_NAME` with:
- `localhost` or `.\SQLEXPRESS` for SQL Server Express
- Your server name for full SQL Server

### Step 3: Restore Dependencies and Build

```bash
# Restore NuGet packages
dotnet restore QuranListeningApp.sln

# Build the solution
dotnet build QuranListeningApp.sln
```

### Step 4: Initialize Database

```bash
# Navigate to Web project directory
cd QuranListeningApp.Web

# Apply database migrations
dotnet ef database update --project ../QuranListeningApp.Infrastructure
```

This will:
- Create the database
- Apply all migrations
- Seed Surah reference data (114 Surahs)
- Create default admin user

### Step 5: Run the Application

```bash
# Run the application
dotnet run --project QuranListeningApp.Web
```

The application will be available at:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

### Step 6: Initial Login

Default admin credentials:
- **Username**: `admin`
- **Password**: `Admin@123`

⚠️ **Important**: Change the default password immediately after first login!

### Step 7: Verify Local Setup

1. Login with admin credentials
2. Navigate to **إدارة المستخدمين** (User Management)
3. Try creating a test teacher account
4. Navigate to **جلسات الاستماع** (Listening Sessions)
5. Verify all features work correctly

---

## 🚀 Azure Production Deployment

### Prerequisites for Azure Deployment

- Azure subscription
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed (or use Cloud Shell)
- Local app running successfully
- Admin access to Azure subscription

### Step 1: Create Azure Resources

#### 1.1 Login to Azure

```bash
az login
```

#### 1.2 Create Resource Group

```bash
az group create --name tasmee3-rg --location "East US"
```

#### 1.3 Create Azure SQL Server

```bash
az sql server create \
  --name tasmee3dbserver \
  --resource-group tasmee3-rg \
  --location "East US" \
  --admin-user tasmee3admin \
  --admin-password "YourSecurePassword123!"
```

⚠️ **Important**: Replace `YourSecurePassword123!` with a strong password of your choice.

#### 1.4 Create Azure SQL Database

```bash
az sql db create \
  --resource-group tasmee3-rg \
  --server tasmee3dbserver \
  --name tasmee3db \
  --service-objective Basic
```

#### 1.5 Configure Firewall Rules

```bash
# Allow Azure services
az sql server firewall-rule create \
  --resource-group tasmee3-rg \
  --server tasmee3dbserver \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Allow your current IP for management
az sql server firewall-rule create \
  --resource-group tasmee3-rg \
  --server tasmee3dbserver \
  --name AllowMyIP \
  --start-ip-address $(curl -s https://ipinfo.io/ip) \
  --end-ip-address $(curl -s https://ipinfo.io/ip)
```

#### 1.6 Create App Service Plan

```bash
az appservice plan create \
  --name tasmee3-plan \
  --resource-group tasmee3-rg \
  --sku B1 \
  --is-linux false
```

#### 1.7 Create Web App

```bash
az webapp create \
  --resource-group tasmee3-rg \
  --plan tasmee3-plan \
  --name tasmee3-app \
  --runtime "DOTNET:8.0"
```

### Step 2: Database Migration to Azure

#### 2.1 Run Migrations to Azure Database

```bash
cd "c:\ws\personal\mine\tasmee3\src\QuranListeningApp.Web"
dotnet ef database update --project ../QuranListeningApp.Infrastructure --connection "Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

This will:
- Create database schema in Azure SQL
- Seed Surah reference data
- Create default admin user

### Step 3: Configure Azure App Service

#### 3.1 Set Connection String

```bash
az webapp config connection-string set \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### 3.2 Set Application Settings

```bash
az webapp config appsettings set \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --settings ASPNETCORE_ENVIRONMENT="Production"
```

#### 3.3 Configure General Settings

```bash
# Enable HTTPS only
az webapp update \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --https-only true

# Set platform to 64-bit
az webapp config set \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --use-32bit-worker-process false
```

### Step 4: Deploy Application

#### 4.1 Publish the Application

```bash
cd "c:\ws\personal\mine\tasmee3\src"
dotnet publish QuranListeningApp.Web/QuranListeningApp.Web.csproj -c Release -o ./publish
```

#### 4.2 Create Deployment Package

```bash
cd publish
Compress-Archive -Path * -DestinationPath ../tasmee3-app.zip -Force
cd ..
```

#### 4.3 Deploy to Azure

```bash
az webapp deployment source config-zip \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --src tasmee3-app.zip
```

### Step 5: Verify Deployment

#### 5.1 Check Application Status

```bash
az webapp browse --name tasmee3-app --resource-group tasmee3-rg
```

#### 5.2 View Application Logs

```bash
az webapp log tail --name tasmee3-app --resource-group tasmee3-rg
```

#### 5.3 Test Application

1. Navigate to your Azure app URL: `https://tasmee3-app.azurewebsites.net`
2. Login with default credentials: `admin` / `Admin@123`
3. Change the default password immediately
4. Test creating users and recording sessions

---

## ⚙️ Configuration Reference

### Local Development Configuration

**File**: `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuranListeningDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "DatabaseSettings": {
    "IsAzureSql": false,
    "EnableRetryOnFailure": false,
    "MaxRetryCount": 1,
    "MaxRetryDelaySeconds": 1,
    "CommandTimeoutSeconds": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Production Configuration

**File**: `appsettings.Production.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "DatabaseSettings": {
    "IsAzureSql": true,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 5,
    "MaxRetryDelaySeconds": 5,
    "CommandTimeoutSeconds": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Error"
    }
  },
  "AllowedHosts": "*"
}
```

### Azure App Service Settings

| Setting | Value | Description |
|---------|-------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment variable |
| `WEBSITE_RUN_FROM_PACKAGE` | `1` | Run from deployment package |
| Platform | 64-bit | Application platform |
| Always On | Enable | Keep app warm (if not free tier) |
| ARR Affinity | Enable | Required for Blazor Server |

---

## 🚨 Troubleshooting

### Common Local Development Issues

#### Issue: Database Connection Failed
**Symptoms**: Can't connect to LocalDB
**Solutions**:
1. Install SQL Server Express or LocalDB
2. Verify connection string in `appsettings.Development.json`
3. Check if SQL Server services are running

#### Issue: Migration Fails
**Symptoms**: `dotnet ef database update` fails
**Solutions**:
1. Ensure you're in the correct directory (`QuranListeningApp.Web`)
2. Check if EF Core tools are installed: `dotnet tool install --global dotnet-ef`
3. Verify database server is accessible

#### Issue: Build Errors
**Symptoms**: Compilation errors
**Solutions**:
1. Restore packages: `dotnet restore`
2. Clean and rebuild: `dotnet clean && dotnet build`
3. Check .NET SDK version: `dotnet --version`

### Common Azure Deployment Issues

#### Issue: Application Won't Start
**Symptoms**: 500 errors, app not loading
**Solutions**:
1. Check application logs: `az webapp log tail`
2. Verify `ASPNETCORE_ENVIRONMENT` is set to `Production`
3. Ensure connection string is correct
4. Check all dependencies are included in deployment

#### Issue: Database Connection Failed
**Symptoms**: Can't connect to Azure SQL
**Solutions**:
1. Verify firewall rules allow Azure services
2. Check connection string format and credentials
3. Test connection from local SQL Server Management Studio
4. Ensure database exists and user has permissions

#### Issue: Arabic Text Not Displaying
**Symptoms**: Text shows as squares or incorrect characters
**Solutions**:
1. The app includes Tahoma font support - this should work automatically
2. Check browser language settings
3. Verify RTL CSS is loading correctly

#### Issue: Performance Issues
**Symptoms**: Slow page loads, timeouts
**Solutions**:
1. Enable Application Insights for monitoring
2. Consider upgrading App Service Plan (B1 → S1)
3. Optimize database queries
4. Enable Always On setting

### Debug Steps

#### For Local Issues:
1. Check Visual Studio Error List
2. Review Output window for build errors
3. Use debugging (F5) to step through code
4. Check SQL Server Object Explorer for database

#### For Azure Issues:
1. Azure Portal → App Service → Diagnose and solve problems
2. Review Application Insights (if enabled)
3. Check Metrics for CPU/Memory usage
4. Review Activity Log for deployment issues

---

## 🔄 Maintenance

### Updating the Application

#### Local Development Updates
```bash
cd "c:\ws\personal\mine\tasmee3\src"
git pull origin main
dotnet restore
dotnet build
dotnet ef database update --project QuranListeningApp.Infrastructure --startup-project QuranListeningApp.Web
```

#### Azure Production Updates
```bash
# 1. Publish new version
cd "c:\ws\personal\mine\tasmee3\src"
dotnet publish QuranListeningApp.Web/QuranListeningApp.Web.csproj -c Release -o ./publish

# 2. Create deployment package
cd publish
Compress-Archive -Path * -DestinationPath ../tasmee3-app-update.zip -Force
cd ..

# 3. Deploy update
az webapp deployment source config-zip \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --src tasmee3-app-update.zip

# 4. Apply database migrations (if any)
dotnet ef database update --project QuranListeningApp.Infrastructure --connection "YOUR_AZURE_CONNECTION_STRING"
```

### Database Maintenance

#### Backup (Azure SQL)
Azure SQL includes automatic backups with point-in-time restore (7-35 days retention).

Manual backup:
```bash
az sql db export \
  --resource-group tasmee3-rg \
  --server tasmee3dbserver \
  --name tasmee3db \
  --admin-user tasmee3admin \
  --admin-password "YourSecurePassword123!" \
  --storage-key-type StorageAccessKey \
  --storage-key "YOUR_STORAGE_KEY" \
  --storage-uri "https://yourstorageaccount.blob.core.windows.net/backups/tasmee3db.bacpac"
```

#### Monitor Database Size
Check database size regularly to avoid exceeding limits:
```sql
SELECT 
    DB_NAME() AS DatabaseName,
    SUM(size * 8 / 1024) AS SizeMB
FROM sys.database_files;
```

### Monitoring and Alerts

#### Application Insights Setup
```bash
# Create Application Insights
az monitor app-insights component create \
  --app tasmee3-insights \
  --location "East US" \
  --resource-group tasmee3-rg \
  --application-type web

# Get instrumentation key
az monitor app-insights component show \
  --app tasmee3-insights \
  --resource-group tasmee3-rg \
  --query "instrumentationKey" -o tsv

# Set app setting
az webapp config appsettings set \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY="YOUR_INSTRUMENTATION_KEY"
```

#### Key Metrics to Monitor
- **Response Time**: < 2 seconds average
- **Error Rate**: < 1%
- **Database DTU Usage**: < 80%
- **App Service CPU/Memory**: < 80%
- **Failed Login Attempts**: Monitor for security

### Cost Optimization

#### Current Estimate (Basic Tier)
- **Azure SQL Database (Basic)**: ~$5/month
- **App Service Plan (B1)**: ~$13/month
- **Total**: ~$18/month

#### Cost Reduction Tips
1. Use Azure SQL Serverless for development/testing
2. Scale down App Service Plan during low usage
3. Enable auto-pause for SQL Database
4. Use Azure Reserved Instances for production

### Security Maintenance

#### Regular Security Tasks
- [ ] Update default admin password
- [ ] Review user accounts monthly
- [ ] Monitor failed login attempts
- [ ] Keep .NET framework updated
- [ ] Review Azure SQL firewall rules
- [ ] Enable Azure Security Center recommendations

#### Security Checklist
- [x] HTTPS-only enforced
- [x] SQL injection protection (EF Core)
- [x] XSS protection (Blazor)
- [x] Authentication required for all features
- [x] Role-based authorization
- [x] Audit logging enabled
- [ ] Two-factor authentication (future enhancement)
- [ ] Azure AD integration (future enhancement)

---

## 📞 Support Resources

### Documentation Links
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Blazor Server Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)

### Useful Commands Reference

```bash
# Azure CLI
az login
az account list
az webapp restart --name tasmee3-app --resource-group tasmee3-rg

# .NET CLI
dotnet --version
dotnet restore
dotnet build
dotnet run
dotnet ef database update

# Entity Framework
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef database drop --force
```

### Emergency Contacts
- **Technical Issues**: [Your Support Email]
- **Azure Billing**: Azure Support Portal
- **Database Issues**: Azure SQL Support

---

---

## 📂 Related Documentation

This comprehensive guide consolidates information from multiple sources. Other available documentation:

- **README.md**: Project overview and general information
- **PROGRESS.md**: Development progress tracking
- **THREADING_FIX_SUMMARY.md**: Technical details about threading issue fixes
- **SECURITY-AUDIT.md**: Security considerations and audit checklist

**Note**: This DEPLOYMENT_GUIDE.md is the primary resource for setup and deployment. Other Azure-related files (AZURE_DEPLOYMENT.md, AZURE_SETUP.md, etc.) are now superseded by this guide.

---

**Last Updated**: October 24, 2025  
**Application Version**: 1.0  
**Guide Version**: 2.0  

**Status**: ✅ **Production Ready**

This guide covers the complete setup and deployment process. For any issues not covered here, please refer to the troubleshooting section or contact support.