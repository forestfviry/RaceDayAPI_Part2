using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------------- Roles ----------------
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.RoleID);
            entity.Property(r => r.RoleName)
                  .HasMaxLength(20)
                  .IsRequired();
            entity.HasIndex(r => r.RoleName).IsUnique();
        });

        // ---------------- Users ----------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.UserID);
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.CreatedAt)
                  .HasDefaultValueSql("GETDATE()")
                  .IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Events ----------------
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.EventID);
            entity.Property(e => e.EventName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EventDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.Location).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()")
                  .IsRequired();

            entity.HasOne(e => e.Organiser)
                  .WithMany(u => u.EventsOrganised)
                  .HasForeignKey(e => e.OrganiserID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Categories ----------------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.CategoryID);
            entity.Property(c => c.CategoryName).HasMaxLength(50).IsRequired();
            entity.Property(c => c.DistanceKm).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(c => c.MaxParticipants)
                  .HasDefaultValue(100)
                  .IsRequired();
            entity.Property(c => c.EntryFee)
                  .HasColumnType("decimal(8,2)")
                  .HasDefaultValue(0m)
                  .IsRequired();

            entity.HasOne(c => c.Event)
                  .WithMany(e => e.Categories)
                  .HasForeignKey(c => c.EventID)
                  .OnDelete(DeleteBehavior.Cascade); // deleting an event removes its categories
        });

        // ---------------- Enrolments ----------------
        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.ToTable("Enrolments");
            entity.HasKey(e => e.EnrolmentID);
            entity.Property(e => e.EnrolmentDate)
                  .HasDefaultValueSql("GETDATE()")
                  .IsRequired();
            entity.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasDefaultValue("Registered")
                  .IsRequired();

            // One user may only enrol once per category (matches UQ_Enrolments_User_Category)
            entity.HasIndex(e => new { e.UserID, e.CategoryID }).IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Enrolments)
                  .HasForeignKey(e => e.UserID)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Enrolments)
                  .HasForeignKey(e => e.CategoryID)
                  .OnDelete(DeleteBehavior.Cascade); // removing a category removes its enrolments
        });

        // ---------------- Results ----------------
        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Results");
            entity.HasKey(r => r.ResultID);
            entity.Property(r => r.FinishTime).HasColumnType("time");
            entity.Property(r => r.Position);
            entity.Property(r => r.Status)
                  .HasMaxLength(20)
                  .HasDefaultValue("Pending")
                  .IsRequired();

            // One result per enrolment (matches EnrolmentID INT NOT NULL UNIQUE)
            entity.HasIndex(r => r.EnrolmentID).IsUnique();

            entity.HasOne(r => r.Enrolment)
                  .WithOne(e => e.Result)
                  .HasForeignKey<Result>(r => r.EnrolmentID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Seed the two roles (matches SQL script) ----------------
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleID = 1, RoleName = "Organiser" },
            new Role { RoleID = 2, RoleName = "Participant" }
        );
    }
}
