using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql.Site.DAL;
using FreeSql.Site.Entity;
using FreeSql.Site.UI.Controllers;
using FreeSql.Site.UI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeSql.Site.UI.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class DocumentsController : BaseController
    {
        public DocumentTypeDAL DocumentTypeDAL { get; set; }

        public DocumentContentDAL DocumentContentDAL { get; set; }

        public DocumentsController()
        {
            this.DocumentTypeDAL = new DocumentTypeDAL();
            this.DocumentContentDAL = new DocumentContentDAL();
        }

        // GET: Documents
        public IActionResult Index(int id = 1)
        {
            var typeList = DocumentTypeDAL.Query(d => d.ID != 0);
            var contentlist = DocumentContentDAL.Query(d => d.Status == 1).list;

            //适应两层结构即可
            var query = (from p in typeList
                         where p.UpID == null || p.UpID == 0
                         select new TreeData(p, typeList).AddChildrens(GetContentTreeData(p.ID, contentlist), (tid) => GetContentTreeData(tid, contentlist))).ToList();

            ViewBag.DocumentList = query;
            ViewBag.DocID = id;
            return View();
        }

        private List<TreeData> GetContentTreeData(int id, List<DocumentContent> contentlist)
        {
            return contentlist.Where(w => w.TypeID == id).Select(s => new TreeData
            {
                id = s.ID,
                text = s.DocTitle,
                datatype = 1
            }).ToList();
        }

        // GET: Documents/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.DocumentID = id;
            var doc = this.DocumentContentDAL.GetByOne(w => w.ID == id);
            ViewBag.DocumentInfo = doc;
            return this.PartialView();
        }

        // GET: Documents/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: Documents/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Documents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Documents/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Documents/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}