using System;
using System.Threading.Tasks;

namespace base_entity
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                using (var uow = BaseEntity.Begin())
                {
                    var id = (await new User1().Save()).Id;
                    uow.Commit();
                }

                var ug1 = new UserGroup();
                ug1.GroupName = "分组一";
                await ug1.Insert();

                var ug2 = new UserGroup();
                ug2.GroupName = "分组二";
                await ug2.Insert();

                var u1 = new User1();

                u1.GroupId = ug1.Id;
                await u1.Save();

                await u1.Delete();
                await u1.Restore();

                u1.Nickname = "x1";
                await u1.Update();

                var u11 = await User1.Find(u1.Id);
                u11.Description = "备注";
                await u11.Save();

                await u11.Delete();

                var u11null = User1.Find(u1.Id);

                var u11s = User1.Where(a => a.Group.Id == ug1.Id).Limit(10).ToList();

                var u11s2 = User1.Select.LeftJoin<UserGroup>((a, b) => a.GroupId == b.Id).Limit(10).ToList();

                var ug1s = UserGroup.Select
                    .IncludeMany(a => a.User1s)
                    .Limit(10).ToList();

                var ug1s2 = UserGroup.Select.Where(a => a.User1s.AsSelect().Any(b => b.Nickname == "x1")).Limit(10).ToList();

                var r1 = new Role();
                r1.Id = "管理员";
                await r1.Save();

                var r2 = new Role();
                r2.Id = "超级会员";
                await r2.Save();

                var ru1 = new RoleUser1();
                ru1.User1Id = u1.Id;
                ru1.RoleId = r1.Id;
                await ru1.Save();

                ru1.RoleId = r2.Id;
                await ru1.Save();

                var u1roles = User1.Select.IncludeMany(a => a.Roles).ToList();
                var u1roles2 = User1.Select.Where(a => a.Roles.AsSelect().Any(b => b.Id == "xx")).ToList();

            }).Wait();

            Console.WriteLine("按任意键结束。。。");
            Console.ReadKey();
        }
    }
}
