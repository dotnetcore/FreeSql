#if netcore

using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql
{
    /// <summary>
    /// 树状基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    [Table(DisableSyncStructure = true)]
    public abstract class BaseEntityTree<TEntity, TKey> : BaseEntity<TEntity, TKey> where TEntity : class
    {
        /// <summary>
        /// 父级id
        /// </summary>
        public TKey ParentId
        {
            get => _ParentId;
            set
            {
                if (Equals(value, default(TKey)) == false && Equals(value, Id))
                    throw new ArgumentException("ParentId 值不能与 Id 相同");
                _ParentId = value;
            }
        }
        public TEntity Parent { get; set; }
        private TKey _ParentId;

        /// <summary>
        /// 下级列表
        /// </summary>
        [Navigate("ParentId")]
        public List<TEntity> Childs { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 名称：技术部-前端
        /// </summary>
        public string FullName { get; set; }

        public List<TEntity> GetAllChilds() => Select.WhereDynamic(this)
            .IncludeMany(a => (a as BaseEntityTree<TEntity, TKey>).Childs,
                t1 => t1.IncludeMany(a1 => (a1 as BaseEntityTree<TEntity, TKey>).Childs,
                t2 => t2.IncludeMany(a2 => (a2 as BaseEntityTree<TEntity, TKey>).Childs,
                t3 => t3.IncludeMany(a3 => (a3 as BaseEntityTree<TEntity, TKey>).Childs,
                t4 => t4.IncludeMany(a4 => (a4 as BaseEntityTree<TEntity, TKey>).Childs,
                t5 => t5.IncludeMany(a5 => (a5 as BaseEntityTree<TEntity, TKey>).Childs,
                t6 => t6.IncludeMany(a6 => (a6 as BaseEntityTree<TEntity, TKey>).Childs,
                t7 => t7.IncludeMany(a7 => (a7 as BaseEntityTree<TEntity, TKey>).Childs,
                t8 => t8.IncludeMany(a8 => (a8 as BaseEntityTree<TEntity, TKey>).Childs,
                t9 => t9.IncludeMany(a9 => (a9 as BaseEntityTree<TEntity, TKey>).Childs,
                t10 => t10.IncludeMany(a10 => (a10 as BaseEntityTree<TEntity, TKey>).Childs))))))))))).ToList()
                .SelectMany(a => (a as BaseEntityTree<TEntity, TKey>).Childs
                .SelectMany(a1 => (a1 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a2 => (a2 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a3 => (a3 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a4 => (a4 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a5 => (a5 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a6 => (a6 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a7 => (a7 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a8 => (a8 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a9 => (a9 as BaseEntityTree<TEntity, TKey>)?.Childs
                .SelectMany(a10 => (a10 as BaseEntityTree<TEntity, TKey>)?.Childs))))))))))).Where(a => a != null).ToList();

        protected void RefershFullName()
        {
            var buf = new List<TEntity>();
            buf.Add(this as TEntity);
            buf.AddRange(this.GetAllChilds());
            var repo = Orm.GetRepository<TEntity>();
            repo.UnitOfWork = UnitOfWork.Current.Value;
            buf = repo.Select.WhereDynamic(buf)
                .Include(a => ((((((((((a as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent
                    as BaseEntityTree<TEntity, TKey>).Parent).ToList(true);
            foreach (var item in buf)
            {
                var up = item as BaseEntityTree<TEntity, TKey>;
                up.Name = up.Name;
                var cur = up.Parent as BaseEntityTree<TEntity, TKey>;
                while (cur != null)
                {
                    up.Name = $"{cur.Name}-{up.Name}";
                    cur = cur.Parent as BaseEntityTree<TEntity, TKey>;
                }
            }
            repo.Update(buf);
        }

        T UpdateIsDelete<T>(bool value, Func<BaseRepository<TEntity>, List<TEntity>, T> func)
        {
            var childs = GetAllChilds();
            childs.Add(this as TEntity);
            var repo = Orm.GetRepository<TEntity>();
            repo.UnitOfWork = UnitOfWork.Current.Value;
            repo.Attach(childs);
            foreach (var item in childs)
                (item as BaseEntity).IsDeleted = false;
            return func(repo, childs);
        }
        public override bool Delete(bool physicalDelete = false) => UpdateIsDelete(true, (repo, chis) => repo.Update(chis)) > 0;
        async public override Task<bool> DeleteAsync(bool physicalDelete = false) => await UpdateIsDelete(true, (repo, chis) => repo.UpdateAsync(chis)) > 0;

        public override bool Restore() => UpdateIsDelete(false, (repo, chis) => repo.Update(chis)) > 0;
        async public override Task<bool> RestoreAsync() => await UpdateIsDelete(false, (repo, chis) => repo.UpdateAsync(chis)) > 0;

        public override TEntity Insert()
        {
            var ret = base.Insert();
            RefershFullName();
            return ret;
        }
        async public override Task<TEntity> InsertAsync()
        {
            var ret = await base.InsertAsync();
            RefershFullName();
            return ret;
        }
        public override bool Update()
        {
            var old = Find(this.Id) as BaseEntityTree<TEntity, TKey>;
            var ret = base.Update();
            if (old.Name != this.Name || Equals(old.ParentId, old.ParentId) == false) RefershFullName();
            return ret;
        }
        async public override Task<bool> UpdateAsync()
        {
            var old = Find(this.Id) as BaseEntityTree<TEntity, TKey>;
            var ret = await base.UpdateAsync();
            if (old.Name != this.Name || Equals(old.ParentId, old.ParentId) == false) RefershFullName();
            return ret;
        }
        public override TEntity Save()
        {
            var old = Find(this.Id) as BaseEntityTree<TEntity, TKey>;
            var ret = base.Save();
            if (old.Name != this.Name || Equals(old.ParentId, old.ParentId) == false) RefershFullName();
            return ret;
        }
        async public override Task<TEntity> SaveAsync()
        {
            var old = Find(this.Id) as BaseEntityTree<TEntity, TKey>;
            var ret = await base.SaveAsync();
            if (old.Name != this.Name || Equals(old.ParentId, old.ParentId) == false) RefershFullName();
            return ret;
        }
    }
}

#endif