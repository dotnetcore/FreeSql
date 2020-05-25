using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FreeSql.DataAnnotations;

namespace FreeSql.Extensions.EfCoreFluentApi
{
    public class EfCoreTableFluent
    {
        IFreeSql _fsql;
        TableFluent _tf;
        internal Type _entityType;
        internal EfCoreTableFluent(IFreeSql fsql, TableFluent tf, Type entityType)
        {
            _fsql = fsql;
            _tf = tf;
            _entityType = entityType;
        }

        public EfCoreTableFluent ToTable(string name)
        {
            _tf.Name(name);
            return this;
        }
        public EfCoreTableFluent ToView(string name)
        {
            _tf.DisableSyncStructure(true);
            _tf.Name(name);
            return this;
        }

        public EfCoreColumnFluent Property(string property) => new EfCoreColumnFluent(_tf.Property(property));

        /// <summary>
        /// 使用 FreeSql FluentApi 方法，当 EFCore FluentApi 方法无法表示的时候使用
        /// </summary>
        /// <returns></returns>
        public TableFluent Help() => _tf;

        #region HasKey
        public EfCoreTableFluent HasKey(string key)
        {
            if (key == null) throw new ArgumentException("参数错误 key 不能为 null");
            foreach (string name in key.Split(','))
            {
                if (string.IsNullOrEmpty(name.Trim())) continue;
                _tf.Property(name.Trim()).IsPrimary(true);
            }
            return this;
        }
        #endregion

        #region HasIndex
        public HasIndexFluent HasIndex(string index)
        {
            if (index == null) throw new ArgumentException("参数错误 index 不能为 null");
            var indexName = $"idx_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var columns = new List<string>();
            foreach (string name in index.Split(','))
            {
                if (string.IsNullOrEmpty(name.Trim())) continue;
                columns.Add(name.Trim());
            }
            _tf.Index(indexName, string.Join(", ", columns), false);
            return new HasIndexFluent(_tf, indexName, columns);
        }
        public class HasIndexFluent
        {
            TableFluent _modelBuilder;
            string _indexName;
            List<string> _columns;
            bool _isUnique;

            internal HasIndexFluent(TableFluent modelBuilder, string indexName, List<string> columns)
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
        public HasOneFluent HasOne(string one)
        {
            if (string.IsNullOrEmpty(one)) throw new ArgumentException("参数错误 one 不能为 null");
            if (_entityType.GetPropertiesDictIgnoreCase().TryGetValue(one, out var oneProperty) == false) throw new ArgumentException($"参数错误 {one} 属性不存在");
            return new HasOneFluent(_fsql, _tf, _entityType, oneProperty.PropertyType, one);
        }
        public class HasOneFluent
        {
            IFreeSql _fsql;
            TableFluent _tf;
            Type _entityType1;
            Type _entityType2;
            string _selfProperty;
            string _selfBind;
            string _withManyProperty;
            string _withOneProperty;
            string _withOneBind;

            internal HasOneFluent(IFreeSql fsql, TableFluent modelBuilder, Type entityType1, Type entityType2, string oneProperty)
            {
                _fsql = fsql;
                _tf = modelBuilder;
                _entityType1 = entityType1;
                _entityType2 = entityType2;
                _selfProperty = oneProperty;
            }
            public HasOneFluent WithMany(string many)
            {
                if (many == null) throw new ArgumentException("参数错误 many 不能为 null");
                if (_entityType2.GetPropertiesDictIgnoreCase().TryGetValue(many, out var manyProperty) == false) throw new ArgumentException($"参数错误 {many} 属性不存在");
                _withManyProperty = manyProperty.Name;
                if (string.IsNullOrEmpty(_selfBind) == false)
                    _fsql.CodeFirst.ConfigEntity(_entityType2, eb2 => eb2.Navigate(many, _selfBind));
                return this;
            }
            public HasOneFluent WithOne(string one, string foreignKey)
            {
                if (string.IsNullOrEmpty(one)) throw new ArgumentException("参数错误 one 不能为 null");
                if (_entityType1.GetPropertiesDictIgnoreCase().TryGetValue(one, out var oneProperty) == false) throw new ArgumentException($"参数错误 {one} 属性不存在");
                if (oneProperty != _entityType1) throw new ArgumentException($"参数错误 {one} 属性不存在");
                _withOneProperty = oneProperty.Name;

                if (foreignKey == null) throw new ArgumentException("参数错误 foreignKey 不能为 null");
                foreach (string name in foreignKey.Split(','))
                {
                    if (string.IsNullOrEmpty(name.Trim())) continue;
                    _withOneBind += ", " + name.Trim();
                }
                if (string.IsNullOrEmpty(_withOneBind)) throw new ArgumentException("参数错误 foreignKey");
                _withOneBind = _withOneBind.TrimStart(',', ' ');
                if (string.IsNullOrEmpty(_selfBind) == false)
                    _fsql.CodeFirst.ConfigEntity(_entityType2, eb2 => eb2.Navigate(_withOneProperty, _withOneBind));
                return this;
            }
            public HasOneFluent HasForeignKey(string foreignKey)
            {
                if (foreignKey == null) throw new ArgumentException("参数错误 foreignKey 不能为 null");
                foreach (string name in foreignKey.Split(','))
                {
                    if (string.IsNullOrEmpty(name.Trim())) continue;
                    _selfBind += ", " + name.Trim();
                }
                if (string.IsNullOrEmpty(_selfBind)) throw new ArgumentException("参数错误 foreignKey");
                _selfBind = _selfBind.TrimStart(',', ' ');
                _tf.Navigate(_selfProperty, _selfBind);
                if (string.IsNullOrEmpty(_withManyProperty) == false)
                    _fsql.CodeFirst.ConfigEntity(_entityType2, eb2 => eb2.Navigate(_withManyProperty, _selfBind));
                if (string.IsNullOrEmpty(_withOneProperty) == false && string.IsNullOrEmpty(_withOneBind) == false)
                    _fsql.CodeFirst.ConfigEntity(_entityType2, eb2 => eb2.Navigate(_withOneProperty, _withOneBind));
                return this;
            }
        }
        #endregion

        #region HasMany
        public HasManyFluent HasMany(string many)
        {
            if (string.IsNullOrEmpty(many)) throw new ArgumentException("参数错误 many 不能为 null");
            if (_entityType.GetPropertiesDictIgnoreCase().TryGetValue(many, out var manyProperty) == false) throw new ArgumentException($"参数错误 {many} 集合属性不存在");
            if (typeof(IEnumerable).IsAssignableFrom(manyProperty.PropertyType) == false || manyProperty.PropertyType.IsGenericType == false) throw new ArgumentException("参数错误 {many} 不是集合属性");
            return new HasManyFluent(_fsql, _tf, _entityType, manyProperty.PropertyType.GetGenericArguments()[0], manyProperty.Name);
        }
        public class HasManyFluent
        {
            IFreeSql _fsql;
            TableFluent _tf;
            Type _entityType1;
            Type _entityType2;
            string _selfProperty;
            string _selfBind;
            string _withOneProperty;
            string _withManyProperty;

            internal HasManyFluent(IFreeSql fsql, TableFluent modelBuilder, Type entityType1, Type entityType2, string manyProperty)
            {
                _fsql = fsql;
                _tf = modelBuilder;
                _entityType1 = entityType1;
                _entityType2 = entityType2;
                _selfProperty = manyProperty;
            }

            public void WithMany(string many, Type middleType)
            {
                if (string.IsNullOrEmpty(many)) throw new ArgumentException("参数错误 many 不能为 null");
                if (_entityType2.GetPropertiesDictIgnoreCase().TryGetValue(many, out var manyProperty) == false) throw new ArgumentException($"参数错误 {many} 集合属性不存在");
                if (typeof(IEnumerable).IsAssignableFrom(manyProperty.PropertyType) == false || manyProperty.PropertyType.IsGenericType == false) throw new ArgumentException("参数错误 {many} 不是集合属性");
                _withManyProperty = manyProperty.Name;
                _tf.Navigate(_selfProperty, null, middleType);
                _fsql.CodeFirst.ConfigEntity(_entityType2, eb2 => eb2.Navigate(_withManyProperty, null, middleType));
            }
            public HasManyFluent WithOne(string one)
            {
                if (string.IsNullOrEmpty(one)) throw new ArgumentException("参数错误 one 不能为 null");
                if (_entityType2.GetPropertiesDictIgnoreCase().TryGetValue(one, out var oneProperty) == false) throw new ArgumentException($"参数错误 {one} 属性不存在");
                if (oneProperty.PropertyType != _entityType1) throw new ArgumentException($"参数错误 {one} 属性不存在");
                _withOneProperty = oneProperty.Name;
                if (string.IsNullOrEmpty(_selfBind) == false)
                    _fsql.CodeFirst.ConfigEntity(_entityType2, eb2 => eb2.Navigate(oneProperty.Name, _selfBind));
                return this;
            }
            public HasManyFluent HasForeignKey(string foreignKey)
            {
                if (foreignKey == null) throw new ArgumentException("参数错误 foreignKey 不能为 null");
                foreach (string name in foreignKey.Split(','))
                {
                    if (string.IsNullOrEmpty(name.Trim())) continue;
                    _selfBind += ", " + name.Trim();
                }
                if (string.IsNullOrEmpty(_selfBind)) throw new ArgumentException("参数错误 foreignKey");
                _selfBind = _selfBind.TrimStart(',', ' ');
                _tf.Navigate(_selfProperty, _selfBind);
                if (string.IsNullOrEmpty(_withOneProperty) == false)
                    _fsql.CodeFirst.ConfigEntity(_entityType2, eb2 => eb2.Navigate(_withOneProperty, _selfBind));
                return this;
            }
        }
        #endregion

        public EfCoreTableFluent Ignore(string property)
        {
            _tf.Property(property).IsIgnore(true);
            return this;
        }

        /// <summary>
        /// 使用 Repository + EnableAddOrUpdateNavigateList + NoneParameter 方式插入种子数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public EfCoreTableFluent HasData(IEnumerable<object> data)
        {
            if (data.Any() == false) return this;
            var sdCopy = data.Select(a => (object)a).ToList();
            var sdCopyLock = new object();
            _fsql.Aop.SyncStructureAfter += new EventHandler<Aop.SyncStructureAfterEventArgs>((s, e) =>
            {
                object[] sd = null;
                lock (sdCopyLock)
                    sd = sdCopy?.ToArray();
                if (sd == null || sd.Any() == false) return;
                foreach (var et in e.EntityTypes)
                {
                    if (et != _entityType) continue;
                    if (_fsql.Select<object>().AsType(et).Any()) continue;

                    var repo = _fsql.GetRepository<object>();
                    repo.DbContextOptions.EnableAddOrUpdateNavigateList = true;
                    repo.DbContextOptions.NoneParameter = true;
                    repo.AsType(et);
                    repo.Insert(sd);

                    lock (sdCopyLock)
                        sdCopy = null;
                }
            });
            return this;
        }
    }
}
