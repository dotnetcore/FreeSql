using Newtonsoft.Json.Linq;
using HuaweiCloud.GaussDBTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Newtonsoft.Json
{
    public class GaussDBTypesConverter : JsonConverter
    {
        private static readonly Type typeof_BitArray = typeof(BitArray);

        private static readonly Type typeof_GaussDBPoint = typeof(GaussDBPoint);
        private static readonly Type typeof_GaussDBLine = typeof(GaussDBLine);
        private static readonly Type typeof_GaussDBLSeg = typeof(GaussDBLSeg);
        private static readonly Type typeof_GaussDBBox = typeof(GaussDBBox);
        private static readonly Type typeof_GaussDBPath = typeof(GaussDBPath);
        private static readonly Type typeof_GaussDBPolygon = typeof(GaussDBPolygon);
        private static readonly Type typeof_GaussDBCircle = typeof(GaussDBCircle);

        private static readonly Type typeof_Cidr = typeof((IPAddress, int));
        private static readonly Type typeof_IPAddress = typeof(IPAddress);
        private static readonly Type typeof_PhysicalAddress = typeof(PhysicalAddress);

        private static readonly Type typeof_String = typeof(string);

        private static readonly Type typeof_GaussDBRange_int = typeof(GaussDBRange<int>);
        private static readonly Type typeof_GaussDBRange_long = typeof(GaussDBRange<long>);
        private static readonly Type typeof_GaussDBRange_decimal = typeof(GaussDBRange<decimal>);
        private static readonly Type typeof_GaussDBRange_DateTime = typeof(GaussDBRange<DateTime>);
        public override bool CanConvert(Type objectType)
        {
            Type ctype = objectType.IsArray ? objectType.GetElementType() : objectType;
            var ctypeGenericType1 = ctype.GenericTypeArguments.FirstOrDefault();

            if (ctype == typeof_BitArray) return true;

            if (ctype == typeof_GaussDBPoint || ctypeGenericType1 == typeof_GaussDBPoint) return true;
            if (ctype == typeof_GaussDBLine || ctypeGenericType1 == typeof_GaussDBLine) return true;
            if (ctype == typeof_GaussDBLSeg || ctypeGenericType1 == typeof_GaussDBLSeg) return true;
            if (ctype == typeof_GaussDBBox || ctypeGenericType1 == typeof_GaussDBBox) return true;
            if (ctype == typeof_GaussDBPath || ctypeGenericType1 == typeof_GaussDBPath) return true;
            if (ctype == typeof_GaussDBPolygon || ctypeGenericType1 == typeof_GaussDBPolygon) return true;
            if (ctype == typeof_GaussDBCircle || ctypeGenericType1 == typeof_GaussDBCircle) return true;

            if (ctype == typeof_Cidr || ctypeGenericType1 == typeof_Cidr) return true;
            if (ctype == typeof_IPAddress) return true;
            if (ctype == typeof_PhysicalAddress) return true;

            if (ctype == typeof_GaussDBRange_int || ctypeGenericType1 == typeof_GaussDBRange_int) return true;
            if (ctype == typeof_GaussDBRange_long || ctypeGenericType1 == typeof_GaussDBRange_long) return true;
            if (ctype == typeof_GaussDBRange_decimal || ctypeGenericType1 == typeof_GaussDBRange_decimal) return true;
            if (ctype == typeof_GaussDBRange_DateTime || ctypeGenericType1 == typeof_GaussDBRange_DateTime) return true;

            return false;
        }

        private static readonly Regex GaussDBPointParseRegex = new Regex("\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\)");
        static GaussDBPoint GaussDBPointParse(string s)
        {
            Match match = GaussDBPointParseRegex.Match(s);
            if (!match.Success)
                throw new FormatException("Not a valid point: " + s);

            return new GaussDBPoint(double.Parse(match.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
        }
        static readonly Regex GaussDBLineRegex = new Regex(@"\{(-?\d+.?\d*),(-?\d+.?\d*),(-?\d+.?\d*)\}");
        static GaussDBLine GaussDBLineParse(string s)
        {
            var m = GaussDBLineRegex.Match(s);
            if (!m.Success)
                throw new FormatException("Not a valid line: " + s);
            return new GaussDBLine(
                double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
            );
        }
        static readonly Regex GaussDBLSegRegex = new Regex(@"\[\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)\]");
        static GaussDBLSeg GaussDBLSegParse(string s)
        {
            var m = GaussDBLSegRegex.Match(s);
            if (!m.Success)
            {
                throw new FormatException("Not a valid line: " + s);
            }
            return new GaussDBLSeg(
                double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
            );
        }
        static readonly Regex GaussDBBoxRegex = new Regex(@"\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)");
        static GaussDBBox GaussDBBoxParse(string s)
        {
            var m = GaussDBBoxRegex.Match(s);
            return new GaussDBBox(
                new GaussDBPoint(double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                                double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)),
                new GaussDBPoint(double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                                double.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat))
            );
        }
        static GaussDBPath GaussDBPathParse(string s)
        {
            var open = s[0] == '[' ? true : (s[0] == '(' ? false : throw new Exception("Invalid path string: " + s));
            Debug.Assert(s[s.Length - 1] == (open ? ']' : ')'));
            var result = new GaussDBPath(open);
            var i = 1;
            while (true)
            {
                var i2 = s.IndexOf(')', i);
                result.Add(GaussDBPointParse(s.Substring(i, i2 - i + 1)));
                if (s[i2 + 1] != ',')
                    break;
                i = i2 + 2;
            }
            return result;
        }
        static GaussDBPolygon GaussDBPolygonParse(string s)
        {
            var points = new List<GaussDBPoint>();
            var i = 1;
            while (true)
            {
                var i2 = s.IndexOf(')', i);
                points.Add(GaussDBPointParse(s.Substring(i, i2 - i + 1)));
                if (s[i2 + 1] != ',')
                    break;
                i = i2 + 2;
            }
            return new GaussDBPolygon(points);
        }
        static readonly Regex GaussDBCircleRegex = new Regex(@"<\((-?\d+.?\d*),(-?\d+.?\d*)\),(\d+.?\d*)>");
        static GaussDBCircle GaussDBCircleParse(string s)
        {
            var m = GaussDBCircleRegex.Match(s);
            if (!m.Success)
                throw new FormatException("Not a valid circle: " + s);

            return new GaussDBCircle(
                double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
            );
        }

        private object YieldJToken(Type ctype, JToken jt, int rank)
        {
            if (jt.Type == JTokenType.Null) return null;
            if (rank == 0)
            {
                var ctypeGenericType1 = ctype.GenericTypeArguments.FirstOrDefault();//ctype.Namespace == "System" && ctype.Name.StartsWith("Nullable`") ? ctype.GenericTypeArguments.FirstOrDefault() : null;
                if (ctype == typeof_BitArray) return jt.ToString().ToBitArray();

                if (ctype == typeof_GaussDBPoint || ctypeGenericType1 == typeof_GaussDBPoint) return GaussDBPointParse(jt.ToString());
                if (ctype == typeof_GaussDBLine || ctypeGenericType1 == typeof_GaussDBLine) return GaussDBLineParse(jt.ToString());
                if (ctype == typeof_GaussDBLSeg || ctypeGenericType1 == typeof_GaussDBLSeg) return GaussDBLSegParse(jt.ToString());
                if (ctype == typeof_GaussDBBox || ctypeGenericType1 == typeof_GaussDBBox) return GaussDBBoxParse(jt.ToString());
                if (ctype == typeof_GaussDBPath || ctypeGenericType1 == typeof_GaussDBPath) return GaussDBPathParse(jt.ToString());
                if (ctype == typeof_GaussDBPolygon || ctypeGenericType1 == typeof_GaussDBPolygon) return GaussDBPolygonParse(jt.ToString());
                if (ctype == typeof_GaussDBCircle || ctypeGenericType1 == typeof_GaussDBCircle) return GaussDBCircleParse(jt.ToString());

                if (ctype == typeof_Cidr || ctypeGenericType1 == typeof_Cidr)
                {
                    var cidrArgs = jt.ToString().Split(new[] { '/' }, 2);
                    return (IPAddress.Parse(cidrArgs.First()), cidrArgs.Length >= 2 ? int.TryParse(cidrArgs[1], out var tryCdirSubnet) ? tryCdirSubnet : 0 : 0);
                }
                if (ctype == typeof_IPAddress) return IPAddress.Parse(jt.ToString());
                if (ctype == typeof_PhysicalAddress) return PhysicalAddress.Parse(jt.ToString());

                if (ctype == typeof_GaussDBRange_int || ctypeGenericType1 == typeof_GaussDBRange_int) return jt.ToString().ToGaussDBRange<int>();
                if (ctype == typeof_GaussDBRange_long || ctypeGenericType1 == typeof_GaussDBRange_long) return jt.ToString().ToGaussDBRange<long>();
                if (ctype == typeof_GaussDBRange_decimal || ctypeGenericType1 == typeof_GaussDBRange_decimal) return jt.ToString().ToGaussDBRange<decimal>();
                if (ctype == typeof_GaussDBRange_DateTime || ctypeGenericType1 == typeof_GaussDBRange_DateTime) return jt.ToString().ToGaussDBRange<DateTime>();

                return null;
            }

            var jtarr = jt.ToArray();
            var ret = Array.CreateInstance(ctype, jtarr.Length);
            var jtarrIdx = 0;
            foreach (var a in jtarr)
            {
                var t2 = YieldJToken(ctype, a, rank - 1);
                ret.SetValue(t2, jtarrIdx++);
            }
            return ret;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int rank = objectType.IsArray ? objectType.GetArrayRank() : 0;
            Type ctype = objectType.IsArray ? objectType.GetElementType() : objectType;

            var ret = YieldJToken(ctype, JToken.Load(reader), rank);
            if (ret != null && rank > 0) return ret;
            return ret;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type objectType = value.GetType();
            if (objectType.IsArray)
            {
                int rank = objectType.GetArrayRank();
                int[] indices = new int[rank];
                GetJObject(value as Array, indices, 0).WriteTo(writer);
            }
            else
                GetJObject(value).WriteTo(writer);
        }
        public static JToken GetJObject(object value)
        {
            if (value is BitArray) return JToken.FromObject((value as BitArray)?.To1010());
            if (value is IPAddress) return JToken.FromObject((value as IPAddress)?.ToString());
            if (value is ValueTuple<IPAddress, int> || value is ValueTuple<IPAddress, int>?)
            {
                ValueTuple<IPAddress, int>? cidrValue = (ValueTuple<IPAddress, int>?)value;
                return JToken.FromObject(cidrValue == null ? null : $"{cidrValue.Value.Item1.ToString()}/{cidrValue.Value.Item2.ToString()}");
            }
            return JToken.FromObject(value?.ToString());
        }
        public static JToken GetJObject(Array value, int[] indices, int idx)
        {
            if (idx == indices.Length)
            {
                return GetJObject(value.GetValue(indices));
            }
            JArray ja = new JArray();
            if (indices.Length == 1)
            {
                foreach (object a in value)
                    ja.Add(GetJObject(a));
                return ja;
            }
            int lb = value.GetLowerBound(idx);
            int ub = value.GetUpperBound(idx);
            for (int b = lb; b <= ub; b++)
            {
                indices[idx] = b;
                ja.Add(GetJObject(value, indices, idx + 1));
            }
            return ja;
        }
    }
}