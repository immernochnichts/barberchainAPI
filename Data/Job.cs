using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class Job
    {
        public override bool Equals(object obj)
        => obj is Job j && j.Id == Id;

        public override int GetHashCode()
            => Id.GetHashCode();

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(256)]
        [Column("name")]
        public string Name { get; set; }

        [Required, MaxLength(2048)]
        [Column("descr")]
        public string Descr { get; set; }

        [Required]
        [Column("duration_atu")]
        public short DurationAtu { get; set; }

        [Required]
        [Column("default_price")]
        public double DefaultPrice { get; set; }

        [Column("color_css")]
        public string? ColorCSS { get; set; }
    }
}