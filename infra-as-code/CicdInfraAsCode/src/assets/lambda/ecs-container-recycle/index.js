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

    console.log('Trace: Main logic start. Cluster ARN is: ' + clusterArn);

    var ecs = new AWS.ECS();

    // https://docs.aws.amazon.com/AWSJavaScriptSDK/latest/AWS/ECS.html#listTasks-property
    var getTasksParams = {
        cluster: clusterArn,
        desiredStatus: "RUNNING",
        launchType: "FARGATE",
    };
    try { // This try/catch may look excessive/too-defensive, but it exists to compensate 
        // for weird exception handling behavior on Node 10 runtime (was fine on Node 8, but 8 is at EOL on AWS Lambda).
        // TODO: re-test removal of the try/catch at some point in the future.
        var tasks = await ecs.listTasks(getTasksParams).promise();
        console.log('Trace: Main logic: successfully got ECS tasks:\n' + JSON.stringify(tasks));
        //return tasks;
    }
    catch (err) {
        console.log('TRACE: Main logic: ERROR getting tasks from ECS cluster:\n' + JSON.stringify(err));
        return null;
    }

    await Promise.all(tasks.taskArns.map(async ecsTaskArn => {
        console.log('TRACE: Main logic: about to stop task: ' + ecsTaskArn);
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