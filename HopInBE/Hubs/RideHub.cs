using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HopInBE.Database_Model;
using AutoMapper;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.DBCommon;

namespace HopInBE.Hubs
{
    public class RideHub : Hub
    {
        private readonly IBaseRepository<RideRequest> _rideRequests;
        private readonly IBaseRepository<DriverLocation> _driverLocations;
        private static readonly ConcurrentDictionary<string, bool> RideLocks = new();
        private readonly IDbSettings dbSettings;
        private readonly IMapper mapper;

        public RideHub(IDbSettings dbSettings, IMapper mapperObj)
        {
            this.dbSettings = dbSettings;
            this.mapper = mapperObj;
            _rideRequests = Factory<RideRequest>.getInstance(dbSettings);
            _driverLocations = Factory<DriverLocation>.getInstance(dbSettings);
        }

        public async Task RequestRide(string userId, double userLatitude, double userLongitude)
        {
            var rideRequest = new RideRequest
            {
                UserId = userId,
                PickupLatitude = userLatitude,
                PickupLongitude = userLongitude,
                Status = "Pending"
            };
            await _rideRequests.collection.InsertOneAsync(rideRequest);

            var availableDrivers = await _driverLocations.collection
                .Find(d => d.IsAvailable)
                .ToListAsync();

            var nearestDrivers = availableDrivers
                .OrderBy(d => GetDistance(userLatitude, userLongitude, d.Latitude, d.Longitude))
                .Take(5)
                .ToList();

            foreach (var driver in nearestDrivers)
            {
                await Clients.Client(driver.DriverId).SendAsync("RideRequest", rideRequest.Id, userLatitude, userLongitude);
            }
        }

        public async Task AcceptRide(string driverId, string rideRequestId)
        {
            if (!RideLocks.TryAdd(rideRequestId, true))
            {
                await Clients.Caller.SendAsync("RideAlreadyTaken");
                return;
            }

            var rideRequest = await _rideRequests.collection
                .Find(r => r.Id == rideRequestId && r.Status == "Pending")
                .FirstOrDefaultAsync();

            if (rideRequest == null)
            {
                await Clients.Caller.SendAsync("RideNotAvailable");
                return;
            }

            var update = Builders<RideRequest>.Update
                .Set(r => r.Status, "Assigned")
                .Set(r => r.AssignedDriverId, driverId);

            await _rideRequests.collection.UpdateOneAsync(r => r.Id == rideRequestId, update);

            await Clients.User(rideRequest.UserId).SendAsync("RideConfirmed", driverId);
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var d1 = lat1 * (Math.PI / 180.0);
            var d2 = lat2 * (Math.PI / 180.0);
            var longDiff = (lon2 - lon1) * (Math.PI / 180.0);
            var distance = Math.Sin(d1) * Math.Sin(d2) + Math.Cos(d1) * Math.Cos(d2) * Math.Cos(longDiff);
            return Math.Acos(distance) * 6371; // Distance in km
        }
    }
}
