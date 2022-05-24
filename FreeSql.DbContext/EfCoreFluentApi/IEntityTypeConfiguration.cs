namespace FreeSql.Extensions.EfCoreFluentApi
{
    public interface IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        void Configure(EfCoreTableFluent<TEntity> model);
    }
}