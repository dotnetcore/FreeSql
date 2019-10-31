using System.Linq.Expressions;

namespace FreeSql.Internal.Model
{
    public class SelectTableInfo
    {
        public TableInfo Table { get; set; }

        private string _alias;
        public string Alias
        {
            get => _alias;
            set
            {
                _alias = value;
                if (string.IsNullOrEmpty(AliasInit)) AliasInit = value;
            }
        }
        public string AliasInit { get; set; }
        public string On { get; set; }
        public string NavigateCondition { get; set; }
        public ParameterExpression Parameter { get; set; }
        public SelectTableInfoType Type { get; set; }

        public string Cascade { get; set; }
    }
    public enum SelectTableInfoType { From, LeftJoin, InnerJoin, RightJoin, Parent }
}
