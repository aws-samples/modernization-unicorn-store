cd C:\Users\Administrator\InitScripts\VSCELicense
Import-Module .\VSCELicense
Get-VSCELicenseExpirationDate -Version VS2019
Set-VSCELicenseExpirationDate -Version VS2019
