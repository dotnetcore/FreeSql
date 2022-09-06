using System;
using System.Collections.Generic;

namespace FreeSql.Internal.Model
{

    public class AggregateRootTrackingChangeInfo
    {

        public List<NativeTuple<Type, object>> InsertLog { get; } = new List<NativeTuple<Type, object>>();

        public List<NativeTuple<Type, object, object, List<string>>> UpdateLog { get; } = new List<NativeTuple<Type, object, object, List<string>>>();

        public List<NativeTuple<Type, object[]>> DeleteLog { get; } = new List<NativeTuple<Type, object[]>>();

    }
}