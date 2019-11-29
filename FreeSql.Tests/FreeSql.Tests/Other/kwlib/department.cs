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
    public class department
    {
        /// <summary>
        /// 部门ID  
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public int id { get; set; }

        /// <summary>
        /// 员工列表  对应employee.deptid
        /// </summary>
        [Navigate("deptid")] 
        public List<employee> Employees { get; set; }

        /// <summary>
        /// 上级部门ID  
        /// </summary>
        public int? supdeptid { get; set; }
        /// <summary>
        /// 上级部门对象
        /// </summary>
        [Navigate("supdeptid")]
        public department parentdepartments { get; set; }

        /// <summary>
        /// 部门主管ID  
        /// </summary>
        public int? managerid { get; set; }
        /// <summary>
        /// 部门主管对象
        /// </summary>
        [Navigate("managerid")]
        public employee manager { get; set; }


        /// <summary>
        /// 下级部门列表
        /// </summary>
        [Navigate("supdeptid")]
        public List<department> childDepartments { get; set; }


        [Navigate(ManyToMany = typeof(department_employee))]
        public List<employee> employees22 { get; set; }


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