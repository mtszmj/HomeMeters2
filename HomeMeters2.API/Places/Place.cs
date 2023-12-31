namespace HomeMeters2.API.Places;

public class Place
{
    private Place()
    {
    }

    public Place(string name, string description, DateTime dateCreatedUtc)
    {
        Name = name;
        Description = description;
        DateCreatedUtc = dateCreatedUtc;
    }

    public int Id { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime DateCreatedUtc { get; private set; }
    public DateTime? DateModifiedUtc { get; private set; }
    public bool IsSoftDeleted { get; private set; }
    public DateTime? DateSoftDeletedUtc { get; private set; }

    public void Delete()
    {
        IsSoftDeleted = true;
        DateSoftDeletedUtc = TimeProvider.System.GetUtcNow().UtcDateTime;
    }

    public bool Update(string? name, string? description)
    {
        if (Name.Equals(name) && Description.Equals(description))
            return false;
        
        Name = name ?? Name;
        Description = description ?? Description;
        DateModifiedUtc = TimeProvider.System.GetUtcNow().UtcDateTime;

        return true;
    }
}
