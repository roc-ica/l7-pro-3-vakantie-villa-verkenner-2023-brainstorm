using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VillaVerkenerAPI.Models;

public partial class MyAppDbContext : DbContext
{
    public MyAppDbContext()
    {
    }

    public MyAppDbContext(DbContextOptions<MyAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Villa> Villas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.VillaImageId).HasName("PRIMARY");

            entity.ToTable("Image");

            entity.HasIndex(e => e.VillaId, "FKVillaID_idx");

            entity.HasIndex(e => e.VillaImageId, "VillaImageID_UNIQUE").IsUnique();

            entity.Property(e => e.VillaImageId).HasColumnName("VillaImageID");
            entity.Property(e => e.ImageLocation).HasMaxLength(255);
            entity.Property(e => e.VillaId).HasColumnName("VillaID");

            entity.HasOne(d => d.Villa).WithMany(p => p.Images)
                .HasForeignKey(d => d.VillaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Image_VillaID_Villa");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PRIMARY");

            entity.ToTable("Request");

            entity.HasIndex(e => e.RequestId, "RequestID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.VillaId, "Request_VillaID_Villa_idx");

            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.VillaId).HasColumnName("VillaID");

            entity.HasOne(d => d.Villa).WithMany(p => p.Requests)
                .HasForeignKey(d => d.VillaId)
                .HasConstraintName("Request_VillaID_Villa");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Session");

            entity.HasIndex(e => e.Session1, "SessionID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.UserId, "Session_UserID_User_idx");

            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.Session1)
                .HasMaxLength(64)
                .HasColumnName("Session");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Session_UserID_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "Email_UNIQUE").IsUnique();

            entity.HasIndex(e => e.UserId, "UserID_UNIQUE").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Password).HasMaxLength(64);
        });

        modelBuilder.Entity<Villa>(entity =>
        {
            entity.HasKey(e => e.VillaId).HasName("PRIMARY");

            entity.ToTable("Villa");

            entity.HasIndex(e => e.VillaId, "VillaID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.VillaImageId, "VillaImageID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.VillaImageId, "Villa_VillaImageID_Image_idx");

            entity.Property(e => e.VillaId).HasColumnName("VillaID");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Locatie).HasMaxLength(128);
            entity.Property(e => e.Naam).HasMaxLength(45);
            entity.Property(e => e.Omschrijving).HasColumnType("mediumtext");
            entity.Property(e => e.Prijs).HasPrecision(12);
            entity.Property(e => e.VillaImageId).HasColumnName("VillaImageID");

            entity.HasOne(d => d.VillaImage).WithOne(p => p.VillaNavigation)
                .HasForeignKey<Villa>(d => d.VillaImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Villa_VillaImageID_Image");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
