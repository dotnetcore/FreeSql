using FreeSql.Internal;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace FreeSql.QuestDb.Curd
{

    class QuestDbInsertOrUpdate<T1> : Internal.CommonProvider.InsertOrUpdateProvider<T1> where T1 : class
    {
        public QuestDbInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override string ToSql()
        {
            var dbParams = new List<DbParameter>();
            if (_sourceSql != null)
            {
                var data = new List<T1>();
                data.Add((T1)_table.Type.CreateInstanceGetDefaultValue());
                var sql = getInsertSql(data, false, false);
                var sb = new StringBuilder();
                sb.Append(sql.Substring(0, sql.IndexOf(") VALUES")));
                sb.Append(") \r\n");
                WriteSourceSelectUnionAll(null, sb, null);
                sb.Append(sql.Substring(sql.IndexOf("\r\nON CONFLICT(") + 2));
                return sb.ToString();
            }
            if (_source?.Any() != true) return null;

            var sqls = new string[2];
            var ds = SplitSourceByIdentityValueIsNull(_source);
            if (ds.Item1.Any()) sqls[0] = string.Join("\r\n\r\n;\r\n\r\n", ds.Item1.Select(a => getInsertSql(a, false, true)));
            if (ds.Item2.Any()) sqls[1] = string.Join("\r\n\r\n;\r\n\r\n", ds.Item2.Select(a => getInsertSql(a, true, true)));
            _params = dbParams.ToArray();
            if (ds.Item2.Any() == false) return sqls[0];
            if (ds.Item1.Any() == false) return sqls[1];
            return string.Join("\r\n\r\n;\r\n\r\n", sqls);

            string getInsertSql(List<T1> data, bool flagInsert, bool noneParameter)
            {
                var insert = _orm.Insert<T1>()
                    .AsTable(_tableRule).AsType(_table.Type)
                    .WithConnection(_connection)
                    .WithTransaction(_transaction)
                    .NoneParameter(noneParameter) as Internal.CommonProvider.InsertProvider<T1>;
                insert._source = data;
                insert._table = _table;
                insert._noneParameterFlag = flagInsert ? "cuc" : "cu";

                string sql = "";
                if (IdentityColumn != null && flagInsert) sql = insert.ToSql();
                else
                {
                    var ocdu = new OnConflictDoUpdate<T1>(insert.InsertIdentity());
                    ocdu._tempPrimarys = _tempPrimarys;
                    var cols = _table.Columns.Values.Where(a => _tempPrimarys.Contains(a) == false && a.Attribute.CanUpdate == true && _updateIgnore.ContainsKey(a.Attribute.Name) == false);
                    ocdu.UpdateColumns(cols.Select(a => a.Attribute.Name).ToArray());
                    if (_doNothing == true || cols.Any() == false)
                        ocdu.DoNothing();
                    sql = ocdu.ToSql();
                }
                if (string.IsNullOrEmpty(sql)) return null;
                if (insert._params?.Any() == true) dbParams.AddRange(insert._params);
                return sql;
            }
        }
    }
}