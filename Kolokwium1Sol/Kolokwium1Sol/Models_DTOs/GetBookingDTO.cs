namespace Kolokwium1Sol.Models_DTOs;

public class GetBookingDTO
{
    public DateTime Date { get; set; }
    public Guest Guest { get; set; }
    public Employee Employee { get; set; }
    public List<Attraction> Attractions { get; set; }
}

public class Guest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class Employee
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
}

public class Attraction
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Amount { get; set; }
}