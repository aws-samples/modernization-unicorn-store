var AWS = require('aws-sdk');

/*
The helper extracting User Parameters from the CodePipeline stage
settings, then invoking user event handler, after which it calls
back CodePipeline to report whether user handler has succeeded
or failed.

CodePipeline seems to call Lambda function in the "fire-and-forget" manner,
but then CodePipeline appears to wait for a callback from the Lambda to
report whether it succeeded or failed. Otherwise CodePipeline stage hangs for
twenty minutes. The requirement to proactively communicate back to CodePipeline
whether Lambda succeeded or failed pushes quite a bit of responsibility (and
requires writing quite a bit of boilerplate code, which is better made reusable)
*/
async function lambdaWrapper(event, context, userDataAsyncProcessor) {
    // Refer to https://docs.aws.amazon.com/codepipeline/latest/userguide/actions-invoke-lambda-function.html
    // for details re: how to create CodePipeline-aware Lambda function.

    var pipelineInfo = event["CodePipeline.job"];
    
    var clusterArn = pipelineInfo.data.actionConfiguration.configuration.UserParameters; 
    var jobId = pipelineInfo.id;
    var invokeId = context.invokeid;

    var result = await codePipelineLambdaCaller(jobId, clusterArn, invokeId, userDataAsyncProcessor);
    return result;
}

async function codePipelineLambdaCaller(jobId, pipelineStageUserData, invokeId, userDataAsyncProcessor) {
    var codePipeline = new AWS.CodePipeline();
    try {
        await userDataAsyncProcessor(pipelineStageUserData);
    }
    catch(err) {
        var jobFailReportParams = {
            jobId: jobId,
            failureDetails: {
                message: JSON.stringify(err),
                type: 'JobFailed',
                externalExecutionId: invokeId
            }
        };
        var failureReportResult = await codePipeline.putJobFailureResult(jobFailReportParams).promise();
        return failureReportResult;
    }

    var successReportResult = await codePipeline.putJobSuccessResult({jobId: jobId}).promise();
    return successReportResult;
}

module.exports =
{
    lambdaWrapper
}