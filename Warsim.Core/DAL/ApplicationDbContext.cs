using System;
using System.Data.Entity;

using Warsim.Core.Game;
using Warsim.Core.Notifications;
using Warsim.Core.Users;
using Warsim.Core.Users.Friendships;

namespace Warsim.Core.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
            this.Configuration.LazyLoadingEnabled = true;
        }

        public DbSet<ApplicationUser> Users { get; set; }

        public DbSet<ApplicationUserLogin> UserLogins { get; set; }

        public DbSet<Friend> Friends { get; set; }

        public DbSet<FriendRequest> FriendRequests { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Map> Maps { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Friends)
                .WithRequired(f => f.User1)
                .HasForeignKey(f => f.User1Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Friends)
                .WithRequired(f => f.User2)
                .HasForeignKey(f => f.User2Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.FriendRequests)
                .WithRequired(f => f.User)
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(f => f.UserLogins)
                .WithOptional()
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(f => f.Notifications)
                .WithOptional()
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<FriendRequest>()
                .HasKey(x => new { x.FutureFriendId, x.UserId });
            modelBuilder.Entity<FriendRequest>()
                .HasRequired(f => f.FutureFriend)
                .WithMany()
                .HasForeignKey(f => f.FutureFriendId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<FriendRequest>()
                .HasRequired(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<Friend>()
                .HasKey(x => new { x.User1Id, x.User2Id });
            modelBuilder.Entity<Friend>()
                .HasRequired(f => f.User1)
                .WithMany()
                .HasForeignKey(f => f.User1Id);
            modelBuilder.Entity<Friend>()
                .HasRequired(f => f.User2)
                .WithMany()
                .HasForeignKey(f => f.User2Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUserLogin>().HasKey(x => new { x.ProviderUserId, x.UserId });

            modelBuilder.Entity<Map>().HasKey(x => x.Id);

            modelBuilder.Entity<Map>().Ignore(x => x.SceneObjects);

            modelBuilder.Entity<Notification>().HasKey(x => x.Id);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}