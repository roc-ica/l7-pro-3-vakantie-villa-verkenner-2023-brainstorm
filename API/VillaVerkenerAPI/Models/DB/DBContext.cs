using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VillaVerkenerAPI.Models.DB;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<LocationTag> LocationTags { get; set; }

    public virtual DbSet<PropertyTag> PropertyTags { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Villa> Villas { get; set; }

    public virtual DbSet<VillaLocationTag> VillaLocationTags { get; set; }

    public virtual DbSet<VillaPropertyTag> VillaPropertyTags { get; set; }

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

        modelBuilder.Entity<LocationTag>(entity =>
        {
            entity.HasKey(e => e.LocationTagId).HasName("PRIMARY");

            entity.HasIndex(e => e.LocationTag1, "LocationTag_UNIQUE").IsUnique();

            entity.Property(e => e.LocationTagId).HasColumnName("LocationTagID");
            entity.Property(e => e.LocationTag1)
                .HasMaxLength(45)
                .HasColumnName("LocationTag");
        });

        modelBuilder.Entity<PropertyTag>(entity =>
        {
            entity.HasKey(e => e.PropertyTagId).HasName("PRIMARY");

            entity.HasIndex(e => e.PropertyTag1, "LocationTag_UNIQUE").IsUnique();

            entity.Property(e => e.PropertyTagId).HasColumnName("PropertyTagID");
            entity.Property(e => e.PropertyTag1)
                .HasMaxLength(45)
                .HasColumnName("PropertyTag");
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
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.VillaId).HasColumnName("VillaID");

            entity.HasOne(d => d.Villa).WithMany(p => p.Requests)
                .HasForeignKey(d => d.VillaId)
                .HasConstraintName("Request_VillaID_Villa");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PRIMARY");

            entity.ToTable("Session");

            entity.HasIndex(e => e.SessionKey, "SessionID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.UserId, "Session_UserID_User_idx");

            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.SessionKey).HasMaxLength(64);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Sessions)
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
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        modelBuilder.Entity<Villa>(entity =>
        {
            entity.HasKey(e => e.VillaId).HasName("PRIMARY");

            entity.ToTable("Villa");

            entity.HasIndex(e => e.VillaId, "VillaID_UNIQUE").IsUnique();

            entity.Property(e => e.VillaId).HasColumnName("VillaID");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Locatie).HasMaxLength(128);
            entity.Property(e => e.Naam).HasMaxLength(45);
            entity.Property(e => e.Omschrijving).HasColumnType("mediumtext");
            entity.Property(e => e.Prijs).HasPrecision(12);
        });

        modelBuilder.Entity<VillaLocationTag>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.LocationTagId, "VillaLocationTags_LocationTagID_LocationTags_idx");

            entity.HasIndex(e => e.VillaId, "VillaLocationTags_VillaID_Villa_idx");

            entity.Property(e => e.LocationTagId).HasColumnName("LocationTagID");
            entity.Property(e => e.VillaId).HasColumnName("VillaID");

            entity.HasOne(d => d.LocationTag).WithMany()
                .HasForeignKey(d => d.LocationTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VillaLocationTags_LocationTagID_LocationTags");

            entity.HasOne(d => d.Villa).WithMany()
                .HasForeignKey(d => d.VillaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VillaLocationTags_VillaID_Villa");
        });

        modelBuilder.Entity<VillaPropertyTag>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.VillaId, "VillaLocationTags_VillaID_Villa_idx");

            entity.HasIndex(e => e.PropertyTagId, "VillaPropertyTags_PropertyTagID_PropertyTags_idx");

            entity.Property(e => e.PropertyTagId).HasColumnName("PropertyTagID");
            entity.Property(e => e.VillaId).HasColumnName("VillaID");

            entity.HasOne(d => d.PropertyTag).WithMany()
                .HasForeignKey(d => d.PropertyTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VillaPropertyTags_PropertyTagID_PropertyTags");

            entity.HasOne(d => d.Villa).WithMany()
                .HasForeignKey(d => d.VillaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VillaPropertyTags_VillaID_Villa");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
