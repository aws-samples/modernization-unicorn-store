@echo off

del /F /Q C:\Users\Administrator\StartupArea\WorkingArea\*.*

cd C:\Users\Administrator\.aws\
del config
del credentials

cd C:\Users\Administrator\AppData\Local\AWSToolkit\teamexplorer\
del connections.json

rd /s /q "C:\ProjectsTemp\"
rd /s /q "C:\Projects\"

cd C:\Users\Administrator\StartupArea\scripts
