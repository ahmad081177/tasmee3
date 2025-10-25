# Environment Configuration Guide

This guide explains how to run the Quran Listening App in different environments with the correct configuration files.

## Configuration Files Overview

### 1. `appsettings.json` (Base Configuration)
- **Purpose**: Default/fallback configuration
- **Environment**: Default base settings
- **Database**: Local SQL Server LocalDB
- **Usage**: Fallback when no specific environment is set

### 2. `appsettings.Development.json` (Local Development)
- **Purpose**: Local development on your laptop
- **Environment**: Development
- **Database**: Local SQL Server LocalDB (`QuranListeningDb`)
- **Features**: 
  - Detailed logging enabled
  - Exception details shown
  - Developer dashboard enabled
  - No retry policies (faster for debugging)

### 3. `appsettings.Production.json` (Azure Production)
- **Purpose**: Azure App Service production deployment
- **Environment**: Production
- **Database**: Azure SQL Database (`tasmee3db77`)
- **Features**:
  - Minimal logging for performance
  - Error details hidden for security
  - Retry policies enabled for reliability
  - Azure SQL optimizations

## How to Run Locally (Development)

### Method 1: Using Visual Studio 2022
1. **Open the solution** in Visual Studio 2022
2. **Set the startup project** to `QuranListeningApp.Web`
3. **Check the environment**: 
   - Go to Project Properties → Debug → General
   - Ensure `ASPNETCORE_ENVIRONMENT` is set to `Development`
4. **Run the application** by pressing `F5` or clicking the green play button
5. **Verify**: The app will use `appsettings.Development.json` automatically

### Method 2: Using Command Line
```powershell
# Navigate to the web project directory
cd C:\ws\personal\mine\tasmee3\src\QuranListeningApp.Web

# Set the environment variable (PowerShell)
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Run the application
dotnet run

# The app will start on https://localhost:7000 and http://localhost:5000
```

### Method 3: Using dotnet watch (Hot Reload)
```powershell
# Navigate to the web project directory
cd C:\ws\personal\mine\tasmee3\src\QuranListeningApp.Web

# Set environment and run with hot reload
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet watch run
```

## How to Publish to Azure from Visual Studio

### Step 1: Configure Publishing Profile
1. **Right-click** on `QuranListeningApp.Web` project
2. **Select** "Publish..."
3. **Choose** "Azure" → "Azure App Service (Windows)"
4. **Select** your existing app service: `tasmee3`
5. **Click** "Finish" to create the publish profile

### Step 2: Configure Environment for Production
1. **In the Publish dialog**, click on the **"..."** (More actions) menu
2. **Select** "Edit"
3. **Go to** "Settings" tab
4. **Expand** "File Publish Options"
5. **Check** "Remove additional files at destination"
6. **In Configuration**, ensure it's set to **"Release"**

### Step 3: Set Environment Variables in Azure
1. **Go to Azure Portal** → App Services → `tasmee3`
2. **Navigate to** "Configuration" → "Application settings"
3. **Add/Update** the following setting:
   ```
   Name: ASPNETCORE_ENVIRONMENT
   Value: Production
   ```
4. **Click** "Save"

### Step 4: Publish the Application
1. **In Visual Studio**, click **"Publish"**
2. **The app will**:
   - Build in Release mode
   - Use `appsettings.Production.json` for configuration
   - Connect to Azure SQL Database
   - Apply production logging settings

## Environment Variables Explanation

### ASPNETCORE_ENVIRONMENT Values:
- **`Development`**: Uses `appsettings.Development.json` + `appsettings.json`
- **`Production`**: Uses `appsettings.Production.json` + `appsettings.json`
- **Not Set**: Uses only `appsettings.json`

### Configuration Precedence (Higher numbers override lower):
1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (environment-specific)
3. Environment variables
4. Command-line arguments

## Database Configuration Details

### Local Development Database
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuranListeningDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
- **Server**: LocalDB instance
- **Database**: `QuranListeningDb`
- **Authentication**: Windows Authentication (Trusted_Connection)

### Azure Production Database
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tasmee3dbserver77.database.windows.net;Database=tasmee3db77;User Id=tasmee3dbserver77-admin;Password=$WIFMGcnc71TgCJS;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
```
- **Server**: Azure SQL Server
- **Database**: `tasmee3db77`
- **Authentication**: SQL Authentication

## Troubleshooting

### Local Development Issues:
1. **LocalDB not running**:
   ```powershell
   SqlLocalDB start MSSQLLocalDB
   ```

2. **Database doesn't exist**:
   ```powershell
   cd C:\ws\personal\mine\tasmee3\src\QuranListeningApp.Web
   dotnet ef database update
   ```

3. **Wrong environment**:
   - Check `Properties\launchSettings.json` in your project
   - Verify `ASPNETCORE_ENVIRONMENT` is set to `Development`

### Azure Production Issues:
1. **Connection string problems**:
   - Verify Azure SQL Server firewall rules
   - Check connection string format in `appsettings.Production.json`

2. **Environment not set**:
   - Verify `ASPNETCORE_ENVIRONMENT=Production` in Azure App Service Configuration

3. **Database schema issues**:
   - Ensure migrations are applied to Azure database
   - Check Azure SQL Query Editor for schema status

## Quick Commands Reference

### Local Development:
```powershell
# Start development server
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run

# Update local database
dotnet ef database update

# Create new migration
dotnet ef migrations add "MigrationName"
```

### Check Current Environment:
```powershell
# In PowerShell
echo $env:ASPNETCORE_ENVIRONMENT

# In application (add to a page)
@inject IWebHostEnvironment Environment
<p>Current Environment: @Environment.EnvironmentName</p>
```

This setup ensures clear separation between your local development environment and Azure production environment, with appropriate configurations for each scenario.