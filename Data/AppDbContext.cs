using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Models;
namespace Sym_Connect__Capstone2_.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<GroupDetails> GroupDetails { get; set; }
        public DbSet<YouthActivity> Activities { get; set; }
        public DbSet<Facility> Facilities { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // GROUP → GROUPDETAILS
            // One Group can have many GroupDetails rows
            modelBuilder.Entity<GroupDetails>()
                .HasOne(gd => gd.Group)
                .WithMany(g => g.GroupDetails)
                .HasForeignKey(gd => gd.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // MEMBER → GROUPDETAILS
            // One Member can have many GroupDetails rows
            modelBuilder.Entity<GroupDetails>()
                .HasOne(gd => gd.Member)
                .WithMany(m => m.GroupDetails)
                .HasForeignKey(gd => gd.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // GROUP → LEADER (Leader is a Member)
            // One Group has one Leader which comes from Members table
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Leader)
                .WithMany()
                .HasForeignKey(g => g.LeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // FACILITY → ACTIVITIES
            // One Facility can have many Activities
            modelBuilder.Entity<Facility>()
                .HasMany(f => f.Activities)
                .WithOne(a => a.Facility)
                .HasForeignKey(a => a.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
