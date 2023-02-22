using System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Bogus.DataSets;
using Travel_Agency.Models;
using System.Data.SqlTypes;
using System.Web.SessionState;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core;
using System.Data.Entity;


namespace Travel_Agency.Controllers
{
    public class BookingsController : Controller
    {
        private TravelDbContext db = new TravelDbContext();

        public string Dep_Location { get; set; }
        public string Dest_Location { get; set; }
        public DateTime Dep_Date { get; set; }
        public DateTime Return_Date { get; set; }
        public async Task<ActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["PriceSortParm"] = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CountrySortParm"] = sortOrder == "Country" ? "country_desc" : "Country";
            ViewData["CurrentFilter"] = searchString;
            

            // select all flight from db.Flight where Dep_location, Dest_Location and Dep_Date match the form data
            var flights = from f in db.Flight
                          where f.Dep_location == Dep_Location && f.Dest_location == Dest_Location && f.Dep_date == Dep_Date
                          select f;

            int temp = Int32.Parse(searchString);

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

            // return the view with the filtered flights
            return View(await flights.ToListAsync());
        }

        public async Task<Dictionary<string, IEnumerable<Flight>>> SearchFlight()
        {

            Dictionary<string, IEnumerable<Flight>> flights = new Dictionary<string, IEnumerable<Flight>>();
            flights.Add("Departure", await FindFlights("Departure"));
            flights.Add("Return", await FindFlights());
            
            return flights;
        }

        public async Task<IEnumerable<Flight>> FindFlights(string flightType = null)
        {
            var flightList = await db.Flight.ToListAsync();

            IEnumerable<Flight> flights;

            if (flightType == "Departure")
            {
                flights = flightList.Where(f => f.Dep_location == this.Dep_Location && f.Dest_location == this.Dest_Location && f.Dep_date.ToString().Contains(this.Dep_Date.ToShortDateString()) && f.FlightDetails.Available_Seats > 0).ToList();
            }
            else
            {
                flights = flightList.Where(f => f.Dep_location == this.Dest_Location && f.Dest_location == this.Dep_Location && f.Dep_date.ToString().Contains(this.Return_Date.ToShortDateString()) && f.FlightDetails.Available_Seats > 0).ToList();
            }
            if (flights.Any())
            {
                return flights;
            }
            return null;

        }


        [HttpPost]
        public async Task<ActionResult> FlightResults(FormCollection form)
        {
            Dep_Location = form["Dep_Location"];
            Dest_Location = form["Dest_Location"];
            Dep_Date = DateTime.Parse(form["Dep_Date"]);

            // Check if return date is empty
            if (form["Return_Date"] != "")
            {
                Return_Date = DateTime.Parse(form["Return_Date"]);
            }

            var flights = await SearchFlight();
            if (flights != null)
            {
                return View("Index", flights);
            }
            else
            {
                ViewBag.Message = "No flights found";
                return View("Index");
            }
        }

        #region Book
        public Task<ActionResult> Book(string Departure, string Return = null)
        {

            // get the departure flight details from the database
            var departureFlightDetails = db.FlightDetails.FirstOrDefault(f => f.Flight_Number == Departure);
            TempData["DepartureFlightDetails"] = departureFlightDetails;

            // get the return flight details from the database, check if not null
            FlightDetails returnFlightDetails = null;
            if (Return != null)
            {
                returnFlightDetails = db.FlightDetails.FirstOrDefault(f => f.Flight_Number == Return);
                TempData["ReturnFlightDetails"] = returnFlightDetails;
            }

            ViewBag.Customer = new Customer();
            ViewBag.DepartureFlightDetails = departureFlightDetails;
            ViewBag.ReturnFlightDetails = returnFlightDetails;
            ViewBag.Bookings = new Booking();

            return Task.FromResult<ActionResult>(View());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Book(FormCollection form)
        {
            var numSeatsDep = form["DepartureFlightSeats"];
            var numSeatsRet = form["ReturnFlightSeats"];

            var seatsDep = int.Parse(numSeatsDep);
            // check if numSeatsRet is not null
            int seatsRet = 0;
            if (numSeatsRet != null)
            {
                seatsRet = int.Parse(numSeatsRet);
            }

            TempData["numSeatsDep"] = seatsDep;
            TempData["numSeatsRet"] = seatsRet;
            return RedirectToAction("Payment", "Bookings");

        }

        #endregion Book

        #region Payment
        public ActionResult Payment()
        {
            ViewBag.Bookings = new Booking();

            ViewBag.Customers = TempData["customer"];
            var numSeatsDep = (int)TempData["numSeatsDep"];
            var numSeatsRet = (int)TempData["numSeatsRet"];
            var departureFlightDetails = TempData["departureFlightDetails"] as FlightDetails;
            var returnFlightDetails = TempData["returnFlightDetails"] as FlightDetails;

            // check if numSeatsRet is not 0
            if (numSeatsRet != 0)
            {
                ViewBag.DepartureFlightDetails = departureFlightDetails;
                ViewBag.ReturnFlightDetails = returnFlightDetails;
                ViewBag.numSeatsDep = numSeatsDep;
                ViewBag.numSeatsRet = numSeatsRet;
                var depPrice = departureFlightDetails.Flight.Seat_Price * numSeatsDep;
                var retPrice = returnFlightDetails.Flight.Seat_Price * numSeatsRet;
                ViewBag.DeparturePrice = depPrice;
                ViewBag.ReturnPrice = retPrice;
                ViewBag.TotalPrice = depPrice + retPrice;
            }
            else
            {
                ViewBag.DepartureFlightDetails = departureFlightDetails;
                ViewBag.numSeatsDep = numSeatsDep;
                var depPrice = departureFlightDetails.Flight.Seat_Price * numSeatsDep;
                ViewBag.TotalPrice = depPrice;
                ViewBag.ReturnPrice = 0;
            }


            TempData.Keep("numSeatsDep");
            TempData.Keep("numSeatsRet");
            TempData.Keep("departureFlightDetails");
            TempData.Keep("returnFlightDetails");


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Payment([Bind(Include = "payer_id,payer_name,credit_card_number,cvv")] PaymentInfo paymentInfo)
        {

            if (ModelState.IsValid)
            {
                var departureFlightDetails = TempData["departureFlightDetails"] as FlightDetails;
                var returnFlightDetails = TempData["returnFlightDetails"] as FlightDetails;
                var numSeatsDep = (int)TempData["numSeatsDep"];
                var totalSeats = numSeatsDep;

                //check if TempData["numSeatsRet"] is not 0
                int numSeatsRet = 0;
                if (TempData["numSeatsRet"] != null)
                {
                    numSeatsRet = (int)TempData["numSeatsRet"];
                }

                var email = User.Identity.Name;

                PaymentInfo pi = new PaymentInfo();
                pi.payer_id = paymentInfo.payer_id;
                pi.payer_name = paymentInfo.payer_name;
                pi.credit_card_number = paymentInfo.credit_card_number;
                pi.c_email = email;
                pi.cvv = paymentInfo.cvv;
                db.PaymentInfo.Add(pi);

                Booking departureBK = new Booking();
                departureBK.c_Email = email;
                departureBK.Flight_Number = departureFlightDetails.Flight_Number;
                departureBK.Dep_date = departureFlightDetails.Dep_date;
                departureBK.b_Price = departureFlightDetails.Price * numSeatsDep;
                departureBK.numSeats = numSeatsDep;
                db.Booking.Add(departureBK);

                var depFlight = db.FlightDetails.FirstOrDefault(f => f.Flight_Number == departureFlightDetails.Flight_Number);
                if (depFlight != null) depFlight.Available_Seats -= numSeatsDep;

                if (returnFlightDetails != null)
                {
                    Booking returnBK = new Booking();
                    returnBK.c_Email = email;
                    returnBK.Flight_Number = returnFlightDetails.Flight_Number;
                    returnBK.Dep_date = returnFlightDetails.Dep_date;
                    returnBK.b_Price = returnFlightDetails.Price * numSeatsRet;
                    returnBK.numSeats = numSeatsRet;
                    db.Booking.Add(returnBK);

                    var returnFlight = db.FlightDetails.FirstOrDefault(f => f.Flight_Number == returnFlightDetails.Flight_Number);
                    if (returnFlight != null) returnFlight.Available_Seats -= numSeatsRet;
                }


                if (ModelState.IsValid)
                {
                    await db.SaveChangesAsync();
                }

                return RedirectToAction("ThankForBooking", "Bookings");

            }

            return View(paymentInfo);
        }

        #endregion Payment

        //ThankForBooking page
        public ActionResult ThankForBooking()
        {
            return View();
        }

        // GET: Bookings/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = await db.Booking.FindAsync(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            ViewBag.c_Email = new SelectList(db.Customer, "c_Email", "c_FirstName", booking.c_Email);
            ViewBag.Flight_Number = new SelectList(db.FlightDetails, "Flight_Number", "Flight_Number", booking.Flight_Number);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "c_Email,b_ID,b_Price,Flight_Number,Dep_date")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.c_Email = new SelectList(db.Customer, "c_Email", "c_FirstName", booking.c_Email);
            ViewBag.Flight_Number = new SelectList(db.FlightDetails, "Flight_Number", "Flight_Number", booking.Flight_Number);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = await db.Booking.FindAsync(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Booking booking = await db.Booking.FindAsync(id);
            db.Booking.Remove(booking);
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

    }
}
