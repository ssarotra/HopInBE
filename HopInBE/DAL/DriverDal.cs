using AutoMapper;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.Database_Model;
using HopInBE.RequestModel;
using HopInBE.ResponseModel;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using HopInBE.DataAccess.DBCommon;
using HopInBE.DataAccess;

namespace HopInBE.DAL
{
    public class DriverDal
    {
        

        private readonly IDbSettings dbSettings;
        private readonly IMapper mapper;
        private IHttpContextAccessor httpContextAccessor;

        private readonly IBaseRepository<Driver> _driver;
        private readonly IBaseRepository<OTPStorage> _otpStorage;
        private readonly IBaseRepository<RideRequest> _rideRequest;

        public DriverDal(IDbSettings dbSettings, IMapper mapperObj, IHttpContextAccessor httpContextAccessor)
        {
            this.dbSettings = dbSettings;
            this.mapper = mapperObj;
            this.httpContextAccessor = httpContextAccessor;
            _driver = Factory<Driver>.getInstance(dbSettings);
            _otpStorage = Factory<OTPStorage>.getInstance(dbSettings);
            _rideRequest = Factory<RideRequest>.getInstance(dbSettings); 
        }

        public async Task<bool> UpdateDriverLocation(string driverId, double latitude, double longitude)
        {
            var filter = Builders<Driver>.Filter.Eq(d => d.Id, driverId);
            var update = Builders<Driver>.Update.Set(d => d.Location, new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(longitude, latitude)));

            var result = await _driver.collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> GenerateAndStoreOtp(string mobileNumber)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            return await StoreOTP(mobileNumber, otp);
        }

        public async Task<string> VerifyOtpAndLogin(string mobileNumber, string otp)
        {
            bool isOtpValid = await VerifyOTP(mobileNumber, otp);
            if (!isOtpValid) return string.Empty;

            var filter = Builders<Driver>.Filter.Eq(d => d.mobileNumber, mobileNumber);
            var driver = await _driver.collection.Find(filter).FirstOrDefaultAsync();
            if (driver == null) return string.Empty;

            var token = GenerateJwtToken(driver.mobileNumber, "Driver");
            var update = Builders<Driver>.Update.Set(d => d.IsOnline, true).Set(d => d.IsAvailable, true);
            await _driver.collection.UpdateOneAsync(filter, update);

            return token;
        }

        public async Task<string> RequestOTP(string mobileNumber)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            await StoreOTP(mobileNumber, otp);
            return otp;
        }

        public async Task<bool> StoreOTP(string mobileNumber, string otp)
        {
            var otpEntry = new OTPStorage { MobileNumber = mobileNumber, OTP = otp, ExpiryTime = DateTime.UtcNow.AddMinutes(5) };
            await _otpStorage.collection.InsertOneAsync(otpEntry);
            return true;
        }

        public async Task<bool> VerifyOTP(string mobileNumber, string otp)
        {
            var filter = Builders<OTPStorage>.Filter.And(
                Builders<OTPStorage>.Filter.Eq(o => o.MobileNumber, mobileNumber),
                Builders<OTPStorage>.Filter.Eq(o => o.OTP, otp),
                Builders<OTPStorage>.Filter.Gt(o => o.ExpiryTime, DateTime.UtcNow)
            );
            var otpEntry = await _otpStorage.collection.Find(filter).FirstOrDefaultAsync();
            return otpEntry != null;
        }

        public async Task<string> LoginDriver(string mobileNumber, string otp)
        {
            bool isOtpValid = await VerifyOTP(mobileNumber, otp);
            if (!isOtpValid) return string.Empty;

            var filter = Builders<Driver>.Filter.Eq(d => d.mobileNumber, mobileNumber);
            var driver = await _driver.collection.Find(filter).FirstOrDefaultAsync();
            if (driver == null) return string.Empty;

            var token = GenerateJwtToken(driver.mobileNumber, "Driver");
            var update = Builders<Driver>.Update.Set(d => d.IsOnline, true).Set(d => d.IsAvailable, driver.AssignedRideId == null);
            await _driver.collection.UpdateOneAsync(filter, update);

            return token;
        }

        public async Task<bool> LogoutDriver(string driverId)
        {
            var filter = Builders<Driver>.Filter.Eq(d => d.Id, driverId);
            var update = Builders<Driver>.Update.Set(d => d.IsOnline, false).Set(d => d.IsAvailable, false);
            var result = await _driver.collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> AssignDriverToRide(string driverId, string rideId)
        {
            var filter = Builders<Driver>.Filter.Eq(d => d.Id, driverId);
            var update = Builders<Driver>.Update.Set(d => d.AssignedRideId, rideId).Set(d => d.IsAvailable, false);
            var result = await _driver.collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> CompleteRide(string driverId)
        {
            // Find the driver
            var driver = await _driver.collection.Find(d => d.Id == driverId).FirstOrDefaultAsync();
            if (driver == null || string.IsNullOrEmpty(driver.AssignedRideId))
                return false; // No ride assigned

            var rideId = driver.AssignedRideId;

            // Mark the ride as completed
            var rideFilter = Builders<RideRequest>.Filter.Eq(r => r.Id, rideId);
            var rideUpdate = Builders<RideRequest>.Update.Set(r => r.Status, "Completed");
            var rideResult = await _rideRequest.collection.UpdateOneAsync(rideFilter, rideUpdate);

            if (rideResult.ModifiedCount > 0)
            {
                // Process payment
                await ProcessPayment(rideId);

                // Check if a second ride was pre-assigned and is waiting
                var secondRideFilter = Builders<RideRequest>.Filter.And(
                    Builders<RideRequest>.Filter.Eq(r => r.AssignedDriverId, driverId),
                    Builders<RideRequest>.Filter.Eq(r => r.Status, "Waiting") // Status for pre-assigned ride
                );

                var secondRide = await _rideRequest.collection.Find(secondRideFilter).FirstOrDefaultAsync();

                if (secondRide != null)
                {
                    // Mark the second ride as ready to start
                    var updateSecondRide = Builders<RideRequest>.Update.Set(r => r.Status, "Assigned");
                    await _rideRequest.collection.UpdateOneAsync(secondRideFilter, updateSecondRide);
                }
                else
                {
                    // Mark the driver as available only if no second ride is assigned
                    var driverFilter = Builders<Driver>.Filter.Eq(d => d.Id, driverId);
                    var driverUpdate = Builders<Driver>.Update.Set(d => d.AssignedRideId, null).Set(d => d.IsAvailable, true);
                    await _driver.collection.UpdateOneAsync(driverFilter, driverUpdate);
                }

                return true;
            }

            return false;
        }

        // Dummy function for payment processing
        private async Task<bool> ProcessPayment(string rideRequestId)
        {
            // Add actual payment processing logic here
            return true;
        }


        public async Task<bool> UpdateDriverAvailability(string driverId, double estimatedTimeRemaining)
        {
            var filter = Builders<Driver>.Filter.Eq(d => d.Id, driverId);
            bool isAvailable = estimatedTimeRemaining <= 5; // Mark available if 5 minutes or less remain

            var update = Builders<Driver>.Update.Set(d => d.IsAvailable, isAvailable);
            var result = await _driver.collection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> AssignRideToDriver(string driverId, string rideRequestId)
        {
            var filter = Builders<Driver>.Filter.And(
                Builders<Driver>.Filter.Eq(d => d.Id, driverId),
                Builders<Driver>.Filter.Eq(d => d.IsAvailable, true) // Only assign if available
            );

            var rideFilter = Builders<RideRequest>.Filter.Eq(r => r.Id, rideRequestId);
            var rideUpdate = Builders<RideRequest>.Update.Set(r => r.AssignedDriverId, driverId).Set(r => r.Status, "Assigned");

            var driverUpdate = Builders<Driver>.Update.Set(d => d.IsAvailable, false); // Mark unavailable once assigned

            var rideResult = await _rideRequest.collection.UpdateOneAsync(rideFilter, rideUpdate);
            var driverResult = await _driver.collection.UpdateOneAsync(filter, driverUpdate);

            return rideResult.ModifiedCount > 0 && driverResult.ModifiedCount > 0;
        }

        public async Task<bool> StartRide(string rideRequestId, string enteredPin)
        {
            var ride = await _rideRequest.collection.Find(r => r.Id == rideRequestId).FirstOrDefaultAsync();
            if (ride == null || ride.Status != "Assigned") return false;

            if (ride.PIN != enteredPin) return false; // Ensure the correct PIN is entered

            var filter = Builders<RideRequest>.Filter.Eq(r => r.Id, rideRequestId);
            var update = Builders<RideRequest>.Update.Set(r => r.Status, "Ongoing");

            var result = await _rideRequest.collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }


        private string GenerateJwtToken(string mobileNumber, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.AppSetting("JwtKey", "AppSettings").ToString()));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("MobileNumber", mobileNumber),
                new Claim("Role", role)
            };

            var token = new JwtSecurityToken(
                ConfigHelper.AppSetting("JwtIssuer", "AppSettings").ToString(),
                ConfigHelper.AppSetting("JwtIssuer", "AppSettings").ToString(),
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
