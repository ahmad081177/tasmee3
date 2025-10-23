# 🚀 Quick Start - Azure Deployment

## Pre-Deployment Checklist ✅

You have:
- ✅ Azure SQL Database: `tasmee3db` on `tasmee3dbserver.database.windows.net`
- ✅ Database credentials: `tasmee3admin` (password ready)
- ✅ Code with Azure SQL support implemented
- ✅ Local build successful

---

## Fast Track Deployment (30 minutes)

### 1️⃣ Migrate Database Schema (5 min)

```powershell
# Navigate to project
cd src\QuranListeningApp.Web

# Set Azure connection (replace YOUR_PASSWORD)
$env:ConnectionStrings__DefaultConnection="Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;User ID=tasmee3admin;Password=YOUR_PASSWORD;Encrypt=True;Connection Timeout=30;"

# Apply migrations to Azure SQL
dotnet ef database update --project ..\QuranListeningApp.Infrastructure

# Clear password from environment
Remove-Item Env:\ConnectionStrings__DefaultConnection
```

**Expected Output:**
```
Applying migration '20251020175929_InitialCreate'.
Applying migration '20251020194313_ChangedToSingleSurahPerSession'.
Done.
```

---

### 2️⃣ Create Azure App Service (5 min)

#### Azure Portal Method:
1. Go to https://portal.azure.com
2. Click **Create a resource**
3. Search for **Web App**
4. Fill in:
   - **Resource Group**: Create new or use existing
   - **Name**: `tasmee3-app` (or your choice)
   - **Runtime**: `.NET 8 (LTS)`
   - **Operating System**: Windows
   - **Region**: Same as SQL server
   - **Pricing**: Free F1 or Basic B1
5. Click **Review + Create** → **Create**

#### Azure CLI Method:
```bash
# Login
az login

# Create App Service Plan (skip if you have one)
az appservice plan create \
  --name tasmee3-plan \
  --resource-group YOUR_RESOURCE_GROUP \
  --sku F1 \
  --is-linux false

# Create Web App
az webapp create \
  --resource-group YOUR_RESOURCE_GROUP \
  --plan tasmee3-plan \
  --name tasmee3-app \
  --runtime "DOTNET:8"
```

---

### 3️⃣ Configure App Service (10 min)

#### Method A: Azure Portal
**3.1 Set Environment:**
1. Go to App Service → **Configuration**
2. **Application settings** tab
3. Click **+ New application setting**
4. Name: `ASPNETCORE_ENVIRONMENT`, Value: `Production`
5. Click **OK**

**3.2 Set Connection String:**
1. Switch to **Connection strings** tab
2. Click **+ New connection string**
3. Name: `DefaultConnection`
4. Value: 
   ```
   Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;Persist Security Info=False;User ID=tasmee3admin;Password=YOUR_ACTUAL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```
5. Type: `SQLAzure`
6. Click **OK**

**3.3 Save:**
7. Click **Save** at the top
8. Click **Continue** when prompted

#### Method B: Azure CLI
```bash
# Set environment
az webapp config appsettings set \
  --resource-group YOUR_RESOURCE_GROUP \
  --name tasmee3-app \
  --settings ASPNETCORE_ENVIRONMENT=Production

# Set connection string (replace YOUR_PASSWORD)
az webapp config connection-string set \
  --resource-group YOUR_RESOURCE_GROUP \
  --name tasmee3-app \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:tasmee3dbserver.database.windows.net,1433;Initial Catalog=tasmee3db;User ID=tasmee3admin;Password=YOUR_PASSWORD;Encrypt=True;Connection Timeout=30;"
```

---

### 4️⃣ Configure SQL Firewall (2 min)

```bash
# Allow Azure services to access database
az sql server firewall-rule create \
  --resource-group YOUR_RESOURCE_GROUP \
  --server tasmee3dbserver \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

**Or via Portal:**
1. SQL Database → Set server firewall
2. Toggle **Allow Azure services** to ON
3. Save

---

### 5️⃣ Deploy Application (5 min)

#### Visual Studio 2022:
1. Right-click `QuranListeningApp.Web` → **Publish**
2. **Azure** → **Azure App Service (Windows)**
3. Sign in to Azure
4. Select `tasmee3-app`
5. **Publish**
6. Wait for deployment (1-3 minutes)

#### Command Line:
```powershell
# Build and publish
cd src
dotnet publish QuranListeningApp.Web -c Release -o ..\publish

# Deploy
cd ..\publish
Compress-Archive -Path * -DestinationPath ..\deploy.zip -Force

az webapp deployment source config-zip `
  --resource-group YOUR_RESOURCE_GROUP `
  --name tasmee3-app `
  --src ..\deploy.zip
```

---

### 6️⃣ Test Deployment (3 min)

1. **Open Application:**
   ```
   https://tasmee3-app.azurewebsites.net
   ```

2. **Login:**
   - Email: `admin@system.local`
   - Password: `Admin123!`

3. **Verify:**
   - ✅ Login successful
   - ✅ Dashboard loads
   - ✅ School branding shows (if configured)
   - ✅ Can navigate to Users, Sessions, Reports

4. **Test Cold Start:**
   - Leave app idle for 5 minutes
   - Refresh page
   - Should see loading indicator if database was paused
   - Page loads successfully after 3-5 seconds

---

## ✅ Success Checklist

After deployment, verify:
- [ ] Application accessible at `https://YOUR_APP_NAME.azurewebsites.net`
- [ ] Can login with default admin credentials
- [ ] Dashboard displays correctly
- [ ] Can create a listening session
- [ ] Data saves to Azure SQL database
- [ ] Loading indicator works during database cold start
- [ ] No errors in browser console
- [ ] No errors in Azure App Service logs

---

## 🔧 Immediate Next Steps

1. **Change Admin Password**
   ```
   Login → Admin → Dashboard → Settings → Change Password
   ```

2. **Upload School Logo**
   ```
   Admin → Settings → School Logo → Upload
   ```

3. **Set School Name**
   ```
   Admin → Settings → School Name → Enter name → Save
   ```

4. **Create Test Teacher Account**
   ```
   Admin → Users → Add User → Select Teacher role
   ```

5. **Create Test Student Account**
   ```
   Admin → Users → Add User → Select Student role
   ```

---

## 📊 Enable Monitoring (Optional but Recommended)

### Application Insights (5 min)
1. Azure Portal → App Service `tasmee3-app`
2. **Application Insights** (left menu)
3. **Turn on Application Insights**
4. Create new or select existing
5. Click **Apply**

**Benefits:**
- Track response times
- Monitor database retries
- Detect errors automatically
- Analyze user behavior

---

## 💰 Cost Management

### Current Setup (Free Tier)
- **App Service F1**: $0/month
- **Azure SQL (2GB, Serverless)**: $5-15/month
- **Total**: ~$5-15/month

### Optimize for Auto-Pause
1. SQL Database → **Compute + storage**
2. Set **Auto-pause delay**: 60 minutes
3. **Min vCores**: 0.5
4. **Max vCores**: 1
5. Click **Apply**

**Savings:**
- Database pauses after 1 hour idle
- Only charged for active time
- 60-70% cost reduction

---

## 🆘 Quick Troubleshooting

### Can't connect to database?
```powershell
# Test connection from local machine
Test-NetConnection tasmee3dbserver.database.windows.net -Port 1433
```
✅ Success: `TcpTestSucceeded : True`  
❌ Failed: Check firewall rules

### Application won't start?
Check logs:
```bash
az webapp log tail --resource-group YOUR_RESOURCE_GROUP --name tasmee3-app
```

### Database login failed?
1. Verify password in Connection String
2. Check user has `db_owner` role
3. Restart App Service

---

## 📚 Reference Documents

- **AZURE_SETUP.md** - Detailed setup guide (your specific configuration)
- **DEPLOYMENT_GUIDE.md** - General deployment information
- **AZURE_SQL_IMPLEMENTATION.md** - Technical implementation details
- **README.md** - Project overview

---

## 🎯 You're Ready!

Your application is now:
- ✅ Azure SQL ready with retry policy
- ✅ Cold start handling implemented
- ✅ Loading indicators in place
- ✅ Environment-specific configuration
- ✅ Production-ready security

**Just deploy and go! 🚀**

---

**Estimated Total Time**: 30 minutes  
**Difficulty**: Easy (step-by-step guide)  
**Cost**: ~$5-15/month (Azure SQL serverless)

