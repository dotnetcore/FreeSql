using System;

namespace FreeSql.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OracleUS7AsciiAttribute : Attribute
    {
        public OracleUS7AsciiAttribute() { }
        public OracleUS7AsciiAttribute(string encoding)
        {
            this.Encoding = encoding;
        }

        /// <summary>
        /// 编码
        /// </summary>
        public string Encoding { get; set; } = "GB2312";
    }
}
