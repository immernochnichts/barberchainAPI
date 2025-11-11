using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class Barbershop
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required, MaxLength(256)]
        public string Address { get; set; }

        [Required, MaxLength(256)]
        public string Phone { get; set; }

        public int ReviewSum { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;

        [Required]
        public TimeSpan OpeningTime { get; set; } = new TimeSpan(9, 0, 0);

        [Required]
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(21, 0, 0);

        [Required]
        public int ManagerAccountId { get; set; }
        [ForeignKey(nameof(ManagerAccountId))]
        public Account Manager { get; set; }

        // Navigation
        public ICollection<Barber> Barbers { get; set; }
        public ICollection<ImageBshop> Images { get; set; }
    }
}