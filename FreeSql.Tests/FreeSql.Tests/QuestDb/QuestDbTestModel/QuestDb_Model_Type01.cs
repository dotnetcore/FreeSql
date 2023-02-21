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
    class QuestDb_Model_Type01
    {
        public string TestString { get; set; }

        public decimal? TestDecimal { get; set; }

        public short TestShort { get; set; }

        public int TestInt { get; set; }

        public long TestLong { get; set; }

        public double TestDouble { get; set; }

        [AutoSubtable(SubtableType.Day)] 
        public DateTime? TestTime { get; set; }

        public bool? TestBool { get; set; }
    }
}