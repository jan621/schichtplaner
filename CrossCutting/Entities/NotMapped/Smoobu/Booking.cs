using System.Text.Json.Serialization;

namespace CrossCutting.Entities.NotMapped.Smoobu;

using System;
using System.Collections.Generic;

public class Apartment
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class Channel
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class Related
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class Booking
{
    public int Id { get; set; }
    public object? ReferenceId { get; set; }
    public string? Type { get; set; }
    public string? Arrival { get; set; }
    public string? Departure { get; set; }
    public string? CreatedAt { get; set; }
    public string? ModifiedAt { get; set; }
    public Apartment? Apartment { get; set; }
    public Channel? Channel { get; set; }
    public object? GuestName { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public object? Email { get; set; }
    public object? Phone { get; set; }
    public int? Adults { get; set; }
    public int? Children { get; set; }
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public string? Notice { get; set; }
    public string? AssistantNotice { get; set; }
    public decimal? Price { get; set; }
    public string? PricePaid { get; set; }
    public double? CommissionIncluded { get; set; }
    public int? Prepayment { get; set; }
    public string? PrepaymentPaid { get; set; }
    public int? Deposit { get; set; }
    public string? DepositPaid { get; set; }
    public string? Language { get; set; }
    public string? GuestAppUrl { get; set; }
    
    [JsonPropertyName("is-blocked-booking")]
    public bool IsBlockedBooking { get; set; }
    
    public int? GuestId { get; set; }
    public List<Related>? Related { get; set; }
}

public class RootBooking
{
    public int? page_count { get; set; }
    public int? PageSize { get; set; }
    public int? TotalItems { get; set; }
    public int? Page { get; set; }
    public List<Booking>? Bookings { get; set; }
}
