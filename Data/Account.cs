using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourApp.Models
{
    public enum AccountRole
    {
        User,
        Barber,
        Manager,
        Admin
    }

    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(256)]
        public string Lastname { get; set; }

        [Required, MaxLength(256)]
        public string Restname { get; set; }

        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        public byte[] Hash { get; set; }

        [Required]
        public DateTime RegTime { get; set; }

        public byte[] ProfilePic { get; set; }

        [Required]
        public AccountRole Role { get; set; } = AccountRole.User;

        [MaxLength(256)]
        public string Bio { get; set; }

        public DateTime? BirthDate { get; set; }

        // Navigation
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}