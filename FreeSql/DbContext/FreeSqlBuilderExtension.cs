namespace FreeSql.DbContext
{
    internal static class FreeSqlBuilderExtension
    {
        public static FreeSqlBuilder UseSlave( this FreeSqlBuilder builder, FreeSqlBuilderConfiguration contextConfiguration)
        {
            foreach(var slave in contextConfiguration.SlaveList)
            {
                builder.UseSlave(slave);
            }

            return builder;
        }
    }
}
