using BlazorAccessControl.EFCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExampleNet8
{
    public class MyDBContext : BlazorAccessControl.EFCore.DBContext
    {
        private readonly IConfiguration config;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(config.GetValue<bool?>("Database:EnableSensitiveDataLogging") ?? false);
            optionsBuilder.UseSqlite(config.GetConnectionString("DefaultConnection"));
            base.OnConfiguring(optionsBuilder);
        }

        public MyDBContext(DbContextOptions<MyDBContext> options, IConfiguration config)
        {
            this.config = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().HasData(
            new List<ApplicationUser>
                {
                    new ApplicationUser {Id="01KFWWJK1H5MFEGN2CHSYKS46C", UserName="User001", SecurityStamp="01KFWWJK1H5MFEGN2CHSYKS46C", ConcurrencyStamp="01KFWWJK1H5MFEGN2CHSYKS46C" },
                    new ApplicationUser {Id="01KFWWJQG16YSRQKZ6VP6DKNWZ", UserName="User002", SecurityStamp="01KFWWJQG16YSRQKZ6VP6DKNWZ", ConcurrencyStamp="01KFWWJQG16YSRQKZ6VP6DKNWZ" },
                    new ApplicationUser {Id="01KFWWJTYHK0FQ1Y82PYVZTCH2", UserName="User003", SecurityStamp="01KFWWJTYHK0FQ1Y82PYVZTCH2", ConcurrencyStamp="01KFWWJTYHK0FQ1Y82PYVZTCH2" },
                    new ApplicationUser {Id="01KFWWJXW1FVZ5EJF60601AHY8", UserName="User004", SecurityStamp="01KFWWJXW1FVZ5EJF60601AHY8", ConcurrencyStamp="01KFWWJXW1FVZ5EJF60601AHY8" },
                    new ApplicationUser {Id="01KFWWK0Q10RJF4KAQK00VFQKV", UserName="User005", SecurityStamp="01KFWWK0Q10RJF4KAQK00VFQKV", ConcurrencyStamp="01KFWWK0Q10RJF4KAQK00VFQKV" },
                    new ApplicationUser {Id="01KFWWK3TSEW6C3K0V9AC715PM", UserName="User006", SecurityStamp="01KFWWK3TSEW6C3K0V9AC715PM", ConcurrencyStamp="01KFWWK3TSEW6C3K0V9AC715PM" },
                    new ApplicationUser {Id="01KFWWK6Q9T520T36ESNQYD111", UserName="User007", SecurityStamp="01KFWWK6Q9T520T36ESNQYD111", ConcurrencyStamp="01KFWWK6Q9T520T36ESNQYD111" },
                }
            );
        }
    }
}
