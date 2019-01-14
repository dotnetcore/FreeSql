//using FreeSql.Site.Entity;
using FreeSql.Site.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Site.DAL
{
    public class TemplateExampleDAL
    {
        /// <summary>
        /// 新增方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long Insert(TemplateExample model)
        {
            return DataBaseType.MySql.DB().Insert<TemplateExample>(model).ExecuteIdentity();
        }

        /// <summary>
        /// 修改方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(TemplateExample model)
        {
            return DataBaseType.MySql.DB().Update<TemplateExample>(model.ID).ExecuteUpdated().Count > 0;
        }

        /// <summary>
        /// 删除方法
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(long id)
        {
            return DataBaseType.MySql.DB().Delete<TemplateExample>(id).ExecuteDeleted().Count > 0;
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public TemplateExample GetByOne(Expression<Func<TemplateExample, bool>> where)
        {
            return DataBaseType.MySql.DB().Select<TemplateExample>()
                 .Where(where).ToOne();
        }

        /// <summary>
        /// 查询方法
        /// </summary>
        /// <param name="where"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        public List<TemplateExample> Query(Expression<Func<TemplateExample, bool>> where,
            Expression<Func<TemplateExample, TemplateExample>> orderby = null)
        {
            var list = DataBaseType.MySql.DB().Select<TemplateExample>()
                .Where(where);
            if (orderby != null) list = list.OrderBy(b => b.CreateDt);
            return list.ToList();
        }
    }
}
