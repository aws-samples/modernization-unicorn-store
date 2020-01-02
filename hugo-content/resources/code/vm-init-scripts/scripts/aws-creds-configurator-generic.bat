@echo off
echo starting script

if not exist credentials (

mkdir C:\ProjectsTemp
cd C:\ProjectsTemp
git clone https://github.com/vgribok/modernization-unicorn-store.git
cd modernization-unicorn-store
git checkout development
REM dotnet build UnicornStore.sln -c DebugSqlServer
REM "%ProgramFiles(x86)%\Common Files\Microsoft Shared\MSEnv\vslauncher.exe" UnicornStore.sln
echo checked out latest project

cd C:\Users\Administrator\.aws\
echo Configuring AWS CLI credentials
guidgen | tr -d " \t\n\r" > username.txt
powershell -Command "aws iam create-user --user-name (gc username.txt)"
echo new user created

powershell -Command "aws iam create-access-key --user-name (gc username.txt) ^> accesskey.txt"
echo access key created

powershell -Command "aws iam attach-user-policy --policy-arn arn:aws:iam::aws:policy/AdministratorAccess --user-name (gc username.txt)"
echo access policy attached

powershell -Command "(gc credentials.template) -replace 'ACCESSKEY', (gc accesskey.txt | jq -r .AccessKey.AccessKeyId) | Out-File -encoding ASCII credentials.template"
echo access key added

powershell -Command "(gc credentials.template) -replace 'SECRETKEY', (gc accesskey.txt | jq -r .AccessKey.SecretAccessKey) | Out-File -encoding ASCII credentials"
echo secret key added

curl --silent http://169.254.169.254/latest/dynamic/instance-identity/document | jq -r .region > region.txt
echo got ec2 region

powershell -Command "(gc config.template) -replace 'AWSREGION', (gc region.txt) | Out-File -encoding ASCII config"
echo region added


copy C:\Users\Administrator\.aws\region.txt C:\Users\Administrator\AppData\Local\AWSToolkit\teamexplorer\
copy C:\Users\Administrator\.aws\username.txt C:\Users\Administrator\
copy C:\Users\Administrator\.aws\accesskey.txt C:\Users\Administrator\
copy C:\Users\Administrator\.aws\region.txt C:\Users\Administrator\

REM del username.txt
REM del region.txt
REM del accesskey.txt
REM del credentials.template
REM del config.template

if not exist connections.json (
echo Configuring CodeCommit region url
cd C:\Users\Administrator\AppData\Local\AWSToolkit\teamexplorer\
REM curl --silent http://169.254.169.254/latest/dynamic/instance-identity/document | jq -r .region > region.txt
powershell -Command "(gc connections.template) -replace 'AWSREGION', (gc region.txt) | Out-File -encoding ASCII connections.json"
echo codecommit region configured 

REM del region.txt
REM del connections.template

) else (
echo CodeCommit region url already configured
)

cd C:\Users\Administrator\

powershell -Command "aws configure set aws_access_key_id (gc accesskey.txt | jq -r .AccessKey.AccessKeyId)"
powershell -Command "aws configure set aws_secret_access_key (gc accesskey.txt | jq -r .AccessKey.SecretAccessKey)"
powershell -Command "aws configure set default.region (gc region.txt)"
echo aws reconfigured

refreshenv
echo environment refreshed


powershell -Command "aws iam create-service-specific-credential --user-name (gc username.txt) --service-name codecommit.amazonaws.com" > codecommit-creds.txt
echo codecommit credentials created
REM del codecommit-creds.txt

guidgen | tr -d " \t\n\r" > reponame.txt
powershell -Command "(gc repofolder.template) -replace 'REPONAME', (gc reponame.txt) | Out-File -encoding ASCII repofolder"
echo repofolder details added

powershell -Command "aws codecommit create-repository --repository-name (gc reponame.txt) --repository-description New-repository-description" > repodetails.txt
echo codecommit repository created

REM del repodetails.txt



REM set /p repovar=<reponame.txt
REM echo test repo path is C:\Projects\%repovar%

mkdir C:\Projects
cd C:\Projects
git config --system credential.helper "!aws codecommit credential-helper $@"
git config --system credential.UseHttpPath true
git config --system user.email "user@codecommit.aws"
powershell -Command "git config --system user.name (gc C:\Users\Administrator\codecommit-creds.txt | jq -r .ServiceSpecificCredential.UserName)"
echo git configuration set

REM powershell -Command "gc C:\Users\Administrator\repodetails.txt | jq -r .repositoryMetadata.repositoryName" > C:\Users\Administrator\reponame.txt

powershell -Command "git clone (gc C:\\Users\\Administrator\\repodetails.txt | jq -r .repositoryMetadata.cloneUrlHttp) (gc C:\\Users\\Administrator\\reponame.txt)"
echo repo cloned from codecommit

powershell -Command "robocopy C:\ProjectsTemp\modernization-unicorn-store\ (gc C:\Users\Administrator\repofolder) /E /Z /ZB /R:5 /W:5 /TBD /NP /V /XD C:\ProjectsTemp\modernization-unicorn-store\.git C:\ProjectsTemp\modernization-unicorn-store\.github"
echo files copied

cd C:\Users\Administrator

powershell -Command "(gc open-repo-folder.template) -replace 'REPOFOLDER', (gc reponame.txt) | Out-File -encoding ASCII open-repo-folder.bat"
call C:\Users\Administrator\open-repo-folder.bat
echo folder ready for git commands

git add -A
git commit -m "first commit"
git push

echo files pushed to git

rd /s /q "C:\ProjectsTemp\"
echo 
REM cd C:\Projects\%repovar%\
REM echo inside new repo

REM "%ProgramFiles(x86)%\Common Files\Microsoft Shared\MSEnv\vslauncher.exe" UnicornStore.sln



) else (
echo Credentials already configured
)



