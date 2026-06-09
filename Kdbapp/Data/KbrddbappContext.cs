using System;
using System.Collections.Generic;
using Kdbapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Kdbapp.Data;

public partial class KbrddbappContext : DbContext
{
    public KbrddbappContext()
    {
    }

    public KbrddbappContext(DbContextOptions<KbrddbappContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Component> Components { get; set; }
    public virtual DbSet<Customfile> Customfiles { get; set; }
    public virtual DbSet<Gallery> Galleries { get; set; }
    public virtual DbSet<Keyboardconfiguration> Keyboardconfigurations { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
    public virtual DbSet<Orderstatushistory> Orderstatushistories { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Database=kbrddbapp;Username=postgres;Password=8585");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Component>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("components_pk");
            entity.ToTable("components");
            entity.HasIndex(e => e.Category, "ind_components_category");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category).HasColumnType("character varying").HasColumnName("category");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasColumnType("character varying").HasColumnName("image_url");
            entity.Property(e => e.InStook).HasColumnName("in_stook");
            entity.Property(e => e.Name).HasColumnType("character varying");
            entity.Property(e => e.Price).HasColumnName("price");
        });

        modelBuilder.Entity<Customfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customfiles_pk");
            entity.ToTable("customfiles");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConfigId).HasColumnName("config_id");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.Filetype).HasColumnType("character varying").HasColumnName("filetype");
            entity.Property(e => e.IsChecked).HasColumnName("is_checked");
            entity.HasOne(d => d.Config).WithMany(p => p.Customfiles)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("customfiles_keyboardconfigurations_fk");
        });

        modelBuilder.Entity<Gallery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gallery_pk");
            entity.ToTable("gallery");
            entity.HasIndex(e => e.IsModerated, "ind_gallery_moderation").HasFilter("(is_moderated = true)");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Authorid).HasColumnName("authorid");
            entity.Property(e => e.ConfigId).HasColumnName("config_id");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.IsModerated).HasDefaultValue(false).HasColumnName("is_moderated");
            entity.Property(e => e.Likescount).HasColumnName("likescount");
            entity.Property(e => e.Title).HasMaxLength(200).HasColumnName("title");
            entity.HasOne(d => d.Author).WithMany(p => p.Galleries)
                .HasForeignKey(d => d.Authorid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("gallery_users_fk");
            entity.HasOne(d => d.Config).WithMany(p => p.Galleries)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("gallery_keyboardconfigurations_fk");
        });

         modelBuilder.Entity<Keyboardconfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("keyboardconfigurations_pk");
            entity.ToTable("keyboardconfigurations");
            entity.HasIndex(e => e.Configurationhash, "keyboardconfigurations_unique").IsUnique();
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseColor).HasMaxLength(20).HasColumnName("casecolor");
            entity.Property(e => e.CasesizeId).HasColumnName("casesize_id");
            entity.Property(e => e.Configurationhash).HasColumnType("character varying").HasColumnName("configurationhash");
            entity.Property(e => e.CustommodelUrl).HasColumnName("custommodel_url");
            entity.Property(e => e.CustomprintUrl).HasColumnName("customprint_url");
            entity.Property(e => e.CustomPrintImageUrl).HasMaxLength(500).HasColumnName("customprintimageurl");
            entity.Property(e => e.HasCustomPrint).HasDefaultValue(false).HasColumnName("hascustomprint");
            entity.Property(e => e.IsSoundInsulated).HasColumnName("is_sound_insulated");
            entity.Property(e => e.KeycapColor).HasMaxLength(20).HasColumnName("keycapcolor");
            entity.Property(e => e.KeycapMaterialType).HasMaxLength(20).HasColumnName("keycapmaterialtype");
            entity.Property(e => e.KeycapsId).HasColumnName("keycaps_id");
            entity.Property(e => e.Layout).HasMaxLength(10).HasColumnName("layout");
            entity.Property(e => e.RgbMode).HasDefaultValue(false).HasColumnName("rgbmode");
            entity.Property(e => e.SwitchColor).HasMaxLength(20).HasColumnName("switchcolor");
            entity.Property(e => e.SwitchType).HasMaxLength(20).HasColumnName("switchtype");
            entity.Property(e => e.SwitchtypeId).HasColumnName("switchtype_id");
            entity.Property(e => e.TotalPriceRaw).HasMaxLength(50).HasColumnName("totalpriceraw");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VolumeColor).HasMaxLength(20).HasColumnName("volumecolor");

            entity.HasOne(d => d.Casesize).WithMany()
                .HasForeignKey(d => d.CasesizeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("keyboardconfigurations_components_fk");

            entity.HasOne(d => d.Keycaps).WithMany()
                .HasForeignKey(d => d.KeycapsId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("keyboardconfigurations_components_fk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Keyboardconfigurations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("keyboardconfigurations_users_fk");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pk");
            entity.ToTable("orders");
            entity.HasIndex(e => e.Status, "ind_orders_status");
            entity.HasIndex(e => e.UserId, "ix_orders_user_id");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConfigurationId).HasColumnName("configuration_id");
            entity.Property(e => e.Contactemail).HasMaxLength(255).HasColumnName("contactemail");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ShippingAddress).HasColumnName("shipping_address");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            entity.Property(e => e.TrackNumber).HasMaxLength(50).HasColumnName("track_number");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.HasOne(d => d.Configuration).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ConfigurationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_keyboardconfigurations_fk");
            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_order_status_fk");
            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_users_fk");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_status_pk");
            entity.ToTable("order_status");
            entity.HasIndex(e => e.Name, "order_status_unique").IsUnique();
            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.Name).HasColumnType("character varying").HasColumnName("name");
        });

        modelBuilder.Entity<Orderstatushistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orderstatushistory_pk");
            entity.ToTable("orderstatushistory");
            entity.HasIndex(e => e.OrderId, "ind_orderstatushistory_order_id");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Changedbyid).HasColumnName("changedbyid");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("updated_at");
            entity.HasOne(d => d.Order).WithMany(p => p.Orderstatushistories)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("orderstatushistory_orders_fk");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pk");
            entity.ToTable("users");
            entity.HasIndex(e => e.Login, "users_unique").IsUnique();
            entity.HasIndex(e => e.Email, "users_unique_1").IsUnique();
            entity.Property(e => e.Id).HasDefaultValueSql("nextval('newtable_id_seq'::regclass)").HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
            entity.Property(e => e.Isactive).HasDefaultValue(true).HasColumnName("isactive");
            entity.Property(e => e.Login).HasMaxLength(100).HasColumnName("login");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.PasswordHash).HasMaxLength(255).HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber).HasMaxLength(30).HasColumnName("phone_number");
            entity.Property(e => e.RegisteredAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("registered_at");
            entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValueSql("'user'::character varying").HasColumnName("role");
            entity.Property(e => e.Surname).HasMaxLength(100).HasColumnName("surname");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}