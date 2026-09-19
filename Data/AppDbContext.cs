using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.AnnouncementModels;
using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.Models.EventModels;
using Sym_Connect__Capstone2_.Models.FacilityModels;
using Sym_Connect__Capstone2_.Models.MediaAndDocuments;
using Sym_Connect__Capstone2_.Models.Notification;
using Sym_Connect__Capstone2_.Services.Implementation;
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

        public DbSet<Facility> Facilities { get; set; }
        public DbSet<FacilityPhoto> FacilityPhotos { get; set; }

        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<EvaluationDetails> EvaluationDetail { get; set; }
        public DbSet<EvaluationQuestion> EvaluationQuestions { get; set; }

        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventGroup> EventGroups { get; set; }
        public DbSet<EvaluationSubmission> EvaluationSubmissions { get; set; }

        public DbSet<EvaluationResponse> EvaluationResponses { get; set; }

        public DbSet<Election> Elections { get; set; }
        public DbSet<PartyList> PartyLists { get; set; }
        public DbSet<Candidates> Candidates { get; set; }
        public DbSet<Position> positions { get; set; }

        public DbSet<MediaFolder> MediaFolders { get; set; }
        public DbSet<MediaFile> MediaFiles { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<EventFacility> EventFacilities { get; set; }


        public DbSet<Vote> Votes { get; set; }

        public DbSet<EmailVerification> EmailVerifications { get; set; }


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

            modelBuilder.Entity<Facility>()
              .Property(f => f.FacilityTypes)
              .HasConversion(
                  v => string.Join(',', v),
                  v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
              );

            // MEMBER → GROUPDETAILS
            // One Member can have many GroupDetails rows
            modelBuilder.Entity<GroupDetails>()
                .HasOne(gd => gd.Member)
                .WithMany(m => m.GroupDetails)
                .HasForeignKey(gd => gd.MemberId)

                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Evaluation>()
            .HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Attendance>()
           .HasIndex(a => new { a.MemberId, a.EventId })
               .IsUnique();



            modelBuilder.Entity<GroupDetails>()
            .HasIndex(gd => new { gd.MemberId })
            .IsUnique();

            // GROUP → LEADER (Leader is a Member)
            // One Group has one Leader which comes from Members table
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Leader)
                .WithMany()
                .HasForeignKey(g => g.LeaderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<EvaluationQuestion>()
                .Property(e => e.AnswerType)
                .HasConversion<string>(); //convertion type to string 

            modelBuilder.Entity<EventGroup>()
            .HasIndex(eg => new { eg.EventId, eg.GroupId })
            .IsUnique();


            modelBuilder.Entity<EvaluationResponse>()
                .HasOne(r => r.EvaluationSubmission)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);   // changed from Cascade



            modelBuilder.Entity<EventFacility>()
                .HasKey(ef => new
                {
                    ef.EventId,
                    ef.FacilityId
                });

            modelBuilder.Entity<EventFacility>()
            .HasOne(ef => ef.Event)
            .WithMany(e => e.ExpectedFacilities)
            .HasForeignKey(ef => ef.EventId)
            .OnDelete(DeleteBehavior.Cascade);


            // EventFacility → Facility
            modelBuilder.Entity<EventFacility>()
                .HasOne(ef => ef.Facility)
                .WithMany()
                .HasForeignKey(ef => ef.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);


               modelBuilder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

               modelBuilder.Entity<Vote>()
                .HasIndex(v => new { v.UserId, v.ElectionId, v.positionId })
                .IsUnique();


            modelBuilder.Entity<Member>()
            .HasOne(m => m.User)
            .WithOne(u => u.Member)
            .HasForeignKey<Member>(m => m.UserId)
            .IsRequired(false) // allows null UserId
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Position)
                .WithMany()
                .HasForeignKey(v => v.positionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        }
    }

