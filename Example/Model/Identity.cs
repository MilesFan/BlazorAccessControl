using BlazorAccessControl.Interface;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Example
{

    public class ApplicationUser: IdentityUser<string>, IUser
    {
        public ApplicationUser()
        {
            Id = Ulid.NewUlid().ToString();
            SecurityStamp = Ulid.NewUlid().ToString();
            ConcurrencyStamp = Ulid.NewUlid().ToString();
            UserName = string.Empty;

        }
        public string? DisplayName { get; set; }
        public virtual ICollection<ApplicationUserClaim> Claims { get; set; } = new List<ApplicationUserClaim>();
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = new List<ApplicationUserLogin>();
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; } = new List<ApplicationUserToken>();
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
        
        public ICollection<IRole> GetRoles()
        {
            return UserRoles.Select(i=>i.Role).Cast<IRole>().ToArray();
        }
        public void SetRoles(ICollection<IRole> roles)
        {
            UserRoles = roles
                .Cast<ApplicationRole>()
                .Select(i=>new ApplicationUserRole {
                    Role = i,
                    RoleId = i.Id,
                    User = this,
                    UserId = this.Id
                })
                .ToArray();
        }
    }

    public class ApplicationRole: IdentityRole<string>, IRole
    {
        public ApplicationRole()
        {
            Id = Ulid.NewUlid().ToString();
        }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new List<ApplicationRoleClaim>();
    }

    public class ApplicationUserRole: IdentityUserRole<string>
    {
        public virtual ApplicationUser User { get; set; } = default!;
        public virtual ApplicationRole Role { get; set; } = default!;
    }

    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser User { get; set; } = default!;
    }

    public class ApplicationUserLogin : IdentityUserLogin<string>
    {
        public virtual ApplicationUser User { get; set; } = default!;
    }

    public class ApplicationRoleClaim : IdentityRoleClaim<string>
    {
        public virtual ApplicationRole Role { get; set; } = default!;
    }

    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser User { get; set; } = default!;
    }
}
