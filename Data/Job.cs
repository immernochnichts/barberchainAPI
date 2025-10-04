using System.ComponentModel.DataAnnotations;

namespace YourApp.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(256)]
        public string Name { get; set; }

        [Required, MaxLength(2048)]
        public string Descr { get; set; }

        [Required]
        public short DurationAtu { get; set; }

        [Required]
        public double DefaultPrice { get; set; }
    }
}