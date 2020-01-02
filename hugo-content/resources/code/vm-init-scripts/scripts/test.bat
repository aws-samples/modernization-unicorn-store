@echo off
powershell -Command "(gc open-repo-folder.template) -replace 'REPOFOLDER', (gc reponame.txt) | Out-File -encoding ASCII open-repo-folder.bat"
call C:\Users\Administrator\open-repo-folder.bat

dir
