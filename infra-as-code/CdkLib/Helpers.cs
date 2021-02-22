using Amazon.CDK.AWS.IAM;
using System.Linq;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using SecMan = Amazon.CDK.AWS.SecretsManager;

namespace CdkLib
{
    public static partial class Helpers
    {
        /// <summary>
        /// TODO: Make password more policy more secure.
        /// Creates Secret Construct definition (Props) for an auto-generated
        /// password that gets materialized only when stack runs, ensuring
        /// that password won't be outputted into the CF stack and saved as
        /// plain text anywhere.
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public static SecMan.SecretProps CreateAutoGenPasswordSecretDef(string secretName, int passwordLength = 10)
        {
            return new SecMan.SecretProps
            {
                SecretName = secretName,
                GenerateSecretString = new SecMan.SecretStringGenerator
                {
                    ExcludeCharacters = "/@\" ",
                    PasswordLength = passwordLength,
                }
            };
        }

        public static PolicyStatement[] FromPolicyProps(params PolicyStatementProps[] propses) =>
            propses
                .Where(props => props != null)
                .Select(props => new PolicyStatement(props))
                .ToArray()
            ;

        public static StageProps StageFromActions(string stageName, params Action[] actions) =>
                new StageProps
                {
                    StageName = stageName,
                    Actions = actions
                };

        public static IManagedPolicy[] FromAwsManagedPolicies(params string[] awsPolicyNames) =>
            awsPolicyNames
                .Where(policy => !string.IsNullOrWhiteSpace(policy))
                .Select(policy => ManagedPolicy.FromAwsManagedPolicyName(policy))
                .ToArray()
            ;
    }
}
