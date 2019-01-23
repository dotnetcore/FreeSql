using FreeSql.Site.Entity;
using FreeSql.Site.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Site.DAL
{
    public class DocumentContentDAL
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long Insert(DocumentContent model)
        {
            return DataBaseType.MySql.DB().Insert<DocumentContent>(model).ExecuteIdentity();
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(DocumentContent model)
        {
            var runsql = DataBaseType.MySql.DB().Update<DocumentContent>().SetSource(model);
            return runsql.ExecuteAffrows() > 0;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(long id)
        {
            return DataBaseType.MySql.DB().Delete<DocumentContent>(id).ExecuteDeleted().Count > 0;
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public DocumentContent GetByOne(Expression<Func<DocumentContent, bool>> where)
        {
            return DataBaseType.MySql.DB().Select<DocumentContent>()
                 .Where(where).ToOne();
        }


        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public long Count(Expression<Func<DocumentContent, bool>> where)
        {
            return DataBaseType.MySql.DB().Select<DocumentContent>()
                 .Where(where).Count();
        }

        /// <summary>
        /// 查询功能
        /// </summary>
        /// <param name="where"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        public (List<DocumentContent> list, long count) Query(Expression<Func<DocumentContent, bool>> where,
            Expression<Func<DocumentContent, DocumentContent>> orderby = null, PageInfo pageInfo = null)
        {
            //设置查询条件
            var list = DataBaseType.MySql.DB().Select<DocumentContent>()
                .Where(where);

            //设置排序
            if (orderby != null) list = list.OrderBy(b => b.CreateDt);

            var count = list.Count();
            //设置分页操作
            if (pageInfo != null && pageInfo.IsPaging)
                list.Skip(pageInfo.PageIndex * pageInfo.PageSize).Limit(pageInfo.PageSize);

            //执行查询
            return (list.ToList(), count);
        }
    }
}
