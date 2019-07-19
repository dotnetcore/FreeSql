using System;

namespace base_entity
{
    class Program
    {
        static void Main(string[] args)
        {
            var ug1 = new UserGroup();
            ug1.GroupName = "分组一";
            ug1.Insert();

            var ug2 = new UserGroup();
            ug2.GroupName = "分组二";
            ug2.Insert();

            var u1 = new User1();
            var u2 = new User2();

            u1.GroupId = ug1.Id;
            u1.Save();

            u2.GroupId = ug2.Id;
            u2.Save();

            u1.Delete();
            u1.Restore();

            u1.Nickname = "x1";
            u1.Update();

            u2.Delete();
            u2.Restore();

            u2.Username = "x2";
            u2.Update();

            var u11 = User1.Find(u1.Id);
            u11.Description = "备注";
            u11.Save();

            u11.Delete();

            var u11null = User1.Find(u1.Id);

            var u11s = User1.Where(a => a.Group.Id == ug1.Id).Limit(10).ToList();
            var u22s = User2.Where(a => a.Group.Id == ug2.Id).Limit(10).ToList();

            var u11s2 = User1.Select.LeftJoin<UserGroup>((a, b) => a.GroupId == b.Id).Limit(10).ToList();
            var u22s2 = User2.Select.LeftJoin<UserGroup>((a, b) => a.GroupId == b.Id).Limit(10).ToList();

            var ug1s = UserGroup.Select
                .IncludeMany(a => a.User1s)
                .IncludeMany(a => a.User2s)
                .Limit(10).ToList();

            var ug1s2 = UserGroup.Select.Where(a => a.User1s.AsSelect().Any(b => b.Nickname == "x1")).Limit(10).ToList();

            var r1 = new Role();
            r1.Id = "管理员";
            r1.Save();

            var r2 = new Role();
            r2.Id = "超级会员";
            r2.Save();

            var ru1 = new RoleUser1();
            ru1.User1Id = u1.Id;
            ru1.RoleId = r1.Id;
            ru1.Save();

            ru1.RoleId = r2.Id;
            ru1.Save();

            var u1roles = User1.Select.IncludeMany(a => a.Roles).ToList();
            var u1roles2 = User1.Select.Where(a => a.Roles.AsSelect().Any(b => b.Id == "xx")).ToList();

            Console.WriteLine("按任意键结束。。。");
            Console.ReadKey();
        }
    }
}
