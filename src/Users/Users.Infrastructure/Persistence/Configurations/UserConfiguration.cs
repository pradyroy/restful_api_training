using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UserName)
            .HasColumnName("user_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.UserName).IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(u => u.EmailId)
            .HasColumnName("email_id")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.MobileNum)
            .HasColumnName("mobile_num")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.ProfilePicUrl)
            .HasColumnName("profile_pic_url")
            .HasMaxLength(500);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
