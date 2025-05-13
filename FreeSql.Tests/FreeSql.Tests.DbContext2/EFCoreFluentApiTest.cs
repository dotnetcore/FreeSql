using FreeSql;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Microsoft.EntityFrameworkCore.Tests.FreeSqlFluentApi.EFCoreFluentApiTest;

namespace Microsoft.EntityFrameworkCore.Tests.FreeSqlFluentApi
{
    public class EFCoreFluentApiTest
    {
        [Fact]
        public void EFCoreToFreeSql()
        {
            using (var fsql = g.CreateMemory())
            {
                fsql.CodeFirst.ApplyConfigurationFromEFCore(typeof(BloggingContext));
                fsql.Insert(new Blog()).ExecuteAffrows();
                fsql.Select<Blog>().ToList();
            }
        }

        #region 测试 DbContext
        public class BloggingContext : Microsoft.EntityFrameworkCore.DbContext
        {
            public DbSet<Blog> Blogs => Set<Blog>();
            public DbSet<Post> Posts => Set<Post>();
            public DbSet<Tag> Tags => Set<Tag>();
            public DbSet<Comment> Comments => Set<Comment>();
            public DbSet<User> Users => Set<User>();
            //public DbSet<DetailedPost> DetailedPosts => Set<DetailedPost>(); // TPH 需要 DbSet
            public DbSet<BlogHeader> BlogHeaders => Set<BlogHeader>(); // 无键实体类型 DbSet

            public BloggingContext() : base() { }

            // 配置数据库连接 (如果不用 Startup.cs 或 DI 注入)
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseSqlite("data source=:memory:");
                    // 启用敏感数据日志记录（仅用于开发）
                    optionsBuilder.EnableSensitiveDataLogging();
                }
            }

            // Fluent API 配置的核心方法
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // --- 全局配置 (Model-Wide Configuration) ---

                // 设置默认 Schema (如果数据库支持)
                modelBuilder.HasDefaultSchema("blogging");

                // 定义数据库序列 (Sequence)
                modelBuilder.HasSequence<int>("BlogIdSequence", schema: "shared")
                    .StartsAt(1000)
                    .IncrementsBy(5);

                // --- 实体级别配置 (Entity Configuration) ---

                // 配置 Blog 实体
                modelBuilder.Entity<Blog>(entity =>
                {
                    // 映射到特定的表名和 Schema (覆盖默认 Schema)
                    entity.ToTable("BlogInfo", "dbo");

                    // 配置主键 (Primary Key) - EF Core 通常能自动发现名为 Id 或 <TypeName>Id 的属性
                    entity.HasKey(b => b.BlogId);
                    // .HasName("PK_BlogInfo"); // 自定义主键约束名

                    // 使用序列生成主键值 (需要数据库支持，如 SQL Server, PostgreSQL)
                    // entity.Property(b => b.BlogId)
                    //       .HasDefaultValueSql("NEXT VALUE FOR shared.BlogIdSequence"); // 使用上面定义的序列

                    // 配置属性 (Property Configuration)
                    entity.Property(b => b.Url)
                          .IsRequired() // 设置为必需 (NOT NULL)
                          .HasMaxLength(250) // 设置最大长度
                          .HasColumnName("BlogUrl"); // 映射到不同的列名

                    entity.Property(b => b.Name)
                          .HasColumnType("nvarchar(150)"); // 指定数据库列类型

                    entity.Property(b => b.Rating)
                          .HasPrecision(18, 2); // 设置 decimal 的精度和小数位数

                    // 配置并发标记 (Concurrency Token) - RowVersion/Timestamp
                    entity.Property(b => b.RowVersion)
                          .IsRowVersion(); // 效果同 [Timestamp]

                    // 忽略不需要映射到数据库的属性
                    entity.Ignore(b => b.LoadedFromDatabase);

                    // 配置索引 (Index)
                    entity.HasIndex(b => b.Url)
                          .IsUnique() // 创建唯一索引
                          .HasDatabaseName("IX_Blog_Url"); // 自定义索引名称

                    // 配置全局查询筛选器 (Global Query Filter) - 例如用于软删除
                    entity.HasQueryFilter(b => !b.IsDeleted);

                    // 配置数据库生成的默认值
                    entity.Property(b => b.CreationTimestamp)
                          .HasDefaultValueSql("current_timestamp"); // 使用数据库函数设置默认值
                                                               // .HasDefaultValue(new DateTime(2000, 1, 1)); // 使用固定值设置默认值

                    // 配置检查约束 (Check Constraint) - 需要数据库支持
                    entity.HasCheckConstraint("CK_Blog_Rating", "[Rating] >= 0 AND [Rating] <= 5", c => c.HasName("CK_BlogRatingRange"));

                    // 配置值转换 (Value Conversion) - 将 Uri 对象与数据库中的 string 相互转换
                    entity.Property(b => b.SiteUri)
                          .HasConversion(
                              v => v == null ? null : v.ToString(), // 模型 -> 数据库
                              v => v == null ? null : new Uri(v)    // 数据库 -> 模型
                          )
                          .HasMaxLength(500); // 转换后的 string 类型需要指定长度

                    // 配置自有类型 (Owned Type) - AuditInfo
                    // 默认情况下，自有类型的属性会映射到拥有者实体的主表中，列名为 "OwnedTypeName_PropertyName"
                    entity.OwnsOne(b => b.AuditInfo, ownedNavigationBuilder =>
                    {
                        ownedNavigationBuilder.Property(a => a.CreatedAt).HasColumnName("DateCreated"); // 自定义列名
                        ownedNavigationBuilder.Property(a => a.UpdatedAt).HasColumnName("DateModified");
                        // ownedNavigationBuilder.ToTable("BlogAudit"); // 也可以将自有类型映射到单独的表
                    });

                    // --- 关系配置 (Relationship Configuration) ---

                    // 配置与 Post 的一对多关系 (Blog 有多个 Post)
                    // EF Core 通常能自动发现导航属性并配置关系，但这里显式配置以作演示
                    entity.HasMany(b => b.Posts)          // Blog 有多个 Posts
                          .WithOne(p => p.Blog)           // 每个 Post 属于一个 Blog
                          .HasForeignKey(p => p.BlogId)   // Post 中的外键是 BlogId
                          .IsRequired()                   // 外键是必需的 (关联的 Blog 不能为 null)
                          .OnDelete(DeleteBehavior.Cascade); // 当 Blog 删除时，级联删除其 Posts

                    // 配置与 User 的一对多关系 (一个 User 拥有多个 Blog)
                    // 这里外键在 Blog 实体中 (OwnerId)
                    entity.HasOne(b => b.Owner)           // 每个 Blog 有一个 Owner (User)
                          .WithMany(u => u.OwnedBlogs)    // 一个 User 有多个 OwnedBlogs
                          .HasForeignKey(b => b.OwnerId)  // Blog 表中的外键是 OwnerId
                          .IsRequired()
                          .OnDelete(DeleteBehavior.Restrict); // 当 User 删除时，如果其还拥有 Blog，则阻止删除 User
                });

                // 配置 Post 实体
                modelBuilder.Entity<Post>(entity =>
                {
                    entity.ToTable("Posts"); // 使用默认 Schema "blogging"

                    entity.HasKey(p => p.PostId);

                    entity.Property(p => p.Title)
                          .IsRequired()
                          .HasMaxLength(200);

                    entity.Property(p => p.Content)
                          .HasColumnType("ntext"); // 适用于旧 SQL Server，新版建议 nvarchar(max)

                    // 配置枚举到字符串的转换 (Value Conversion)
                    entity.Property(p => p.Status)
                          .HasConversion<string>() // 将枚举存储为字符串
                          .HasMaxLength(20);       // 设定存储字符串的长度
                                                   // 或者转换为 int: .HasConversion<int>()

                    // 配置与 Blog 的多对一关系 (显式配置，虽然通常可省略)
                    entity.HasOne(p => p.Blog)
                          .WithMany(b => b.Posts)
                          .HasForeignKey(p => p.BlogId); // OnDelete 行为已在 Blog 端配置

                    // 配置与 User (Author) 的多对一关系 (外键可空)
                    entity.HasOne(p => p.Author)
                          .WithMany(u => u.AuthoredPosts)
                          .HasForeignKey(p => p.AuthorId)
                          .IsRequired(false) // 外键 AuthorId 可以为 NULL
                          .OnDelete(DeleteBehavior.ClientSetNull); // 当 Author 删除时，将 Post 的 AuthorId 设为 NULL (客户端行为)
                                                                   // .OnDelete(DeleteBehavior.SetNull); // 数据库行为 (如果数据库支持)

                    // 配置与 Comment 的一对多关系
                    entity.HasMany(p => p.Comments)
                          .WithOne(c => c.Post)
                          .HasForeignKey(c => c.PostId)
                          .OnDelete(DeleteBehavior.Cascade);

                    // 配置与 Tag 的多对多关系 (Many-to-Many)
                    entity.HasMany(p => p.Tags)
                          .WithMany(t => t.Posts)
                          // EF Core 6+ 可以自动创建连接表。如果需要自定义连接表：
                          .UsingEntity<Dictionary<string, object>>( // 使用字典类型作为连接实体
                              "PostTagLink", // 连接表名称
                              j => j.HasOne<Tag>().WithMany().HasForeignKey("TagForeignKey").HasPrincipalKey(t => t.TagId), // 配置 Tag 端
                              j => j.HasOne<Post>().WithMany().HasForeignKey("PostForeignKey").HasPrincipalKey(p => p.PostId), // 配置 Post 端
                              j =>
                              {
                                  j.Property<DateTime>("LinkedDate").HasDefaultValueSql("GETUTCDATE()"); // 连接表中的附加列
                                  j.HasKey("PostForeignKey", "TagForeignKey"); // 定义连接表的复合主键
                                  j.ToTable("PostTagMap", "linking"); // 自定义连接表的名称和 Schema
                              });
                    // 也可以使用显式连接实体类 `PostTag` 并配置两个一对多关系来替代 `UsingEntity`

                    // 配置复合索引 (Composite Index)
                    entity.HasIndex(p => new { p.BlogId, p.Title })
                          .HasDatabaseName("IX_Post_BlogId_Title");

                    // --- 继承映射配置 (Inheritance Mapping) ---
                    // TPH (Table Per Hierarchy) 是默认策略
                    // 需要配置鉴别器 (Discriminator) 列
                    //entity.HasDiscriminator<string>("PostType") // 鉴别器列名和类型
                    //      .HasValue<Post>("StandardPost")       // 基类的鉴别器值
                    //      .HasValue<DetailedPost>("DetailedBlogPost"); // 派生类的鉴别器值

                    // 如果是 TPT (Table Per Type)
                    // modelBuilder.Entity<Post>().ToTable("Posts");
                    // modelBuilder.Entity<DetailedPost>().ToTable("DetailedPosts"); // 派生类映射到单独的表

                    // 如果是 TPC (Table Per Concrete Type) - EF Core 7+
                    // modelBuilder.Entity<Post>().UseTpcMappingStrategy()
                    //      .ToTable("StandardPosts");
                    // modelBuilder.Entity<DetailedPost>().UseTpcMappingStrategy()
                    //      .ToTable("DetailedBlogPosts");
                    // 注意：TPC 不支持数据库生成的主键策略，如 Identity 或 Sequence。
                });

                // 配置 DetailedPost 实体 (派生类)
                //modelBuilder.Entity<DetailedPost>(entity =>
                //{
                //    // TPH 模式下，属性默认映射到基类表中
                //    entity.Property(dp => dp.Metadata).HasMaxLength(500);
                //    entity.Property(dp => dp.Summary).HasColumnType("nvarchar(max)");

                //    // TPT 模式下需要配置与基类的关系 (主键既是主键也是外键)
                //    // entity.HasOne(dp => dp.Blog).WithMany().HasForeignKey(dp => dp.BlogId); // TPT 时可能需要
                //});


                // 配置 Tag 实体
                modelBuilder.Entity<Tag>(entity =>
                {
                    entity.ToTable("Tags", "taxonomy"); // 自定义 Schema

                    // 配置字符串主键
                    entity.HasKey(t => t.TagId);
                    entity.Property(t => t.TagId).HasMaxLength(50); // 字符串主键通常需要指定长度

                    entity.Property(t => t.Name)
                          .IsRequired()
                          .HasMaxLength(100);

                    entity.HasIndex(t => t.Name)
                          .IsUnique(); // 标签名唯一
                });

                // 配置 Comment 实体
                modelBuilder.Entity<Comment>(entity =>
                {
                    // 使用默认表名 "Comments" 和默认 Schema "blogging"
                    entity.HasKey(c => c.CommentId);

                    entity.Property(c => c.Text).IsRequired();

                    entity.Property(c => c.PostedDate)
                          .HasDefaultValueSql("GETUTCDATE()");
                });

                // 配置 User 实体
                modelBuilder.Entity<User>(entity =>
                {
                    entity.ToTable("ApplicationUsers", "identity"); // 自定义表名和 Schema

                    entity.HasKey(u => u.UserId);

                    entity.Property(u => u.Username)
                          .IsRequired()
                          .HasMaxLength(100);

                    entity.HasIndex(u => u.Username)
                          .IsUnique(); // 用户名唯一

                    entity.Property(u => u.Email)
                          .HasMaxLength(254); // Email 标准长度

                    entity.HasIndex(u => u.Email)
                           .IsUnique()
                           .HasFilter("[Email] IS NOT NULL"); // 可空列上的唯一索引 (SQL Server 特定语法)

                    // 配置自有类型 (Owned Type) - Address
                    // 映射到与 User 相同的表中 (默认)
                    entity.OwnsOne(u => u.ShippingAddress, owned =>
                    {
                        // 可以为自有类型的列名添加前缀或自定义
                        owned.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200);
                        owned.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
                        owned.Property(a => a.PostCode).HasColumnName("ShippingPostCode").HasMaxLength(20);
                        owned.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100);

                        // 如果需要将 Address 映射到单独的表 "UserAddresses"
                        // owned.ToTable("UserAddresses", "identity");
                        // owned.WithOwner().HasForeignKey("OwnerUserId"); // 需要显式配置外键
                    });
                });

                // 配置无键实体类型 (Keyless Entity Type / Query Type)
                modelBuilder.Entity<BlogHeader>(entity =>
                {
                    entity.HasNoKey(); // 明确指出没有主键

                    // 映射到数据库视图
                    entity.ToView("vw_BlogHeader", "dbo");

                    // 或者映射到 SQL 查询 (EF Core 5+)
                    // entity.ToSqlQuery("SELECT Name, Url FROM Blogs WHERE IsDeleted = 0");
                });

                // --- Seed Data (填充种子数据) ---
               // modelBuilder.Entity<Tag>().HasData(
               //     new Tag { TagId = "tech", Name = "Technology" },
               //     new Tag { TagId = "efcore", Name = "Entity Framework Core" },
               //     new Tag { TagId = "dotnet", Name = ".NET" }
               // );

               // modelBuilder.Entity<User>().HasData(
               //     new User { UserId = 1, Username = "AdminUser", Email = "admin@example.com" }
               // );

               // modelBuilder.Entity<Blog>().HasData(
               //    new Blog { BlogId = 1, Name = "EF Core Blog", Url = "http://blogs.msdn.com/efcore", Rating = 5, OwnerId = 1, IsDeleted = false, CreationTimestamp = DateTime.UtcNow, AuditInfo = new AuditInfo { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow } }
               //);

                // 注意：HasData 对于有复杂关系或自有类型的种子数据配置可能比较麻烦，有时需要手动插入。
                // 对于 AuditInfo 这样的 Owned Type，EF Core 7+ 支持在 HasData 中直接初始化。

                // 调用基类方法 (如果基类有 OnModelCreating 实现)
                base.OnModelCreating(modelBuilder);
            }
        }

        public class Blog
        {
            public int BlogId { get; set; } // 主键 (Primary Key)
            public string? Name { get; set; } // 可空字符串
            public string Url { get; set; } = null!; // 必需字符串
            public decimal Rating { get; set; } // Decimal 类型
            public bool IsDeleted { get; set; } // 用于软删除的标志
            public DateTime CreationTimestamp { get; set; } // 创建时间戳
            public Uri? SiteUri { get; set; } // 演示值转换

            // 并发标记 (Concurrency Token)
            [Timestamp] // DataAnnotation 方式, Fluent API 也能配
            public byte[]? RowVersion { get; set; }

            // 导航属性 (Navigation Properties)
            public List<Post> Posts { get; set; } = new List<Post>(); // 一对多 (Blog -> Post)

            // 外键属性 (Foreign Key Property) - 可选，EF Core 可自动生成影子属性
            public int OwnerId { get; set; }
            public User Owner { get; set; } = null!; // 一对多 (User -> Blog)

            // 自有类型 (Owned Type) - 嵌入式
            public AuditInfo AuditInfo { get; set; } = new AuditInfo();

            // 计算属性或非映射属性 (Ignored Property)
            public bool LoadedFromDatabase { get; set; }
        }

        public class Post
        {
            public int PostId { get; set; }
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public PostStatus Status { get; set; } // 枚举类型，演示值转换

            // 外键属性
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!; // 多对一 (Post -> Blog)

            public int? AuthorId { get; set; } // 可空外键
            public User? Author { get; set; } // 多对一 (Post -> User)

            // 导航属性
            public List<Comment> Comments { get; set; } = new List<Comment>(); // 一对多 (Post -> Comment)
            public List<Tag> Tags { get; set; } = new List<Tag>(); // 多对多 (Post <-> Tag)

            // 用于 TPH 继承演示
            // public string PostType { get; set; } // Discriminator (鉴别器), EF Core 会自动添加
        }

        // 继承示例 (TPH - Table Per Hierarchy)
        //public class DetailedPost : Post
        //{
        //    public string? Metadata { get; set; }
        //    public string? Summary { get; set; }
        //}

        public class Tag
        {
            public string TagId { get; set; } = null!; // 字符串主键
            public string Name { get; set; } = null!;

            // 导航属性
            public List<Post> Posts { get; set; } = new List<Post>(); // 多对多 (Tag <-> Post)
        }

        public class Comment
        {
            public int CommentId { get; set; }
            public string Text { get; set; } = null!;
            public DateTime PostedDate { get; set; }

            // 外键属性
            public int PostId { get; set; }
            public Post Post { get; set; } = null!; // 多对一 (Comment -> Post)
        }

        public class User
        {
            public int UserId { get; set; }
            public string Username { get; set; } = null!;
            public string? Email { get; set; } // 可空，可能有唯一约束

            // 导航属性
            public List<Blog> OwnedBlogs { get; set; } = new List<Blog>(); // 一对多 (User -> Blog)
            public List<Post> AuthoredPosts { get; set; } = new List<Post>(); // 一对多 (User -> Post)

            // 自有类型 (Owned Type) - 可以映射到同一张表或不同表
            public Address? ShippingAddress { get; set; }
        }

        // 自有类型 (Owned Type) / 值对象 (Value Object)
        [Owned] // 可以用 DataAnnotation 标记，也可以完全用 Fluent API 配置
        public class AuditInfo
        {
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        [Owned]
        public class Address
        {
            public string Street { get; set; } = null!;
            public string City { get; set; } = null!;
            public string PostCode { get; set; } = null!;
            public string Country { get; set; } = null!;
        }

        // 无键实体类型 (Keyless Entity Type) / 查询类型 (Query Type)
        // 通常用于映射数据库视图或存储过程/函数结果
        public class BlogHeader
        {
            public string? Name { get; set; }
            public string Url { get; set; } = null!;
        }

        public enum PostStatus
        {
            Draft,
            Published,
            Archived
        }
        #endregion
    }
}