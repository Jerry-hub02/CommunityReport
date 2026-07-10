using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunityReport.Models.Domain
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<ApplicationUser> AspNetUsers { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatRoomMember> ChatRoomMembers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relatioinships
            builder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(u => u.UserId);
        }
    }
}
