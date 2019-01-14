using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FreeSql.Site.DAL;
using FreeSql.Site.Entity;
using FreeSql.Site.Entity.Common;
using FreeSql.Site.UI.Admin.Common;
using FreeSql.Site.UI.Areas.BBS.Models;
using FreeSql.Site.UI.Common;
using FreeSql.Site.UI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeSql.Site.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DocumentController : AdminBaseController
    {
        public DocumentTypeDAL DocumentTypeDAL { get; set; }

        public DocumentContentDAL DocumentContentDAL { get; set; }

        public DocumentController()
        {
            this.DocumentTypeDAL = new DocumentTypeDAL();
            this.DocumentContentDAL = new DocumentContentDAL();
        }


        public IActionResult Index()
        {
            DocumentContent model = new DocumentContent();
            return View(model);
        }

        #region 文档分类
        public IActionResult DocType()
        {
            DocumentType model = new DocumentType();
            return View(model);
        }

        #endregion

        #region 文档内容
        public IActionResult DocContent()
        {
            DocumentContent model = new DocumentContent();
            return View(model);
        }

        [HttpGet]
        public IActionResult DocContentList(string searchContent, string seniorQueryJson, int page = 1, int limit = 10)
        {
            DocumentContent model = null;
            if (!string.IsNullOrWhiteSpace(seniorQueryJson))
            {
                model = Newtonsoft.Json.JsonConvert.DeserializeObject<DocumentContent>(seniorQueryJson);
            }
            Expression<Func<DocumentContent, bool>> predicate = i => 1 == 0;
            var searchPredicate = PredicateExtensions.True<DocumentContent>();
            if (model != null)
            {
                if (model.TypeID >= 0)
                    searchPredicate = searchPredicate.And(u => u.TypeID == model.TypeID);

                if (!string.IsNullOrEmpty(model.DocTitle))
                    searchPredicate = searchPredicate.And(u => u.DocTitle.IndexOf(model.DocTitle) != -1);
            }
            var contents = DocumentContentDAL.Query(searchPredicate);

            return Json(new DataPage<DocumentContent>
            {
                code = "0",
                msg = "",
                count = contents.count,
                data = contents.list
            });
        }

        // GET: Documents/Create
        public ActionResult DocContentCreate()
        {

            return View();
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocContentCreate(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocContentDelete(int id, IFormCollection collection)
        {
            bool flag = false;
            flag = DocumentContentDAL.Delete(id);
            return Json(flag);
        }

        #endregion 
    }
}