using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;

/// <summary>
/// 员工信息表 
/// </summary>    
[Serializable]
[Index("员工代码badgenumber唯一", "badgenumber", true)]
public class userinfo : BaseEntity<userinfo>
{
    /// <summary>
    /// 员工ID  
    /// </summary>
    [Column(IsPrimary = true)]
    [System.ComponentModel.DisplayName("员工ID ")]
    [System.ComponentModel.DataAnnotations.Required()]
    public int userid { get; set; }

    /// <summary>
    /// 工号  
    /// </summary>
    [System.ComponentModel.DisplayName("工号")]
    [System.ComponentModel.DataAnnotations.Required()]
    [Column(Name = "BADGENUMBER", DbType = "VARCHAR(24)")]
    public String badgenumber { get; set; }

    /// <summary>
    /// 姓名  
    /// </summary>
    [System.ComponentModel.DisplayName("姓名")]
    [System.ComponentModel.DataAnnotations.Required()]
    [Column(DbType = "varchar(40) NULL")]

    public String Name { get; set; }
    /// <summary>
    /// 身份证  
    /// </summary>
    [System.ComponentModel.DisplayName("身份证证")]
    [System.ComponentModel.DataAnnotations.Required()]
    public String IDCardNo { get; set; }



    /// <summary>
    /// 行动电话
    /// </summary>
    [System.ComponentModel.DisplayName("行动电话")]
    [Column(DbType = "varchar(20) NULL")]
    public string pager { get; set; }

    /// <summary>
    /// 邮件地址  
    /// </summary>
    [System.ComponentModel.DisplayName("邮件地址 ")]
    public String email { get; set; }




    /// <summary>
    /// 办公电话  
    /// </summary>
    [System.ComponentModel.DisplayName("办公电话")]
    [Column(DbType = "varchar(20) NULL")]
    public String ophone { get; set; }

    /// <summary>
    /// 入职时间  
    /// </summary>
    [System.ComponentModel.DisplayName("入职时间")]
    [Column(DbType = "date")]
    public DateTime? hiredday { get; set; }




    /// <summary>
    /// 生日
    /// </summary>
    [System.ComponentModel.DisplayName("生日 ")]
    [Column(DbType = "date")]
    public DateTime? birthday { get; set; }


    /// <summary>
    /// 民族
    /// </summary>
    [System.ComponentModel.DisplayName("民族")]
    public string minzu { get; set; }

    /// <summary>
    /// 籍贯  
    /// </summary>
    [System.ComponentModel.DisplayName("籍贯")]
    public String homeaddress { get; set; }


    /// <summary>
    /// 合同日期  
    /// </summary>
    [System.ComponentModel.DisplayName("合同日期")]
    [Column(DbType = "date")]
    public DateTime? hetongdate { get; set; }

    /// <summary>
    /// 家庭地址
    /// </summary>
    [System.ComponentModel.DisplayName("家庭地址")]
    [Column(DbType = "varchar(80) NULL")]
    public String street { get; set; }


    /// <summary>
    /// 邮编  
    /// </summary>
    [System.ComponentModel.DisplayName("邮编")]
    [Column(DbType = "varchar(12) NULL")]
    public String zip { get; set; }

    [System.ComponentModel.DisplayName("城市")]
    [Column(Name = "CITY", DbType = "varchar(2)")]
    public string CITY { get; set; }

    [System.ComponentModel.DisplayName("省份")]
    [Column(DbType = "varchar(2) NULL")]
    public string STATE { get; set; }


    /// <summary>
    /// 编号
    /// </summary>
    [System.ComponentModel.DisplayName("编号")]

    public string ssn { get; set; }

    [Column(DbType = "varchar(8) NULL")]
    public string GENDER { get; set; } = "M";
    /// <summary>
    /// 职务
    /// </summary>
    [System.ComponentModel.DisplayName("职务")]
    [Column(DbType = "varchar(20) NULL")]
    public string title { get; set; }


    public short? VERIFICATIONMETHOD { get; set; }//验证方式
    public short? DEFAULTDEPTID { get; set; } = 1;//所属部门ID号
    public short? ATT { get; set; } = 1;//考勤有效 
    public short? INLATE { get; set; } = 1;//计迟到 
    public short? OUTEARLY { get; set; } = 1;//计早退

    public short? OVERTIME { get; set; }

    public short? SEP { get; set; } = 1;
    public short HOLIDAY { get; set; } = 1;//假日休息
    public string PASSWORD { get; set; }//口令
    public short LUNCHDURATION { get; set; } = 1;//有午休
    public string MVerifyPass { get; set; }//考勤验证密码

    //[Column(DbType = "image NULL")]
    //public byte[] PHOTO { get; set; }
    //[Column(DbType = "image NULL")]
    //public byte[] Notes { get; set; }

    public int? VerifyCode { get; set; }
    public int? Expires { get; set; }
    public int? ValidCount { get; set; }
    public int? UseAccGroupTZ { get; set; }

    public int? AccGroup { get; set; }
    public int? FaceGroup { get; set; }
    public int? EMPRIVILEGE { get; set; }
    public int? InheritDeptRule { get; set; }
    public int? RegisterOT { get; set; }
    public int? MinAutoSchInterval { get; set; }
    public int? AutoSchPlan { get; set; }
    public int? InheritDeptSchClass { get; set; }
    public int? InheritDeptSch { get; set; }
    public int? privilege { get; set; }
    public int? TimeZone1 { get; set; }
    public int? TimeZone2 { get; set; }
    public int? TimeZone3 { get; set; }

    [Column(DbType = "date")]
    public DateTime? ValidTimeEnd { get; set; }
    [Column(DbType = "date")]
    public DateTime? ValidTimeBegin { get; set; }

    /// <summary>
    /// 家庭电话  
    /// </summary>
    [System.ComponentModel.DisplayName("家庭电话")]
    [Column(DbType = "varchar(20) NULL")]
    public String fphone { get; set; }

    /// <summary>
    /// 卡号  
    /// </summary>
    [System.ComponentModel.DisplayName("卡号 ")]
    [Column(Name = "CardNo", DbType = "varchar(20)")]
    public String CardNo { get; set; }



    /// <summary>
    /// 身份证有效期
    /// </summary>
    [System.ComponentModel.DisplayName("身份证有效期 ")]
    public String idcardvalidtime { get; set; } = new DateTime(2099, 12, 31).ToString();





    /// <summary>
    /// 离职日期  
    /// </summary>
    [System.ComponentModel.DisplayName("离职日期")]
    [Column(DbType = "date")]
    public DateTime? leavedate { get; set; }

    /// <summary>
    /// 登录密码  
    /// </summary>
    [System.ComponentModel.DisplayName("登录密码")]
    public String loginpass { get; set; }


    /// <summary>
    /// 相片地址  
    /// </summary>
    [System.ComponentModel.DisplayName("相片地址")]
    public String picurl { get; set; }

    /// <summary>
    /// 上级主管  
    /// </summary>
    [System.ComponentModel.DisplayName("上级主管")]
    public int? managerid { get; set; }
    ///// <summary>
    ///// 上级主管对象
    ///// </summary>
    //[Navigate("managerid")]
    //public userinfo pManager { get; set; }



    [Navigate(ManyToMany = typeof(dept_user))]
    public List<DEPARTMENTS> depts { get; set; }

    /// <summary>
    /// 管理员标志
    /// </summary>

    [System.ComponentModel.DisplayName("管理员标志")]
    public short? SECURITYFLAGS { get; set; }//管理员标志
















}










