using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseService.Entities
{
    [Table("admin_api_users")]
    public class AdminApiUser
    {
        [Key]
        public string Username { get; set; }
        public string Password { get; set; }
    }
}