using FreeSql.Site.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Site.DAL
{
    public class DocumentTypeDAL
    {
        public long Insert(DocumentType model)
        {
            return Db.mysql.Insert<DocumentType>(model).ExecuteIdentity();
        }

        public bool Update(DocumentType model)
        {
            return Db.mysql.Update<DocumentType>(model.ID).ExecuteUpdated().Count > 0;
        }

        public bool Delete(long id)
        {
            return Db.mysql.Delete<DocumentType>(id).ExecuteDeleted().Count > 0;
        }

        public List<DocumentType> Query(Expression<Func<DocumentType, bool>> where,
            Expression<Func<DocumentType, DocumentType>> orderby = null)
        {
            var list = Db.mysql.Select<DocumentType>()
                .Where(where);
            if (orderby != null) list = list.OrderBy(b => b.CreateDt);
            return list.ToList();
        }
    }
}
