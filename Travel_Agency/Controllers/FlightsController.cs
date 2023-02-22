using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Travel_Agency.Models;
using Bogus;
using Regex = System.Text.RegularExpressions.Regex;
using System.Data.Entity.Validation;

namespace Travel_Agency.Controllers
{
    public class FlightsController : Controller
    {
        private TravelDbContext db = new TravelDbContext();

        // GET: Flights
        public async Task<ActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["PriceSortParm"] = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CountrySortParm"] = sortOrder == "Country" ? "country_desc" : "Country";
            ViewData["CurrentFilter"] = searchString;


            // select all flight from db.Flight where Dep_location, Dest_Location and Dep_Date match the form data
            // create IQueryable for Flight

            var flights = db.Flight.Select(f => f);

            if (!String.IsNullOrEmpty(searchString))
            {
                flights = flights.Where(f => f.Dep_location.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "price_desc":
                    flights = flights.OrderByDescending(f => f.Seat_Price);
                    break;
                case "Country":
                    flights = flights.OrderBy(f => f.Dep_location);
                    break;
                case "country_desc":
                    flights = flights.OrderByDescending(f => f.Dep_location);
                    break;
                default:
                    flights = flights.OrderBy(f => f.Seat_Price);
                    break;
            }

            return View("Index", await flights.AsNoTracking().ToListAsync());
        }

        // GET: Flights/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flight flight = await db.Flight.FindAsync(id);
            if (flight == null)
            {
                return HttpNotFound();
            }
            return View(flight);
        }

        // GET: Flights/Create
        public ActionResult Create()
        {
            ViewBag.Flight_Number = new SelectList(db.FlightDetails, "Flight_Number", "Flight_Number");
            return View();
        }

        // POST: Flights/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include =
                "Flight_Number,f_ID,Dep_location,Dest_location,Dep_date,Arr_date,Max_seat,Seat_Price,Duration,c_Email")]
            Flight flight)
        {
            if (ModelState.IsValid)
            {
                db.Flight.Add(flight);
                await db.SaveChangesAsync();
                //Save a new FlightDetails record
                FlightDetails fd = new FlightDetails();
                fd.Flight_Number = flight.Flight_Number;
                fd.Dep_date = flight.Dep_date;
                fd.Arr_date = flight.Arr_date;
                fd.Price = flight.Seat_Price;
                fd.Available_Seats = flight.Max_seat;
                db.FlightDetails.Add(fd);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.Flight_Number =
                new SelectList(db.FlightDetails, "Flight_Number", "Flight_Number", flight.Flight_Number);
            return View(flight);
        }

        // GET: Flights/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flight flight = await db.Flight.FindAsync(id);
            if (flight == null)
            {
                return HttpNotFound();
            }
            ViewBag.Flight_Number = new SelectList(db.FlightDetails, "Flight_Number", "Flight_Number", flight.Flight_Number);
            return View(flight);
        }

        // POST: Flights/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Flight_Number,f_ID,Dep_location,Dest_location,Dep_date,Arr_date,Max_seat,Seat_Price,Duration,c_Email")] Flight flight)
        {
            if (ModelState.IsValid)
            {
                db.Entry(flight).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Flight_Number = new SelectList(db.FlightDetails, "Flight_Number", "Flight_Number", flight.Flight_Number);
            return View(flight);
        }

        // GET: Flights/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flight flight = await db.Flight.FindAsync(id);
            if (flight == null)
            {
                return HttpNotFound();
            }
            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Flight flight = await db.Flight.FindAsync(id);
            db.Flight.Remove(flight);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult GenerateRandomFlights()
        {
            var departureFlights = GenerateDepartureFlights();
            var returnFlights = GenerateReturnFlights(departureFlights);

            

            if (ModelState.IsValid)
            {
                db.Flight.AddRange(departureFlights);
                db.Flight.AddRange(returnFlights);
                // update the FlightDetails table with the new flights
                foreach (var flight in departureFlights)
                {
                    FlightDetails fd = new FlightDetails();
                    fd.Flight_Number = flight.Flight_Number;
                    fd.Dep_date = flight.Dep_date;
                    fd.Arr_date = flight.Arr_date;
                    fd.Price = flight.Seat_Price;
                    fd.Available_Seats = flight.Max_seat;
                    db.FlightDetails.Add(fd);
                }

                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // generate departure flights, return a list of Flight
        public List<Flight> GenerateDepartureFlights()
        {
            var seats = new[] { 100, 120, 150, 180, 200, 220 };
            var all = GetCountries();
            var departureFlights = new Faker<Flight>()
                // Flight_Number by using Regex
                .RuleFor(f => f.Flight_Number, f => $"{f.Random.String2(2, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")}{f.Random.Number(100, 999)}")
                .RuleFor(f => f.Dep_location, f => f.PickRandom(all))
                .RuleFor(f => f.Dest_location, f => f.PickRandom(all))
                .RuleFor(f => f.Dep_date, f => f.Date.Between(DateTime.Now, new DateTime(2025, 12, 31)))
                .RuleFor(f => f.Arr_date, (f, flight) => flight.Dep_date.Value.AddHours(f.Random.Int(1, 25)))
                .RuleFor(f => f.Max_seat, f => f.PickRandom(seats))
                .RuleFor(f => f.Seat_Price, f => f.Random.Int(100, 1000))
                .RuleFor(f => f.Duration, (f, flight) => (flight.Arr_date.Value - flight.Dep_date.Value).Hours)
                .Generate(30);
            return departureFlights;
        }


        public List<Flight> GenerateReturnFlights(List<Flight> departureFlights)
        {
            var seats = new[] { 100, 120, 150, 180, 200, 220 };
            var all = GetCountries();
            var returnFlights = new List<Flight>();
            foreach (var depFlight in departureFlights)
            {
                var returnFlightsForDeparture = new Faker<Flight>()
                    .RuleFor(f => f.Flight_Number,
                        f => $"{f.Random.String2(2, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")}{f.Random.Number(100, 999)}")
                    .RuleFor(f => f.Dep_location, f => depFlight.Dest_location)
                    .RuleFor(f => f.Dest_location, f => depFlight.Dep_location)
                    .RuleFor(f => f.Dep_date, f => depFlight.Arr_date.Value.AddDays(f.Random.Int(1, 7)))
                    .RuleFor(f => f.Arr_date, (f, flight) => flight.Dep_date.Value.AddHours(f.Random.Int(1, 25)))
                    .RuleFor(f => f.Max_seat, f => f.PickRandom(seats))
                    .RuleFor(f => f.Seat_Price, f => f.Random.Int(100, 1000))
                    .RuleFor(f => f.Duration, (f, flight) => (flight.Arr_date.Value - flight.Dep_date.Value).Hours);
                    
                returnFlights.AddRange(returnFlightsForDeparture.Generate(5));
            }
            return returnFlights;
        }





        public List<string> GetCountries()
        {
            List<string> europe = new List<string>()
            {
                "London", "Paris", "Rome", "Madrid", "Berlin", "Barcelona", "Vienna", "Prague", "Amsterdam", "Brussels",
                "Budapest", "Warsaw", "Bucharest", "Copenhagen", "Dublin", "Hamburg", "Milan", "Munich", "Lisbon",
                "Athens", "Stockholm", "Zurich", "Istanbul", "Vienna", "Budapest", "Warsaw", "Bucharest", "Copenhagen",
                "Dublin", "Hamburg", "Milan", "Munich", "Lisbon", "Athens", "Stockholm", "Zurich", "Istanbul"
            };
            List<string> asia = new List<string>()
            {
                "Tokyo", "Delhi", "Shanghai", "Sao Paulo", "Mexico City", "Cairo", "Mumbai", "Beijing", "Dhaka", "Osaka",
                "New York", "Karachi", "Buenos Aires", "Chongqing", "Istanbul", "Kolkata", "Manila", "Rio de Janeiro",
                "Tianjin", "Lagos", "Moscow", "Kinshasa", "Lahore", "Shenzhen", "Guangzhou", "Bangkok", "Jakarta",
                "Seoul", "Ho Chi Minh City", "Hong Kong", "Lima", "Chennai", "Nagoya", "Baghdad", "Tehran", "Taipei",
                "Bangalore", "Hyderabad", "Chengdu", "Ahmedabad", "Riyadh", "Singapore", "Surat", "Ankara", "Pune",
                "Kuala Lumpur", "Johannesburg", "Shenyang", "Dar es Salaam", "Harbin", "Xian", "Foshan", "Luanda",
                "Chittagong", "Hyderabad", "Chengdu", "Ahmedabad", "Riyadh", "Singapore", "Surat", "Ankara", "Pune",
                "Kuala Lumpur", "Johannesburg", "Shenyang", "Dar es Salaam", "Harbin", "Xian", "Foshan", "Luanda",
                "Chittagong"
            };
            List<string> middle_east = new List<string>()
            {
                "Tel-Aviv", "Dubai", "Abu Dhabi", "Riyadh", "Jeddah", "Cairo", "Istanbul", "Baku", "Muscat", "Amman",
                "Beirut", "Doha", "Kuwait City", "Bahrain", "Manama", "Baghdad", "Tehran", "Damascus"
            };
            List<string> north_america = new List<string>()
            {
                "New York", "Los Angeles", "Chicago", "Houston", "Philadelphia", "Phoenix", "San Antonio", "San Diego",
                "Dallas", "San Jose", "Austin", "Jacksonville", "San Francisco", "Indianapolis", "Columbus",
                "Fort Worth",
                "Charlotte", "Detroit", "El Paso", "Memphis", "Boston", "Seattle", "Denver", "Washington", "Nashville",
                "Baltimore", "Louisville", "Milwaukee", "Portland", "Las Vegas", "Oklahoma City", "Albuquerque",
                "Tucson", "Fresno", "Sacramento", "Long Beach", "Kansas City", "Mesa", "Virginia Beach", "Atlanta",
                "Colorado Springs", "Omaha", "Raleigh", "Miami", "Oakland", "Minneapolis", "Tulsa", "Cleveland",
                "Wichita", "Arlington", "New Orleans", "Bakersfield", "Tampa", "Honolulu", "Aurora", "Anaheim",
                "Santa Ana", "St. Louis", "Riverside", "Corpus Christi", "Pittsburgh", "Lexington", "Anchorage",
                "Stockton", "Cincinnati", "St. Paul", "Toledo", "Greensboro", "Newark", "Plano", "Henderson",
                "Lincoln", "Buffalo", "Jersey City", "Chula Vista", "Fort Wayne", "Orlando", "St. Petersburg",
                "Chandler", "Laredo", "Norfolk", "Durham", "Madison", "Lubbock", "Irvine", "Winston-Salem",
                "Glendale", "Garland", "Hialeah", "Reno", "Chesapeake", "Gilbert", "Baton Rouge", "Irving",
                "Scottsdale", "North Las Vegas", "Fremont", "Boise City", "Richmond", "San Bernardino", "Birmingham",
                "Spokane", "Rochester", "Des Moines", "Modesto", "Fayetteville", "Tacoma", "Oxnard", "Fontana",
                "Columbus", "Montgomery", "Moreno Valley", "Shreveport", "Aurora", "Yonkers", "Akron",
                "Huntington Beach",
                "Little Rock", "Augusta", "Amarillo", "Glendale", "Mobile", "Grand Rapids", "Salt Lake City",
                "Tallahassee",
                "Huntsville", "Grand Prairie", "Knoxville", "Worcester", "Newport News", "Brownsville", "Overland Park",
                "Santa Clarita", "Providence", "Garden Grove", "Chattanooga", "Oceanside", "Jackson", "Fort Lauderdale",
                "Santa Rosa", "Rancho Cucamonga", "Port St. Lucie", "Tempe", "Ontario", "Vancouver", "Sioux Falls",
                "Springfield", "Peoria", "Pembroke Pines", "Elk Grove", "Salem", "Lancaster", "Corona", "Eugene",
            };
            List<string> all = new List<string>();
            all.AddRange(europe);
            all.AddRange(asia);
            all.AddRange(middle_east);
            all.AddRange(north_america);

            return all;
        }
    }
}
