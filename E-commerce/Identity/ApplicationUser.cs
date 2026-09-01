using Microsoft.AspNetCore.Identity;

namespace E_commerce.Identity;

// Extends IdentityUser to provide custom user fields if needed in the future,
// while decoupling the Identity architecture from the framework's default class.
public class ApplicationUser : IdentityUser
{

}