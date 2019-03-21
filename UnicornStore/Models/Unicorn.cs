using System.ComponentModel.DataAnnotations;

namespace UnicornStore.Models
{
    public class Unicorn
    {
        public int UnicornId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}