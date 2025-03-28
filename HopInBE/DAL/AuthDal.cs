using AutoMapper;
using HopInBE.DataAccess;
using HopInBE.DataAccess.DBCommon;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.Database_Model;
using HopInBE.ResponseModel;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static HopInBE.RequestModel.OTP;
using static HopInBE.RequestModel.Register;

namespace HopInBE.DAL
{
    public class AuthDal
    {
        private readonly IBaseRepository<User> _users;
        private readonly IBaseRepository<Driver> _drivers;
        private readonly Random _random = new Random();
        private readonly IDbSettings dbSettings;
        private IHttpContextAccessor httpContextAccessor;
        public readonly IMapper mapper;

        public AuthDal(IDbSettings dbSettings, IMapper mapperObj, IHttpContextAccessor httpContextAccessor) 
        {
            this.dbSettings = dbSettings;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapperObj;
            _users = Factory<User>.getInstance(dbSettings);
            _drivers = Factory<Driver>.getInstance(dbSettings); 
        }

        public async Task<string> register(RegistrationRequest request)
        {
            if (request.Role == "User")
            {
                var existingUser = await _users.collection.Aggregate().Match(u => u.mobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (existingUser != null) return ("User already registered");

                var newUser = new User
                {
                    userName = request.Name,
                    mobileNumber = request.MobileNumber,
                    ridePin = request.RidePin,
                    otp = string.Empty
                };
                await _users.collection.InsertOneAsync(newUser);
                return ("User registered successfully");
            }
            else if (request.Role == "Driver")
            {
                var existingDriver = await _drivers.collection.Aggregate().Match(d => d.mobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (existingDriver != null) return ("Driver already registered");

                var newDriver = new Driver
                {
                    driverName = request.Name,
                    mobileNumber = request.MobileNumber,
                    ridePin = request.RidePin,
                    otp = string.Empty,
                    vehicleType = request.VehicleType,
                    vehicleNumber = request.VehicleNumber,
                    upiId = request.UpiId
                };
                await _drivers.collection.InsertOneAsync(newDriver);
                return ("Driver registered successfully");
            }
            return ("Invalid Role");
        }

        public async Task<string> requestotp(OtpRequest request)
        {
            var otp = _random.Next(100000, 999999).ToString();
            if (request.Role == "User")
            {
                var user = await _users.collection.Aggregate().Match(u => u.mobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (user == null)
                {
                    user = new User { mobileNumber = request.MobileNumber, otp = otp };
                    await _users.collection.InsertOneAsync(user);
                }
                else
                {
                    var update = Builders<User>.Update.Set(u => u.otp, otp);
                    await _users.collection.UpdateOneAsync(u => u.mobileNumber == request.MobileNumber, update);
                }
            }
            else if (request.Role == "Driver")
            {
                var driver = await _drivers.collection.Aggregate().Match(d => d.mobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (driver == null)
                {
                    driver = new Driver { mobileNumber = request.MobileNumber, otp = otp };
                    await _drivers.collection.InsertOneAsync(driver);
                }
                else
                {
                    var update = Builders<Driver>.Update.Set(d => d.otp, otp);
                    await _drivers.collection.UpdateOneAsync(d => d.mobileNumber == request.MobileNumber, update);
                }
            }
            //Console.WriteLine($"OTP for {request.MobileNumber}: {otp}");
            return ("OTP sent successfully");
        }

        public async Task<ServiceResponse<string>> verifyotp(OtpVerificationRequest request)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            if (request.Role == "User")
            {
                var user = await _users.collection.Aggregate().Match(u => u.mobileNumber == request.MobileNumber && u.otp == request.Otp).FirstOrDefaultAsync();
                if (user == null)
                {
                    serviceResponse.Status = HttpStatusCode.Unauthorized;
                    serviceResponse.Message = "Invalid OTP";
                    return serviceResponse;
                }
                var token = GenerateJwtToken(user.mobileNumber, "User");
                serviceResponse.Status = HttpStatusCode.OK;
                serviceResponse.Message = "token";
                return serviceResponse;
            }
            else if (request.Role == "Driver")
            {
                var driver = await _drivers.collection.Aggregate().Match(d => d.mobileNumber == request.MobileNumber && d.otp == request.Otp).FirstOrDefaultAsync();
                if (driver == null)
                {
                    serviceResponse.Status = HttpStatusCode.Unauthorized;
                    serviceResponse.Message = "Invalid OTP";
                    return serviceResponse;
                }
                var token = GenerateJwtToken(driver.mobileNumber, "Driver");
                serviceResponse.Status = HttpStatusCode.OK;
                serviceResponse.Message = "token";
                return serviceResponse;
            }
            serviceResponse.Status = HttpStatusCode.BadRequest;
            serviceResponse.Message = "Invalid Role";
            return serviceResponse;

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
