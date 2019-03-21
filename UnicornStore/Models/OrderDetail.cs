using System.ComponentModel.DataAnnotations.Schema;

namespace UnicornStore.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }

        public int BlessingId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public virtual Blessing Blessing { get; set; }

        public virtual Order Order { get; set; }
    }
}