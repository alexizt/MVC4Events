﻿using System.Web;
using System.Web.Mvc;

namespace mvc4events.Controllers
{
    public class HelloWorldController : Controller
    {
        //
        // GET: /HelloWorld/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /HelloWorld/Welcome
        public ActionResult Welcome(string name, int numTimes = 1)
        {
            ViewBag.Message = "Hello " + name;
            ViewBag.NumTimes = numTimes;
            return View();
        }

    }
}
