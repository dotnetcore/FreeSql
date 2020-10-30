using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _507
    {
        [Fact]
        public void SelectTest()
        {
            var fsql = g.sqlite;
            var _bodyAuditRepository = fsql.GetRepository<BodyAuditEntity>();

            var sql = _bodyAuditRepository.Select
                .Include(a => a.ClassInfo)
                .Count(out var total)
                .OrderBy(true, a => a.ClassInfo.period)
                .OrderBy(true, a => a.ClassInfo.className)
                .Page(2, 10)
                .ToList();
        }
        public class BodyAuditEntity
        {
            public string Id { get; set; }

            /// <summary>
            /// 班期标识-BodyScheduleEntity班期主键
            /// </summary>
            [Column(Name = "ClassId", StringLength = 70, Position = 2)]
            public string ClassId { get; set; }

            /// <summary>
            /// 姓名
            /// </summary>
            [Column(Name = "Name", StringLength = 20, Position = 3)]
            public string Name { get; set; }



            /// <summary>
            /// 出生日期
            /// </summary>
            [Column(Name = "Birthday", StringLength = 20, Position = 5)]
            public string Birthday { get; set; }

            /// <summary>
            /// 身份证
            /// </summary>
            [Column(Name = "IdCard", StringLength = 20, Position = 6)]
            public string IdCard { get; set; }

            /// <summary>
            /// 联系电话
            /// </summary>
            [Column(Name = "TelPhone", StringLength = 15, Position = 7)]
            public string TelPhone { get; set; }

            /// <summary>
            /// 学校名称
            /// </summary>
            [Column(Name = "SchoolName", StringLength = 50, Position = 8)]
            public string SchoolName { get; set; }

            /// <summary>
            /// 初潮年龄
            /// </summary>
            [Column(Name = "StuCCNL", StringLength = 20, Position = 9)]
            public int? StuCCNL { get; set; }

            /// <summary>
            /// 联系地址
            /// </summary>
            [Column(Name = "Address", StringLength = 100, Position = 10)]
            public string Address { get; set; }

            /// <summary>
            /// 审核人标识
            /// </summary>
            [Column(Name = "CheckUserId", StringLength = 70)]
            public string CheckUserId { get; set; }

            /// <summary>
            /// 审核人
            /// </summary>
            [Column(Name = "CheckUserName", StringLength = 20)]
            public string CheckUserName { get; set; }

            /// <summary>
            /// 审核时间
            /// </summary>
            [Column(Name = "CheckTime")]
            public DateTime? CheckTime { get; set; }

            /// <summary>
            /// 拒绝原因
            /// </summary>
            [Column(Name = "RefuseReason", StringLength = 100)]
            public string RefuseReason { get; set; }

            /// <summary>
            /// 采集时间
            /// </summary>
            [Column(Name = "CollectionDate")]
            public DateTime? CollectionDate { get; set; }

            /// <summary>
            /// 附件标识（多附件）
            /// </summary>
            [Column(Name = "AttachmentId", StringLength = 500)]
            public string AttachmentId { get; set; }

            /// <summary>
            /// 性别
            /// </summary>
            [Column(Name = "Sex", StringLength = 2, Position = 4)]
            public string Sex { get; set; }

            #region 导航属性
            /// <summary>
            /// 班期信息
            /// </summary>
            [Navigate(nameof(ClassId))]
            public virtual BodyScheduleEntity ClassInfo { get; set; }
            #endregion
        }
        public class BodyScheduleEntity
        {
            public string Id { get; set; }

            [Column(Name = "period")]
            public int? period { get; set; }

            [Column(Name = "maxQuota")]
            public int? maxQuota { get; set; }

            [Column(Name = "allDays")]
            public int? allDays { get; set; }

            [Column(Name = "schoolId", StringLength = 64)]
            public string bodySchoolId { get; set; }

            [Column(Name = "ownership", StringLength = 64)]
            public string ownership { get; set; }


            [Column(Name = "className", StringLength = 20)]
            public string className { get; set; }

            [Column(Name = "sex", StringLength = 10)]
            public string sex { get; set; }

            [Column(Name = "classType", StringLength = 20)]
            public string classType { get; set; }
        }

    }
}
