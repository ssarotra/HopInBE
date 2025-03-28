using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Threading.Tasks;
using HopInBE.Database_Model;

namespace HopInBE.Hubs
{
    public class GpsHub : Hub
    {
        private readonly IMongoCollection<DriverLocation> _driverLocations;

        public GpsHub(IMongoClient client)
        {
            var database = client.GetDatabase("RideBookingDB");
            _driverLocations = database.GetCollection<DriverLocation>("DriverLocations");
        }

        public async Task UpdateDriverLocation(string driverId, double latitude, double longitude)
        {
            var filter = Builders<DriverLocation>.Filter.Eq(d => d.DriverId, driverId);
            var update = Builders<DriverLocation>.Update
                .Set(d => d.Latitude, latitude)
                .Set(d => d.Longitude, longitude)
                .Set(d => d.IsAvailable, true);

            await _driverLocations.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

            await Clients.All.SendAsync("DriverLocationUpdated", driverId, latitude, longitude);
        }

        public async Task RequestNearestDriver(double userLatitude, double userLongitude)
        {
            var availableDrivers = await _driverLocations.Find(d => d.IsAvailable).ToListAsync();

            var nearestDriver = availableDrivers
                .OrderBy(d => GetDistance(userLatitude, userLongitude, d.Latitude, d.Longitude))
                .FirstOrDefault();

            if (nearestDriver != null)
            {
                await Clients.Caller.SendAsync("NearestDriverFound", nearestDriver.DriverId, nearestDriver.Latitude, nearestDriver.Longitude);
            }
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var d1 = lat1 * (System.Math.PI / 180.0);
            var d2 = lat2 * (System.Math.PI / 180.0);
            var longDiff = (lon2 - lon1) * (System.Math.PI / 180.0);
            var distance = System.Math.Sin(d1) * System.Math.Sin(d2) + System.Math.Cos(d1) * System.Math.Cos(d2) * System.Math.Cos(longDiff);
            return System.Math.Acos(distance) * 6371; // Returns distance in km
        }
    }
}
