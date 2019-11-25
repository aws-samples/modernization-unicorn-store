@echo off
echo starting script

cd C:\
if not exist Projects (
mkdir C:\Projects
cd C:\Projects
git clone https://github.com/vgribok/modernization-unicorn-store.git
cd modernization-unicorn-store
git checkout cdk-module
dotnet build UnicornStore.sln -c DebugPostgres
echo checked out latest project

cd C:\Users\Administrator\InitScripts\VSCELicense
powershell -File Execute-On-Start.ps1

cd C:\Users\Administrator\
if not exist StartupArea (
mkdir StartupArea
)
robocopy C:\Projects\modernization-unicorn-store\hugo-content\resources\code\vm-init-scripts C:\Users\Administrator\StartupArea /E /Z /ZB /R:5 /W:5 /TBD /NP /V
echo copied scripts from repo to working area

cd C:\Users\Administrator\StartupArea\scripts
call C:\Users\Administrator\StartupArea\scripts\steps-configurator.bat
)