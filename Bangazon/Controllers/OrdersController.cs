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

        // GET: Orders/Details/5
        public async Task<IActionResult> Details()
        {
            OrderDetailViewModel model = new OrderDetailViewModel();
            var currentUser = await GetCurrentUserAsync();

            Order order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(m => m.UserId == currentUser.Id.ToString() && m.PaymentTypeId == null);

            model.Order = order;

            model.LineItems = order
                .OrderProducts
                .GroupBy(op => op.Product)
                .Select(g => new OrderLineItem
                {
                    Product = g.Key,
                    Units = g.Select(l => l.ProductId).Count(),
                    Cost = g.Key.Price * g.Select(l => l.ProductId).Count()
                }).ToList()
                ;


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

       
        // POST: Orders/DeleteItem/5 
        //The following code deletes a single item from the cart. It does not add an item back into the
        //quantity of the table because the item is actually not removed from the database until the item is actually purchased. 
        //This delete is a separate method than the above which is left for deleting whole orders. 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItemFromCart(int prodId)
        {

            var currentUser = await GetCurrentUserAsync();
            Order order = await _context.Order
               .Include(o => o.PaymentType)
               .Include(o => o.User)
               .Include(o => o.OrderProducts)
               .ThenInclude(op => op.Product)
               .FirstOrDefaultAsync(m => m.UserId == currentUser.Id.ToString() && m.PaymentTypeId == null);

            OrderProduct orderProduct =  await _context.OrderProduct 
            .FirstOrDefaultAsync(op => op.OrderId == order.OrderId && op.ProductId == prodId);
            _context.OrderProduct.Remove(orderProduct);
             await _context.SaveChangesAsync(); 
            return RedirectToAction(nameof(Details));

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
