namespace Kolokwium1Sol.Models_DTOs;

public class AddBookingDTO
{
    public int BookingId { get; set; }
    public int GuestId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public List<AttractionIn> Attractions { get; set; } =  new List<AttractionIn>();
    
}

public class AttractionIn
{
    public string Name { get; set; } = string.Empty;
    public int Amount { get; set; }
}