using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _283
    {
        [Fact]
        public void SelectTest()
        {
            IFreeSql db = g.sqlserver;

            db.Transaction(() =>
            {
                db.Delete<BuildDictionary>().Where("1=1").ExecuteAffrows();
                db.Delete<Build2>().Where("1=1").ExecuteAffrows();

                var dictionaries = new BuildDictionary[]
                {
                    new BuildDictionary { Type = 1, Code = 'A', Name = "办公建筑" },
                    new BuildDictionary { Type = 1, Code = 'B', Name = "商场建筑" },
                    new BuildDictionary { Type = 1, Code = 'C', Name = "宾馆饭店建筑" },
                    new BuildDictionary { Type = 1, Code = 'D', Name = "文化教育建筑" },
                    new BuildDictionary { Type = 1, Code = 'E', Name = "医疗卫生建筑" },
                    new BuildDictionary { Type = 1, Code = 'F', Name = "体育建筑" },
                    new BuildDictionary { Type = 1, Code = 'G', Name = "综合建筑" },
                    new BuildDictionary { Type = 1, Code = 'Z', Name = "其他建筑" },

                    new BuildDictionary { Type = 2, Code = 'A', Name = "结构建筑" },
                    new BuildDictionary { Type = 2, Code = 'B', Name = "框剪结构" },
                    new BuildDictionary { Type = 2, Code = 'C', Name = "剪力墙结构" },
                    new BuildDictionary { Type = 2, Code = 'D', Name = "砖混结构" },
                    new BuildDictionary { Type = 2, Code = 'E', Name = "钢结构" },
                    new BuildDictionary { Type = 2, Code = 'F', Name = "筒体结构" },
                    new BuildDictionary { Type = 2, Code = 'G', Name = "木结构" },
                    new BuildDictionary { Type = 2, Code = 'Z', Name = "其他" },

                    new BuildDictionary { Type = 3, Code = 'A', Name = "集中式全空气系统" },
                    new BuildDictionary { Type = 3, Code = 'B', Name = "风机盘管+新风系统" },
                    new BuildDictionary { Type = 3, Code = 'C', Name = "分体式空调或 VRV 的局部式机组系统" },
                    new BuildDictionary { Type = 3, Code = 'Z', Name = "其他" },

                    new BuildDictionary { Type = 4, Code = 'A', Name = "散热器采暖" },
                    new BuildDictionary { Type = 4, Code = 'B', Name = "地板辐射采暖" },
                    new BuildDictionary { Type = 4, Code = 'C', Name = "电辐射采暖" },
                    new BuildDictionary { Type = 4, Code = 'D', Name = "空调系统集中供暖" },
                    new BuildDictionary { Type = 4, Code = 'Z', Name = "其他" },

                    new BuildDictionary { Type = 5, Code = 'A', Name = "砖" },
                    new BuildDictionary { Type = 5, Code = 'B', Name = "建筑砌块" },
                    new BuildDictionary { Type = 5, Code = 'C', Name = "板材墙体" },
                    new BuildDictionary { Type = 5, Code = 'D', Name = "复合墙板和墙体" },
                    new BuildDictionary { Type = 5, Code = 'E', Name = "玻璃幕墙" },
                    new BuildDictionary { Type = 5, Code = 'Z', Name = "其他" },

                    new BuildDictionary { Type = 6, Code = 'A', Name = "内保温" },
                    new BuildDictionary { Type = 6, Code = 'B', Name = "外保温" },
                    new BuildDictionary { Type = 6, Code = 'C', Name = "夹芯保温" },
                    new BuildDictionary { Type = 6, Code = 'Z', Name = "其他" },

                    new BuildDictionary { Type = 7, Code = 'A', Name = "单玻单层窗" },
                    new BuildDictionary { Type = 7, Code = 'B', Name = "单玻双层窗" },
                    new BuildDictionary { Type = 7, Code = 'C', Name = "单玻单层窗+单玻双层窗" },
                    new BuildDictionary { Type = 7, Code = 'D', Name = "中空双层玻璃窗" },
                    new BuildDictionary { Type = 7, Code = 'E', Name = "中空三层玻璃窗" },
                    new BuildDictionary { Type = 7, Code = 'F', Name = "中空充惰性气体" },
                    new BuildDictionary { Type = 7, Code = 'Z', Name = "其他" },

                    new BuildDictionary { Type = 8, Code = 'A', Name = "普通玻璃" },
                    new BuildDictionary { Type = 8, Code = 'B', Name = "镀膜玻璃" },
                    new BuildDictionary { Type = 8, Code = 'C', Name = "Low-e 玻璃" },
                    new BuildDictionary { Type = 8, Code = 'Z', Name = "其他" },

                    new BuildDictionary { Type = 9, Code = 'A', Name = "钢窗" },
                    new BuildDictionary { Type = 9, Code = 'B', Name = "铝合金" },
                    new BuildDictionary { Type = 9, Code = 'C', Name = "木窗" },
                    new BuildDictionary { Type = 9, Code = 'D', Name = "断热窗框" },
                    new BuildDictionary { Type = 9, Code = 'E', Name = "塑钢" },
                    new BuildDictionary { Type = 9, Code = 'Z', Name = "其他" },
                };

                db.Insert(dictionaries).ExecuteAffrows();

                Build2 build = new Build2
                {
                    ID = 1,
                    Name = "建筑 1",
                    BuildFunctionCode = 'A',
                    BuildStructureCode = 'A',
                    AirTypeCode = 'A',
                    HeatTypeCode = 'A',
                    WallMaterialTypeCode = 'A',
                    WallWarmTypeCode = 'A',
                    WallWindowsTypeCode = 'A',
                    GlassTypeCode = 'A',
                    WinFrameMaterialCode = 'A'
                };

                db.Insert(build).ExecuteAffrows();
            });

            Build2 build = db.Select<Build2>()
                    .InnerJoin(a => a.BuildFunctionCode == a.BuildFunction.Code && a.BuildFunction.Type == 1)
                    .InnerJoin(a => a.BuildStructureCode == a.BuildStructure.Code && a.BuildStructure.Type == 2)
                    .InnerJoin(a => a.AirTypeCode == a.AirType.Code && a.AirType.Type == 3)
                    .InnerJoin(a => a.HeatTypeCode == a.HeatType.Code && a.HeatType.Type == 4)
                    .InnerJoin(a => a.WallMaterialTypeCode == a.WallMaterialType.Code && a.WallMaterialType.Type == 5)
                    .InnerJoin(a => a.WallWarmTypeCode == a.WallWarmType.Code && a.WallWarmType.Type == 6)
                    .InnerJoin(a => a.WallWindowsTypeCode == a.WallWindowsType.Code && a.WallWindowsType.Type == 7)
                    .InnerJoin(a => a.GlassTypeCode == a.GlassType.Code && a.GlassType.Type == 8)
                    .InnerJoin(a => a.WinFrameMaterialCode == a.WinFrameMaterial.Code && a.WinFrameMaterial.Type == 9)
                    .Where(a => a.ID == 1)
                    .ToOne();

            Assert.NotNull(build);
        }

        [Table(Name = "F_Build2")]
        public class Build2
        {
            public int ID { get; set; }

            public string Name { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char BuildFunctionCode { get; set; }

            [Navigate(nameof(BuildFunctionCode))]
            public BuildDictionary BuildFunction { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char BuildStructureCode { get; set; }

            [Navigate(nameof(BuildStructureCode))]
            public BuildDictionary BuildStructure { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char AirTypeCode { get; set; }

            [Navigate(nameof(AirTypeCode))]
            public BuildDictionary AirType { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char HeatTypeCode { get; set; }

            [Navigate(nameof(HeatTypeCode))]
            public BuildDictionary HeatType { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char WallMaterialTypeCode { get; set; }

            [Navigate(nameof(WallMaterialTypeCode))]
            public BuildDictionary WallMaterialType { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char WallWarmTypeCode { get; set; }

            [Navigate(nameof(WallWarmTypeCode))]
            public BuildDictionary WallWarmType { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char WallWindowsTypeCode { get; set; }

            [Navigate(nameof(WallWindowsTypeCode))]
            public BuildDictionary WallWindowsType { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char GlassTypeCode { get; set; }

            [Navigate(nameof(GlassTypeCode))]
            public BuildDictionary GlassType { get; set; }

            [Column(MapType = typeof(string), DbType = "char")]
            public char WinFrameMaterialCode { get; set; }

            [Navigate(nameof(WinFrameMaterialCode))]
            public BuildDictionary WinFrameMaterial { get; set; }
        }

        [Table(Name = "F_BuildDictionary")]
        public class BuildDictionary
        {
            [Column(IsPrimary = true)]
            public int Type { get; set; }

            [Column(IsPrimary = true, MapType = typeof(string), DbType = "char")]
            public char Code { get; set; }

            [Column(StringLength = 20, IsNullable = false)]
            public string Name { get; set; }
        }
    }
}
