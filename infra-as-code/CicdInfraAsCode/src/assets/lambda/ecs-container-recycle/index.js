var AWS = require('aws-sdk');
var CodePipe = require('./cdklib/codePipeline');

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

exports.handler = async (event, context) => {
    return await CodePipe.lambdaWrapper(event, context, recycleClusterTasks);
}