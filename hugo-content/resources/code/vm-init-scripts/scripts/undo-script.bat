:: This script is called from "lab-assets-deletion.bat"
@echo off

del /F /Q C:\Users\Administrator\StartupArea\WorkingArea\*.*

cd C:\Users\Administrator\.aws\
del config
del credentials

cd C:\Users\Administrator\AppData\Local\AWSToolkit\teamexplorer\
del connections.json

REM rd /s /q "C:\ProjectsTemp\"
rd /s /q "C:\Projects\"
rd /s /q "C:\Users\Administrator\StartupArea\"

cd C:\Users\Administrator\InitScripts
