namespace HomeMeters2.API.Places;

public class InMemoryTestData
{
    public List<Place> Places { get; } = new List<Place>()
    {
        new Place("Place 1", "", new DateTime(2023, 01, 01, 10, 0, 0))
        {
            Id = 1,
        },
        new Place("Place 2", "", new DateTime(2023, 02, 02, 10, 0, 0))
        {
            Id = 2,
        },
        new Place("Place 3", "", new DateTime(2023, 03, 03, 10, 0, 0))
        {
            Id = 3,
        },
    };

    public InMemoryTestData()
    {
        Places.Last().Delete();
    }
}