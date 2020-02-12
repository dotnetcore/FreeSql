using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FreeSql.DataAnnotations;

namespace FreeSql.Extensions.EfCoreFluentApi
{
    public class EfCoreTableFluent<T>
    {
        TableFluent<T> _tf;
        internal EfCoreTableFluent(TableFluent<T> tf)
        {
            _tf = tf;
        }

        public EfCoreTableFluent<T> ToTable(string name)
        {
            _tf.Name(name);
            return this;
        }
        public EfCoreTableFluent<T> ToView(string name)
        {
            _tf.DisableSyncStructure(true);
            _tf.Name(name);
            return this;
        }

        public EfCoreColumnFluent Property<TProperty>(Expression<Func<T, TProperty>> property) => new EfCoreColumnFluent(_tf.Property(property));
        public EfCoreColumnFluent Property(string property) => new EfCoreColumnFluent(_tf.Property(property));

        public TableFluent<T> Help() => _tf;

        #region HasKey
        public EfCoreTableFluent<T> HasKey(Expression<Func<T, object>> key)
        {
            if (key?.Body == null) return this;
            var exp = key.Body;
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    _tf.Property((exp as MemberExpression).Member.Name).IsPrimary(true);
                    break;
                case ExpressionType.New:
                    foreach (var member in (exp as NewExpression).Members)
                        _tf.Property(member.Name).IsPrimary(true);
                    break;
            }
            return this;
        }
        #endregion

        #region HasIndex
        public HasIndexFluent HasIndex(Expression<Func<T, object>> index)
        {
            if (index?.Body == null) throw new ArgumentException("参数错误 index 不能为 null");
            var exp = index.Body;

            var indexName = $"idx_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var columns = new List<string>();
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    columns.Add((exp as MemberExpression).Member.Name);
                    break;
                case ExpressionType.New:
                    foreach (var member in (exp as NewExpression).Members)
                        columns.Add(member.Name);
                    break;
            }
            _tf.Index(indexName, string.Join(", ", columns), false);
            return new HasIndexFluent(_tf, indexName, columns);
        }
        public class HasIndexFluent
        {
            TableFluent<T> _modelBuilder;
            string _indexName;
            List<string> _columns;
            bool _isUnique;

            internal HasIndexFluent(TableFluent<T> modelBuilder, string indexName, List<string> columns)
            {
                _modelBuilder = modelBuilder;
                _indexName = indexName;
                _columns = columns;
            }
            public HasIndexFluent IsUnique()
            {
                _isUnique = true;
                _modelBuilder.Index(_indexName, string.Join(", ", _columns), _isUnique);
                return this;
            }
            public HasIndexFluent HasName(string name)
            {
                _modelBuilder.IndexRemove(_indexName);
                _indexName = name;
                _modelBuilder.Index(_indexName, string.Join(", ", _columns), _isUnique);
                return this;
            }
        }
        #endregion

        #region HasOne
        public HasOneFluent<T2> HasOne<T2>(Expression<Func<T, T2>> one)
        {
            if (one?.Body == null) throw new ArgumentException("参数错误 one 不能为 null");
            var exp = one.Body;

            var oneProperty = "";
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    oneProperty = (exp as MemberExpression).Member.Name;
                    break;
            }
            if (string.IsNullOrEmpty(oneProperty)) throw new ArgumentException("参数错误 one");
            return new HasOneFluent<T2>(_tf, oneProperty);
        }
        public class HasOneFluent<T2>
        {
            TableFluent<T> _tf;
            string _selfProperty;
            string _selfBind;
            string _withManyProperty;
            string _withOneProperty;
            string _withOneBind;

            internal HasOneFluent(TableFluent<T> modelBuilder, string oneProperty)
            {
                _tf = modelBuilder;
                _selfProperty = oneProperty;
            }
            public HasOneFluent<T2> WithMany<TMany>(Expression<Func<T2, TMany>> many)
            {
                if (many?.Body == null) throw new ArgumentException("参数错误 many 不能为 null");
                var exp = many.Body;

                switch (exp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        _withManyProperty = (exp as MemberExpression).Member.Name;
                        break;
                }
                if (string.IsNullOrEmpty(_withManyProperty)) throw new ArgumentException("参数错误 many");
                if (string.IsNullOrEmpty(_selfBind) == false)
                    _tf.ConfigEntity<T2>(eb2 => eb2.Navigate(_withManyProperty, _selfBind));
                return this;
            }
            public HasOneFluent<T2> WithOne(Expression<Func<T2, T>> one, Expression<Func<T2, object>> foreignKey)
            {
                if (one?.Body == null) throw new ArgumentException("参数错误 one 不能为 null");
                var exp = one.Body;

                switch (exp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        _withOneProperty = (exp as MemberExpression).Member.Name;
                        break;
                }
                if (string.IsNullOrEmpty(_withOneProperty)) throw new ArgumentException("参数错误 one");

                if (foreignKey?.Body == null) throw new ArgumentException("参数错误 foreignKey 不能为 null");
                exp = foreignKey.Body;

                switch (exp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        _withOneBind = (exp as MemberExpression).Member.Name;
                        _withOneBind = _withOneBind.TrimStart(',', ' ');
                        break;
                    case ExpressionType.New:
                        _withOneBind = "";
                        foreach (var member in (exp as NewExpression).Members)
                            _withOneBind += ", " + member.Name;
                        _withOneBind = _withOneBind.TrimStart(',', ' ');
                        break;
                }
                if (string.IsNullOrEmpty(_withOneBind)) throw new ArgumentException("参数错误 foreignKey");
                if (string.IsNullOrEmpty(_selfBind) == false)
                    _tf.ConfigEntity<T2>(eb2 => eb2.Navigate(_withOneProperty, _withOneBind));
                return this;
            }
            public HasOneFluent<T2> HasForeignKey(Expression<Func<T, object>> foreignKey)
            {
                if (foreignKey?.Body == null) throw new ArgumentException("参数错误 foreignKey 不能为 null");
                var exp = foreignKey.Body;

                switch (exp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        _selfBind = (exp as MemberExpression).Member.Name;
                        _selfBind = _selfBind.TrimStart(',', ' ');
                        break;
                    case ExpressionType.New:
                        _selfBind = "";
                        foreach (var member in (exp as NewExpression).Members)
                            _selfBind += ", " + member.Name;
                        _selfBind = _selfBind.TrimStart(',', ' ');
                        break;
                }
                if (string.IsNullOrEmpty(_selfBind)) throw new ArgumentException("参数错误 foreignKey");
                _tf.Navigate(_selfProperty, _selfBind);
                if (string.IsNullOrEmpty(_withManyProperty) == false)
                    _tf.ConfigEntity<T2>(eb2 => eb2.Navigate(_withManyProperty, _selfBind));
                if (string.IsNullOrEmpty(_withOneProperty) == false && string.IsNullOrEmpty(_withOneBind) == false)
                    _tf.ConfigEntity<T2>(eb2 => eb2.Navigate(_withOneProperty, _withOneBind));
                return this;
            }
        }
        #endregion

        #region HasMany
        public HasManyFluent<T2> HasMany<T2>(Expression<Func<T, IEnumerable<T2>>> many)
        {
            if (many?.Body == null) throw new ArgumentException("参数错误 many 不能为 null");
            var exp = many.Body;

            var manyProperty = "";
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    manyProperty = (exp as MemberExpression).Member.Name;
                    break;
            }
            if (string.IsNullOrEmpty(manyProperty)) throw new ArgumentException("参数错误 many");
            return new HasManyFluent<T2>(_tf, manyProperty);
        }
        public class HasManyFluent<T2>
        {
            TableFluent<T> _tf;
            string _selfProperty;
            string _selfBind;
            string _withOneProperty;
            string _withManyProperty;

            internal HasManyFluent(TableFluent<T> modelBuilder, string manyProperty)
            {
                _tf = modelBuilder;
                _selfProperty = manyProperty;
            }

            public void WithMany(Expression<Func<T2, IEnumerable<T>>> many, Type middleType)
            {
                if (many?.Body == null) throw new ArgumentException("参数错误 many 不能为 null");
                var exp = many.Body;

                switch (exp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        _withManyProperty = (exp as MemberExpression).Member.Name;
                        break;
                }
                if (string.IsNullOrEmpty(_withManyProperty)) throw new ArgumentException("参数错误 many");

                _tf.Navigate(_selfProperty, null, middleType);
                _tf.ConfigEntity<T2>(eb2 => eb2.Navigate(_withManyProperty, null, middleType));
            }
            public HasManyFluent<T2> WithOne(Expression<Func<T2, T>> one)
            {
                if (one?.Body == null) throw new ArgumentException("参数错误 one 不能为 null");
                var exp = one.Body;

                switch (exp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        _withOneProperty = (exp as MemberExpression).Member.Name;
                        break;
                }
                if (string.IsNullOrEmpty(_withOneProperty)) throw new ArgumentException("参数错误 one");
                
                if (string.IsNullOrEmpty(_selfBind) == false)
                    _tf.ConfigEntity<T2>(eb2 => eb2.Navigate(_withOneProperty, _selfBind));
                return this;
            }
            public HasManyFluent<T2> HasForeignKey(Expression<Func<T, object>> foreignKey)
            {
                if (foreignKey?.Body == null) throw new ArgumentException("参数错误 foreignKey 不能为 null");
                var exp = foreignKey.Body;

                switch (exp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        _selfBind = (exp as MemberExpression).Member.Name;
                        _selfBind = _selfBind.TrimStart(',', ' ');
                        break;
                    case ExpressionType.New:
                        _selfBind = "";
                        foreach (var member in (exp as NewExpression).Members)
                            _selfBind += ", " + member.Name;
                        _selfBind = _selfBind.TrimStart(',', ' ');
                        break;
                }
                if (string.IsNullOrEmpty(_selfBind)) throw new ArgumentException("参数错误 foreignKey");
                _tf.Navigate(_selfProperty, _selfBind);
                if (string.IsNullOrEmpty(_withOneProperty) == false)
                    _tf.ConfigEntity<T2>(eb2 => eb2.Navigate(_withOneProperty, _selfBind));
                return this;
            }
        }
        #endregion

        public EfCoreTableFluent<T> Ignore<TProperty>(Expression<Func<T, TProperty>> property)
        {
            _tf.Property(property).IsIgnore(true);
            return this;
        }
        //public EfCoreTableFluent<T> HasData(T data) => HasData(new[] { data });
        //public EfCoreTableFluent<T> HasData(IEnumerable<T> data)
        //{
        //    return this;
        //}
    }
}
