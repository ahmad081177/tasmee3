# ⚠️ SECURITY REMINDER

## 🔐 Critical: Database Password

### Where to Store Password

✅ **SAFE - Use These:**
- Azure App Service Configuration (Connection Strings)
- Azure Key Vault
- Environment variables on server
- Password manager (1Password, LastPass, etc.)
- Secure notes (encrypted)

❌ **NEVER - Don't Use These:**
- appsettings.json or appsettings.Production.json
- Git repository
- Plain text files
- Email or chat messages
- Code comments
- Documentation files

---

## 🎯 Your Setup

### Azure SQL Credentials
```
Server: tasmee3dbserver.database.windows.net
Database: tasmee3db
User: tasmee3admin
Password: [YOU HAVE THIS SEPARATELY - NEVER COMMIT IT]
```

### appsettings.Production.json Status
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...Password=PLACEHOLDER_REPLACE_IN_AZURE;..."
  }
}
```

✅ This file is **SAFE to commit** because it has placeholder  
✅ Real password goes **ONLY** in Azure Portal

---

## 📋 Before Every Git Commit

Check these files **DO NOT** contain real password:

```powershell
# Quick check
Select-String -Path "src\QuranListeningApp.Web\appsettings*.json" -Pattern "Password="
```

**Expected Output:**
```
appsettings.Production.json:6:...Password=PLACEHOLDER_REPLACE_IN_AZURE;...
```

If you see actual password → **STOP** → Remove it before committing!

---

## 🚨 If Password Accidentally Committed

### Immediate Actions:

1. **Change the password immediately** in Azure Portal:
   ```
   SQL Server → tasmee3dbserver → Reset password
   ```

2. **Update App Service connection string** with new password

3. **Remove from git history** (advanced):
   ```bash
   # WARNING: This rewrites git history
   git filter-branch --force --index-filter \
     "git rm --cached --ignore-unmatch src/QuranListeningApp.Web/appsettings.Production.json" \
     --prune-empty --tag-name-filter cat -- --all
   
   git push --force --all
   ```

4. **Or simpler**: Create new database user with different password

---

## ✅ Safe Deployment Workflow

### Local Development
```json
// appsettings.Development.json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;..."
}
```
✅ No password needed (Windows authentication)

### Production
```
Azure Portal → App Service → Configuration → Connection strings
```
✅ Password stored encrypted in Azure

---

## 🔒 Additional Security Best Practices

### Change Default Credentials
After first deployment:
1. Login as admin@system.local
2. Change password immediately
3. Create new admin account
4. Disable or delete default admin (future)

### Enable HTTPS Only
```bash
az webapp update \
  --resource-group YOUR_RESOURCE_GROUP \
  --name tasmee3-app \
  --set httpsOnly=true
```

### Limit SQL Firewall
Instead of allowing all Azure services (0.0.0.0), add specific App Service IP:
1. Get outbound IPs: App Service → Properties → Outbound IP addresses
2. Add each IP to SQL firewall rules

### Enable Azure AD Authentication (Future)
Replace SQL authentication with Azure AD for better security

### Regular Security Audit
- [ ] Review SQL firewall rules monthly
- [ ] Check App Service access logs
- [ ] Monitor for unusual activity
- [ ] Keep all packages updated

---

## 📞 Emergency Contacts

### If Security Breach Suspected:
1. **Immediately** change database password
2. **Review** SQL audit logs (if enabled)
3. **Check** Application Insights for unusual patterns
4. **Rotate** all secrets and credentials

### Azure Support
- https://portal.azure.com → Help + support
- Community: https://stackoverflow.com/questions/tagged/azure-sql-database

---

## 🎓 Remember

> **Security is not a one-time task, it's an ongoing practice.**

- ✅ Passwords in Azure Portal only
- ✅ HTTPS only in production
- ✅ Change default credentials
- ✅ Regular security audits
- ✅ Keep software updated
- ✅ Monitor access logs

---

**Created**: October 23, 2025  
**Purpose**: Protect your Azure SQL credentials  
**Importance**: 🔴 CRITICAL - Read before deploying
