using HomeMeters2.API.Constants;
using HomeMeters2.API.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeMeters2.API.DataAccess.Configurations;

public class PlaceConfiguration : IEntityTypeConfiguration<Place>
{
    public void Configure(EntityTypeBuilder<Place> builder)
    {
        builder.HasQueryFilter(x => !x.IsSoftDeleted);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(PlaceConstants.PlaceNameMaximumLength);
        
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(PlaceConstants.PlaceDescriptionMaximumLength);
    }
}