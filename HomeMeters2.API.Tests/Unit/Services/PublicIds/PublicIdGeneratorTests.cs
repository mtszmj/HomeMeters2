using HomeMeters2.API.Services.PublicIds;
using Microsoft.Extensions.Options;

namespace HomeMeters2.API.Tests.Unit.Services.PublicIds;

public class PublicIdGeneratorTests
{
    [TestCase(0, 0, 0, "bA3aB2aa")]
    [TestCase(1, 0, 0, "2B11c23c")]
    [TestCase(1, 9999, 9998, "Ac21bAca3AC1Bb")]
    [TestCase(12, 13, 14, "caABAbCb2")]
    [TestCase(9999, 1, 1243, "B3A2bBCCaa222")]
    public void encode_int_ids(int id1, int id2, int id3, string expected)
    {
        var sut = new PublicIdGenerator(new OptionsWrapper<SqidsConfiguration>(new SqidsConfiguration
        {
            Alphabet = "abcABC123",
            MinimumLength = 8
        }));

        var result = sut.Encode(id1, id2, id3);

        result.Should().Be(expected);
    }
    
    [TestCase(0, 0, 0, "bA3aB2aa")]
    [TestCase(1, 0, 0, "2B11c23c")]
    [TestCase(1, 9999, 9998, "Ac21bAca3AC1Bb")]
    [TestCase(12, 13, 14, "caABAbCb2")]
    [TestCase(9999, 1, 1243, "B3A2bBCCaa222")]
    public void decode_int_ids(int id1, int id2, int id3, string encoded)
    {
        var sut = new PublicIdGenerator(new OptionsWrapper<SqidsConfiguration>(new SqidsConfiguration
        {
            Alphabet = "abcABC123",
            MinimumLength = 8
        }));

        var result = sut.Decode(encoded);

        result.Should().BeEquivalentTo(new int[] { id1, id2, id3 });
    }
    
    [TestCase(0, 0, 0, "bA3aB2aa")]
    [TestCase(1, 0, 0, "2B11c23c")]
    [TestCase(1, 99999999999, 99999999998, "Ac2cbBc2bBBCbaaa3BCcB3CccbCaab")]
    [TestCase(1200000000000, 1300000000000, 14, "cba32A2AA21CCCCB11a1bB312cc333Cb2")]
    [TestCase(99999999999, 1, 1243567890, "2Ba3Bca33Aa2221CcAA12abBcC11")]
    public void encode_long_ids(long id1, long id2, long id3, string expected)
    {
        var sut = new PublicIdGenerator(new OptionsWrapper<SqidsConfiguration>(new SqidsConfiguration
        {
            Alphabet = "abcABC123",
            MinimumLength = 8
        }));

        var result = sut.EncodeLong(id1, id2, id3);

        result.Should().Be(expected);
    }
    
    [TestCase(0, 0, 0, "bA3aB2aa")]
    [TestCase(1, 0, 0, "2B11c23c")]
    [TestCase(1, 99999999999, 99999999998, "Ac2cbBc2bBBCbaaa3BCcB3CccbCaab")]
    [TestCase(1200000000000, 1300000000000, 14, "cba32A2AA21CCCCB11a1bB312cc333Cb2")]
    [TestCase(99999999999, 1, 1243567890, "2Ba3Bca33Aa2221CcAA12abBcC11")]
    public void decode_long_ids(long id1, long id2, long id3, string encoded)
    {
        var sut = new PublicIdGenerator(new OptionsWrapper<SqidsConfiguration>(new SqidsConfiguration
        {
            Alphabet = "abcABC123",
            MinimumLength = 8
        }));

        var result = sut.DecodeLong(encoded);

        result.Should().BeEquivalentTo(new long[] { id1, id2, id3 });
    }
}