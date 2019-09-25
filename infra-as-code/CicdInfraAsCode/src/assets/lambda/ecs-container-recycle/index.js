var AWS = require('aws-sdk');

/*
 * Entry point for debugging in VsCode and alike
*/
async function debug() {
    var args = process.argv.slice(2);
    await recycleClusterTasks(args[0]);
}
debug();

async function recycleClusterTasks(clusterArn) {
    var ecs = new AWS.ECS();

    // https://docs.aws.amazon.com/AWSJavaScriptSDK/latest/AWS/ECS.html#listTasks-property
    var getTasksParams = {
        cluster: clusterArn,
        desiredStatus: "RUNNING",
        launchType: "FARGATE",
    };
    var tasks = await ecs.listTasks(getTasksParams).promise();
    //return tasks;

    await Promise.all(tasks.taskArns.map(async ecsTaskArn => {
        // https://docs.aws.amazon.com/AWSJavaScriptSDK/latest/AWS/ECS.html#stopTask-property
        var stopEcsTaskParams = {
            task: ecsTaskArn,
            cluster: clusterArn,
            reason: 'Restarting cluster containers after new build pushed to ECR'
        };

        var stoppedInfo = await ecs.stopTask(stopEcsTaskParams).promise();
        console.log(JSON.stringify(stoppedInfo, undefined, 2));
    }));
}

async function codePipelineLambdaFunc(event, context) {
    // Refer to https://docs.aws.amazon.com/codepipeline/latest/userguide/actions-invoke-lambda-function.html
    // for details re: how to create CodePipeline-aware Lambda function.

    var pipelineInfo = event["CodePipeline.job"];
    
    var clusterArn = pipelineInfo.data.actionConfiguration.configuration.UserParameters; 
    var jobId = pipelineInfo.id;
    var invokeId = context.invokeid;

    var result = await codePipelineLambdaCaller(jobId, clusterArn, invokeId);
    return result;
}

async function codePipelineLambdaCaller(jobId, clusterArn, invokeId) {
    var codePipeline = new AWS.CodePipeline();
    try {
        await recycleClusterTasks(clusterArn);
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

exports.handler = async (event, context) => {
    return await codePipelineLambdaFunc(event, context);
}