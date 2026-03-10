using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniAuth.Domain.Entities;

namespace MiniAuth.Infrastructure.Data.Mappings;

public class PostMap : BaseMap<Post>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Content)
            .IsRequired();

        builder.Property(p => p.AuthorId)
            .IsRequired();

        builder.HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
