:: This script is called by Windows Scheduler on System Startup 
:: The EC2 instance must have a Role with AdministratorAccess policy attached.
@echo off
echo starting script

cd C:\
if not exist Projects (
:: The rest of the script is executed only if "c:\Projects" directory does not exist.

:: Clone Unicorn Store app sample & init scripts from GitHub
mkdir C:\Projects
cd C:\Projects
git clone https://github.com/vgribok/modernization-unicorn-store.git
cd modernization-unicorn-store

:: Chekout "cdk-module" branch with the CDK workshop codebase 
git checkout cdk-module
echo checked out latest project

:: Build the app
dotnet build UnicornStore.sln -c DebugPostgres

:: Postpone VS Community Edition sign-in nag
cd C:\Users\Administrator\InitScripts\VSCELicense
powershell -File Execute-On-Start.ps1

:: Copy all scripts to the "C:\Users\Administrator\StartupArea"
cd C:\Users\Administrator\
if not exist StartupArea (
mkdir StartupArea
)
robocopy C:\Projects\modernization-unicorn-store\hugo-content\resources\code\vm-init-scripts C:\Users\Administrator\StartupArea /E /Z /ZB /R:5 /W:5 /TBD /NP /V
echo copied scripts from repo to working area

cd C:\Users\Administrator\StartupArea\scripts

:: Invoke "steps-configurator.bat"
call C:\Users\Administrator\StartupArea\scripts\steps-configurator.bat
)
