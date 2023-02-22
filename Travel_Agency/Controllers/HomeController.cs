using Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Travel_Agency.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faker;
using Bogus;
using System.Diagnostics;
using Microsoft.Ajax.Utilities;


namespace Travel_Agency.Controllers
{
    public class HomeController : Controller
    {

        TravelDbContext db = new TravelDbContext();

        public ActionResult Index()
        {
            ViewBag.From = db.Flight.Select(x => x.Dep_location).Distinct();

            ViewBag.To = db.Flight.Select(x => x.Dest_location).Distinct();

            return View();
        }

    }
}