var CodePipeline = require('./cdklib/codePipeline');
var ECS = require('./cdklib/ecs');

/*
 * Entry point for debugging in VsCode and alike
*/
async function debug() {
    var args = process.argv.slice(2);
    await ECS.refreshEcsCluster(args[0]);
}
debug();

/*
 * Entry point for the CodePipeline event sink.
 * Used by the CDK FunctionProps.Handler property. 
 * See CreateLambdaForRestartingEcsApp() method in the CDK project.
 */
exports.handler = async (event, context) => {
    return await CodePipeline.lambdaWrapper(event, context, ECS.refreshEcsCluster);
}