using Kolokwium1Sol.Models_DTOs;

namespace Kolokwium1Sol.Services;

public interface IDBService
{
    Task<GetBookingDTO> GetBookingByIdAsync(int id);
    Task AddBookingAsync(AddBookingDTO bookingInput);
}