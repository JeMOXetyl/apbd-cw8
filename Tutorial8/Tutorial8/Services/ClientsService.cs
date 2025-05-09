using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True";

    public async Task<List<ClientTripDTO>> GetClientTrips(int idClient)
    {
        var trips = new List<ClientTripDTO>();
        
        string command = @"
                SELECT ct.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, ct.RegisteredAt, ct.PaymentDate
                FROM Client_Trip ct
                JOIN Trip t ON ct.IdTrip = t.IdTrip
                WHERE ct.IdClient = @idClient";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@idClient", idClient);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int idTrip = reader.GetInt32(0);
                        var clientTrip = new ClientTripDTO()
                        {
                            IdTrip = idTrip,
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            RegisteredAt = reader.GetInt32(5),
                            PaymentDate = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                        };
                        trips.Add(clientTrip);
                    }
                }
            }

            return trips;
        }
    }

    public async Task<int> CreateClient(ClientDTO client)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand();
        
        com.Connection = con;
        string command = @"
            INSERT INTO Client(Firstname, Lastname, Email, Telephone, Pesel)
            VALUES(@Firstname, @Lastname, @Email, @Telephone, @Pesel);
            SELECT SCOPE_IDENTITY();";

        com.Parameters.AddWithValue("@Firstname", client.FirstName);
        com.Parameters.AddWithValue("@Lastname", client.LastName);
        com.Parameters.AddWithValue("@Email", client.Email);
        com.Parameters.AddWithValue("@Telephone", client.Telephone);
        com.Parameters.AddWithValue("@Pesel", client.Pesel);

        try
        {
            await con.OpenAsync();
            int clientId = Convert.ToInt32(await com.ExecuteScalarAsync());
            return clientId;
        }
        catch (Exception e)
        {
            return -1;
        }
    }

    public async Task<int> RegisterClientTrip(int idClient, int idTrip)
    {
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();
        
        await using var clientQuery = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @idClient", con);
        clientQuery.Parameters.AddWithValue("@idClient", idClient);
        if(await clientQuery.ExecuteScalarAsync() == null) return -1;
        
        await using var tripQuery = new SqlCommand("SELECT 1 FROM Trip WHERE IdTrip = @idTrip", con);
        tripQuery.Parameters.AddWithValue("@idTrip", idTrip);
        if(await clientQuery.ExecuteScalarAsync() == null) return -2;
        
        await using var capacityQuery = new SqlCommand(@"
                    SELECT CASE WHEN COUNT(ct.IdClient) < t.MaxPeople THEN 1 ELSE 0 END
                    FROM Trip t
                    LEFT JOIN Client_Trip ct ON ct.IdTrip = t.IdTrip
                    WHERE t.IdTrip = @idTrip
                    GROUP BY t.MaxPeople", con);
        capacityQuery.Parameters.AddWithValue("@idTrip", idTrip);
        if(await capacityQuery.ExecuteScalarAsync() == null) return -3;
        
        var registeredAt = DateTime.Now.Year * 1000 + DateTime.Now.Month * 100 + DateTime.Now.Day;
        int? paymentDate = null;

        await using var cmd = new SqlCommand(@"
            INSERT INTO ClientTrip(IdClient, IdTrip, RegisteredAt, PaymentDate)
            VALUES(@IdClient, @IdTrip, @RegisteredAt, @PaymentDate)", con);
        
        cmd.Parameters.AddWithValue("@IdClient", idClient);
        cmd.Parameters.AddWithValue("@IdTrip", idTrip);
        cmd.Parameters.AddWithValue("@RegisteredAt", registeredAt);
        cmd.Parameters.AddWithValue("@PaymentDate", paymentDate);
        
        if(await cmd.ExecuteScalarAsync() != null)
            return 1;
        else return -4;
    }

    public async Task<int> RemoveClientTrip(int idClient, int idTrip)
    {
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();
        
        await using var regQuery = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @idClient AND IdTrip = @idTrip", con);
        regQuery.Parameters.AddWithValue("@idClient", idClient);
        regQuery.Parameters.AddWithValue("@idTrip", idTrip);
        if(await regQuery.ExecuteScalarAsync() == null) return -1;

        await using var cmd =
            new SqlCommand("DELETE FROM Client_Trip WHERE IdClient = @idClient AND IdTrip = @idTrip", con);
        cmd.Parameters.AddWithValue("@idClient", idClient);
        cmd.Parameters.AddWithValue("@idTrip", idTrip);
        if (await cmd.ExecuteScalarAsync() != null)
            return 1;
        return -2;
    }
}
