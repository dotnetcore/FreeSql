# CodeFirst

| 数据库 | 支持的类型类型 |
| - | - |
| MySql | bool, sbyte, short, int, long, byte, ushort, uint, ulong, double, float, decimal, Guid, TimeSpan, DateTime<br>bool?, sbyte?, short?, int?, long?, byte?, ushort?, uint?, ulong?, double?, float?, decimal?, Guid?, TimeSpan?, DateTime?<br>byte[], string, Enum & FlagsEnum<br>MygisPoint, MygisLineString, MygisPolygon, MygisMultiPoint, MygisMultiLineString, MygisMultiPolygon |
| SqlServer | bool, sbyte, short, int, long, byte, ushort, uint, ulong, double, float, decimal, Guid, TimeSpan, DateTime, DateTimeOffset<br>bool?, sbyte?, short?, int?, long?, byte?, ushort?, uint?, ulong?, double?, float?, decimal?, Guid?, TimeSpan?, DateTime?, DateTimeOffset?<br>byte[], string, Enum & FlagsEnum |


```csharp
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
    .Build();
```

### 自动同步实体结构【开发环境必备】

自动同步实体结构到数据库，程序运行中检查实体表是否存在，然后创建或修改

```csharp
fsql.CodeFirst.IsAutoSyncDataStructure = true;
```

> 此功能默认为开启状态，发布正式环境后，请修改此设置

> 虽然【自动同步实体结构】功能开发非常好用，但是有个坏处，就是数据库后面会很乱，没用的字段一大堆

### 手工同步实体结构

| 实体＆表对比 | 添加 | 改名 | 删除 |
| - | - | - | - |
|  | √ | √ | X |

| 实体属性＆字段对比 | 添加 | 修改可空 | 修改自增 | 修改类型 | 改名 | 删除 |
| - | - | - | - | - | - | - |
|  | √ | √ | √ | √ | √ | X |

> 为了保证安全，不提供删除字段


1、提供方法对比实体，与数据库中的变化部分

```csharp
var t1 = mysql.CodeFirst.GetComparisonDDLStatements<Topic>();

class Topic {
	[Column(IsIdentity = true, IsPrimary = true)]
	public int Id { get; set; }
	public int Clicks { get; set; }
	public TestTypeInfo Type { get; set; }
	public string Title { get; set; }
	public DateTime CreateTime { get; set; }
	public ushort fusho { get; set; }
}
```
```sql
CREATE TABLE IF NOT EXISTS `cccddd`.`Topic` ( 
  `Id` INT(11) NOT NULL AUTO_INCREMENT, 
  `Clicks` INT(11) NOT NULL, 
  `Title` VARCHAR(255), 
  `CreateTime` DATETIME NOT NULL, 
  `fusho` SMALLINT(5) UNSIGNED NOT NULL, 
  PRIMARY KEY (`Id`)
) Engine=InnoDB CHARACTER SET utf8;
```

2、指定实体的表名

指定 Name 后，实体类名变化不影响数据库对应的表
```csharp
[Table(Name = "tb_topic111")]
class Topic {
  //...
}
```

3、无指定实体的表名，修改实体类名

指定数据库旧的表名，修改实体命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库表；否则将视为【创建新表】

```csharp
[Table(OldName = "Topic")]
class Topic2 {
  //...
}
```
```sql
ALTER TABLE `cccddd`.`Topic` RENAME TO `cccddd`.`Topic2`;
```

4、修改属性的类型

把 Id 类型改为 uint 后
```sql
ALTER TABLE `cccddd`.`Topic2` MODIFY `Id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT;
```
```csharp
[Column(DbType = "varchar(128)")]
public string Title { get; set; }
```
```sql
ALTER TABLE `cccddd`.`Topic2` MODIFY `Title2` VARCHAR(128);
```

5、指定属性的字段名

这样指定后，修改实体的属性名不影响数据库对应的列
```csharp
[Column(Name = "titl2")]
public string Title { get; set; }
```

6、无指定属性的字段名，修改属性名

指定数据库旧的列名，修改实体属性命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库字段；否则将视为【新增字段】

```csharp
[Column(OldName = "Title2")]
public string Title { get; set; }
```
```sql
ALTER TABLE `cccddd`.`Topic2` CHANGE COLUMN `Title2` `Title` VARCHAR(255);
```

7、提供方法同步结构

```csharp
var t2 = fsql.CodeFirst.SyncStructure<Topic>();
//同步实体类型到数据库
```
