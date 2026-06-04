using BlazorAccessControl.Interface;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Xml.Linq;

namespace BlazorAccessControl.EFCore
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
        public string? DisplayName {
            get
            {
                if (Claims is null || Claims.Count()==0) return UserName;
                var LanguageIETFTag = Thread.CurrentThread.CurrentCulture.IetfLanguageTag ?? "en";
                var displayname = Claims.FirstOrDefault(i => i.ClaimType == $"DisplayName-{LanguageIETFTag}");
                if (displayname is null && LanguageIETFTag != "en")
                    displayname = Claims.FirstOrDefault(i => i.ClaimType == $"DisplayName-en");
                return displayname?.ClaimValue ?? UserName;
            } 
        }
        public virtual List<ApplicationUserClaim> Claims { get; set; } = new List<ApplicationUserClaim>();

        public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = new List<ApplicationUserLogin>();
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; } = new List<ApplicationUserToken>();
        public virtual List<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
        public ICollection<IRole> GetRoles()
        {
            return UserRoles.Select(i=>i.Role).Cast<IRole>().ToArray();
        }
        public ICollection<IClaim> GetClaims(string? ClaimType = null) =>
                Claims.Where(i=> ClaimType is null || i.ClaimType == ClaimType)
                      .Cast<IClaim>()
                      .ToArray();

        public void SetClaims(ICollection<IClaim> claims) => Claims = claims.Cast<ApplicationUserClaim>().ToList();


        public void RemoveClaim(string ClaimType)
        {
            for(int i = Claims.Count - 1; i >= 0; i--)
            {
                if (Claims.ElementAt(i).ClaimType == ClaimType)
                {
                    Claims.Remove(Claims.ElementAt(i));
                }
            }
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
                .ToList();
        }

        public void UpsertClaim(string ClaimType, string? ClaimValue)
        {
            if (string.IsNullOrEmpty(ClaimValue))
                RemoveClaim(ClaimType);
            else
            {
                var existingClaim = Claims.FirstOrDefault(i => i.ClaimType == ClaimType);
                if (existingClaim != null) {
                    existingClaim.ClaimType = ClaimType;
                    existingClaim.ClaimValue = ClaimValue;
                }
                else
                {
                    Claims.Add(new ApplicationUserClaim
                        {
                            Id = Ulid.NewUlid().ToString(),
                            ClaimType =  ClaimType,
                            ClaimValue = ClaimValue,
                            User = this,
                            UserId = this.Id
                        });
                }
            }
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

    public class ApplicationUserClaim : IdentityUserClaim<string>, IClaim
    {
        [Key, StringLength(26)]
        public new string Id { get; set;} = Ulid.NewUlid().ToString();
        public virtual ApplicationUser User { get; set; } = default!;
    }

    public class ApplicationUserLogin : IdentityUserLogin<string>
    {
        public virtual ApplicationUser User { get; set; } = default!;
    }

    public class ApplicationRoleClaim : IdentityRoleClaim<string>
    {
        [Key, StringLength(26)]
        public new string Id { get; set;} = Ulid.NewUlid().ToString();
        public virtual ApplicationRole Role { get; set; } = default!;
    }

    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser User { get; set; } = default!;
    }
}
