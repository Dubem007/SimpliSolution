using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Domain.Entities
{
    [Table(nameof(UserProfileRole))]
    public class UserProfileRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid RoleId { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        #region Navigation properties
        public User User { get; set; }
        [ForeignKey(nameof(RoleId))]
        public UserRole UserRole { get; set; }
        #endregion
    }
}
