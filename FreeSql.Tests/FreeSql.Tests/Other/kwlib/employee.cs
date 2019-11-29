using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kwlib
{
    /// <summary>
    /// 员工表 
    /// </summary>    
    [Serializable]
    [Index("员工代码empcode唯一", "empcode", true)]
    public class employee
    {

        
        /// <summary>
        /// 员工ID  
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        [System.ComponentModel.DisplayName("员工ID ")]
        public int id { get; set; }

        [System.ComponentModel.DisplayName("上级主管ID")]
        /// <summary>
        /// 上级主管ID  
        /// </summary>
        public int? managerid { get; set; }
        /// <summary>
        /// 上级主管对象
        /// </summary>
        [Navigate("managerid")]
        public employee parentManager { get; set; }

        /// <summary>
        /// 下级员工列表
        /// </summary>
        [Navigate("managerid")]
        public List<employee> Employees { get; set; }


        [Navigate(ManyToMany = typeof(department_employee))]
        public List<department> departments { get; set; }

        [System.ComponentModel.DisplayName("部门ID ")]
        /// <summary>
        /// 部门ID  
        /// </summary>
        public int? deptid { get; set; }
        /// <summary>
        /// 部门对象
        /// </summary>
        [Navigate("deptid")]
        public department Department { get; set; }



        [System.ComponentModel.DisplayName("员工工号")]
        /// <summary>
        /// 员工工号  
        /// </summary>
        public String empcode { get; set; }



        [System.ComponentModel.DisplayName("员工姓名")]
        /// <summary>
        /// 员工姓名  
        /// </summary>
        public String empname { get; set; }


        [System.ComponentModel.DisplayName("地址")]
        /// <summary>
        /// 地址
        /// </summary>
        public String address { get; set; }


        [System.ComponentModel.DisplayName("工卡ID ")]
        /// <summary>
        /// 工卡ID  
        /// </summary>

        public String cardid { get; set; }


        [System.ComponentModel.DisplayName("邮件地址 ")]
        /// <summary>
        /// 邮件地址  
        /// </summary>

        public String email { get; set; }


        [System.ComponentModel.DisplayName("合同日期")]
        /// <summary>
        /// 合同日期  
        /// </summary>

        public DateTime? hetongdate { get; set; }


        [System.ComponentModel.DisplayName("籍贯")]
        /// <summary>
        /// 籍贯  
        /// </summary>

        public String homeaddress { get; set; }


        [System.ComponentModel.DisplayName("入职时间")]
        /// <summary>
        /// 入职时间  
        /// </summary>

        public DateTime jointime { get; set; }


        [System.ComponentModel.DisplayName("离职日期")]
        /// <summary>
        /// 离职日期  
        /// </summary>
        public DateTime? leavedate { get; set; }


        [System.ComponentModel.DisplayName("登录密码")]
        /// <summary>
        /// 登录密码  
        /// </summary>
        public String loginpass { get; set; }



        [System.ComponentModel.DisplayName("电话")]
        /// <summary>
        /// 电话  
        /// </summary>
        public String phone { get; set; }


        [System.ComponentModel.DisplayName("相片地址")]
        /// <summary>
        /// 相片地址  
        /// </summary>

        public String picurl { get; set; }



        [System.ComponentModel.DisplayName("身份证")]
        /// <summary>
        /// 身份证  
        /// </summary>

        public String sfz { get; set; }


    }









}
