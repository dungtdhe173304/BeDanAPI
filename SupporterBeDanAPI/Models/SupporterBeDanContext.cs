using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SupporterBeDanAPI.Models;

public partial class SupporterBeDanContext : DbContext
{
    public SupporterBeDanContext()
    {
    }

    public SupporterBeDanContext(DbContextOptions<SupporterBeDanContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ExamRegistration> ExamRegistrations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<RegistrationStatus> RegistrationStatuses { get; set; }

    public virtual DbSet<ExamCompletionStatus> ExamCompletionStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("DB");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ExamRegi__3214EC0728318F35");

            entity.HasIndex(e => e.SupporterId, "IX_Exam_SupporterId");

            entity.HasIndex(e => e.UserId, "IX_Exam_UserId");

            entity.Property(e => e.ContactInfo).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.Slot).HasMaxLength(50);
            entity.Property(e => e.Spcode)
                .HasMaxLength(50)
                .HasColumnName("SPCode");
            entity.Property(e => e.Subject).HasMaxLength(100);

            entity.HasOne(d => d.Supporter).WithMany(p => p.ExamRegistrationSupporters)
                .HasForeignKey(d => d.SupporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exam_Supporter");

            entity.HasOne(d => d.User).WithMany(p => p.ExamRegistrationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exam_User");

            entity.HasOne(d => d.RegistrationStatus).WithMany(p => p.ExamRegistrations)
                .HasForeignKey(d => d.RegistrationStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exam_RegistrationStatus");

            entity.HasOne(d => d.ExamCompletionStatus).WithMany(p => p.ExamRegistrations)
                .HasForeignKey(d => d.ExamCompletionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exam_CompletionStatus");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC075BEB68E4");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F646FD8280").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0795336F7A");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4B5D7F6BD").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Facebook).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
