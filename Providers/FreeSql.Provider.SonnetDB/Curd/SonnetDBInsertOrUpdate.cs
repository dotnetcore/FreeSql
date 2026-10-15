using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using SonnetDB.Sql;

namespace FreeSql.SonnetDB.Curd
{
    /// <summary>将 FreeSql 的插入或更新操作映射到关系表的原子 ON CONFLICT。</summary>
    sealed class SonnetDBInsertOrUpdate<T1> : InsertOrUpdateProvider<T1> where T1 : class
    {
        public SonnetDBInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
            if (typeof(T1) != typeof(object) && typeof(T1) != typeof(Dictionary<string, object>))
                SonnetDBModel.EnsureTable(_table, "INSERT ... ON CONFLICT");
        }

        public override string ToSql()
        {
            SonnetDBModel.EnsureTable(_table, "INSERT ... ON CONFLICT");
            if (_sourceSql == null && _source?.Any() != true) return null;
            if (_tempPrimarys == null || _tempPrimarys.Length == 0)
                throw new InvalidOperationException("SonnetDB InsertOrUpdate 必须指定主键或唯一索引列作为冲突目标。");
            if (_table.VersionColumn != null && _updateSetDict.ContainsKey(_table.VersionColumn.Attribute.Name))
                throw new NotSupportedException("SonnetDB ROWVERSION 由数据库自动生成，InsertOrUpdate 不能为其赋值。");

            var parameters = new List<DbParameter>();
            if (_sourceSql != null)
            {
                var insert = CreateInsert(new List<T1> { (T1)_table.Type.CreateInstanceGetDefaultValue() }, false);
                var sql = insert.ToSql();
                var valuesIndex = sql.IndexOf(") VALUES", StringComparison.OrdinalIgnoreCase);
                if (valuesIndex < 0) throw new InvalidOperationException("SonnetDB 无法生成 InsertOrUpdate 的 INSERT 列清单。");
                _params = Array.Empty<DbParameter>();
                return sql.Substring(0, valuesIndex + 1) + "\r\n" + _sourceSql.TrimEnd().TrimEnd(';') + "\r\n" + BuildConflictClause();
            }

            var split = SplitSourceByIdentityValueIsNull(_source);
            var statements = new List<string>();
            foreach (var rows in split.Item1) statements.Add(BuildInsert(rows, false));
            foreach (var rows in split.Item2) statements.Add(BuildInsert(rows, true));
            _params = parameters.ToArray();
            return string.Join(";\r\n", statements.Where(a => !string.IsNullOrWhiteSpace(a)));

            string BuildInsert(List<T1> rows, bool generateIdentity)
            {
                var insert = CreateInsert(rows, generateIdentity);
                var sql = insert.ToSql();
                if (insert._params?.Length > 0) parameters.AddRange(insert._params);
                return generateIdentity || string.IsNullOrEmpty(sql) ? sql : sql + BuildConflictClause();
            }
        }

        InsertProvider<T1> CreateInsert(List<T1> rows, bool generateIdentity)
        {
            var insert = (InsertProvider<T1>)_orm.Insert<T1>()
                .AsTable(_tableRule).AsType(_table.Type)
                .WithConnection(_connection).WithTransaction(_transaction)
                .NoneParameter(true);
            insert._source = rows;
            insert._table = _table;
            if (!generateIdentity && _tempPrimarys == _table.Primarys) insert.InsertIdentity();
            return insert;
        }

        string BuildConflictClause()
        {
            var target = string.Join(", ", _tempPrimarys.Select(a => _commonUtils.QuoteSqlName(a.Attribute.Name)));
            var prefix = " ON CONFLICT (" + target + ") ";
            var columns = _table.ColumnsByPosition.Where(a => !a.Attribute.IsVersion &&
                (_updateSetDict.ContainsKey(a.Attribute.Name) ||
                 !_tempPrimarys.Contains(a) && a.Attribute.CanUpdate && !a.Attribute.IsIdentity &&
                 !_updateIgnore.ContainsKey(a.Attribute.Name))).ToArray();
            if (_doNothing || columns.Length == 0) return prefix + "DO NOTHING";

            return prefix + "DO UPDATE SET " + string.Join(", ", columns.Select(column =>
            {
                var name = _commonUtils.QuoteSqlName(column.Attribute.Name);
                if (!_updateSetDict.TryGetValue(column.Attribute.Name, out var value))
                    value = "EXCLUDED." + name;
                else
                    value = RewriteConflictReferences(value);
                return name + " = " + value;
            }));
        }

        static string RewriteConflictReferences(string sql)
        {
            var tokens = SqlLexer.Tokenize(sql);
            for (var index = tokens.Count - 2; index >= 0; index--)
            {
                var token = tokens[index];
                if (token.Kind != TokenKind.IdentifierLiteral || token.IsQuotedIdentifier ||
                    tokens[index + 1].Kind != TokenKind.Dot) continue;
                if (string.Equals(token.Text, "t1", StringComparison.OrdinalIgnoreCase))
                    sql = sql.Remove(token.Position, tokens[index + 1].Position + 1 - token.Position);
                else if (string.Equals(token.Text, "t2", StringComparison.OrdinalIgnoreCase))
                    sql = sql.Remove(token.Position, token.Text.Length).Insert(token.Position, "EXCLUDED");
            }
            return sql;
        }
    }
}
