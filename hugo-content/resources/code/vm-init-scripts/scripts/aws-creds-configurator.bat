@echo off
echo starting script

cd C:\Users\Administrator\
if not exist .aws (
mkdir .aws
)
echo .aws directory created

cd C:\Users\Administrator\.aws\
if not exist credentials (
powershell -executionpolicy bypass -file C:\ProgramData\Amazon\EC2-Windows\Launch\Scripts\InitializeInstance.ps1

net user Administrator Passw0rd
echo Admnistrator password set to Passw0rd

cd C:\Users\Administrator\StartupArea\WorkingArea
del /F /Q C:\Users\Administrator\StartupArea\WorkingArea\*.*
copy C:\Users\Administrator\StartupArea\templates\*.* C:\Users\Administrator\StartupArea\WorkingArea
echo working area prepared

cd C:\Users\Administrator\StartupArea\WorkingArea
guidgen | tr -d " \t\n\r" >> username.txt
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
powershell -Command "(gc connections.template) -replace 'AWSREGION', (gc region.txt) | Out-File -encoding ASCII connections.json"
echo codecommit region configured 
powershell -Command "(gc '.gitconfig.template') -replace 'AWSREGION', (gc region.txt) | Out-File -encoding ASCII '.gitconfig'"
echo codecommit region configured 
powershell -Command "(gc gitremote.template) -replace 'AWSREGION', (gc region.txt) | Out-File -encoding ASCII gitremote.txt"
echo codecommit region configured 

cd C:\Users\Administrator\AppData\Local\AWSToolkit
if not exist teamexplorer (
mkdir teamexplorer
)
echo teamexplorer directory created

copy /Y C:\Users\Administrator\StartupArea\WorkingArea\credentials C:\Users\Administrator\.aws\
copy /Y C:\Users\Administrator\StartupArea\WorkingArea\config C:\Users\Administrator\.aws\
echo copied files to .aws

copy /Y C:\Users\Administrator\StartupArea\WorkingArea\connections.json C:\Users\Administrator\AppData\Local\AWSToolkit\teamexplorer\
echo copied files to teamexplorer

copy /Y C:\Users\Administrator\StartupArea\WorkingArea\.gitconfig C:\Users\Administrator
echo gitconfig copied

cd C:\Users\Administrator\StartupArea\WorkingArea
powershell -Command "aws configure set aws_access_key_id (gc accesskey.txt | jq -r .AccessKey.AccessKeyId)"
powershell -Command "aws configure set aws_secret_access_key (gc accesskey.txt | jq -r .AccessKey.SecretAccessKey)"
powershell -Command "aws configure set default.region (gc region.txt)"
echo aws reconfigured
refreshenv
echo environment refreshed
SLEEP 10

cd C:\Users\Administrator\StartupArea\WorkingArea
powershell -Command "aws iam create-service-specific-credential --user-name (gc username.txt) --service-name codecommit.amazonaws.com" > codecommit-creds.txt
echo codecommit credentials created

cd C:\Projects
git config --system credential.helper "!aws codecommit credential-helper $@"
git config --system credential.UseHttpPath true
git config --system user.email "user@codecommit.aws"
powershell -Command "git config --system user.name (gc C:\Users\Administrator\StartupArea\WorkingArea\codecommit-creds.txt | jq -r .ServiceSpecificCredential.UserName)"
echo git configuration set

cd C:\Projects\modernization-unicorn-store\infra-as-code\CicdInfraAsCode\src\
cdk bootstrap
echo cdk bootstraped

cd C:\Projects\modernization-unicorn-store\
copy /Y C:\Users\Administrator\StartupArea\WorkingArea\gitremote.txt C:\Projects\modernization-unicorn-store\
powershell -Command "git remote add aws (gc gitremote.txt)"
del gitremote.txt
echo git remote added

cd C:\Users\Administrator\StartupArea\scripts

) else (
echo Credentials already configured
)
