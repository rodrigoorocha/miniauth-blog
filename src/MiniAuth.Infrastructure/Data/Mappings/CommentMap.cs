using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniAuth.Domain.Entities;

namespace MiniAuth.Infrastructure.Data.Mappings;

public class CommentMap : BaseMap<Comment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.AuthorId)
            .IsRequired();

        builder.HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
