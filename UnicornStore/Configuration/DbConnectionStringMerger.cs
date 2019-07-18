using System.Data.Common;

namespace UnicornStore.Configuration
{
    internal static class DbConnectionStringMerger
    {

        internal static DbConnectionStringBuilder MergeDbConnectionStringBuilders(this DbConnectionStringBuilder overrideConnectionInfo, string defaultConnectionString)
        {
            var defaultConnectionStringBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = defaultConnectionString
            };

            overrideConnectionInfo.MergeDbConnectionStringBuilders(defaultConnectionStringBuilder);
            return defaultConnectionStringBuilder;
        }

        internal static void MergeDbConnectionStringBuilders(this DbConnectionStringBuilder overrideConnectionInfo, DbConnectionStringBuilder defaultConnectionStringBuilder)
        {
            foreach (string csParam in overrideConnectionInfo.Keys)
            {
                object csPartValue = overrideConnectionInfo[csParam];

                if (csPartValue == null)
                    continue;

                string csPartValueString = csPartValue as string;

                if (csPartValueString == null || csPartValueString != string.Empty)
                    defaultConnectionStringBuilder[csParam] = csPartValue;
            }
        }
    }
}
