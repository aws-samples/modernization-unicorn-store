using System.Linq;
using Amazon.CDK.AWS.ECS;
using System.Collections.Generic;
using SecMan = Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK;

namespace InfraAsCode.Reusable
{
    public static class SdkExtensions
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
                    PasswordLength = passwordLength,
                }
            };
        }

        /// <summary>
        /// The work-around for the "Resolution error: System.Reflection.TargetParameterCountException: Parameter count mismatch"
        /// bug when using Secrets as is
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="secretName"></param>
        /// <returns></returns>
        private static Secret WrapSecretBug(this Construct parent, Secret secret, string secretName)
        {
            var smSecret = SecMan.Secret.FromSecretArn(parent, $"{secretName}BugWorkaround", secret.Arn);
            return Secret.FromSecretsManager(smSecret);
        }

        public static SecMan.Secret CreateSecretConstruct(this SecMan.SecretProps smSecretDef, Construct parent)
        {
            // Assuming here that it's OK if secret Construct name matches secret name
            return new SecMan.Secret(parent, smSecretDef.SecretName, smSecretDef);
        }

        public static Secret CreateSecret(this SecMan.SecretProps smSecretDef, Construct parent)
        {
            SecMan.Secret smSecret = smSecretDef.CreateSecretConstruct(parent);
            return smSecret.CreateSecret(parent, smSecretDef.SecretName);
        }

        public static Secret CreateSecret(this SecMan.Secret smSecret, Construct parent, string secretName)
        {
            // TODO: WrapSecretBug() part should be removed after "Parameter count mismatch" issue is resolved
            return parent.WrapSecretBug(Secret.FromSecretsManager(smSecret), secretName);
        }
    }
}
