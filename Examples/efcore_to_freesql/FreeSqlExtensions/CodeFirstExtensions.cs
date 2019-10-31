using FreeSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class CodeFirstExtensions
{

    public static void ConfigEntity(this ICodeFirst codeFirst, IModel efmodel)
    {

        foreach (var type in efmodel.GetEntityTypes())
        {

            codeFirst.ConfigEntity(type.ClrType, a =>
            {

                //表名
                var relationalTableName = type.FindAnnotation("Relational:TableName");
                if (relationalTableName != null)
                    a.Name(relationalTableName.Value?.ToString() ?? type.ClrType.Name);

                foreach (var prop in type.GetProperties())
                {

                    var freeProp = a.Property(prop.Name);

                    //列名
                    var relationalColumnName = prop.FindAnnotation("Relational:ColumnName");
                    if (relationalColumnName != null)
                        freeProp.Name(relationalColumnName.Value?.ToString() ?? prop.Name);

                    //主键
                    freeProp.IsPrimary(prop.IsPrimaryKey());

                    //自增
                    freeProp.IsIdentity(
                        prop.ValueGenerated == ValueGenerated.Never ||
                        prop.ValueGenerated == ValueGenerated.OnAdd ||
                        prop.GetAnnotations().Where(z =>
                            z.Name == "SqlServer:ValueGenerationStrategy" && z.Value.ToString().Contains("IdentityColumn") //sqlserver 自增
                            || z.Value.ToString().Contains("IdentityColumn") //其他数据库实现未经测试
                        ).Any()
                    );

                    //可空
                    freeProp.IsNullable(prop.AfterSaveBehavior != PropertySaveBehavior.Throw);

                    //类型
                    var relationalColumnType = prop.FindAnnotation("Relational:ColumnType");
                    if (relationalColumnType != null)
                    {

                        var dbType = relationalColumnType.ToString();
                        if (!string.IsNullOrEmpty(dbType))
                        {

                            var maxLength = prop.FindAnnotation("MaxLength");
                            if (maxLength != null)
                                dbType += $"({maxLength})";

                            freeProp.DbType(dbType);
                        }
                    }
                }
            });
        }
    }
}