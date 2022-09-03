using FreeSql;
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Internal.Model
{
    public class AggregateRootTrackingChangeInfo
    {
        public List<NativeTuple<Type, object>> InsertLog { get; } = new List<NativeTuple<Type, object>>();
        public List<NativeTuple<Type, object, object, List<string>>> UpdateLog { get; } = new List<NativeTuple<Type, object, object, List<string>>>();
        public List<NativeTuple<Type, object[]>> DeleteLog { get; } = new List<NativeTuple<Type, object[]>>();
    }
}