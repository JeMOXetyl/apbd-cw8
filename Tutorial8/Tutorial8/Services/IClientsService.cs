using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientTripDTO>> GetClientTrips(int idClient);
    Task<int> CreateClient(ClientDTO client);
    Task<int> RegisterClientTrip(int idClient, int idTrip);
    Task<int> RemoveClientTrip(int idClient, int idTrip);
}