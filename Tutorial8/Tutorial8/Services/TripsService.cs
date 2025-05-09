using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";
    
    public async Task<Dictionary<int, TripDTO>> GetTrips()
    {
        var trips = new Dictionary<int, TripDTO>();

        string command = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName
            FROM Trip t
            JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
            JOIN Country c ON ct.IdCountry = c.IdCountry
            ORDER BY t.Name DESC";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idTrip = reader.GetInt32(0);
                    if (!trips.TryGetValue(idTrip, out var trip))
                    {
                        trip = new TripDTO()
                        {
                            Id = idTrip,
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            Countries = new List<CountryDTO>()
                        };
                        trips.Add(idTrip, trip);
                    }
                    trip.Countries.Add(new CountryDTO()
                    {
                        Name = reader.GetString(6)
                    });
                }
            }
        }
        

        return trips;
    }
    
}