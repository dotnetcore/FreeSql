using Newtonsoft.Json.Linq;
using NpgsqlTypes;
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
    public class PostgreSQLTypesConverter : JsonConverter
    {
        private static readonly Type typeof_BitArray = typeof(BitArray);

        private static readonly Type typeof_NpgsqlPoint = typeof(NpgsqlPoint);
        private static readonly Type typeof_NpgsqlLine = typeof(NpgsqlLine);
        private static readonly Type typeof_NpgsqlLSeg = typeof(NpgsqlLSeg);
        private static readonly Type typeof_NpgsqlBox = typeof(NpgsqlBox);
        private static readonly Type typeof_NpgsqlPath = typeof(NpgsqlPath);
        private static readonly Type typeof_NpgsqlPolygon = typeof(NpgsqlPolygon);
        private static readonly Type typeof_NpgsqlCircle = typeof(NpgsqlCircle);

        private static readonly Type typeof_Cidr = typeof((IPAddress, int));
        private static readonly Type typeof_IPAddress = typeof(IPAddress);
        private static readonly Type typeof_PhysicalAddress = typeof(PhysicalAddress);

        private static readonly Type typeof_String = typeof(string);

        private static readonly Type typeof_NpgsqlRange_int = typeof(NpgsqlRange<int>);
        private static readonly Type typeof_NpgsqlRange_long = typeof(NpgsqlRange<long>);
        private static readonly Type typeof_NpgsqlRange_decimal = typeof(NpgsqlRange<decimal>);
        private static readonly Type typeof_NpgsqlRange_DateTime = typeof(NpgsqlRange<DateTime>);
        public override bool CanConvert(Type objectType)
        {
            Type ctype = objectType.IsArray ? objectType.GetElementType() : objectType;
            var ctypeGenericType1 = ctype.GenericTypeArguments.FirstOrDefault();

            if (ctype == typeof_BitArray) return true;

            if (ctype == typeof_NpgsqlPoint || ctypeGenericType1 == typeof_NpgsqlPoint) return true;
            if (ctype == typeof_NpgsqlLine || ctypeGenericType1 == typeof_NpgsqlLine) return true;
            if (ctype == typeof_NpgsqlLSeg || ctypeGenericType1 == typeof_NpgsqlLSeg) return true;
            if (ctype == typeof_NpgsqlBox || ctypeGenericType1 == typeof_NpgsqlBox) return true;
            if (ctype == typeof_NpgsqlPath || ctypeGenericType1 == typeof_NpgsqlPath) return true;
            if (ctype == typeof_NpgsqlPolygon || ctypeGenericType1 == typeof_NpgsqlPolygon) return true;
            if (ctype == typeof_NpgsqlCircle || ctypeGenericType1 == typeof_NpgsqlCircle) return true;

            if (ctype == typeof_Cidr || ctypeGenericType1 == typeof_Cidr) return true;
            if (ctype == typeof_IPAddress) return true;
            if (ctype == typeof_PhysicalAddress) return true;

            if (ctype == typeof_NpgsqlRange_int || ctypeGenericType1 == typeof_NpgsqlRange_int) return true;
            if (ctype == typeof_NpgsqlRange_long || ctypeGenericType1 == typeof_NpgsqlRange_long) return true;
            if (ctype == typeof_NpgsqlRange_decimal || ctypeGenericType1 == typeof_NpgsqlRange_decimal) return true;
            if (ctype == typeof_NpgsqlRange_DateTime || ctypeGenericType1 == typeof_NpgsqlRange_DateTime) return true;

            return false;
        }

        private static readonly Regex NpgsqlPointParseRegex = new Regex("\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\)");
        static NpgsqlPoint NpgsqlPointParse(string s)
        {
            Match match = NpgsqlPointParseRegex.Match(s);
            if (!match.Success)
                throw new FormatException("Not a valid point: " + s);

            return new NpgsqlPoint(double.Parse(match.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
        }
        static readonly Regex NpgsqlLineRegex = new Regex(@"\{(-?\d+.?\d*),(-?\d+.?\d*),(-?\d+.?\d*)\}");
        static NpgsqlLine NpgsqlLineParse(string s)
        {
            var m = NpgsqlLineRegex.Match(s);
            if (!m.Success)
                throw new FormatException("Not a valid line: " + s);
            return new NpgsqlLine(
                double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
            );
        }
        static readonly Regex NpgsqlLSegRegex = new Regex(@"\[\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)\]");
        static NpgsqlLSeg NpgsqlLSegParse(string s)
        {
            var m = NpgsqlLSegRegex.Match(s);
            if (!m.Success)
            {
                throw new FormatException("Not a valid line: " + s);
            }
            return new NpgsqlLSeg(
                double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
            );
        }
        static readonly Regex NpgsqlBoxRegex = new Regex(@"\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)");
        static NpgsqlBox NpgsqlBoxParse(string s)
        {
            var m = NpgsqlBoxRegex.Match(s);
            return new NpgsqlBox(
                new NpgsqlPoint(double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                                double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)),
                new NpgsqlPoint(double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                                double.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat))
            );
        }
        static NpgsqlPath NpgsqlPathParse(string s)
        {
            var open = s[0] == '[' ? true : (s[0] == '(' ? false : throw new Exception("Invalid path string: " + s));
            Debug.Assert(s[s.Length - 1] == (open ? ']' : ')'));
            var result = new NpgsqlPath(open);
            var i = 1;
            while (true)
            {
                var i2 = s.IndexOf(')', i);
                result.Add(NpgsqlPointParse(s.Substring(i, i2 - i + 1)));
                if (s[i2 + 1] != ',')
                    break;
                i = i2 + 2;
            }
            return result;
        }
        static NpgsqlPolygon NpgsqlPolygonParse(string s)
        {
            var points = new List<NpgsqlPoint>();
            var i = 1;
            while (true)
            {
                var i2 = s.IndexOf(')', i);
                points.Add(NpgsqlPointParse(s.Substring(i, i2 - i + 1)));
                if (s[i2 + 1] != ',')
                    break;
                i = i2 + 2;
            }
            return new NpgsqlPolygon(points);
        }
        static readonly Regex NpgsqlCircleRegex = new Regex(@"<\((-?\d+.?\d*),(-?\d+.?\d*)\),(\d+.?\d*)>");
        static NpgsqlCircle NpgsqlCircleParse(string s)
        {
            var m = NpgsqlCircleRegex.Match(s);
            if (!m.Success)
                throw new FormatException("Not a valid circle: " + s);

            return new NpgsqlCircle(
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

                if (ctype == typeof_NpgsqlPoint || ctypeGenericType1 == typeof_NpgsqlPoint) return NpgsqlPointParse(jt.ToString());
                if (ctype == typeof_NpgsqlLine || ctypeGenericType1 == typeof_NpgsqlLine) return NpgsqlLineParse(jt.ToString());
                if (ctype == typeof_NpgsqlLSeg || ctypeGenericType1 == typeof_NpgsqlLSeg) return NpgsqlLSegParse(jt.ToString());
                if (ctype == typeof_NpgsqlBox || ctypeGenericType1 == typeof_NpgsqlBox) return NpgsqlBoxParse(jt.ToString());
                if (ctype == typeof_NpgsqlPath || ctypeGenericType1 == typeof_NpgsqlPath) return NpgsqlPathParse(jt.ToString());
                if (ctype == typeof_NpgsqlPolygon || ctypeGenericType1 == typeof_NpgsqlPolygon) return NpgsqlPolygonParse(jt.ToString());
                if (ctype == typeof_NpgsqlCircle || ctypeGenericType1 == typeof_NpgsqlCircle) return NpgsqlCircleParse(jt.ToString());

                if (ctype == typeof_Cidr || ctypeGenericType1 == typeof_Cidr)
                {
                    var cidrArgs = jt.ToString().Split(new[] { '/' }, 2);
                    return (IPAddress.Parse(cidrArgs.First()), cidrArgs.Length >= 2 ? int.TryParse(cidrArgs[1], out var tryCdirSubnet) ? tryCdirSubnet : 0 : 0);
                }
                if (ctype == typeof_IPAddress) return IPAddress.Parse(jt.ToString());
                if (ctype == typeof_PhysicalAddress) return PhysicalAddress.Parse(jt.ToString());

                if (ctype == typeof_NpgsqlRange_int || ctypeGenericType1 == typeof_NpgsqlRange_int) return jt.ToString().ToNpgsqlRange<int>();
                if (ctype == typeof_NpgsqlRange_long || ctypeGenericType1 == typeof_NpgsqlRange_long) return jt.ToString().ToNpgsqlRange<long>();
                if (ctype == typeof_NpgsqlRange_decimal || ctypeGenericType1 == typeof_NpgsqlRange_decimal) return jt.ToString().ToNpgsqlRange<decimal>();
                if (ctype == typeof_NpgsqlRange_DateTime || ctypeGenericType1 == typeof_NpgsqlRange_DateTime) return jt.ToString().ToNpgsqlRange<DateTime>();

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