using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;

        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient(ClientDTO client)
        {   
            try
            {
                var clientId = await _clientsService.CreateClient(client);
                if (clientId != -1)
                {
                    return Ok(clientId);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return BadRequest();
        }
        
        [HttpGet("{idClient}/trips")]
        public async Task<IActionResult> GetClientTrips(int idClient)
        {
            try
            {
                var trips = await _clientsService.GetClientTrips(idClient);
                if (trips == null || !trips.Any())
                {
                    return NotFound();
                }
                return Ok(trips);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPut("{idClient}/trips/{idTrip}")]
        public async Task<IActionResult> RegisterClientTrip(int idClient, int idTrip)
        {
            if (idClient < 0 && idTrip < 0)
            {
                return BadRequest("Invalid id for client/trip");
            }

            try
            {
                var result = await _clientsService.RegisterClientTrip(idClient, idTrip);
                return result switch
                {
                    -1 => NotFound("Client not found"),
                    -2 => NotFound("Trip not found"),
                    -3 => BadRequest("Trip has full capacity"),
                    -4 => StatusCode(500, "Internal server error"),
                    1 => Ok("Client registered successfully")
                };
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpDelete("{idClient}/trips/{idTrip}")]
        public async Task<IActionResult> RemoveClientTrip(int idClient, int idTrip)
        {
            if (idClient < 0 || idTrip < 0)
                return BadRequest("Invalid id for client/trip");
            try
            {
                var result = await _clientsService.RemoveClientTrip(idClient, idTrip);
                return result switch
                {
                    -1 => NotFound("Registration not found"),
                    -2 => StatusCode(500, "Internal server error"),
                    1 => Ok("Registration deleted successfully")
                };
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}