<#
PowerShell helper to install ASP.NET Core Hosting Bundle (attempt) and create an IIS site/app
Usage (run as Administrator):
  .\scripts\setup-iis.ps1 -ProjectPath "D:\path\to\YourProject.csproj" -SiteName "AuthServer" -Port 5043 -HttpsPort 7284 -PublishFolder "C:\inetpub\wwwroot\AuthServer" -InstallHostingBundle

Notes:
- Run PowerShell as Administrator.
- The script will: publish the project, ensure IIS management module is loaded, create an application pool (No Managed Code), create or update an IIS website bound to the specified port, update the project's Properties/launchSettings.json IIS profile applicationUrl, and optionally download+install the ASP.NET Core Hosting Bundle.
- If automatic hosting-bundle download fails, open the Microsoft .NET download page and install the "ASP.NET Core Hosting Bundle" for your .NET runtime.
#>
param(
	[Parameter(Mandatory = $true)]
	[string]$ProjectPath,

	[Parameter(Mandatory = $false)]
	[string]$PublishFolder = "$env:SystemDrive\inetpub\wwwroot\AuthServer",

	[Parameter(Mandatory = $false)]
	[string]$SiteName = "AuthServer",

	[Parameter(Mandatory = $false)]
	[string]$AppPoolName = "AuthServerAppPool",

	[Parameter(Mandatory = $false)]
	[int]$Port = 5043,

	[Parameter(Mandatory = $false)]
	[int]$HttpsPort = 7284,

	[switch]$InstallHostingBundle,

	[switch]$RemoveExistingPublish
)

function Assert-RunningAsAdmin {
	$current = [Security.Principal.WindowsIdentity]::GetCurrent()
	$principal = New-Object Security.Principal.WindowsPrincipal($current)
	if (-not $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)) {
		Write-Host "This script must be run as Administrator. Re-launching as admin..."
		Start-Process -FilePath powershell -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`" $($MyInvocation.BoundParameters | ForEach-Object { "-$($_.Key) -$($_.Value)"" } -join ' ')" -Verb RunAs
		Exit 0
	}
}

Assert-RunningAsAdmin

# Verify project path
if (-not (Test-Path $ProjectPath)) {
	Write-Error "Project file not found: $ProjectPath"
	exit 1
}

$PublishFolder = (Resolve-Path -Path $PublishFolder).Path

# Optional: install hosting bundle
if ($InstallHostingBundle) {
	Write-Host "Attempting to download & install ASP.NET Core Hosting Bundle..."
	$temp = Join-Path $env:TEMP "dotnet-hosting-bundle.exe"
	# Official redirect shortlink - may change in future. If this fails, install manually from Microsoft site.
	$hostingUrl = "https://aka.ms/dotnet/aspnetcore/hostingbundle"
	try {
		Invoke-WebRequest -Uri $hostingUrl -UseBasicParsing -OutFile $temp -ErrorAction Stop
		Write-Host "Downloaded hosting bundle to $temp"
		Start-Process -FilePath $temp -ArgumentList "/quiet" -Wait
		Write-Host "Hosting bundle installer finished."
	}
	catch {
		Write-Warning "Automatic download/install failed: $($_.Exception.Message)"
		Write-Host "Please download and install the ASP.NET Core Hosting Bundle manually from: https://dotnet.microsoft.com/download/dotnet"
	}
}

# Ensure WebAdministration module for IIS cmdlets
if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
	Import-Module WebAdministration -ErrorAction Stop
} else {
	Import-Module WebAdministration | Out-Null
}

# Publish the project
Write-Host "Publishing project..."
$publishCmd = "dotnet publish `"$ProjectPath`" -c Release -o `"$PublishFolder`""
Write-Host $publishCmd
$pub = & dotnet publish $ProjectPath -c Release -o $PublishFolder
if ($LASTEXITCODE -ne 0) {
	Write-Error "dotnet publish failed. See output above."
	exit $LASTEXITCODE
}

# Create application pool
if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
	New-WebAppPool -Name $AppPoolName
	Write-Host "Created app pool: $AppPoolName"
} else {
	Write-Host "App pool already exists: $AppPoolName"
}
# Configure app pool: No Managed Code, identity = ApplicationPoolIdentity
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"

# Create or update site
$existingSite = Get-Website | Where-Object { $_.Name -eq $SiteName }
if ($null -eq $existingSite) {
	Write-Host "Creating new IIS website: $SiteName -> $PublishFolder (port $Port)"
	New-Website -Name $SiteName -PhysicalPath $PublishFolder -Port $Port -ApplicationPool $AppPoolName
} else {
	Write-Host "Site already exists. Updating physical path and app pool."
	# update bindings if needed (keeps existing host headers)
	$existingSite | Set-ItemProperty -Name physicalPath -Value $PublishFolder
	Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName -ErrorAction SilentlyContinue
}

# --- Create or update HTTPS binding and self-signed cert for localhost ---
$httpsBinding = Get-WebBinding -Name $SiteName -Protocol https -ErrorAction SilentlyContinue
try {
	if ($null -eq $httpsBinding) {
		Write-Host "No HTTPS binding found for site. Creating self-signed certificate for 'localhost' and adding HTTPS binding on port $HttpsPort..."
		# Check for existing localhost certificate in LocalMachine\My
		$existingCert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like '*CN=localhost*' -and $_.NotAfter -gt (Get-Date) } | Select-Object -First 1
		if ($null -eq $existingCert) {
			$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\LocalMachine\My" -FriendlyName "$SiteName-localhost" -NotAfter (Get-Date).AddYears(10) -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256
			Write-Host "Created new self-signed cert: $($cert.Thumbprint)"
		} else {
			$cert = $existingCert
			Write-Host "Found existing localhost cert: $($cert.Thumbprint)"
		}

		# Add cert to Trusted Root so browser trusts it (development only)
		try {
			$certLocal = Get-Item "Cert:\LocalMachine\My\$($cert.Thumbprint)"
			$rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root","LocalMachine")
			$rootStore.Open("ReadWrite")
			$rootStore.Add($certLocal)
			$rootStore.Close()
			Write-Host "Added cert to LocalMachine\Root store"
		}
		catch {
			Write-Warning "Failed to add cert to Trusted Root: $($_.Exception.Message)"
		}

		# Create HTTPS binding in IIS
		New-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -SslFlags 0
		# Bind certificate to the port using netsh (ensure existing is removed)
		try { netsh http delete sslcert ipport=0.0.0.0:$HttpsPort } catch {}
		$thumb = $cert.Thumbprint -replace ' ',''
		$appId = '{' + [guid]::NewGuid().ToString() + '}'
		netsh http add sslcert ipport=0.0.0.0:$HttpsPort certhash=$thumb appid=$appId certstorename=MY
	} else {
		Write-Host "HTTPS binding already exists for site. Updating certificate binding..."
		# Update certificate binding for existing https binding (replace existing sslcert)
		$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like '*CN=localhost*' -and $_.NotAfter -gt (Get-Date) } | Select-Object -First 1
		if ($null -eq $cert) {
			$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\LocalMachine\My" -FriendlyName "$SiteName-localhost" -NotAfter (Get-Date).AddYears(10) -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256
		}
		try { netsh http delete sslcert ipport=0.0.0.0:$HttpsPort } catch {}
		$thumb = $cert.Thumbprint -replace ' ',''
		$appId = '{' + [guid]::NewGuid().ToString() + '}'
		netsh http add sslcert ipport=0.0.0.0:$HttpsPort certhash=$thumb appid=$appId certstorename=MY
	}
}
catch {
	Write-Warning "An error occurred while setting up HTTPS binding: $($_.Exception.Message)"
}

# Ensure folder permissions allow IIS AppPool identity to read
Write-Host "Setting folder permissions for IIS AppPool..."
$acl = Get-Acl $PublishFolder
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\$AppPoolName","ReadAndExecute, ListDirectory","ContainerInherit,ObjectInherit","None","Allow")
# If adding by name fails (app pool identity principal may not exist yet), fallback to IIS_IUSRS
try {
	$acl.AddAccessRule($rule)
	Set-Acl -Path $PublishFolder -AclObject $acl
}
catch {
	Write-Warning "Could not add AppPool-specific ACL: $($_.Exception.Message). Falling back to IIS_IUSRS."
	icacls $PublishFolder /grant "IIS_IUSRS:(OI)(CI)RX" /T | Out-Null
}

# Update launchSettings.json IIS profile applicationUrl so Visual Studio knows the site URL
$projectDir = Split-Path -Parent $ProjectPath
$launchPath = Join-Path $projectDir "Properties\launchSettings.json"
if (Test-Path $launchPath) {
	try {
		Write-Host "Updating launchSettings.json IIS applicationUrl -> https://localhost:$HttpsPort;http://localhost:$Port"
		# Backup original
		Copy-Item $launchPath "$launchPath.bak" -Force

	$jsonText = Get-Content $launchPath -Raw
		$json = $jsonText | ConvertFrom-Json
		if (-not $json.profiles) { $json | Add-Member -MemberType NoteProperty -Name profiles -Value @{} }
		$json.profiles.IIS = @{
			commandName = 'IIS';
			launchBrowser = $true;
			launchUrl = '';
			applicationUrl = "https://localhost:$HttpsPort;http://localhost:$Port";
			environmentVariables = @{ ASPNETCORE_ENVIRONMENT = 'Development' }
		}
		# Write back (pretty formatting may differ)
		$json | ConvertTo-Json -Depth 10 | Set-Content $launchPath -Encoding UTF8
	}
	catch {
		Write-Warning "Failed to update launchSettings.json: $($_.Exception.Message)"
	}
} else {
	Write-Host "launchSettings.json not found at $launchPath - skipping update."
}

Write-Host "IIS site setup complete. Browse to http://localhost:$Port/"
Write-Host "If you used https in your app, create a binding with an SSL certificate in IIS Manager."

Exit 0
