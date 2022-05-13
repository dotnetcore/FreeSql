using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1021
    {
        [Fact]
        public void PadLeft()
        {
            var fsql = g.sqlite;

            var list1 = fsql.Select<sport>()
                .OrderBy(t => t.OldId.PadLeft(4))
                .ToList(r => new sport { id = r.id, name = r.name, OldId = r.OldId.PadLeft(4) });
            /*
SELECT a."id" as1, a."name" as2, padl(a."OldId", 4) as3 
FROM "sport" a 
ORDER BY padl(a."OldId", 4)
             */
            var list2 = fsql.Select<sport>()
                .OrderBy(t => t.OldId.PadLeft(2,'0'))
                .ToList(r => new sport { id = r.id, name = r.name, OldId = r.OldId.PadLeft(2, '0') });
            /*
SELECT a."id" as1, a."name" as2, leftstr(REPLACE(padl(a."OldId", 2 ), ' ', '0'), 2-length(a."OldId"))||a."OldId" as3 
FROM "sport" a 
ORDER BY leftstr(REPLACE(padl(a."OldId", 2 ), ' ', '0'), 2-length(a."OldId"))||a."OldId"
             */
            var list3 = fsql.Select<sport>()
               .OrderBy(t => t.OldId.PadRight(4))
               .ToList(r => new sport { id = r.id, name = r.name, OldId = r.OldId.PadRight(4) });
            /*
            SELECT a."id" as1, a."name" as2, padr(a."OldId", 4) as3 
FROM "sport" a 
ORDER BY padr(a."OldId", 4)
             */
            var list4 = fsql.Select<sport>()
              .OrderBy(t => t.OldId.PadRight(2, '0'))
              .ToList(r => new sport { id = r.id, name = r.name, OldId = r.OldId.PadRight(2, '0') });
            /*
             
SELECT a."id" as1, a."name" as2, a."OldId"||rightstr(REPLACE(padr(a."OldId",2),' ','0'),CASE WHEN 2-length(a."OldId")<=0 THEN 0 ELSE 2-length(a."OldId")END) as3 
FROM "sport" a 
ORDER BY a."OldId"||rightstr(REPLACE(padr(a."OldId",2),' ','0'),CASE WHEN 2-length(a."OldId")<=0 THEN 0 ELSE 2-length(a."OldId")END)
             */
        }

        public class sport
        {
            public int id { get; set; }
            public string name { get; set; }
            public string OldId { get; set; }
        }

    }

}
