using FreeSql.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1208
    {
        [Fact]
        public void GlobalFilter01()
        {
            using (var fsql = new FreeSqlBuilder()
              .UseConnectionString(DataType.Sqlite, "data source=:memory:")
              .UseAutoSyncStructure(true)
              .Build())
            {
                fsql.GlobalFilter.Apply<UserEntity>("TenantQuery", a => a.TenantId == 100);

                var userRepository = fsql.GetRepository<UserEntity>();

                var deviceId = 100;
                var sql = userRepository.Select
                            .Where(i => i.DoorDevices.AsSelect().Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"" 
FROM ""issues1208_User"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDeviceUser"" Mx_Mi 
    WHERE (Mx_Mi.""UserId"" = a.""Id"") AND (exists(SELECT 1 
        FROM ""issues1208_DoorDevice"" x 
        WHERE (x.""Id"" = 100) AND (x.""Id"" = Mx_Mi.""DoorDeviceId"") AND (x.""TenantId"" = 100) 
        limit 0,1)) AND (Mx_Mi.""TenantId"" = 100) 
    limit 0,1)) AND (a.""TenantId"" = 100) 
ORDER BY a.""Id"" DESC", sql);

                sql = userRepository.Select
                            .Where(i => i.DoorDevices.AsSelect().DisableGlobalFilter("TenantQuery").Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"" 
FROM ""issues1208_User"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDeviceUser"" Mx_Mi 
    WHERE (Mx_Mi.""UserId"" = a.""Id"") AND (exists(SELECT 1 
        FROM ""issues1208_DoorDevice"" x 
        WHERE (x.""Id"" = 100) AND (x.""Id"" = Mx_Mi.""DoorDeviceId"") 
        limit 0,1)) 
    limit 0,1)) AND (a.""TenantId"" = 100) 
ORDER BY a.""Id"" DESC", sql);

                sql = userRepository.Select.DisableGlobalFilter("TenantQuery")
                            .Where(i => i.DoorDevices.AsSelect().Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"" 
FROM ""issues1208_User"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDeviceUser"" Mx_Mi 
    WHERE (Mx_Mi.""UserId"" = a.""Id"") AND (exists(SELECT 1 
        FROM ""issues1208_DoorDevice"" x 
        WHERE (x.""Id"" = 100) AND (x.""Id"" = Mx_Mi.""DoorDeviceId"") 
        limit 0,1)) 
    limit 0,1)) 
ORDER BY a.""Id"" DESC", sql);

                using (userRepository.DataFilter.Disable("TenantQuery"))
                {
                    sql = userRepository.Select
                            .Where(i => i.DoorDevices.AsSelect().Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                    Assert.Equal(@"SELECT a.""Id"", a.""TenantId"" 
FROM ""issues1208_User"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDeviceUser"" Mx_Mi 
    WHERE (Mx_Mi.""UserId"" = a.""Id"") AND (exists(SELECT 1 
        FROM ""issues1208_DoorDevice"" x 
        WHERE (x.""Id"" = 100) AND (x.""Id"" = Mx_Mi.""DoorDeviceId"") 
        limit 0,1)) 
    limit 0,1)) 
ORDER BY a.""Id"" DESC", sql);
                }

                sql = userRepository.Select
                            .Where(i => i.DoorDevices.Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"" 
FROM ""issues1208_User"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDevice"" x 
    WHERE (exists(SELECT 1 
        FROM ""issues1208_DoorDeviceUser"" Mx_Ma 
        WHERE (Mx_Ma.""DoorDeviceId"" = x.""Id"") AND (Mx_Ma.""UserId"" = a.""Id"") AND (Mx_Ma.""TenantId"" = 100) 
        limit 0,1)) AND (x.""Id"" = 100) AND (x.""TenantId"" = 100) 
    limit 0,1)) AND (a.""TenantId"" = 100) 
ORDER BY a.""Id"" DESC", sql);

                sql = userRepository.Select.DisableGlobalFilter("TenantQuery")
                            .Where(i => i.DoorDevices.Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"" 
FROM ""issues1208_User"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDevice"" x 
    WHERE (exists(SELECT 1 
        FROM ""issues1208_DoorDeviceUser"" Mx_Ma 
        WHERE (Mx_Ma.""DoorDeviceId"" = x.""Id"") AND (Mx_Ma.""UserId"" = a.""Id"") 
        limit 0,1)) AND (x.""Id"" = 100) 
    limit 0,1)) 
ORDER BY a.""Id"" DESC", sql);

                using (userRepository.DataFilter.Disable("TenantQuery"))
                {
                    sql = userRepository.Select
                            .Where(i => i.DoorDevices.Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                    Assert.Equal(@"SELECT a.""Id"", a.""TenantId"" 
FROM ""issues1208_User"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDevice"" x 
    WHERE (exists(SELECT 1 
        FROM ""issues1208_DoorDeviceUser"" Mx_Ma 
        WHERE (Mx_Ma.""DoorDeviceId"" = x.""Id"") AND (Mx_Ma.""UserId"" = a.""Id"") 
        limit 0,1)) AND (x.""Id"" = 100) 
    limit 0,1)) 
ORDER BY a.""Id"" DESC", sql);
                }
            }
        }

        [Table(Name = "issues1208_User")]
        class UserEntity
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            [Navigate(ManyToMany = typeof(DoorDeviceUserEntity))]
            public ICollection<DoorDeviceEntity> DoorDevices { get; set; }

            public int TenantId { get; set; }
        }
        [Table(Name = "issues1208_DoorDeviceUser")]
        class DoorDeviceUserEntity
        {
            public int UserId { get; set; }

            [Navigate(nameof(UserId))]
            public UserEntity User { get; set; }

            public int DoorDeviceId { get; set; }

            [Navigate(nameof(DoorDeviceId))]
            public DoorDeviceEntity DoorDevice { get; set; }
            public int TenantId { get; set; }
        }
        [Table(Name = "issues1208_DoorDevice")]
        class DoorDeviceEntity
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            /// <summary>
            /// 用户
            /// </summary>
            [Navigate(ManyToMany = typeof(DoorDeviceUserEntity))]
            public ICollection<UserEntity> Users { get; set; }
            public int TenantId { get; set; }
        }

        [Fact]
        public void GlobalFilter02()
        {
            using (var fsql = new FreeSqlBuilder()
              .UseConnectionString(DataType.Sqlite, "data source=:memory:")
              .UseAutoSyncStructure(true)
              .Build())
            {
                fsql.GlobalFilter.Apply<UserEntity02>("TenantQuery", a => a.TenantId == 100);
                fsql.GlobalFilter.Apply<UserEntity02>("SoftDeleted", a => a.SoftDeleted == false);

                var userRepository = fsql.GetRepository<UserEntity02>();

                var deviceId = 100;
                var sql = userRepository.Select
                            .Where(i => i.DoorDevices.AsSelect().Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"", a.""SoftDeleted"" 
FROM ""issues1208_User02"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDeviceUser02"" Mx_Mi 
    WHERE (Mx_Mi.""UserId"" = a.""Id"") AND (exists(SELECT 1 
        FROM ""issues1208_DoorDevice02"" x 
        WHERE (x.""Id"" = 100) AND (x.""Id"" = Mx_Mi.""DoorDeviceId"") AND (x.""TenantId"" = 100) AND (x.""SoftDeleted"" = 0) 
        limit 0,1)) AND (Mx_Mi.""TenantId"" = 100) AND (Mx_Mi.""SoftDeleted"" = 0) 
    limit 0,1)) AND (a.""TenantId"" = 100) AND (a.""SoftDeleted"" = 0) 
ORDER BY a.""Id"" DESC", sql);

                sql = userRepository.Select
                            .Where(i => i.DoorDevices.AsSelect().DisableGlobalFilter("TenantQuery").Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"", a.""SoftDeleted"" 
FROM ""issues1208_User02"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDeviceUser02"" Mx_Mi 
    WHERE (Mx_Mi.""UserId"" = a.""Id"") AND (exists(SELECT 1 
        FROM ""issues1208_DoorDevice02"" x 
        WHERE (x.""Id"" = 100) AND (x.""Id"" = Mx_Mi.""DoorDeviceId"") AND (x.""SoftDeleted"" = 0) 
        limit 0,1)) AND (Mx_Mi.""SoftDeleted"" = 0) 
    limit 0,1)) AND (a.""TenantId"" = 100) AND (a.""SoftDeleted"" = 0) 
ORDER BY a.""Id"" DESC", sql);

                sql = userRepository.Select.DisableGlobalFilter("TenantQuery")
                            .Where(i => i.DoorDevices.AsSelect().Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"", a.""SoftDeleted"" 
FROM ""issues1208_User02"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDeviceUser02"" Mx_Mi 
    WHERE (Mx_Mi.""UserId"" = a.""Id"") AND (exists(SELECT 1 
        FROM ""issues1208_DoorDevice02"" x 
        WHERE (x.""Id"" = 100) AND (x.""Id"" = Mx_Mi.""DoorDeviceId"") AND (x.""SoftDeleted"" = 0) 
        limit 0,1)) AND (Mx_Mi.""SoftDeleted"" = 0) 
    limit 0,1)) AND (a.""SoftDeleted"" = 0) 
ORDER BY a.""Id"" DESC", sql);

                using (userRepository.DataFilter.Disable("TenantQuery"))
                {
                    sql = userRepository.Select
                            .Where(i => i.DoorDevices.AsSelect().Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                    Assert.Equal(@"SELECT a.""Id"", a.""TenantId"", a.""SoftDeleted"" 
FROM ""issues1208_User02"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDeviceUser02"" Mx_Mi 
    WHERE (Mx_Mi.""UserId"" = a.""Id"") AND (exists(SELECT 1 
        FROM ""issues1208_DoorDevice02"" x 
        WHERE (x.""Id"" = 100) AND (x.""Id"" = Mx_Mi.""DoorDeviceId"") AND (x.""SoftDeleted"" = 0) 
        limit 0,1)) AND (Mx_Mi.""SoftDeleted"" = 0) 
    limit 0,1)) AND (a.""SoftDeleted"" = 0) 
ORDER BY a.""Id"" DESC", sql);
                }

                sql = userRepository.Select
                            .Where(i => i.DoorDevices.Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"", a.""SoftDeleted"" 
FROM ""issues1208_User02"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDevice02"" x 
    WHERE (exists(SELECT 1 
        FROM ""issues1208_DoorDeviceUser02"" Mx_Ma 
        WHERE (Mx_Ma.""DoorDeviceId"" = x.""Id"") AND (Mx_Ma.""UserId"" = a.""Id"") AND (Mx_Ma.""TenantId"" = 100) AND (Mx_Ma.""SoftDeleted"" = 0) 
        limit 0,1)) AND (x.""Id"" = 100) AND (x.""TenantId"" = 100) AND (x.""SoftDeleted"" = 0) 
    limit 0,1)) AND (a.""TenantId"" = 100) AND (a.""SoftDeleted"" = 0) 
ORDER BY a.""Id"" DESC", sql);

                sql = userRepository.Select.DisableGlobalFilter("TenantQuery")
                            .Where(i => i.DoorDevices.Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                Assert.Equal(@"SELECT a.""Id"", a.""TenantId"", a.""SoftDeleted"" 
FROM ""issues1208_User02"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDevice02"" x 
    WHERE (exists(SELECT 1 
        FROM ""issues1208_DoorDeviceUser02"" Mx_Ma 
        WHERE (Mx_Ma.""DoorDeviceId"" = x.""Id"") AND (Mx_Ma.""UserId"" = a.""Id"") AND (Mx_Ma.""SoftDeleted"" = 0) 
        limit 0,1)) AND (x.""Id"" = 100) AND (x.""SoftDeleted"" = 0) 
    limit 0,1)) AND (a.""SoftDeleted"" = 0) 
ORDER BY a.""Id"" DESC", sql);

                using (userRepository.DataFilter.Disable("TenantQuery"))
                {
                    sql = userRepository.Select
                            .Where(i => i.DoorDevices.Any(x => x.Id == deviceId))
                            .OrderByDescending(true, a => a.Id)
                            .ToSql();
                    Assert.Equal(@"SELECT a.""Id"", a.""TenantId"", a.""SoftDeleted"" 
FROM ""issues1208_User02"" a 
WHERE (exists(SELECT 1 
    FROM ""issues1208_DoorDevice02"" x 
    WHERE (exists(SELECT 1 
        FROM ""issues1208_DoorDeviceUser02"" Mx_Ma 
        WHERE (Mx_Ma.""DoorDeviceId"" = x.""Id"") AND (Mx_Ma.""UserId"" = a.""Id"") AND (Mx_Ma.""SoftDeleted"" = 0) 
        limit 0,1)) AND (x.""Id"" = 100) AND (x.""SoftDeleted"" = 0) 
    limit 0,1)) AND (a.""SoftDeleted"" = 0) 
ORDER BY a.""Id"" DESC", sql);
                }
            }
        }

        [Table(Name = "issues1208_User02")]
        class UserEntity02
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            [Navigate(ManyToMany = typeof(DoorDeviceUserEntity02))]
            public ICollection<DoorDeviceEntity02> DoorDevices { get; set; }

            public int TenantId { get; set; }
            public bool SoftDeleted { get; set; }
        }
        [Table(Name = "issues1208_DoorDeviceUser02")]
        class DoorDeviceUserEntity02
        {
            public int UserId { get; set; }

            [Navigate(nameof(UserId))]
            public UserEntity02 User { get; set; }

            public int DoorDeviceId { get; set; }

            [Navigate(nameof(DoorDeviceId))]
            public DoorDeviceEntity02 DoorDevice { get; set; }
            public int TenantId { get; set; }
            public bool SoftDeleted { get; set; }
        }
        [Table(Name = "issues1208_DoorDevice02")]
        class DoorDeviceEntity02
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            /// <summary>
            /// 用户
            /// </summary>
            [Navigate(ManyToMany = typeof(DoorDeviceUserEntity02))]
            public ICollection<UserEntity02> Users { get; set; }
            public int TenantId { get; set; }
            public bool SoftDeleted { get; set; }
        }
    }
}
