using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinFormApp.Models;
using xamarinFormApp.Services;
using xamarinFormApp.Views;

namespace xamarinFormApp
{
    public partial class App : Application
    {
        public static IFreeSql fsql;

        public App()
        {
            InitializeComponent();
            test();
            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }
        void test()
        {
            List<Item> items;


            items = new List<Item>()
            {
                new Item { Id = Guid.NewGuid().ToString(), Text = "假装 First item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "的哥 Second item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "四风 Third item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "加州 Fourth item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "阳光 Fifth item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "孔雀 Sixth item", Description="This is an item description." }
            };



            try
            {
                #region mssql测试没问题

                //        fsql = new FreeSql.FreeSqlBuilder()
                //.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=192.168.1.100;Initial Catalog=demo;Persist Security Info=True;MultipleActiveResultSets=true;User ID=sa;Password=a123456;Connect Timeout=30;min pool size=1;connection lifetime=15")
                //.UseAutoSyncStructure(true) //自动同步实体结构【开发环境必备】
                //.UseMonitorCommand(cmd => Console.Write(cmd.CommandText))
                //.Build();

                #endregion

                #region Sqlite需要反射,明天有时间再补充代码

                //                fsql = new FreeSql.FreeSqlBuilder()
                //.UseConnectionString(FreeSql.DataType.Sqlite, "Data Source=document.db; Pooling=true;Min Pool Size=1")
                //.UseAutoSyncStructure(true) //自动同步实体结构【开发环境必备】
                //.UseMonitorCommand(cmd => Console.Write(cmd.CommandText))
                //.Build();

                #endregion

                #region mysql使用 reeSql.Provider.MySqlConnector , debug和release都设置为不链接即可

                fsql = new FreeSql.FreeSqlBuilder()
.UseConnectionString(FreeSql.DataType.MySql, "Data Source=192.168.1.100;Port=3306;User ID=root;Password=a123456; Initial Catalog=test;Charset=utf8; SslMode=none;Min pool size=1")
.UseAutoSyncStructure(true) //自动同步实体结构【开发环境必备】
.UseMonitorCommand(cmd => Console.Write(cmd.CommandText))
.Build();
                #endregion

                fsql.CodeFirst.SyncStructure<Item>();
                if (fsql.Select<Item>().Count()<10) fsql.Insert<Item>().AppendData(items).ExecuteAffrows();
                var res = fsql.Select<Item>().ToList(a=>a.Text);
                res.ForEach(a => {
                    Debug.WriteLine(" <== 测试测试测试 ==> " + a);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
