# E-commerce MVC Application - Deployment Guide for Windows Server 2019

This guide will walk you through deploying this .NET Framework 4.8 MVC application on Windows Server 2019 with IIS.

## Prerequisites

Before you begin, ensure you have:
- Windows Server 2019
- Administrator access
- SQL Server installed with the EcommerceDB database
- Internet connection for downloading tools

---

## Part 1: Install Required Software

### Step 1: Install IIS (Internet Information Services)

1. Open **PowerShell as Administrator**
2. Run these commands:

```powershell
# Install IIS with ASP.NET support
Install-WindowsFeature -name Web-Server -IncludeManagementTools
Install-WindowsFeature Web-Asp-Net45
Install-WindowsFeature Web-Net-Ext45
Install-WindowsFeature Web-ISAPI-Ext
Install-WindowsFeature Web-ISAPI-Filter
```

3. Verify IIS is running:
   - Open a browser and go to `http://localhost`
   - You should see the default IIS welcome page

### Step 2: Install .NET Framework 4.8

1. Check if .NET 4.8 is already installed:
   - Open PowerShell and run: `Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"`
   - Look for `Release` value >= 528040 (this means 4.8 is installed)

2. If not installed:
   - Download from: https://dotnet.microsoft.com/download/dotnet-framework/net48
   - Run the installer
   - Restart the server

### Step 3: Install Visual Studio 2022 (for development)

**Option A: Visual Studio 2022 Community (Recommended for beginners)**
1. Download from: https://visualstudio.microsoft.com/downloads/
2. During installation, select:
   - ASP.NET and web development workload
   - .NET Framework 4.8 development tools
3. This will take 30-60 minutes

**Option B: VS Code (Lighter alternative)**
1. Download VS Code from: https://code.visualstudio.com/
2. Install C# extension
3. Install .NET Framework Developer Pack

---

## Part 2: Setup the Database

### Step 1: Create the Database

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Open the file `ecommerce_schema.sql` from your project
4. Execute the script (F5) - this creates the EcommerceDB database

### Step 2: Load Test Data

1. Open `ecommerce_test_data.sql`
2. Execute the script - this populates the database with sample data

### Step 3: Note Your Connection Details

You'll need:
- **Server Name**: Usually `localhost` or `.\SQLEXPRESS` or your server's hostname
- **Database Name**: `EcommerceDB`
- **Authentication**: Windows Authentication (Integrated Security) or SQL Server Authentication

---

## Part 3: Configure the Application

### Step 1: Update Connection String

1. Open `EcommerceMVC/Web.config`
2. Find the `<connectionStrings>` section
3. Update with your SQL Server details:

**For Windows Authentication (Integrated Security):**
```xml
<add name="EcommerceDB" 
     connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=EcommerceDB;Integrated Security=True;MultipleActiveResultSets=True" 
     providerName="System.Data.SqlClient" />
```

**For SQL Server Authentication:**
```xml
<add name="EcommerceDB" 
     connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=EcommerceDB;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=True" 
     providerName="System.Data.SqlClient" />
```

**Examples:**
- Local SQL Server: `Data Source=localhost;...`
- SQL Express: `Data Source=.\SQLEXPRESS;...`
- Named instance: `Data Source=SERVERNAME\INSTANCENAME;...`

---

## Part 4: Build and Test Locally

### Step 1: Open the Project in Visual Studio

1. Open Visual Studio 2022
2. Click **File > Open > Project/Solution**
3. Navigate to `EcommerceMVC` folder
4. Select `EcommerceMVC.csproj`

### Step 2: Restore NuGet Packages

1. In Visual Studio, right-click on the solution in Solution Explorer
2. Click **Restore NuGet Packages**
3. Wait for packages to download (Entity Framework, MVC, etc.)

### Step 3: Build the Project

1. Press **Ctrl+Shift+B** or click **Build > Build Solution**
2. Check the Output window - should say "Build succeeded"
3. Fix any errors if they appear

### Step 4: Test Locally

1. Press **F5** or click the green "Play" button
2. Your default browser should open with the application
3. You should see the dashboard with statistics
4. Test navigation: Customers, Products, Orders
5. Try creating a new customer or product

**If you see errors:**
- Database connection error: Check your connection string
- Missing tables: Run the schema script again
- 404 errors: Check that routing is configured correctly

---

## Part 5: Deploy to IIS

### Step 1: Publish the Application

1. In Visual Studio, right-click on the project (not solution)
2. Click **Publish**
3. Choose **Folder** as the target
4. Set publish location: `C:\inetpub\wwwroot\EcommerceMVC`
5. Click **Publish**
6. Wait for the publish to complete

**What this does:**
- Compiles your code into DLLs
- Copies all necessary files (views, configs, etc.)
- Creates a ready-to-run application folder

### Step 2: Create Application Pool in IIS

1. Open **IIS Manager** (search for "IIS" in Start menu)
2. In the left panel, expand your server name
3. Click on **Application Pools**
4. In the right panel, click **Add Application Pool**
5. Configure:
   - **Name**: `EcommerceMVCPool`
   - **.NET CLR Version**: `.NET CLR Version v4.0.30319`
   - **Managed Pipeline Mode**: `Integrated`
6. Click **OK**

### Step 3: Configure Application Pool Identity

1. Right-click on `EcommerceMVCPool`
2. Click **Advanced Settings**
3. Under **Process Model**, click on **Identity**
4. Choose one of:
   - **ApplicationPoolIdentity** (default, recommended)
   - **NetworkService**
   - **Custom account** (if you need specific database permissions)
5. Click **OK**

### Step 4: Grant Database Permissions

If using Windows Authentication, grant the Application Pool identity access to SQL Server:

1. Open **SQL Server Management Studio**
2. Expand **Security > Logins**
3. Right-click **Logins** > **New Login**
4. For **Login name**, enter:
   - `IIS APPPOOL\EcommerceMVCPool` (for ApplicationPoolIdentity)
5. Go to **User Mapping**
6. Check `EcommerceDB`
7. Select roles: `db_datareader`, `db_datawriter`
8. Click **OK**

### Step 5: Create Website in IIS

1. In IIS Manager, right-click on **Sites**
2. Click **Add Website**
3. Configure:
   - **Site name**: `EcommerceMVC`
   - **Application pool**: Select `EcommerceMVCPool`
   - **Physical path**: `C:\inetpub\wwwroot\EcommerceMVC`
   - **Binding**:
     - Type: `http`
     - IP address: `All Unassigned`
     - Port: `80` (or choose another like `8080` if 80 is in use)
     - Host name: (leave blank for now)
4. Click **OK**

### Step 6: Test the Deployment

1. Open a browser
2. Navigate to: `http://localhost` (or `http://localhost:8080` if you used port 8080)
3. You should see your E-commerce dashboard
4. Test all pages: Customers, Products, Orders

---

## Part 6: Troubleshooting Common Issues

### Issue 1: "HTTP Error 500.19 - Internal Server Error"

**Cause:** Web.config configuration error

**Solution:**
1. Check that ASP.NET is registered with IIS
2. Run in PowerShell as Admin:
```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -i
```

### Issue 2: "Could not load file or assembly 'EntityFramework'"

**Cause:** Missing DLL files

**Solution:**
1. Make sure you published the application (not just copied files)
2. Check that the `bin` folder contains all DLLs
3. Re-publish from Visual Studio

### Issue 3: Database Connection Errors

**Cause:** Connection string or permissions issue

**Solution:**
1. Verify connection string in Web.config
2. Test connection from the server:
   - Open SSMS on the server
   - Try connecting with the same credentials
3. Check SQL Server allows remote connections (if database is on another server)
4. Verify Application Pool identity has database permissions

### Issue 4: "403 - Forbidden" Error

**Cause:** IIS permissions issue

**Solution:**
1. Right-click on the application folder in Windows Explorer
2. Properties > Security tab
3. Click **Edit** > **Add**
4. Add `IIS_IUSRS` and `IIS APPPOOL\EcommerceMVCPool`
5. Give them **Read & Execute** permissions

### Issue 5: Application Pool Keeps Stopping

**Cause:** Application crash or configuration error

**Solution:**
1. Check Event Viewer:
   - Windows Logs > Application
   - Look for errors from IIS or ASP.NET
2. Enable detailed errors in Web.config:
```xml
<system.web>
  <customErrors mode="Off" />
</system.web>
```
3. Check Application Pool settings:
   - Disable "Rapid-Fail Protection" temporarily for debugging

---

## Part 7: Accessing from Other Computers

### Allow Through Windows Firewall

1. Open **Windows Firewall with Advanced Security**
2. Click **Inbound Rules**
3. Click **New Rule**
4. Choose **Port** > **Next**
5. Select **TCP** and enter port `80` (or your chosen port)
6. Allow the connection
7. Apply to all profiles
8. Name it "IIS HTTP"

### Access from Another Computer

- From another computer on the same network:
  - Navigate to: `http://SERVER_IP_ADDRESS`
  - Example: `http://192.168.1.100`

---

## Part 8: Next Steps - Modernization Path

Once this application is running, you can:

1. **Use AWS Porting Assistant for .NET**
   - Analyzes your code for .NET Core compatibility
   - Identifies issues and suggests fixes
   - Download from: https://aws.amazon.com/porting-assistant-dotnet/

2. **Migrate to .NET 6/7/8**
   - Convert to .NET Core/6+
   - Separate API backend from frontend
   - Use modern patterns (dependency injection, async/await)

3. **Add React/Angular Frontend**
   - Build a modern SPA frontend
   - Use the .NET backend as a REST API
   - Better user experience and performance

4. **Deploy to AWS**
   - Use Elastic Beanstalk for easy deployment
   - Or containerize with Docker and use ECS/EKS
   - Use RDS for managed database

---

## Quick Reference Commands

### Restart IIS
```powershell
iisreset
```

### Restart Specific Application Pool
```powershell
Restart-WebAppPool -Name "EcommerceMVCPool"
```

### Check .NET Version
```powershell
Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"
```

### View IIS Logs
```
C:\inetpub\logs\LogFiles\
```

---

## Support and Resources

- **IIS Documentation**: https://docs.microsoft.com/en-us/iis/
- **ASP.NET MVC**: https://docs.microsoft.com/en-us/aspnet/mvc/
- **Entity Framework 6**: https://docs.microsoft.com/en-us/ef/ef6/
- **AWS Porting Assistant**: https://aws.amazon.com/porting-assistant-dotnet/

---

## Summary

You now have a working .NET Framework 4.8 MVC application running on IIS. This represents a typical legacy enterprise application that can be modernized to .NET Core with modern frontend frameworks.

**Key Files to Remember:**
- `Web.config` - Configuration and connection strings
- `Global.asax.cs` - Application startup
- `Controllers/` - Business logic
- `Models/` - Data models and Entity Framework context
- `Views/` - Razor HTML templates

Good luck with your PoC!
