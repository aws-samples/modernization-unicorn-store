@echo off
echo starting script

cd C:\
if not exist Projects (
mkdir C:\Projects
cd C:\Projects
git clone https://github.com/vgribok/modernization-unicorn-store.git
cd modernization-unicorn-store
git checkout feature/hugo-web-site-content
dotnet build UnicornStore.sln -c DebugSqlServer
echo checked out latest project

cd C:\Users\Administrator\
if not exist StartupArea (
mkdir StartupArea
)
robocopy C:\ProjectsTemp\modernization-unicorn-store\dev-image\StartupScripts\ C:\Users\Administrator\StartupArea /E /Z /ZB /R:5 /W:5 /TBD /NP /V
echo copied scripts from repo to working area

cd C:\Users\Administrator\StartupArea\scripts
call C:\Users\Administrator\StartupArea\scripts\steps-configurator.bat
)