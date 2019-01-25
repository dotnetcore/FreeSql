//using FreeSql.Site.Entity;
using FreeSql.Site.Entity;
using FreeSql.Site.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Site.DAL
{
    public class BaseDAL<T> where T : BaseEntity
    {
        /// <summary>
        /// 新增方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual long Insert(T model)
        {
            return DataBaseType.MySql.DB().Insert<T>(model).ExecuteIdentity();
        }

        /// <summary>
        /// 修改方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual bool Update(T model)
        {
            var runsql = DataBaseType.MySql.DB().Update<T>().SetSource(model);
            return runsql.ExecuteAffrows() > 0;
        }

        /// <summary>
        /// 删除方法
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(long id)
        {
            return DataBaseType.MySql.DB().Delete<T>(id).ExecuteDeleted().Count > 0;
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual T GetByOne(Expression<Func<T, bool>> where)
        {
            return DataBaseType.MySql.DB().Select<T>()
                .Where(where).ToOne();
        }

        /// <summary>
        /// 查询方法
        /// </summary>
        /// <param name="where"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        public virtual (List<T> list, long count) Query(Expression<Func<T, bool>> where,
            Expression<Func<T, T>> orderby = null, PageInfo pageInfo = null)
        {
            //设置查询条件
            var list = DataBaseType.MySql.DB().Select<T>()
                .Where(where);

            BaseEntity baseEntity = new BaseEntity();
            //设置排序
            if (orderby != null) list = list.OrderBy(nameof(baseEntity.CreateDt) + " desc ");

            var count = list.Count();
            //设置分页操作
            if (pageInfo != null && pageInfo.IsPaging)
                list.Skip(pageInfo.PageIndex * pageInfo.PageSize).Limit(pageInfo.PageSize);

            //执行查询
            return (list.ToList(), count);
        }
    }
}
