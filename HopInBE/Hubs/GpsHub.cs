using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Threading.Tasks;
using HopInBE.Database_Model;
using AutoMapper;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.DBCommon;

namespace HopInBE.Hubs
{
    public class GpsHub : Hub
    {
        private readonly IBaseRepository<DriverLocation> _driverLocations;
        private readonly IBaseRepository<GpsHistory> _gpsHistory;
        private readonly IDbSettings dbSettings;
        private IHttpContextAccessor httpContextAccessor;
        public readonly IMapper mapper;

        public GpsHub(IDbSettings dbSettings, IMapper mapperObj, IHttpContextAccessor httpContextAccessor)
        {
            this.dbSettings = dbSettings;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapperObj;
            _driverLocations = Factory<DriverLocation>.getInstance(dbSettings);
            _gpsHistory = Factory<GpsHistory>.getInstance(dbSettings);
        }

        public async Task UpdateDriverLocation(string driverId, double latitude, double longitude)
        {
            var filter = Builders<DriverLocation>.Filter.Eq(d => d.DriverId, driverId);
            var update = Builders<DriverLocation>.Update
                .Set(d => d.Latitude, latitude)
                .Set(d => d.Longitude, longitude)
                .Set(d => d.IsAvailable, true);

            await _driverLocations.collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

            var gpsHistoryEntry = new GpsHistory
            {
                DriverId = driverId,
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.UtcNow
            };
            await _gpsHistory.collection.InsertOneAsync(gpsHistoryEntry);

            await Clients.All.SendAsync("DriverLocationUpdated", driverId, latitude, longitude);
        }

        public async Task RequestNearestDriver(double userLatitude, double userLongitude)
        {
            var availableDrivers = await _driverLocations.collection.Aggregate().Match(d => d.IsAvailable).ToListAsync();

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