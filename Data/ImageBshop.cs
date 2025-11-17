using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class ImageBshop
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("fk_bshop")]
        public int BshopId { get; set; }
        [ForeignKey(nameof(BshopId))]
        public Barbershop Bshop { get; set; }

        [Column("img_file")]
        public byte[] ImgFile { get; set; }
    }
}