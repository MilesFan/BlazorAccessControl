using BlazorAccessControl.Interface;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlazorAccessControl.EFCore
{

    public class ApplicationUser<TKey>: IdentityUser<TKey>, IUser<TKey> where TKey:System.IEquatable<TKey>
    {
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

        public virtual List<ApplicationUserClaim<TKey>> Claims { get; set; } = new List<ApplicationUserClaim<TKey>>();

        public virtual ICollection<ApplicationUserLogin<TKey>> Logins { get; set; } = new List<ApplicationUserLogin<TKey>>();
        public virtual ICollection<ApplicationUserToken<TKey>> Tokens { get; set; } = new List<ApplicationUserToken<TKey>>();
        public virtual List<ApplicationUserRole<TKey>> UserRoles { get; set; } = new List<ApplicationUserRole<TKey>>();
        public ICollection<IRole<TKey>> GetRoles()
        {
            return UserRoles.Select(i=>i.Role).Cast<IRole<TKey>>().ToArray();
        }
        public ICollection<IClaim<TKey>> GetClaims(string? ClaimType = null) =>
                Claims.Where(i=> ClaimType is null || i.ClaimType == ClaimType)
                      .Cast<IClaim<TKey>>()
                      .ToArray();

        public void SetClaims(ICollection<IClaim<TKey>> claims) => Claims = claims.Cast<ApplicationUserClaim<TKey>>().ToList();


        public void RemoveClaims(string ClaimType)
        {
            for(int i = Claims.Count - 1; i >= 0; i--)
            {
                if (Claims.ElementAt(i).ClaimType == ClaimType)
                {
                    Claims.Remove(Claims.ElementAt(i));
                }
            }
        }

        public void SetRoles(ICollection<IRole<TKey>> roles)
        {
            UserRoles = roles
                .Cast<ApplicationRole<TKey>>()
                .Select(i=>new ApplicationUserRole<TKey> {
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
                RemoveClaims(ClaimType);
            else
            {
                var existingClaim = Claims.FirstOrDefault(i => i.ClaimType == ClaimType);
                if (existingClaim != null) {
                    existingClaim.ClaimType = ClaimType;
                    existingClaim.ClaimValue = ClaimValue;
                }
                else
                {
                    Claims.Add(new ApplicationUserClaim<TKey>
                        {
                            Id = default!,
                            ClaimType =  ClaimType,
                            ClaimValue = ClaimValue,
                            User = this,
                            UserId = this.Id
                        });
                }
            }
        }

        public void SetClaimValues(string ClaimType, IReadOnlyCollection<string> Values)
        {
            RemoveClaims(ClaimType);
            var claims = new List<ApplicationUserClaim<TKey>>();
            foreach(var value in Values)
            {
                claims.Add(new ApplicationUserClaim<TKey> {
                                Id = default!,
                                ClaimType = ClaimType,
                                ClaimValue = value});
            }
            Claims.AddRange(claims);
        }
    }

    public class ApplicationRole<TKey>: IdentityRole<TKey>, IRole<TKey> where TKey:System.IEquatable<TKey>
    {
        public virtual ICollection<ApplicationUserRole<TKey>> UserRoles { get; set; } = new List<ApplicationUserRole<TKey>>();
        public virtual ICollection<ApplicationRoleClaim<TKey>> RoleClaims { get; set; } = new List<ApplicationRoleClaim<TKey>>();
    }

    public class ApplicationUserRole<TKey>: IdentityUserRole<TKey> where TKey:System.IEquatable<TKey>
    {
        public virtual ApplicationUser<TKey> User { get; set; } = default!;
        public virtual ApplicationRole<TKey> Role { get; set; } = default!;
    }

    public class ApplicationUserClaim<TKey> : IdentityUserClaim<TKey>, IClaim<TKey> where TKey:System.IEquatable<TKey>
    {
        [Key, StringLength(40)]
        public new required TKey Id { get; set;}
        public virtual ApplicationUser<TKey> User { get; set; } = default!;
    }

    public class ApplicationUserLogin<TKey> : IdentityUserLogin<TKey> where TKey:System.IEquatable<TKey>
    {
        public virtual ApplicationUser<TKey> User { get; set; } = default!;
    }

    public class ApplicationRoleClaim<TKey> : IdentityRoleClaim<TKey> where TKey:System.IEquatable<TKey>
    {
        public virtual ApplicationRole<TKey> Role { get; set; } = default!;
    }

    public class ApplicationUserToken<TKey> : IdentityUserToken<TKey> where TKey:System.IEquatable<TKey>
    {
        public virtual ApplicationUser<TKey> User { get; set; } = default!;
    }
}
