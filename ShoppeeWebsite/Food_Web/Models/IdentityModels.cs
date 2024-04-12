using System;
using System.Configuration;
using System.Data.Entity;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Food_Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public bool IsApproved { get; set; }

        public ApplicationUser()
        {
            IsApproved = false;
        }
        public string image { get; set; }

        public string status { get; set; }
        public TimeSpan? Opentime { get; set; }
        public TimeSpan? Closetime { get; set; }
         public string Adress { get; set; }
        public string Fullname { get; set; }
        
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("FoodcontextDB19", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public string ConfirmPassword { get; set; }


        //public System.Data.Entity.DbSet<Food_Web.Models.chef> chefs { get; set; }

        //public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        //public DbSet<ApplicationUser> IdentityUsers { get; set; }

        //public DbSet<ApplicationUser> IdentityUsers { get; set; }
        //IdentityUser


        //public DbSet<ApplicationUser> Users { get; set; }

        //public System.Data.Entity.DbSet<Food_Web.Models.ApplicationUser> ApplicationUsers { get; set; }




    }
    //public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    //{
    //    public ApplicationDbContext()
    //        : base("DefaultConnection", throwIfV1Schema: false)
    //    {
    //    }

    //    public static ApplicationDbContext Create()
    //    {
    //        return new ApplicationDbContext();
    //    }

    //    public System.Data.Entity.DbSet<Food_Web.Models.Product> Products { get; set; }

    //    public System.Data.Entity.DbSet<Food_Web.Models.Category> Categories { get; set; }

    //    public System.Data.Entity.DbSet<Food_Web.Models.chef> chefs { get; set; }
    //    public string ConfirmPassword { get; set; }

    //    // Remove this object set
    //    // public System.Data.Entity.DbSet<Food_Web.Models.ApplicationUser> Users { get; set; }

    //    public System.Data.Entity.DbSet<Food_Web.Models.ApplicationUser> ApplicationUsers { get; set; }
    //}



}