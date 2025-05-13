using Kolokwium1Sol.Models_DTOs;
using Kolokwium1Sol.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1Sol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IDBService _idbService;
    
    public BookingsController(IDBService idbService)
    {
        _idbService = idbService;
    }
    
    // api/bookings/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(int id)
    {
        try
        {
            var result = await _idbService.GetBookingByIdAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    
    // api/appointments
    [HttpPost]
    public async Task<IActionResult> AddBooking([FromBody] AddBookingDTO bookingInput)
    {
        if (!bookingInput.Attractions.Any())
            return BadRequest("At least one item is required.");
        
        try
        {
            await _idbService.AddBookingAsync(bookingInput);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException e)
        {
            return Conflict(e.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        
        return CreatedAtAction(nameof(GetBooking), new { id = bookingInput.BookingId }, bookingInput);
    }
}