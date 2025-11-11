using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public enum AccountRole
    {
        Admin,
        Barber,
        User,
        Manager
    }

    public class Account
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(256)]
        [Column("lastname")]
        public string Lastname { get; set; }

        [Required, MaxLength(256)]
        [Column("restname")]
        public string Restname { get; set; }

        [MaxLength(256)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [Column("hash")]
        public byte[] Hash { get; set; }

        [Required]
        [Column("reg_time")]
        public DateTime RegTime { get; set; }

        [Column("profile_pic")]
        public byte[]? ProfilePic { get; set; }

        [Required]
        [Column("role")]
        public AccountRole Role { get; set; } = AccountRole.User;

        [MaxLength(256)]
        [Column("bio")]
        public string Bio { get; set; }

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        // Navigation
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}