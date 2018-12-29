using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FreeSql.Site.UI.Areas.Example.Controllers
{
    [Area("example")]
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}