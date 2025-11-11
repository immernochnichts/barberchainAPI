using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class ImageBshop
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BshopId { get; set; }
        [ForeignKey(nameof(BshopId))]
        public Barbershop Bshop { get; set; }

        public byte[] ImgFile { get; set; }
    }
}