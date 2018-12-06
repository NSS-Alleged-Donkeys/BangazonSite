using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Bangazon.Models.OrderViewModels;

namespace Bangazon.Controllers
{
    public class OrdersController : Controller
    {
        // Create variable to represent database
        private readonly ApplicationDbContext _context;

        // David Taylor
        // Create variable to represent User Data
        private readonly UserManager<ApplicationUser> _userManager;

        // David Taylor
        // Create component to get current user from the _userManager variable
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // Pass in arguments from private varaibles to be used publicly
        public OrdersController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            // Get the current user
            var user = await GetCurrentUserAsync();

            var applicationDbContext = _context.Order.Include(o => o.PaymentType).Include(o => o.User).Where(o => o.UserId == user.Id);
            return View(await applicationDbContext.ToListAsync());
        }

            /*
                *Author: Madi Peper & David Taylor*
                This is the logic for the view cart
                1. Assign the 'model' to a new instance of the OrderDetailViewModel
                2. Assign the 'currentUser' to the response of the 'GetCurrentUserAysnc' method
                3. Assign the 'order' to the response of the _context
                    3a. _context is the DB, using dot notation to get into the Order table
                    3b. Include the PaymentType, User, OrderProducts, and the OrderProduct.Product (.ThenIncludes) in the response
                    3c. Return the first one that matches the criteria: Order.UserId == the logged in User's Id && the Order.PaymentTypeId == null
                4. Assign the ViewModels' Order property to the response of the 'order'
                5. Assign the ViewModels' LineItems property to the response of the 'order'
                6. Go into the OrderProducts List on the 'order'
                7. GroupBy the Products inside of the OrderProducts List
                    --GroupBy is grouping the objects by a similar property, that creates a Key and a list of the grouped properties
                        --The Key is the ONE property that you wanted to GroupBy
                        --The list is all of the properties that met the GroupBy criteria
                8. Using the Select:
                    8a. for each Product (g = grouped products), create a new OrderLineItem
                    8b. Assign the whole Product (Key) to the OrderLineItem.Product
                    8c. Assign the number of total ProductIds in the List (g) to the OrderLineItem.Units
                    8d. Assign the number of total ProductIds in the List (g) MULTIPLIED by the Product.Price (Key.Price) to the OrderLineItem.Cost
                9. Make sure the above steps are put into a list, because LineItems is of type List
                10. If the 'order' responds with 'null' then return the View of "Empty Cart"
            */
        // GET: Orders/Details/5
        public async Task<IActionResult> Details()
        {
            // 1.
            OrderDetailViewModel model = new OrderDetailViewModel();
            // 2.
            var currentUser = await GetCurrentUserAsync();

            // 3.                   3a.
            Order order = await _context.Order
                // 3b.
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                // 3c.
                .FirstOrDefaultAsync(m => m.UserId == currentUser.Id.ToString() && m.PaymentTypeId == null);
            
            // 4.
            model.Order = order;

            // 5.
            model.LineItems = order
                // 6.
                .OrderProducts
                // 7.
                .GroupBy(op => op.Product)
                // 8.    8a.
                .Select(g => new OrderLineItem
                {
                    // 8b.
                    Product = g.Key,
                    // 8c.
                    Units = g.Select(l => l.ProductId).Count(),
                    // 8d.
                    Cost = g.Key.Price * g.Select(l => l.ProductId).Count()
                // 9.
                }).ToList()
                ;

            // 10.
            if (order == null)
            {
                return View("EmptyCart");
            }
            return View(model);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }

        //POST: 
        [Authorize]
        public async Task<IActionResult> AddToCart(int id)
        {
            // Find the product requested
            Product productToAdd = await _context.Product.SingleOrDefaultAsync(p => p.ProductId == id);

            // Get the current user
            var user = await GetCurrentUserAsync();

            // See if the user has an open order
            var openOrder = await _context.Order.SingleOrDefaultAsync(o => o.User == user && o.PaymentTypeId == null);

            //Initiate the current order
            Order currentOrder;


            // If no order, create one, else add to existing order
            if (openOrder == null)
            {
                currentOrder = new Order();
                currentOrder.UserId = user.Id;
                currentOrder.PaymentTypeId = null;
                _context.Add(currentOrder);
                await _context.SaveChangesAsync();
            }

            else
            {
                currentOrder = openOrder;
            }

            //Create a new instance of the current product
            OrderProduct currentProduct = new OrderProduct();

            //Set the ProductId to equal the the id from the URL
            currentProduct.ProductId = id;

            //Set the OrderId to equal the Id of the current order
            currentProduct.OrderId = currentOrder.OrderId;

            //Adds current product to the database 
            _context.Add(currentProduct);

            //Saves database changes
            await _context.SaveChangesAsync();

            //Redirects to the index page for your orders
            return RedirectToAction("Details", "Orders");
        }
    }
}
