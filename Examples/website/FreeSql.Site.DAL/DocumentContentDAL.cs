//using FreeSql.Site.Entity;
using FreeSql.Site.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Site.DAL
{
    public class DocumentContentDAL
    {
        public long Insert(DocumentContent model)
        {
            return Db.mysql.Insert<DocumentContent>(model).ExecuteIdentity();
        }

        public bool Update(DocumentContent model)
        {
            return Db.mysql.Update<DocumentContent>(model.ID).ExecuteUpdated().Count > 0;
        }

        public bool Delete(long id)
        {
            return Db.mysql.Delete<DocumentContent>(id).ExecuteDeleted().Count > 0;
        }

        public DocumentContent GetByOne(Expression<Func<DocumentContent, bool>> where)
        {
            return Db.mysql.Select<DocumentContent>()
                 .Where(where).ToOne();
        }

        public List<DocumentContent> Query(Expression<Func<DocumentContent, bool>> where,
            Expression<Func<DocumentContent, DocumentContent>> orderby = null)
        {
            var list = Db.mysql.Select<DocumentContent>()
                .Where(where);
            if (orderby != null) list = list.OrderBy(b => b.CreateDt);
            return list.ToList();
        }
    }
}
