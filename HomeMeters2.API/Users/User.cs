using Microsoft.AspNetCore.Identity;

namespace HomeMeters2.API.Users;

public class User : IdentityUser
{
    public Guid PublicId { get; set; }
}