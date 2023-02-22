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
using System.Web.Security;

namespace Travel_Agency.Controllers
{
    public class CustomersController : Controller
    {
        private TravelDbContext db = new TravelDbContext();

        // GET: Customers
        public async Task<ActionResult> Index()
        {
            var booking = db.Booking.Where(x => x.c_Email == User.Identity.Name);
            var flightDetails = db.FlightDetails.Where(x => booking.Any(y => y.Flight_Number == x.Flight_Number));

            var flightDetailsWithPrice = flightDetails.Join(booking,
                flight => flight.Flight_Number,
                bk => bk.Flight_Number,
                (flight, bk) => new
                {
                    Flight = flight,
                    Price = bk.b_Price,
                    AvailableSeats = bk.numSeats
                }).ToList();

            ViewBag.FlightDetails = flightDetailsWithPrice;


            // loop thru flightDetailsWithPrice
            
            


            return View(flightDetailsWithPrice);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "c_Email,c_password")] Customer customer)
        {

            var user = db.Customer.Where(x => x.c_Email == customer.c_Email && x.c_password == customer.c_password).FirstOrDefault();
            if (user != null)
            {
                // Create a forms authentication ticket
                var ticket = new FormsAuthenticationTicket(
                    1,
                    customer.c_Email,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    true,
                    $"First Name: {customer.c_FirstName} Last Name: {customer.c_LastName}");

                // Encrypt the ticket and add it to a cookie
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName,
                    FormsAuthentication.Encrypt(ticket));
                Response.Cookies.Add(cookie);

                // Redirect the user to the requested page
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(customer);
            }

            return View(customer);
        }

        // create an async method to login silently once the user has been registered
        public ActionResult LoginSilently(Customer customer)
        {
            var user = db.Customer.Where(x => x.c_Email == customer.c_Email && x.c_password == customer.c_password).FirstOrDefault();
            var ticket = new FormsAuthenticationTicket(
                1,
                customer.c_Email,
                DateTime.Now,
                DateTime.Now.AddMinutes(30),
                true,
                $"First Name: {customer.c_FirstName} Last Name: {customer.c_LastName}");

            // Encrypt the ticket and add it to a cookie
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName,
                FormsAuthentication.Encrypt(ticket));
            Response.Cookies.Add(cookie);
            return RedirectToAction("Payment", "Bookings");
        }

        public ActionResult Register()
        {
            ViewBag.c_Email = new SelectList(db.Booking, "c_Email", "Flight_Number");
            return View("Register");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register([Bind(Include = "c_Email,c_ID,c_FirstName,c_LastName,c_password")] Customer customer, string flightNumber)
        {
            if (ModelState.IsValid)
            {
                db.Customer.Add(customer);
                await db.SaveChangesAsync();

                // once the customer is registered, log them in silently
                return LoginSilently(customer);

            }

            ViewBag.c_Email = new SelectList(db.Booking, "c_Email", "Flight_Number", customer.c_Email);
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = await db.Customer.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.c_Email = new SelectList(db.Booking, "c_Email", "Flight_Number", customer.c_Email);
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "c_Email,c_ID,c_FirstName,c_LastName,c_password")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.c_Email = new SelectList(db.Booking, "c_Email", "Flight_Number", customer.c_Email);
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = await db.Customer.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Customer customer = await db.Customer.FindAsync(id);
            db.Customer.Remove(customer);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            return;
        }
    }
}
