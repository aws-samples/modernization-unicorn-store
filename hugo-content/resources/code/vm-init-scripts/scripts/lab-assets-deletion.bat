:: This script is called from explicit lab cleanup link (c:\finish-lab.lnk)

cd C:\Users\Administrator\InitScripts
call C:\Users\Administrator\InitScripts\delete-credentials.bat
call C:\Users\Administrator\InitScripts\delete-unicornstore-assets.bat
call C:\Users\Administrator\InitScripts\undo-script.bat
