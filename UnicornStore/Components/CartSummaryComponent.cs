using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UnicornStore.Models;

namespace UnicornStore.Components
{
    [ViewComponent(Name = "CartSummary")]
    public class CartSummaryComponent : ViewComponent
    {
        public CartSummaryComponent(UnicornStoreContext dbContext)
        {
            DbContext = dbContext;
        }

        private UnicornStoreContext DbContext { get; }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = ShoppingCart.GetCart(DbContext, HttpContext);
            
            var cartItems = await cart.GetCartItems();

            ViewBag.CartCount = cartItems.Sum(c => c.Count);
            ViewBag.CartSummary = string.Join("\n", cartItems.Select(c => c.Blessing.Title).Distinct());

            return View();
        }
    }
}