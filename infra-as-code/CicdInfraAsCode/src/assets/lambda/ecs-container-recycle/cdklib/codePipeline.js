var AWS = require('aws-sdk');

/*
TL;DR
This helper method extracts User Parameters from the CodePipeline stage
settings, then invokes your event handler, after which it calls
back CodePipeline to report whether user handler has succeeded
or failed.

Details:
CodePipeline seems to call Lambda function in the "fire-and-forget" manner,
but then CodePipeline appears to wait for a callback from the Lambda to
report whether the Lambda succeeded or failed. Otherwise CodePipeline stage hangs 
for twenty minutes until it times out, with no obvious way to abort it manually. 
The requirement to have Lambda proactively communicate back to CodePipeline
whether the Lambda succeeded or failed pushes quite a bit of responsibility (and
requires writing quite a bit of boilerplate code, which is better made reusable)
on the Lambda developer. This method calls your main function, 
the userDataAsyncProcessor(), and then does the boilerplate stuff - reporting
back to the CodePipeline.
*/
async function lambdaWrapper(event, context, userDataAsyncProcessor) {
    // Refer to https://docs.aws.amazon.com/codepipeline/latest/userguide/actions-invoke-lambda-function.html
    // for details re: how to create CodePipeline-aware Lambda function.

    // console.log('CodePipeline event:\n' + JSON.stringify(event)); // Debug output

    var pipelineInfo = event["CodePipeline.job"];
    var userDataString = pipelineInfo.data.actionConfiguration.configuration.UserParameters;
    console.log('Trace: user data: ' + userDataString);

    var clusterArn;
    try {
        var userData = JSON.parse(userDataString);
        clusterArn = userData.clusterArn == null ? userData : userData.clusterArn;
    }
    catch(err) {
        clusterArn = userDataString;
    }

    var jobId = pipelineInfo.id;
    var invokeId = context.invokeid;

    var result = await codePipelineLambdaCaller(jobId, clusterArn, invokeId, userDataAsyncProcessor);
    return result;
}

async function codePipelineLambdaCaller(jobId, pipelineStageUserData, invokeId, userDataAsyncProcessor) {
    var codePipeline = new AWS.CodePipeline();
    try {
        console.log('Trace: about to start executing main event handling logic');
        await userDataAsyncProcessor(pipelineStageUserData);

        console.log('Trace: reporting success to CodePipeline');
        var successReportResult = await codePipeline.putJobSuccessResult({ jobId: jobId }).promise();
        return successReportResult;
    }
    catch(err) {
        console.log('Trace: Caught error');
        var jobFailReportParams = {
            jobId: jobId,
            failureDetails: {
                message: JSON.stringify(err),
                type: 'JobFailed',
                externalExecutionId: invokeId
            }
        };
        console.log('Trace: reporting error to CodePipeline');
        var failureReportResult = await codePipeline.putJobFailureResult(jobFailReportParams).promise();
        return failureReportResult;
    }
}

module.exports =
    {
        lambdaWrapper
    }