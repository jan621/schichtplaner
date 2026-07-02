namespace CrossCutting.Entities.NotMapped.Guesty;

public class BasicAccommodation
{
    public string Id { get; set; } = string.Empty;

    public string? Name { get; set; }
}

public class Apartment
{
    public string Id { get; set; } = string.Empty;

    public string? Name { get; set; }
}

public class Booking
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Check-in date in "yyyy-MM-dd" format.</summary>
    public string? Arrival { get; set; }

    /// <summary>Check-out date in "yyyy-MM-dd" format.</summary>
    public string? Departure { get; set; }

    public Apartment? Apartment { get; set; }

    public int? Adults { get; set; }

    public int? Children { get; set; }
}

public class RootBooking
{
    public List<Booking>? Bookings { get; set; }
}
