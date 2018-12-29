using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql.Site.UI.Areas.BBS.Models;
using FreeSql.Site.UI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FreeSql.Site.UI.Areas.BBS.Controllers
{
    [Area("BBS")]
    public class BBSContentController : BaseController
    {
        public IActionResult Index()
        {
            BBSContentModel model = new BBSContentModel();
            return View(model);
        }

        public IActionResult Ask() {
            BBSContentModel model = new BBSContentModel();
            return View(model);
        }

    }
}