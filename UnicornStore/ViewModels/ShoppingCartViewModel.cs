using System.Collections.Generic;
using UnicornStore.Models;

namespace UnicornStore.ViewModels
{
    public class ShoppingCartViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}
