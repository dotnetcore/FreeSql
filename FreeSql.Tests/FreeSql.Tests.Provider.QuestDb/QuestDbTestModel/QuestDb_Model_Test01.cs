using FreeSql.Provider.QuestDb.Subtable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace FreeSql.Tests.QuestDb.QuestDbTestModel
{
    [Index("Id_Index", nameof(Id), false)]
    class QuestDb_Model_Test01
    {
        public string Primarys { get; set; }

        [Column(DbType = "symbol",IsPrimary = true)] public string Id { get; set; }

        [Column(OldName = "Name")] public string NameUpdate { get; set; }

        public string NameInsert { get; set; } = "NameDefault";

        public double? Activos { get; set; }

        [AutoSubtable(SubtableType.Day)] public DateTime? CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public bool? IsCompra { get; set; }
    }
}