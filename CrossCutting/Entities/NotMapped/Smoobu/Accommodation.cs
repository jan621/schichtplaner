namespace CrossCutting.Entities.NotMapped.Smoobu;

public class AccommodationResponse
{
    public List<BasicAccommodation> Apartments { get; set; }
}

public class Accommodation
{
    public int Id { get; set; }
    public Location? Location { get; set; }
    public string? TimeZone { get; set; }
    public Rooms? Rooms { get; set; }
    public List<string>? Equipments { get; set; }
    public string? Currency { get; set; }
    public Price? Price { get; set; }
    public Type? Type { get; set; }
}

public abstract class Location
{
    public string? Street { get; set; }
    public string? Zip { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
}

public abstract class Rooms
{
    public int MaxOccupancy { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int? DoubleBeds { get; set; }
    public int? SingleBeds { get; set; }
    public int? SofaBeds { get; set; }
    public int? Couches { get; set; }
    public int? ChildBeds { get; set; }
    public int? QueenSizeBeds { get; set; }
    public int? KingSizeBeds { get; set; }
}

public abstract class Price
{
    public string? Minimal { get; set; }
    public string? Maximal { get; set; }
}

public abstract class Type
{
    public int Id { get; set; }
    public string? Name { get; set; }
}