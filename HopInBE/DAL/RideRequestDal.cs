using AutoMapper;
using HopInBE.DataAccess.DBCommon;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.Database_Model;
using HopInBE.ResponseModel;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HopInBE.DAL
{
    public class RideRequestDal
    {
        private readonly IBaseRepository<RideRequest> _rideRequests;
        private readonly IBaseRepository<DriverLocation> _driverLocations;
        private readonly IDbSettings dbSettings;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        private const double BaseFare = 50.0; // Example base fare
        private const double CostPerKm = 10.0; // Example per km cost
        private const string currency = "INR";

        public RideRequestDal(IDbSettings dbSettings, IMapper mapperObj, IHttpContextAccessor httpContextAccessor)
        {
            this.dbSettings = dbSettings;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapperObj;
            _rideRequests = Factory<RideRequest>.getInstance(dbSettings);
            _driverLocations = Factory<DriverLocation>.getInstance(dbSettings);
        }

        public async Task<FareEstimateResponse> RequestRide(RideRequest request)
        {
            var estimatedFare = CalculateFare(request.PickupLatitude, request.PickupLongitude, request.DropLatitude, request.DropLongitude);
            request.EstimatedFare = estimatedFare.Item1;
            request.Status = "Pending";
            request.RequestTime = DateTime.UtcNow;

            await _rideRequests.collection.InsertOneAsync(request);
            FareEstimateResponse fareEstimateResponse = new FareEstimateResponse(estimatedFare.Item1,estimatedFare.Item2);
            return fareEstimateResponse;
        }

        private (double,double) CalculateFare(double pickupLat, double pickupLon, double dropoffLat, double dropoffLon)
        {
            double distance = GetDistance(pickupLat, pickupLon, dropoffLat, dropoffLon);
            return (BaseFare + (distance * CostPerKm),distance);
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var d1 = lat1 * (Math.PI / 180.0);
            var d2 = lat2 * (Math.PI / 180.0);
            var longDiff = (lon2 - lon1) * (Math.PI / 180.0);
            var distance = Math.Sin(d1) * Math.Sin(d2) + Math.Cos(d1) * Math.Cos(d2) * Math.Cos(longDiff);
            return Math.Acos(distance) * 6371; // Returns distance in km
        }
    }
}
