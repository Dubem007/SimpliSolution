using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AuthService.Domain.Dtos;

namespace AuthService.Domain.Entities
{
    [Table(nameof(User))]
    public class User : IdentityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        [StringLength(maximumLength: 250)]
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        [Required]
        [StringLength(maximumLength: 250)]
        public string Surname { get; set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string Status { get; set; }

        [Required]
        public string CreatedBy { get; set; } = AppConstants.System;

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string ModifiedBy { get; set; } = null;

        public DateTime ModifiedOn { get; set; }
        public int Retries { get; set; }

        public bool IsActive { get; set; } = false;

       

    }
}
