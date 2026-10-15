using SonnetDB.Sql;

namespace FreeSql.SonnetDB.Curd
{
    static class SonnetDBMutationSql
    {
        internal static string RewritePrimaryKeySubquery(string sql, string projection)
        {
            if (string.IsNullOrEmpty(sql) || string.IsNullOrEmpty(projection)) return sql;
            var tokens = SqlLexer.Tokenize(sql);
            for (var index = tokens.Count - 4; index >= 0; index--)
            {
                if (tokens[index].Kind != TokenKind.KeywordSelect ||
                    tokens[index + 1].Kind != TokenKind.Star ||
                    tokens[index + 2].Kind != TokenKind.KeywordFrom ||
                    tokens[index + 3].Kind != TokenKind.LeftParen) continue;
                // Only replace the projection token; literals, comments and
                // whitespace retain their exact text in UPDATE and DELETE.
                var position = tokens[index + 1].Position;
                sql = sql.Remove(position, 1).Insert(position, projection);
            }
            return sql;
        }
    }
}
