using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bangazon.Models;
using Bangazon.Data;
using Microsoft.AspNetCore.Identity;
using Bangazon.Models.HomeIndexViewModel;

namespace Bangazon.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            List<Product> products = _context.Product //Gets all of the products from the database 
                                    .OrderByDescending(p => p.DateCreated) //Orders those products with the newest at the top
                                    .Take(20) //Gets the first 20
                                    .ToList(); //Turns that into a list of products and stores that in the variable 'products'

            IndexViewModel viewModel = new IndexViewModel(); //Creates a new view model that we will pass to our view

            viewModel.AllProducts = products; //Stores the new 20 products on the view model

            return View(viewModel); //Passes the view model with the 20 newest-added products to the view
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
