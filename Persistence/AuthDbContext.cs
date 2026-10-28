using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Persistence;

public sealed class AuthDbContext
    : IdentityDbContext<IdentityUser>
{
    public AuthDbContext(
        DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseOpenIddict();

        builder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(company => company.Id);

            entity.Property(company => company.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(company => company.Code)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(company => company.Code)
                .IsUnique();
        });

        builder.Entity<UserCompany>(entity =>
        {
            entity.ToTable("UserCompanies");
            entity.HasKey(membership => new
            {
                membership.UserId,
                membership.CompanyId
            });

            entity.Property(membership => membership.UserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.HasOne(membership => membership.User)
                .WithMany()
                .HasForeignKey(membership => membership.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(membership => membership.Company)
                .WithMany(company => company.UserCompanies)
                .HasForeignKey(membership => membership.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(membership => membership.CompanyId);
        });
    }
}