using AutoMapper;
using HomeMeters2.API.Places;

namespace HomeMeters2.API.Tests.Unit.Places;

public class PlaceMappingProfileTests
{
    [Test]
    public void validate_configuration()
    {
        var configuration = new MapperConfiguration(x => x.AddProfile(new PlaceMappingProfile()));

        configuration.AssertConfigurationIsValid();
    }
}