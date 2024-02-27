//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Newtonsoft.Json;
//using FreeSql.DataAnnotations;
//using FreeSql;
//using FreeSql.Aop;
//using System.Text.RegularExpressions;
//using System.Runtime.CompilerServices;
//using System.ComponentModel.DataAnnotations;
//using static System.Net.Mime.MediaTypeNames;

///// <summary>
///// YonganInfos 的摘要说明
///// </summary>
//[Table(Name = "yongan_customer")]
//public class YonganInfos : YonganCus
//{
//    #region Property 

//    private static IFreeSql freeSql = FreeSQLBulider.Init;


//    /// <summary>
//    /// [0-失效][1-有效]
//    /// </summary>
//    [Column(IsIgnore = true)]
//    public bool IsValid { get; set; }

//    /// <summary>
//    /// 一般检查表
//    /// </summary>
//    [Navigate(nameof(PID))]
//    public YonganGen MdlGen { get; set; }

//    /// <summary>
//    /// 辅助检查表
//    /// </summary>
//    [Navigate(nameof(PID))]
//    public YonganAss MdlAss { get; set; }

//    /// <summary>
//    /// 化验检查表
//    /// </summary>
//    [Navigate(nameof(YonganLab.PHYSICALID))]
//    public List<YonganLab> ListLab { get; set; }

//    /// <summary>
//    /// 体检建议表
//    /// </summary>
//    [Navigate(nameof(YonganAdv.PHYSICALID))]
//    public List<YonganAdv> ListAdv { get; set; }
//    #endregion

//    #region 数据库交互
//    /// <summary>
//    /// 数据查询
//    /// </summary>
//    public YonganInfos() { }
//    public YonganInfos(string PID)
//    {
//        if (PID == null) return;

//        var InfoRepo = freeSql.GetAggregateRootRepository<YonganInfos>();
//        YonganInfos yonganInfos = InfoRepo
//               .Select
//               .Where(x => x.PID == PID)
//               .First();
//        if (yonganInfos == null) return;

//        this.ModelToModel(yonganInfos, this);
//        this.IsValid = true;
//    }


//    /// <summary>
//    /// 数据同步
//    /// </summary>
//    public void YonganSync()
//    {
//        if (!this.IsValid) return;

//        //建立仓库
//        var InfoRepo = freeSql.GetAggregateRootRepository<YonganInfos>();
//        YonganInfos DBInfos = new YonganInfos(PID);
//        //数据同步
//        this.SyncIDCARD(DBInfos);
//        //快照数据库中的记录
//        InfoRepo.Attach(DBInfos);
//        //对比现在类中的数据
//        //执行插入或更新
//        InfoRepo.InsertOrUpdate(this);
//    }

//    #endregion

//    #region 业务交互
//    /// <summary>
//    /// 登记体检
//    /// </summary>
//    public void YonganRegister()
//    {
//        this.PID = this.GeneratePID();
//        this.MdlGen = new YonganGen { IDCARD = this.IDCARD };
//        this.MdlAss = new YonganAss { IDCARD = this.IDCARD };
//        this.IsValid = true;
//        this.YonganSync();
//    }

//    /// <summary>
//    /// 生成建议
//    /// </summary>
//    public void GenerateAdvise()
//    {
//        List<YonganExcList> ListException = freeSql.Select<YonganExcList>()
//            .Where(x => !string.IsNullOrEmpty(x.ValueType))
//            .Where(x => x.IsValid == "1")
//            .ToList();

//    }

//    /// <summary>
//    /// 检验数据同步
//    /// </summary>
//    public void ToLabdata(bool isToLab)
//    {
//        if (isToLab)
//        {
//            //主表 -> 明细: 数据同步

//        }
//        else
//        {
//            //明细 -> 主表: 数据同步

//        }
//    }

//    #endregion

//    #region 数据同步
//    public void SyncIDCARD(YonganInfos DBInfos)
//    {
//        //身份证联动更新
//        if (DBInfos.IDCARD != this.IDCARD)
//        {
//            this.MdlGen.IDCARD = this.IDCARD;
//            this.MdlAss.IDCARD = this.IDCARD;
//            this.ListLab.ConvertAll(a => a.IDCARD = this.IDCARD);
//            this.ListAdv.ConvertAll(a => a.IDCARD = this.IDCARD);
//        }
//    }
//    #endregion


//    #region 内部数据处理方法
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="mdlFrom"></param>
//    /// <param name="mdlTo"></param>
//    /// <returns></returns>
//    private object ModelToModel(object mdlFrom, object mdlTo)
//    {
//        Type fromT = mdlFrom.GetType();
//        Type toT = mdlTo.GetType();
//        foreach (PropertyInfo ProTo in toT.GetProperties())
//        {
//            foreach (PropertyInfo ProFrom in fromT.GetProperties())
//            {
//                if (ProTo.Name.ToUpper().Equals(ProFrom.Name.ToUpper()))
//                {
//                    object Val = ProFrom.GetValue(mdlFrom, null);
//                    ProTo.SetValue(mdlTo, Val, null);
//                    break;
//                }
//            }
//        }
//        return mdlTo;
//    }

//    /// <summary>
//    /// 生成pid
//    /// </summary>
//    /// <returns></returns>
//    [MethodImpl(MethodImplOptions.Synchronized)]
//    private string GeneratePID()
//    {
//        //PID = 日期(年月日)8位 + 标志位[02-入职体检]2位 + 自增长4位(共14位)           20230112 02 0001
//        string pid = string.Empty;
//        string flagID = "02";
//        string Date = DateTime.Now.ToString("yyyyMMdd");
//        string MaxPID = freeSql.Select<YonganCus>()
//            .Where(a => a.PID.StartsWith($"{Date}{flagID}") && a.PID.Length == 14)
//            .Max(b => b.PID);
//        if (string.IsNullOrEmpty(MaxPID))
//            //当天第一位病人
//            pid = $"{Date}{flagID}0001";
//        else
//            //后续病人 
//            pid = $"{Date}{flagID}{(int.Parse(MaxPID.Substring(MaxPID.Length - 4)) + 1).ToString().PadLeft(4, '0')}";

//        return pid;
//    }

//    /// <summary>
//    /// 获取导航类型
//    /// </summary>
//    /// <returns></returns>
//    public string GetRefType()
//    {
//        var tbref = freeSql.CodeFirst
//    .GetTableByEntity(typeof(YonganInfos))
//    .GetTableRef("MdlAss", true);
//        return tbref.RefType.ToString();
//    }
//    #endregion




//    #region test方法

//    public void YonganTest()
//    {
//        var InfoRepo = freeSql.GetAggregateRootRepository<YonganInfos>();
//        var LabRepo = freeSql.GetAggregateRootRepository<YonganLab>();


//        YonganInfos yonganInfos = new YonganInfos("111");
//        InfoRepo.Attach(yonganInfos);

//        //yonganInfos.ListLab.First().ITEMNAME = "rrr";                     //单条记录更新✔
//        //yonganInfos.ListLab.ConvertAll(lab => lab.ITEMNAME = "eee");      //多条记录更新✔
//        //yonganInfos.ListLab.Add(new YonganLab {ITEMNAME = "ddd" });       //单条记录Add✔ 存在相同主键则不更新
//        //yonganInfos.ListLab.AddRange(new List<YonganLab>                  //多条记录Add×  仅仅插入第一条
//        //{
//        //    new YonganLab{ITEMNAME="e",IDCARD="444"},
//        //    new YonganLab{ITEMNAME="f"},
//        //});

//        yonganInfos.ListLab.Add(new YonganLab { ITEMNAME = "f" });
//        yonganInfos.ListLab.Add(new YonganLab { ITEMNAME = "g" });
//        yonganInfos.ListLab.Add(new YonganLab { ITEMNAME = "h" });

//        //LabRepo.BeginEdit(yonganInfos.ListLab);
//        //yonganInfos.ListLab.Add(new YonganLab { ITEMNAME = "f" } );
//        //yonganInfos.ListLab.Add(new YonganLab { ITEMNAME = "g" } );
//        //yonganInfos.ListLab.Add(new YonganLab { ITEMNAME = "h" } );
//        //LabRepo.EndEdit();

//        //InfoRepo.Update(yonganInfos);
//        InfoRepo.InsertOrUpdate(yonganInfos);
//    }


//    public void YonganTest1()
//    {
//        //var InfoRepo = freeSql.GetAggregateRootRepository<YonganInfos>();
//        //InfoRepo.Attach(new YonganInfos("111"));

//        YonganInfos yonganInfos = new YonganInfos("111");
//        yonganInfos.IDCARD = "555";
//        //yonganInfos.ListLab = new List<YonganLab>
//        //{
//        //        new YonganLab{IDCARD=this.IDCARD,ITEMNAME="a"},
//        //        new YonganLab{IDCARD=this.IDCARD,ITEMNAME="b"},
//        //        new YonganLab{IDCARD=this.IDCARD,ITEMNAME="c"},
//        //        new YonganLab{IDCARD=this.IDCARD,ITEMNAME="d"},
//        //        new YonganLab{IDCARD=this.IDCARD,ITEMNAME="e"},
//        //        new YonganLab{IDCARD=this.IDCARD,ITEMNAME="f"},
//        //};

//        yonganInfos.YonganSync();
//    }


//    public void freeTest1()
//    {
//        var repository = freeSql.GetAggregateRootRepository<Order>();
//        var order = repository.Select.Where(a => a.Id == 1).First(); //此时已自动 Attach

//        order.Details = new List<OrderDetail>
//        {
//            new OrderDetail { Field4 = "field4_01"},
//            new OrderDetail { Field4 = "field4_02"},
//            new OrderDetail { Field4 = "field4_03"}
//        };
//        //order.Details[0].Field4 = "test";  
//        repository.InsertOrUpdate(order);

//    }


//    public void freeTest2()
//    {
//        var repository = freeSql.GetAggregateRootRepository<Order>();
//        var order = freeSql.Select<Order>()
//            .Where(a => a.Id == 1)
//            .First(); //单表数据

//        repository.Attach(order); //快照时 Comments 是 NULL/EMPTY
//        order.Details = new List<OrderDetail>
//        {
//            new OrderDetail { Field4 = "field4_01"},
//            new OrderDetail { Field4 = "field4_02"},
//            new OrderDetail { Field4 = "field4_03"}
//        };
//        repository.InsertOrUpdate(order);


//    }

//    #endregion


//}


//#region testModel

//class Order
//{
//    [Column(IsIdentity = true)]
//    public int Id { get; set; }
//    public string Field2 { get; set; }

//    public OrderExt Extdata { get; set; }

//    [Navigate(nameof(OrderDetail.OrderId))]
//    public List<OrderDetail> Details { get; set; }

//    [Navigate(ManyToMany = typeof(OrderTag))]
//    public List<Tag> Tags { get; set; }
//}
//class OrderExt
//{
//    [Key]
//    public int OrderId { get; set; }
//    public string Field3 { get; set; }

//    public Order Order { get; set; }
//}
//class OrderDetail
//{
//    [Column(IsIdentity = true)]
//    public int Id { get; set; }
//    public int OrderId { get; set; }
//    public string Field4 { get; set; }

//    public OrderDetailExt Extdata { get; set; }
//}
//class OrderDetailExt
//{
//    [Key]
//    public int OrderDetailId { get; set; }
//    public string Field5 { get; set; }

//    public OrderDetail OrderDetail { get; set; }
//}




///// <summary>
///// 中间表
///// </summary>
//class OrderTag
//{
//    [Key]
//    public int OrderId { get; set; }
//    [Key]
//    public int TagId { get; set; }

//    [Navigate(nameof(OrderId))]
//    public Order Order { get; set; }
//    [Navigate(nameof(TagId))]
//    public Tag Tag { get; set; }
//}
//class Tag
//{
//    [Column(IsIdentity = true)]
//    public int Id { get; set; }
//    public string Name { get; set; }

//    [Navigate(ManyToMany = typeof(OrderTag))]
//    public List<Order> Orders { get; set; }
//}


////class Order
////{
////    [Column(IsIdentity = true, IsPrimary = true)]
////    public int Id { get; set; }
////    public string Field2 { get; set; }
////    [Navigate(nameof(OrderComment.OrderId))]
////    public List<OrderComment> Comments { get; set; }
////}
//class OrderComment
//{
//    [Column(IsIdentity = true, IsPrimary = true)]
//    public int Id { get; set; }
//    public int OrderId { get; set; }
//    public string Field6 { get; set; }
//}



//[Table(Name = "test_user")]
//public class test_user
//{
//    [Key]
//    public int ID { get; set; }

//    public string Name { get; set; }

//    [Navigate(nameof(test_userext.NameId))]
//    public List<test_userext> list { get; set; }
//}


//[Table(Name = "test_userext")]
//public class test_userext
//{
//    [Key]
//    public int UserId { get; set; }
//    public int NameId { get; set; }
//    public string NameEXT { get; set; }
//}



//#endregion

