using System.Data.Common;
using Kolokwium1Sol.Models_DTOs;
using Microsoft.Data.SqlClient;

namespace Kolokwium1Sol.Services;

public class DBService : IDBService
{
    private readonly string _connectionString;
    
    public DBService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }

    public async Task<GetBookingDTO> GetBookingByIdAsync (int bookingId)
    {
        string query = @"
            SELECT b.date, g.first_name AS 'g.first_name', g.last_name AS 'g.last_name', g.date_of_birth, e.first_name AS 'e.first_name', e.last_name AS 'e.last_name', e.employee_number, a.name, a.price, ba.amount
            FROM Booking b
                INNER JOIN Guest g on g.guest_id = b.guest_id
                INNER JOIN Employee e on e.employee_id = b.employee_id
                INNER JOIN Booking_Attraction ba on ba.booking_id = b.booking_id
                INNER JOIN Attraction a on a.attraction_id = ba.attraction_id
            WHERE b.booking_id = @BookingId;";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();

        command.Parameters.AddWithValue("@BookingId", bookingId);
        var reader = await command.ExecuteReaderAsync();

        GetBookingDTO? bookingInfo = null;
        
        while (await reader.ReadAsync())
        {
            if (bookingInfo is null)
            {
                bookingInfo = new GetBookingDTO()
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("date")),
                    Guest = new Guest
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("g.first_name")),
                        LastName = reader.GetString(reader.GetOrdinal("g.last_name")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                    },
                    Employee = new Employee
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("e.first_name")),
                        // FirstName = reader.GetString(4),
                        LastName = reader.GetString(reader.GetOrdinal("e.last_name")),
                        // LastName = reader.GetString(5),
                        EmployeeNumber = reader.GetString(reader.GetOrdinal("employee_number")),
                    },
                    Attractions = new List<Attraction>(),
                };
            }

            bookingInfo.Attractions.Add(
                new Attraction
                {
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Amount = reader.GetInt32(reader.GetOrdinal("amount")),
                });
        }
        
        if (bookingInfo is null)
            throw new ArgumentException("Booking not found. ");

        return bookingInfo;
    }



    public async Task AddBookingAsync (AddBookingDTO bookingInput)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();


        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;


        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Guest WHERE guest_id = @GuestId;";
            command.Parameters.AddWithValue("@GuestId", bookingInput.GuestId);
            var guestRes = await command.ExecuteScalarAsync();
            if (guestRes is null)
                throw new ArgumentException($"No GUEST with ID {bookingInput.GuestId} found.");
            
            
            command.Parameters.Clear();
            command.CommandText = "SELECT employee_id FROM Employee WHERE employee_number = @EmployeeNumber;";
            command.Parameters.AddWithValue("@EmployeeNumber", bookingInput.EmployeeNumber);
            var employeeId = await command.ExecuteScalarAsync();
            if (employeeId is null)
                throw new ArgumentException($"No EMPLOYEE with NUMBER {bookingInput.EmployeeNumber} found.");
            
            
            command.Parameters.Clear();
            command.CommandText = @"
                    INSERT INTO Booking
                    VALUES (@BookingId, @GuestId, @EmployeeId, @Date);";
            
            command.Parameters.AddWithValue("@BookingId", bookingInput.BookingId);
            command.Parameters.AddWithValue("@GuestId", bookingInput.GuestId);
            command.Parameters.AddWithValue("@EmployeeId", employeeId);
            command.Parameters.AddWithValue("@Date", DateTime.Now);
            
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("BOOKING with the same ID already exists. ");
            }
            
            
            foreach (var attraction in bookingInput.Attractions)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT attraction_id FROM Attraction WHERE name = @name;";
                command.Parameters.AddWithValue("@name", attraction.Name);
                
                var attractionId = await command.ExecuteScalarAsync();
                if (attractionId is null)
                    throw new ArgumentException($"ATTRACTION {attraction.Name} was not found.");
                
                
                command.Parameters.Clear();
                command.CommandText = @"
                    INSERT INTO Booking_Attraction 
                    VALUES (@BookingId, @AttractionId, @Amount);";
                command.Parameters.AddWithValue("@BookingId", bookingInput.BookingId);
                command.Parameters.AddWithValue("@AttractionId", attractionId);
                command.Parameters.AddWithValue("@Amount", attraction.Amount);
                
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}