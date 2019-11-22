@echo off
echo starting script

cd C:\Users\Administrator\.aws\
del config
del credentials
echo config and credentials deleted

cd C:\Users\Administrator\StartupArea\WorkingArea
powershell -Command "aws iam delete-service-specific-credential --user-name (gc username.txt) --service-specific-credential-id (gc codecommit-creds.txt | jq -r .ServiceSpecificCredential.ServiceSpecificCredentialId)"
echo code commit credentials deleted

powershell -Command "aws iam delete-access-key --access-key-id (gc accesskey.txt | jq -r .AccessKey.AccessKeyId) --user-name (gc username.txt)"
echo access key deleted

powershell -Command "aws iam detach-user-policy --policy-arn arn:aws:iam::aws:policy/AdministratorAccess --user-name (gc username.txt)"
echo policy detached

powershell -Command "aws iam delete-user --user-name (gc username.txt)"
echo iam user deleted
