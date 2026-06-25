using BlazorAccessControl.EFCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorAccessControl.EFCore
{
    public abstract class DBContext<TKey>: IdentityDbContext<ApplicationUser<TKey>, ApplicationRole<TKey>, TKey,
        ApplicationUserClaim<TKey>, ApplicationUserRole<TKey>, ApplicationUserLogin<TKey>,
        ApplicationRoleClaim<TKey>, ApplicationUserToken<TKey>> where TKey: System.IEquatable<TKey>
    {

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser<TKey>>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            
            modelBuilder.Entity<ApplicationUserClaim<TKey>>(b => b.HasKey(uc => uc.Id));
            modelBuilder.Entity<ApplicationRoleClaim<TKey>>(b => b.HasKey(rc => rc.Id));

            modelBuilder.Entity<ApplicationRole<TKey>>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });

        }
    }
}
