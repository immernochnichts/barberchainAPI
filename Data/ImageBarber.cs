using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
{
    public class ImageBarber
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        public byte[] ImgFile { get; set; }
    }
}