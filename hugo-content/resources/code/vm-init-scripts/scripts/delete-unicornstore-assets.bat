:: This script is called from "lab-assets-deletion.bat"
:: Deletes sample app AWS infra
@echo off
echo starting script

cd C:\Users\Administrator\StartupArea\WorkingArea
powershell -Command "aws configure set default.region (gc region.txt)"
echo aws region set

cd C:\Users\Administrator\InitScripts
powershell -Command "aws cloudformation delete-stack --stack-name Unicorn-Store-CI-CD-PipelineStack"
echo store pipeline stack deleted 

powershell -Command "aws cloudformation delete-stack --stack-name UnicornSuperstoreStack"
echo store stack deleted

powershell -Command "aws ecr delete-repository --repository-name unicorn-store-app --force"
echo ecr repo deleted
