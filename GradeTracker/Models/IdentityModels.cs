﻿using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GradeTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

		/*
		 * Set of StudentModel objects to be stored in the database.
		 */
		public DbSet<StudentModel> Students { get; set; }

		public System.Data.Entity.DbSet<GradeTracker.Models.SemesterModel> SemesterModels { get; set; }

        public System.Data.Entity.DbSet<GradeTracker.Models.CourseModel> CourseModels { get; set; }

        public System.Data.Entity.DbSet<GradeTracker.Models.WorkItemModel> WorkItemModels { get; set; }

        public System.Data.Entity.DbSet<GradeTracker.Models.CategoryWeight> CategoryWeights { get; set; }
    }
}