using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Internal.Model
{
    public static class NativeTuple
    {
        public static NativeTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => new NativeTuple<T1, T2>(item1, item2);
        public static NativeTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => new NativeTuple<T1, T2, T3>(item1, item2, item3);
        public static NativeTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) => new NativeTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        public static NativeTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) => new NativeTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        public static NativeTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) => new NativeTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        public static NativeTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) => new NativeTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        public static NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8) => new NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, item8);
        public static NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9) => new NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, item8, item9);
        public static NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10) => new NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10);
    }

    public class NativeTuple<T1, T2>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public NativeTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
    public class NativeTuple<T1, T2, T3>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public NativeTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }
    public class NativeTuple<T1, T2, T3, T4>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public NativeTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
    }
    public class NativeTuple<T1, T2, T3, T4, T5>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public NativeTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
    }
    public class NativeTuple<T1, T2, T3, T4, T5, T6>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
        public NativeTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }
    }
    public class NativeTuple<T1, T2, T3, T4, T5, T6, T7>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
        public T7 Item7 { get; }
        public NativeTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }
    }
    public class NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
        public T7 Item7 { get; }
        public T8 Item8 { get; }
        public NativeTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
        }
    }
    public class NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
        public T7 Item7 { get; }
        public T8 Item8 { get; }
        public T9 Item9 { get; }
        public NativeTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
        }
    }
    public class NativeTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
        public T7 Item7 { get; }
        public T8 Item8 { get; }
        public T9 Item9 { get; }
        public T10 Item10 { get; }
        public NativeTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
        }
    }
}