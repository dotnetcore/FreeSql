using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql.Site.Entity;
using FreeSql.Site.UI.Admin.Common;
using FreeSql.Site.UI.Areas.BBS.Models;
using FreeSql.Site.UI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FreeSql.Site.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TemplateController : AdminBaseController
    {
        public IActionResult Index()
        {
            DocumentContent model = new DocumentContent();
            return View(model);
        }

    }
}