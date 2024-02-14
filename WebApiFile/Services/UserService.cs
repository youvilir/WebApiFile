using WebApiFile.Models;

namespace WebApiFile.Services
{
    public class UserService
    {
        private readonly List<UserWithRole> _roles = new List<UserWithRole>() {
            new UserWithRole {
                Email = "User",
                Password = "User",
                Role = Enums.Role.User
            },
            new UserWithRole {
                Email = "Developer",
                Password = "Developer",
                Role = Enums.Role.Developer
            },
            new UserWithRole {
                Email = "Editor",
                Password = "Editor",
                Role = Enums.Role.Editor
            },
            new UserWithRole {
                Email = "Admin",
                Password = "Admin",
                Role = Enums.Role.Admin
            },
        };

        public UserWithRole? Authenticate(string email, string password)
        {

            var autenticatedUser = _roles.FirstOrDefault(x => x.Email == email && x.Password == password);

            if (autenticatedUser == null) return null;

            return autenticatedUser;
        }
    }
}
