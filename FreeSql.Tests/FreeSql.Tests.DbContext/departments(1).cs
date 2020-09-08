using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;



/// <summary>
/// 部门表  
/// </summary>    
[Serializable]
[Index("部门代码deptcode唯一", "deptcode", true)]
public class DEPARTMENTS : BaseEntity<DEPARTMENTS>
{
    /// <summary>
    /// 部门ID  
    /// </summary>
    [Column(IsPrimary = true)]

    [System.ComponentModel.DisplayName("部门ID")]
    public int deptid { get; set; }

    ///// <summary>
    ///// 员工列表  对应employee.deptid
    ///// </summary>
    //[Navigate("deptid")] 
    //public List<employee> Employees { get; set; }

    /// <summary>
    /// 上级部门ID  
    /// </summary>
    [System.ComponentModel.DisplayName("上级部门ID")]
    public int? supdeptid { get; set; }
    /// <summary>
    /// 上级部门对象
    /// </summary>
    [Navigate("supdeptid")]
    public DEPARTMENTS pDepartments { get; set; }

    /// <summary>
    /// 部门主管ID  
    /// </summary>
    [System.ComponentModel.DisplayName("部门主管ID")]
    public int? managerid { get; set; }
    /// <summary>
    /// 部门主管对象
    /// </summary>
    [Navigate("managerid")]
    public userinfo manager { get; set; }


    ///// <summary>
    ///// 下级部门列表
    ///// </summary>
    //[Navigate("supdeptid")]
    //public List<departments> childDepartments { get; set; }


    [Navigate(ManyToMany = typeof(dept_user))]
    public List<userinfo> employeesMany { get; set; }


    #region MyRegion
    /// <summary>
    /// 部门代码
    /// </summary>
    [System.ComponentModel.DisplayName("部门代码")]
    [System.ComponentModel.DataAnnotations.Required()]
    public string deptcode { get; set; }

    /// <summary>
    /// 部门名称  
    /// </summary>
    [System.ComponentModel.DisplayName("部门名称")]
    [System.ComponentModel.DataAnnotations.Required()]
    public string deptname { get; set; }
    #endregion

    public short? InheritParentSch { get; set; }
    public short? InheritDeptSch { get; set; }
    public short? InheritDeptSchClass { get; set; }
    public short? AutoSchPlan { get; set; }
    public short? InLate { get; set; }
    public short? OutEarly { get; set; }
    public short? InheritDeptRule { get; set; }
    public int? MinAutoSchInterval { get; set; }
    public short? RegisterOT { get; set; }
    public int? DefaultSchId { get; set; }
    public short? ATT { get; set; }
    public short? Holiday { get; set; }
    public short? OverTime { get; set; }



}

