using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DbModels
{
    [Table("users")] 
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(45)]
        public string Username { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("password")] // SQL'deki kolon adını belirtiyoruz
        public string PasswordHash { get; set; }
    }
}
