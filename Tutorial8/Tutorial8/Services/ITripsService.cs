﻿using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<Dictionary<int, TripDTO>> GetTrips();
}