using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FreeSql.Internal.Model {
	class ReadAnonymousTypeInfo {
		public string CsName { get; set; }
		public ConstructorInfo Consturctor { get; set; }
		public ReadAnonymousTypeInfoConsturctorType ConsturctorType { get; set; }
		public List<ReadAnonymousTypeInfo> Childs = new List<ReadAnonymousTypeInfo>();
	}
	enum ReadAnonymousTypeInfoConsturctorType { Arguments, Properties }
}
