using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class Barbershop
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("longitude")]
        public double Longitude { get; set; }

        [Required]
        [Column("latitude")]
        public double Latitude { get; set; }

        [Required, MaxLength(256)]
        [Column("address")]
        public string Address { get; set; }

        [Required, MaxLength(256)]
        [Column("phone")]
        public string Phone { get; set; }

        [Required]
        [Column("opening_time")]
        public TimeSpan OpeningTime { get; set; } = new TimeSpan(9, 0, 0);

        [Required]
        [Column("closing_time")]
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(21, 0, 0);

        [Required]
        [Column("fk_manager_account")]
        public int ManagerAccountId { get; set; }
        [ForeignKey(nameof(ManagerAccountId))]
        public Account Manager { get; set; }

        // Navigation
        public ICollection<Barber> Barbers { get; set; }
        public ICollection<ImageBshop> Images { get; set; }
    }
}