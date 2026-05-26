using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Example
{
    public class DBContext: IdentityDbContext<ApplicationUser, ApplicationRole, string,
        ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
        ApplicationRoleClaim, ApplicationUserToken>
    {

        
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }
        //private IConfiguration _config;
        //public DBContext(IConfiguration configuration, DbContextOptions<DBContext> options)
        //        : base(options)
        //{
        //    _config = configuration;
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var connectionStr = _config.GetConnectionString("DefaultConnection");
        //    optionsBuilder.UseSqlite(connectionStr);
        //    base.OnConfiguring(optionsBuilder);
        //}
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
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

            modelBuilder.Entity<ApplicationRole>(b =>
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

            modelBuilder.Entity<ApplicationUser>().HasData(
            new List<ApplicationUser>
                {
                    new ApplicationUser {Id="01KFWWJK1H5MFEGN2CHSYKS46C", UserName="User001", DisplayName="User001", SecurityStamp="01KFWWJK1H5MFEGN2CHSYKS46C", ConcurrencyStamp="01KFWWJK1H5MFEGN2CHSYKS46C" },
                    new ApplicationUser {Id="01KFWWJQG16YSRQKZ6VP6DKNWZ", UserName="User002", DisplayName="User002", SecurityStamp="01KFWWJQG16YSRQKZ6VP6DKNWZ", ConcurrencyStamp="01KFWWJQG16YSRQKZ6VP6DKNWZ" },
                    new ApplicationUser {Id="01KFWWJTYHK0FQ1Y82PYVZTCH2", UserName="User003", DisplayName="User003", SecurityStamp="01KFWWJTYHK0FQ1Y82PYVZTCH2", ConcurrencyStamp="01KFWWJTYHK0FQ1Y82PYVZTCH2" },
                    new ApplicationUser {Id="01KFWWJXW1FVZ5EJF60601AHY8", UserName="User004", DisplayName="User004", SecurityStamp="01KFWWJXW1FVZ5EJF60601AHY8", ConcurrencyStamp="01KFWWJXW1FVZ5EJF60601AHY8" },
                    new ApplicationUser {Id="01KFWWK0Q10RJF4KAQK00VFQKV", UserName="User005", DisplayName="User005", SecurityStamp="01KFWWK0Q10RJF4KAQK00VFQKV", ConcurrencyStamp="01KFWWK0Q10RJF4KAQK00VFQKV" },
                    new ApplicationUser {Id="01KFWWK3TSEW6C3K0V9AC715PM", UserName="User006", DisplayName="User006", SecurityStamp="01KFWWK3TSEW6C3K0V9AC715PM", ConcurrencyStamp="01KFWWK3TSEW6C3K0V9AC715PM" },
                    new ApplicationUser {Id="01KFWWK6Q9T520T36ESNQYD111", UserName="User007", DisplayName="User007", SecurityStamp="01KFWWK6Q9T520T36ESNQYD111", ConcurrencyStamp="01KFWWK6Q9T520T36ESNQYD111" },
                }
            );
        }
    }
}
