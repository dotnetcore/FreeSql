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
    [Index("员工代码badgenumber唯一", "badgenumber", true)]
    public class userinfo
    {
        /// <summary>
        /// 员工ID  
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        [System.ComponentModel.DisplayName("员工ID ")]
        public int userid { get; set; }

        /// <summary>
        /// 考勤号码  
        /// </summary>
        [System.ComponentModel.DisplayName("考勤号码")]
        public String badgenumber { get; set; }


        /// <summary>
        /// 编号
        /// </summary>
        [System.ComponentModel.DisplayName("编号")]

        public string ssn { get; set; }

        /// <summary>
        /// 身份证  
        /// </summary>
        [System.ComponentModel.DisplayName("身份证/证件号")]
        public String IDCardNo { get; set; }

        /// <summary>
        /// 姓名  
        /// </summary>
        [System.ComponentModel.DisplayName("姓名")]
        public String name { get; set; }



        /// <summary>
        /// 职务
        /// </summary>
        [System.ComponentModel.DisplayName("职务")]
        public string title { get; set; }


        /// <summary>
        /// 生日
        /// </summary>
        [System.ComponentModel.DisplayName("生日 ")]
        public DateTime? birthday { get; set; }
        /// <summary>
        /// 入职时间  
        /// </summary>
        [System.ComponentModel.DisplayName("入职时间")]
        public DateTime? hiredday { get; set; }
        /// <summary>
        /// 合同日期  
        /// </summary>
        [System.ComponentModel.DisplayName("合同日期")]
        public DateTime? hetongdate { get; set; }

        /// <summary>
        /// 家庭地址
        /// </summary>
        [System.ComponentModel.DisplayName("家庭地址")]
        public String street { get; set; }

        /// <summary>
        /// 邮编  
        /// </summary>
        [System.ComponentModel.DisplayName("邮编")]
        public String zip { get; set; }

        /// <summary>
        /// 办公电话  
        /// </summary>
        [System.ComponentModel.DisplayName("办公电话")]
        public String ophone { get; set; }


        /// <summary>
        /// 行动电话
        /// </summary>
        [System.ComponentModel.DisplayName("行动电话")]
        public string pager { get; set; }

        /// <summary>
        /// 家庭电话  
        /// </summary>
        [System.ComponentModel.DisplayName("家庭电话")]
        public String fphone { get; set; }

        /// <summary>
        /// 卡号  
        /// </summary>
        [System.ComponentModel.DisplayName("卡号 ")]
        public String CardNo { get; set; }

        /// <summary>
        /// 邮件地址  
        /// </summary>
        [System.ComponentModel.DisplayName("邮件地址 ")]
        public String email { get; set; }


        /// <summary>
        /// 身份证有效期
        /// </summary>
        [System.ComponentModel.DisplayName("身份证有效期 ")]
        public DateTime idcardvalidtime { get; set; } = new DateTime(2099, 12, 31);



        /// <summary>
        /// 籍贯  
        /// </summary>
        [System.ComponentModel.DisplayName("籍贯")]
        public String homeaddress { get; set; }

        /// <summary>
        /// 民族
        /// </summary>
        [System.ComponentModel.DisplayName("民族")]
        public string minzu { get; set; }

        /// <summary>
        /// 离职日期  
        /// </summary>
        [System.ComponentModel.DisplayName("离职日期")]
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
        /// 上级主管ID  
        /// </summary>
        [System.ComponentModel.DisplayName("上级主管ID")]
        public int? managerid { get; set; }
        /// <summary>
        /// 上级主管对象
        /// </summary>
        [Navigate("managerid")]
        public userinfo pManager { get; set; }



        [Navigate(ManyToMany = typeof(department_userinfo))]
        public List<departments> departments { get; set; }



























    }









}
