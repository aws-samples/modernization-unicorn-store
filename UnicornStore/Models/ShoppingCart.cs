using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace UnicornStore.Models
{
    public class ShoppingCart
    {
        private readonly UnicornStoreContext _dbContext;
        private readonly string _shoppingCartId;

        private ShoppingCart(UnicornStoreContext dbContext, string id)
        {
            _dbContext = dbContext;
            _shoppingCartId = id;
        }

        public static ShoppingCart GetCart(UnicornStoreContext db, HttpContext context) 
            => GetCart(db, GetCartId(context));

        public static ShoppingCart GetCart(UnicornStoreContext db, string cartId)
            => new ShoppingCart(db, cartId);

        public async Task AddToCart(Blessing blessing)
        {
            // Get the matching cart and blessing instances
            var cartItem = await _dbContext.CartItems.SingleOrDefaultAsync(
                c => c.CartId == _shoppingCartId
                && c.BlessingId == blessing.BlessingId);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new CartItem
                {
                    BlessingId = blessing.BlessingId,
                    CartId = _shoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };

                _dbContext.CartItems.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, then add one to the quantity
                cartItem.Count++;
            }
        }

        public int RemoveFromCart(int id)
        {
            // Get the cart
            var cartItem = _dbContext.CartItems.SingleOrDefault(
                cart => cart.CartId == _shoppingCartId
                && cart.CartItemId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    _dbContext.CartItems.Remove(cartItem);
                }
            }

            return itemCount;
        }

        public async Task EmptyCart()
        {
            var cartItems = await _dbContext
                .CartItems
                .Where(cart => cart.CartId == _shoppingCartId)
                .ToArrayAsync();

            _dbContext.CartItems.RemoveRange(cartItems);
        }

        public Task<List<CartItem>> GetCartItems()
        {
            return _dbContext
                .CartItems
                .Where(cart => cart.CartId == _shoppingCartId)
                .Include(c => c.Blessing)
                .ToListAsync();
        }
        
        public Task<List<string>> GetCartBlessingTitles()
        {
            return _dbContext
                .CartItems
                .Where(cart => cart.CartId == _shoppingCartId)
                .Select(c => c.Blessing.Title)
                .OrderBy(n => n)
                .ToListAsync();
        }

        public Task<int> GetCount()
        {
            // Get the count of each item in the cart and sum them up
            return _dbContext
                .CartItems
                .Where(c => c.CartId == _shoppingCartId)
                .Select(c => c.Count)
                .SumAsync();
        }

        public Task<decimal> GetTotal()
        {
            // Multiply blessing price by count of that blessing to get 
            // the current price for each of those blessings in the cart
            // sum all blessing price totals to get the cart total

            return _dbContext
                .CartItems
                .Where(c => c.CartId == _shoppingCartId)
                .Select(c => c.Blessing.Price * c.Count)
                .SumAsync();
        }

        public async Task<int> CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = await GetCartItems();

            // Iterate over the items in the cart, adding the order details for each
            foreach (var item in cartItems)
            {
                //var blessing = _db.Blessings.Find(item.BlessingId);
                var blessing = await _dbContext.Blessings.SingleAsync(a => a.BlessingId == item.BlessingId);

                var orderDetail = new OrderDetail
                {
                    BlessingId = item.BlessingId,
                    OrderId = order.OrderId,
                    UnitPrice = blessing.Price,
                    Quantity = item.Count,
                };

                // Set the order total of the shopping cart
                orderTotal += (item.Count * blessing.Price);

                _dbContext.OrderDetails.Add(orderDetail);
            }

            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Empty the shopping cart
            await EmptyCart();

            // Return the OrderId as the confirmation number
            return order.OrderId;
        }

        // We're using HttpContextBase to allow access to sessions.
        private static string GetCartId(HttpContext context)
        {
            var cartId = context.Session.GetString("Session");

            if (cartId == null)
            {
                //A GUID to hold the cartId. 
                cartId = Guid.NewGuid().ToString();

                // Send cart Id as a cookie to the client.
                context.Session.SetString("Session", cartId);
            }

            return cartId;
        }
    }
}