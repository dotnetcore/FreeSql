# CodeFirst

## 类型映射

| csharp | MySql | SqlServer | PostgreSQL | oracle |
| - | - | - | - | - |
| bool \| bool? | bit(1) | bit | bool | number(1) |
| sbyte \| sbyte? | tinyint(3) | smallint | int2 | number(4) |
| short \| short? | smallint(6) | smallint | int2 | number(6) |
| int \| int? | int(11) | int | int4 | number(11) |
| long \| long? | bigint(20) | bigint | int8 | number(21) |
| byte \| byte? | tinyint(3) unsigned | tinyint | int2 | number(3) |
| ushort \| ushort? | smallint(5) unsigned | int | int4 | number(5) |
| uint \| uint? | int(10) unsigned | bigint | int8 | number(10) |
| ulong \| ulong? | bigint(20) unsigned | decimal(20,0) | numeric(20,0) | number(20) |
| double \| double? | double | float | float8 | float(126) |
| float \| float? | float | real | float4 | float(63) |
| decimal \| decimal? | decimal(10,2) | decimal(10,2) | numeric(10,2) | number(10,2) |
| Guid \| Guid? | char(36) | uniqueidentifier | uuid | char(36 CHAR) |
| TimeSpan \| TimeSpan? | time | time | time | interval day(2) to second(6) |
| DateTime \| DateTime? | datetime | datetime | timestamp | timestamp(6) |
| DateTimeOffset \| DateTimeOffset? | - | - | datetimeoffset | timestamp(6) with local time zone |
| Enum \| Enum? | enum | int | int4 | number(16) |
| FlagsEnum \| FlagsEnum? | set | bigint | int8 | number(32) |
| byte[] | varbinary(255) | varbinary(255) | bytea | blob |
| string | varchar(255) | nvarchar(255) | varchar(255) | nvarchar2(255) |
| MygisPoint | point | - | - | - |
| MygisLineString | linestring | - | - | - |
| MygisPolygon | polygon | - | - | - |
| MygisMultiPoint | multipoint | - | - | - |
| MygisMultiLineString | multilinestring | - | - | - |
| MygisMultiPolygon | multipolygon | - | - | - |
| BitArray | - | - | varbit(64) | - |
| NpgsqlPoint \| NpgsqlPoint? | - | - | point | - |
| NpgsqlLine \| NpgsqlLine? | - | - | line | - |
| NpgsqlLSeg \| NpgsqlLSeg? | - | - | lseg | - |
| NpgsqlBox \| NpgsqlBox? | - | - | box | - |
| NpgsqlPath \| NpgsqlPath? | - | - | path | - |
| NpgsqlPolygon \| NpgsqlPolygon? | - | - | polygon | - |
| NpgsqlCircle \| NpgsqlCircle? | - | - | circle | - |
| (IPAddress Address, int Subnet) \| (IPAddress Address, int Subnet)? | - | - | cidr | - |
| IPAddress | - | - | inet | - |
| PhysicalAddress | - | - | macaddr | - |
| NpgsqlRange\<int\> \| NpgsqlRange\<int\>? | - | - | int4range | - |
| NpgsqlRange\<long\> \| NpgsqlRange\<long\>? | - | - | int8range | - |
| NpgsqlRange\<decimal\> \| NpgsqlRange\<decimal\>? | - | - | numrange | - |
| NpgsqlRange\<DateTime\> \| NpgsqlRange\<DateTime\>? | - | - | tsrange | - |
| PostgisPoint | - | - | geometry | - |
| PostgisLineString | - | - | geometry | - |
| PostgisPolygon | - | - | geometry | - |
| PostgisMultiPoint | - | - | geometry | - |
| PostgisMultiLineString | - | - | geometry | - |
| PostgisMultiPolygon | - | - | geometry | - |
| PostgisGeometry | - | - | geometry | - |
| PostgisGeometryCollection | - | - | geometry | - |
| Dictionary<string, string> | - | - | hstore | - |
| JToken | - | - | jsonb | - |
| JObject | - | - | jsonb | - |
| JArray | - | - | jsonb | - |
| 数组 | - | - | 以上所有类型都支持 | - |

> 以上类型和长度是默认值，可手工设置，如 string 属性可指定 [Column(DbType = "varchar(max)")]

```csharp
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
    .UseAutoSyncStructure(true)

    .UseMonitorCommand(
        cmd => {
            Console.WriteLine(cmd.CommandText);
        }, //监听SQL命令对象，在执行前
        (cmd, traceLog) => {
            Console.WriteLine(traceLog);
        }) //监听SQL命令对象，在执行后
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
