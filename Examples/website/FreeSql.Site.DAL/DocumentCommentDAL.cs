//using FreeSql.Site.Entity;
using FreeSql.Site.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Site.DAL
{
    public class DocumentCommentDAL
    {
        /// <summary>
        /// 新增方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long Insert(DocumentComment model)
        {
            return DataBaseType.MySql.DB().Insert<DocumentComment>(model).ExecuteIdentity();
        }

        /// <summary>
        /// 修改方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(DocumentComment model)
        {
            return DataBaseType.MySql.DB().Update<DocumentComment>(model.ID).ExecuteUpdated().Count > 0;
        }

        /// <summary>
        /// 删除方法
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(long id)
        {
            return DataBaseType.MySql.DB().Delete<DocumentComment>(id).ExecuteDeleted().Count > 0;
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public DocumentComment GetByOne(Expression<Func<DocumentComment, bool>> where)
        {
            return DataBaseType.MySql.DB().Select<DocumentComment>()
                 .Where(where).ToOne();
        }

        /// <summary>
        /// 查询方法
        /// </summary>
        /// <param name="where"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        public List<DocumentComment> Query(Expression<Func<DocumentComment, bool>> where,
            Expression<Func<DocumentComment, DocumentComment>> orderby = null)
        {
            var list = DataBaseType.MySql.DB().Select<DocumentComment>()
                .Where(where);
            if (orderby != null) list = list.OrderBy(b => b.CreateDt);
            return list.ToList();
        }
    }
}
