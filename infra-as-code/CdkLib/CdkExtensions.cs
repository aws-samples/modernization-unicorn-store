using Amazon.CDK.AWS.ECS;
using SecMan = Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK;

namespace CdkLib
{
    public static class CdkExtensions
    {
        public static SecMan.Secret CreateSecretConstruct(this SecMan.SecretProps smSecretDef, Construct parent)
        {
            // Assuming here that it's OK if secret Construct name matches secret name
            return new SecMan.Secret(parent, smSecretDef.SecretName, smSecretDef);
        }

        public static Secret CreateSecret(this SecMan.SecretProps smSecretDef, Construct parent)
        {
            SecMan.Secret smSecret = smSecretDef.CreateSecretConstruct(parent);
            return Secret.FromSecretsManager(smSecret);
        }
    }
}
