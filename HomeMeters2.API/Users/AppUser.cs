using HomeMeters2.API.Places;
using Microsoft.AspNetCore.Identity;

namespace HomeMeters2.API.Users;

public class AppUser : IdentityUser
{
    public Guid PublicId { get; set; }
    public ICollection<Place> Places { get; set; } = new List<Place>();
}