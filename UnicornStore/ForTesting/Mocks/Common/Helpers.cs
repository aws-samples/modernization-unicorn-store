using System;

namespace UnicornStore.Mocks.Common
{
    internal class Helpers
    {
        internal static void ThrowIfConditionFailed(Func<bool> condition, string errorMessage)
        {
            if (!condition())
            {
                throw new Exception(errorMessage);
            }
        }
    }
} 
