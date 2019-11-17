var AWS = require('aws-sdk');

/*
 * Loads new ECS Tasks from the same Task Definition (using updated container image)
 * and then takes down older tasks using old container image, with zero downtime
 */
async function refreshEcsCluster(clusterArn) {

    console.log('Trace: Main logic start. Cluster ARN is: ' + clusterArn);

    var ecs = new AWS.ECS(); // https://docs.aws.amazon.com/AWSJavaScriptSDK/latest/AWS/ECS.html#listTasks-property

    var getServiceParams = {
        cluster: clusterArn,
        launchType: "FARGATE",
    };

    var services = null;
    try { // This try/catch may look excessive/too-defensive, but it exists to compensate 
        // for weird exception handling behavior on Node 10 runtime (was fine on Node 8, but 8 is at EOL on AWS Lambda).
        // TODO: re-test removal of the try/catch at some point in the future.
        services = await ecs.listServices(getServiceParams).promise();
        if (services == null)
            return null;
    }
    catch (err) {
        console.log('TRACE: Main logic: ERROR getting services from ECS cluster:\n' + JSON.stringify(err));
        return null;
    }

    await Promise.all(services.serviceArns.map(async ecsServiceArn => {
        console.log('TRACE: Main logic: about to updated ECS service: ' + ecsServiceArn);
        // https://docs.aws.amazon.com/AmazonECS/latest/APIReference/API_UpdateService.html
        var updateEcsServiceParams = {
            cluster: clusterArn,
            forceNewDeployment: true,
            service: ecsServiceArn
        };

        var serviceInfo = await ecs.updateService(updateEcsServiceParams).promise();
        console.log(JSON.stringify(serviceInfo, undefined, 2));
    }));
}

module.exports = {
    refreshEcsCluster
}