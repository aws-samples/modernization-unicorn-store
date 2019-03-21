using System;
using System.ComponentModel.DataAnnotations;

namespace UnicornStore.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        public string CartId { get; set; }
        public int BlessingId { get; set; }
        public int Count { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; } 

        public virtual Blessing Blessing { get; set; }
    }
}