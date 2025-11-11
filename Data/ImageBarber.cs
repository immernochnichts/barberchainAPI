using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class ImageBarber
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("fk_barber")]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        [Column("img_file")]
        public byte[] ImgFile { get; set; }
    }
}