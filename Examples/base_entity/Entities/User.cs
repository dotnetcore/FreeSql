using System;
using System.Collections.Generic;
using System.Text;

public class UserGroup : BaseEntity<UserGroup, int>
{
    /// <summary>
    /// 组名
    /// </summary>
    public string GroupName { get; set; }

    public List<User1> User1s { get; set; }
}

public class Role : BaseEntity<Role, string>
{
    public List<User1> User1s { get; set; }
}
public class RoleUser1 : BaseEntity<RoleUser1>
{
    public string RoleId { get; set; }
    public Guid User1Id { get; set; }

    public Role Role { get; set; }
    public User1 User1 { get; set; }
}

public class User1 : BaseEntity<User1, Guid>
{
    public int GroupId { get; set; }
    public UserGroup Group { get; set; }

    public List<Role> Roles { get; set; }

    /// <summary>
    /// 登陆名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
}
