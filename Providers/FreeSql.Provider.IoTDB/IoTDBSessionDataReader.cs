using Apache.IoTDB.Data;
using Apache.IoTDB.DataStructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;

namespace FreeSql.IoTDB
{
    internal sealed class IoTDBSessionDataReader : DbDataReader
    {
        private readonly SessionDataSet _dataSet;
        private readonly IoTDBConnection _connection;
        private readonly bool _closeConnection;
        private readonly List<string> _columnNames;
        private readonly IReadOnlyList<string> _serverNames;
        private readonly List<Type> _columnTypes;
        private object[] _currentValues;
        private bool _hasCurrent;
        private bool _prefetched;
        private bool _prefetchedHasRow;
        private bool _closed;

        public IoTDBSessionDataReader(SessionDataSet dataSet, IoTDBConnection connection, bool closeConnection)
        {
            _dataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
            _connection = connection;
            _closeConnection = closeConnection;
            var fullNames = dataSet.GetColumnNames()
                .Select((name, index) => string.IsNullOrWhiteSpace(name) ? "Column" + index.ToString(CultureInfo.InvariantCulture) : name)
                .ToList();
            _serverNames = fullNames;
            var leafNames = fullNames.Select(name => name.Substring(name.LastIndexOf('.') + 1)).ToList();
            var duplicateLeaves = new HashSet<string>(leafNames.GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1).Select(group => group.Key), StringComparer.OrdinalIgnoreCase);
            _columnNames = fullNames.Select((name, index) => duplicateLeaves.Contains(leafNames[index]) ? name : leafNames[index]).ToList();
            var typeNames = dataSet.GetColumnTypes();
            _columnTypes = _columnNames.Select((name, index) => GetColumnType(typeNames, index, name)).ToList();
        }

        public override int Depth => 0;
        public override int FieldCount => _columnNames.Count;
        public override bool HasRows
        {
            get
            {
                if (!_prefetched)
                {
                    _prefetchedHasRow = ReadNextRecord();
                    _prefetched = true;
                }
                return _prefetchedHasRow;
            }
        }
        public override bool IsClosed => _closed;
        public override int RecordsAffected => -1;
        public override object this[int ordinal] => GetValue(ordinal);
        public override object this[string name] => GetValue(GetOrdinal(name));

        public override bool Read()
        {
            if (_closed) return false;
            if (_prefetched)
            {
                _prefetched = false;
                _hasCurrent = _prefetchedHasRow;
                return _hasCurrent;
            }
            _hasCurrent = ReadNextRecord();
            return _hasCurrent;
        }

        public override bool NextResult() => false;
        public override string GetName(int ordinal) => _columnNames[ordinal];
        public override int GetOrdinal(string name)
        {
            for (var index = 0; index < _columnNames.Count; index++)
                if (_columnNames[index].Equals(name, StringComparison.OrdinalIgnoreCase)) return index;
            throw new IndexOutOfRangeException(name);
        }
        public override Type GetFieldType(int ordinal)
        {
            return _columnTypes[ordinal];
        }
        public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;
        public override object GetValue(int ordinal)
        {
            if (!_hasCurrent || ordinal < 0 || ordinal >= FieldCount) throw new InvalidOperationException("The reader is not positioned on a row.");
            var value = _currentValues[ordinal];
            if (value == null || value == DBNull.Value) return DBNull.Value;
            if (IsTimeColumn(_columnNames[ordinal]) && value is long milliseconds)
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
            return value;
        }
        public override int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, FieldCount);
            for (var index = 0; index < count; index++) values[index] = GetValue(index);
            return count;
        }
        public override bool IsDBNull(int ordinal) => GetValue(ordinal) == DBNull.Value;
        public override bool GetBoolean(int ordinal) => Convert.ToBoolean(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override byte GetByte(int ordinal) => Convert.ToByte(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override char GetChar(int ordinal) => Convert.ToChar(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override DateTime GetDateTime(int ordinal)
        {
            var value = GetValue(ordinal);
            if (value is DateTime dateTime) return dateTime;
            if (IsTimeColumn(_columnNames[ordinal])) return DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(value, CultureInfo.InvariantCulture)).UtcDateTime;
            return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }
        public override decimal GetDecimal(int ordinal) => Convert.ToDecimal(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override double GetDouble(int ordinal) => Convert.ToDouble(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override float GetFloat(int ordinal) => Convert.ToSingle(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override Guid GetGuid(int ordinal)
        {
            var value = GetValue(ordinal);
            return value is Guid guid ? guid : Guid.Parse(Convert.ToString(value, CultureInfo.InvariantCulture));
        }
        public override short GetInt16(int ordinal) => Convert.ToInt16(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override int GetInt32(int ordinal) => Convert.ToInt32(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override long GetInt64(int ordinal) => Convert.ToInt64(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override string GetString(int ordinal) => Convert.ToString(GetValue(ordinal), CultureInfo.InvariantCulture);
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            var bytes = (byte[])GetValue(ordinal);
            var available = Math.Max(0, Math.Min(length, bytes.Length - (int)dataOffset));
            if (buffer != null) Array.Copy(bytes, dataOffset, buffer, bufferOffset, available);
            return available;
        }
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            var chars = GetString(ordinal).ToCharArray();
            var available = Math.Max(0, Math.Min(length, chars.Length - (int)dataOffset));
            if (buffer != null) Array.Copy(chars, dataOffset, buffer, bufferOffset, available);
            return available;
        }
        public override DataTable GetSchemaTable()
        {
            var schema = new DataTable("SchemaTable");
            schema.Columns.Add("ColumnName", typeof(string));
            schema.Columns.Add("ColumnOrdinal", typeof(int));
            schema.Columns.Add("DataType", typeof(Type));
            for (var index = 0; index < FieldCount; index++)
                schema.Rows.Add(GetName(index), index, GetFieldType(index));
            return schema;
        }
        public override IEnumerator GetEnumerator() => new DbEnumerator(this, false);
        public override void Close() => CloseCore();

        protected override void Dispose(bool disposing)
        {
            if (disposing) CloseCore();
            base.Dispose(disposing);
        }

        private void CloseCore()
        {
            if (_closed) return;
            _closed = true;
            _dataSet.Dispose();
            if (_closeConnection) _connection.Close();
        }

        private bool ReadNextRecord()
        {
            if (!_dataSet.HasNextAsync().GetAwaiter().GetResult())
            {
                _currentValues = null;
                return false;
            }
            // RowRecord.Values omits null fields. Read by the server's exact
            // column names so nullable metadata and sparse series stay aligned.
            _currentValues = new object[_serverNames.Count];
            for (var i = 0; i < _serverNames.Count; i++)
                _currentValues[i] = i == 0 && IsTimeColumn(_serverNames[i])
                    ? (object)_dataSet.GetLong(_serverNames[i]) : _dataSet.GetObject(_serverNames[i]);
            return true;
        }

        private static bool IsTimeColumn(string name) =>
            name.Equals("time", StringComparison.OrdinalIgnoreCase) || name.Equals("timestamp", StringComparison.OrdinalIgnoreCase);

        private static Type GetColumnType(IReadOnlyList<string> typeNames, int index, string columnName)
        {
            if (IsTimeColumn(columnName)) return typeof(DateTime);
            if (index >= typeNames.Count) return typeof(object);
            switch (typeNames[index]?.ToUpperInvariant())
            {
                case "BOOLEAN": return typeof(bool);
                case "INT32": return typeof(int);
                case "INT64": return typeof(long);
                case "FLOAT": return typeof(float);
                case "DOUBLE": return typeof(double);
                case "TEXT":
                case "STRING": return typeof(string);
                case "BLOB": return typeof(byte[]);
                case "TIMESTAMP":
                case "DATE": return typeof(DateTime);
                default: return typeof(object);
            }
        }
    }
}
