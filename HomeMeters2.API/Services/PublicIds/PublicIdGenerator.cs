using Microsoft.Extensions.Options;
using Sqids;

namespace HomeMeters2.API.Services.PublicIds;

public class PublicIdGenerator
{
    private readonly SqidsEncoder<int> _intEncoder;
    private readonly SqidsEncoder<long> _longEncoder;

    public PublicIdGenerator(IOptions<SqidsConfiguration> configuration)
    {
        _intEncoder = new SqidsEncoder<int>(new SqidsOptions
        {
            Alphabet = configuration.Value.Alphabet,
            MinLength = configuration.Value.MinimumLength,
        });
        
        _longEncoder = new SqidsEncoder<long>(new SqidsOptions
        {
            Alphabet = configuration.Value.Alphabet,
            MinLength = configuration.Value.MinimumLength,
        });
    }

    public string Encode(params int[] numbers)
    {
        return _intEncoder.Encode(numbers);
    }

    public string EncodeLong(params long[] numbers)
    {
        return _longEncoder.Encode(numbers);
    }

    public IReadOnlyList<int> Decode(ReadOnlySpan<char> id)
    {
        return _intEncoder.Decode(id);
    }

    public IReadOnlyList<long> DecodeLong(ReadOnlySpan<char> id)
    {
        return _longEncoder.Decode(id);
    }
}