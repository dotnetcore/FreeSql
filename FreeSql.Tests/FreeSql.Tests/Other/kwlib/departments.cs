using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace kwlib
{
    /// <summary>
    /// 部门表  
    /// </summary>    
    [Serializable]
    [Index("部门代码deptcode唯一", "deptcode", true)]
    public class departments
    {
        /// <summary>
        /// 部门ID  
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public int deptid { get; set; }

        ///// <summary>
        ///// 员工列表  对应employee.deptid
        ///// </summary>
        //[Navigate("deptid")] 
        //public List<employee> Employees { get; set; }

        /// <summary>
        /// 上级部门ID  
        /// </summary>
        public int? supdeptid { get; set; }
        /// <summary>
        /// 上级部门对象
        /// </summary>
        [Navigate("supdeptid")]
        public departments pDepartments { get; set; }

        /// <summary>
        /// 部门主管ID  
        /// </summary>
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


        [Navigate(ManyToMany = typeof(department_userinfo))]
        public List<userinfo> employeesMany { get; set; }


        #region MyRegion
        /// <summary>
        /// 部门代码
        /// </summary>
        public string deptcode { get; set; }

        /// <summary>
        /// 部门名称  
        /// </summary>
        public string deptname { get; set; }
        #endregion




    }

}