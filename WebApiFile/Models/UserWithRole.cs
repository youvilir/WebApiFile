using System.ComponentModel.DataAnnotations;
using WebApiFile.Enums;

namespace WebApiFile.Models
{
    public class UserWithRole
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public Role Role { get; set; }
    }
}
