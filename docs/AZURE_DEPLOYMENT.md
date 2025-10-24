# Azure Deployment Guide for Quran Listening App

## Overview
This guide will help you deploy the Quran Listening App to Azure with SQL Database support.

## 📋 Prerequisites
- Azure subscription
- Azure CLI installed (or use Cloud Shell)
- Local app running successfully on LocalDB

## 🚀 Step 1: Create Azure Resources

### 1.1 Login to Azure
```bash
az login
```

### 1.2 Create Resource Group
```bash
az group create --name tasmee3-rg --location "East US"
```

### 1.3 Create Azure SQL Server
```bash
az sql server create \
  --name tasmee3dbserver \
  --resource-group tasmee3-rg \
  --location "East US" \
  --admin-user tasmee3admin \
  --admin-password "YourSecurePassword123!"
```

### 1.4 Create Azure SQL Database
```bash
az sql db create \
  --resource-group tasmee3-rg \
  --server tasmee3dbserver \
  --name tasmee3db \
  --service-objective Basic
```

### 1.5 Configure Firewall Rules
```bash
# Allow Azure services
az sql server firewall-rule create \
  --resource-group tasmee3-rg \
  --server tasmee3dbserver \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Allow your current IP (run this from your machine)
az sql server firewall-rule create \
  --resource-group tasmee3-rg \
  --server tasmee3dbserver \
  --name AllowMyIP \
  --start-ip-address $(curl -s https://ipinfo.io/ip) \
  --end-ip-address $(curl -s https://ipinfo.io/ip)
```

### 1.6 Create App Service Plan
```bash
az appservice plan create \
  --name tasmee3-plan \
  --resource-group tasmee3-rg \
  --sku B1 \
  --is-linux
```

### 1.7 Create Web App
```bash
az webapp create \
  --resource-group tasmee3-rg \
  --plan tasmee3-plan \
  --name tasmee3-app \
  --runtime "DOTNETCORE:8.0"
```

## 🗄️ Step 2: Configure Database Connection

### 2.1 Get Connection String
```bash
az sql db show-connection-string \
  --server tasmee3dbserver \
  --name tasmee3db \
  --client ado.net
```

### 2.2 Configure App Settings
```bash
# Set the connection string (replace with your actual values)
az webapp config connection-string set \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### 2.3 Set Environment Variables
```bash
az webapp config appsettings set \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --settings ASPNETCORE_ENVIRONMENT="Production"
```

## 🔧 Step 3: Local Database Migration

### 3.1 Update Connection String for Migration
Add this to your `appsettings.Development.json` temporarily:
```json
{
  "ConnectionStrings": {
    "AzureConnection": "Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 3.2 Run Migrations to Azure Database
```bash
cd "c:\ws\personal\mine\tasmee3\src\QuranListeningApp.Web"
dotnet ef database update --connection "Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## 📦 Step 4: Deploy Application

### 4.1 Publish the App
```bash
cd "c:\ws\personal\mine\tasmee3\src"
dotnet publish QuranListeningApp.Web/QuranListeningApp.Web.csproj -c Release -o ./publish
```

### 4.2 Create Deployment Package
```bash
cd publish
Compress-Archive -Path * -DestinationPath ../tasmee3-app.zip
cd ..
```

### 4.3 Deploy to Azure
```bash
az webapp deployment source config-zip \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --src tasmee3-app.zip
```

## 🔐 Step 5: Security Configuration

### 5.1 Configure HTTPS Only
```bash
az webapp update \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --https-only true
```

### 5.2 Configure Custom Domain (Optional)
```bash
# If you have a custom domain
az webapp config hostname add \
  --webapp-name tasmee3-app \
  --resource-group tasmee3-rg \
  --hostname yourdomain.com
```

## 🔍 Step 6: Verification

### 6.1 Check Application Status
```bash
az webapp browse --name tasmee3-app --resource-group tasmee3-rg
```

### 6.2 View Application Logs
```bash
az webapp log tail --name tasmee3-app --resource-group tasmee3-rg
```

## 📋 Step 7: Post-Deployment Configuration

### 7.1 Default Login Credentials
- **Username**: admin
- **Password**: Admin@123

⚠️ **Important**: Change the default admin password immediately after first login!

### 7.2 Configure App Settings via Portal
1. Go to Azure Portal → App Services → tasmee3-app
2. Navigate to Configuration → Application Settings
3. Verify these settings:
   - `ASPNETCORE_ENVIRONMENT` = "Production"
   - `WEBSITE_RUN_FROM_PACKAGE` = "1"

## 🔄 Ongoing Maintenance

### Update Application
```bash
# 1. Publish new version
cd "c:\ws\personal\mine\tasmee3\src"
dotnet publish QuranListeningApp.Web/QuranListeningApp.Web.csproj -c Release -o ./publish

# 2. Create new deployment package
cd publish
Compress-Archive -Path * -DestinationPath ../tasmee3-app-v2.zip -Force
cd ..

# 3. Deploy update
az webapp deployment source config-zip \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --src tasmee3-app-v2.zip
```

### Database Updates
```bash
# Run new migrations
dotnet ef database update --connection "Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## 🚨 Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Check firewall rules
   - Verify connection string
   - Ensure password is correct

2. **App Won't Start**
   - Check application logs: `az webapp log tail`
   - Verify ASPNETCORE_ENVIRONMENT setting
   - Check that all dependencies are included

3. **Arabic Text Not Displaying**
   - App includes Tahoma font support
   - RTL layout is configured automatically

4. **Performance Issues**
   - Monitor with Application Insights
   - Consider scaling up the App Service Plan
   - Optimize database queries

### Monitoring
```bash
# Enable Application Insights
az monitor app-insights component create \
  --app tasmee3-insights \
  --location "East US" \
  --resource-group tasmee3-rg

# Link to Web App
az webapp config appsettings set \
  --resource-group tasmee3-rg \
  --name tasmee3-app \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY="your-instrumentation-key"
```

## 💰 Cost Optimization

- **Basic SQL Database**: ~$5/month
- **B1 App Service Plan**: ~$13/month
- **Total estimated cost**: ~$18/month

For production use, consider:
- S1 App Service Plan for better performance
- Standard SQL Database for more storage and performance
- Azure CDN for static files
- Application Gateway for advanced routing

## 🔗 Useful Links

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Entity Framework Core with Azure SQL](https://docs.microsoft.com/en-us/ef/core/providers/sql-server/)

## 📞 Support

If you encounter issues:
1. Check Azure Portal → App Service → Diagnose and solve problems
2. Review application logs
3. Verify database connectivity
4. Check firewall settings

---

**Note**: Replace `YourSecurePassword123!` with a strong password of your choice throughout this guide.