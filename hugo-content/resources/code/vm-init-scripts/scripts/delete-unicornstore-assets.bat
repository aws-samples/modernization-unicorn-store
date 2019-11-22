@echo off
echo starting script

aws cloudformation delete-stack --stack-name Unicorn-Store-CI-CD-PipelineStack
echo store pipeline stack deleted 

aws cloudformation delete-stack --stack-name UnicornSuperstoreStack
echo store stack deleted

aws ecr delete-repository --repository-name unicorn-store-app --force
echo ecr repo deleted
