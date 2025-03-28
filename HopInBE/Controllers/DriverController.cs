using AutoMapper;
using HopInBE.DAL;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.RequestModel;
using HopInBE.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System;
using System.Threading.Tasks;
using static HopInBE.RequestModel.DriverLoginLogout;

namespace HopInBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly DriverDal _driverDal;

        public DriverController(IDbSettings dbSettings, IMapper mapperObj, IHttpContextAccessor httpContextAccessor)
        {
            _driverDal = new DriverDal(dbSettings, mapperObj, httpContextAccessor);
        }

        [HttpPost("updatelocation")]
        public async Task<ServiceResponse<string>> UpdateDriverLocation(DriverLocationUpdateRequest request)
        {
            var response = new ServiceResponse<string>();
            bool isUpdated = await _driverDal.UpdateDriverLocation(request.DriverId, request.Latitude, request.Longitude);

            if (isUpdated)
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "Location updated successfully.";
            }
            else
            {
                response.Status = HttpStatusCode.BadRequest;
                response.Message = "Failed to update location.";
            }

            return response;
        }

        [HttpPost("requestotp")]
        public async Task<ServiceResponse<string>> RequestOtp(DriverOtpRequest request)
        {
            var response = new ServiceResponse<string>();
            bool isOtpSent = await _driverDal.GenerateAndStoreOtp(request.MobileNumber);

            if (isOtpSent)
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "OTP sent successfully.";
            }
            else
            {
                response.Status = HttpStatusCode.BadRequest;
                response.Message = "Failed to send OTP.";
            }

            return response;
        }

        [HttpPost("login")]
        public async Task<ServiceResponse<string>> DriverLogin(DriverLoginRequest request)
        {
            var response = new ServiceResponse<string>();
            var token = await _driverDal.VerifyOtpAndLogin(request.MobileNumber, request.Otp);

            if (!string.IsNullOrEmpty(token))
            {
                response.Status = HttpStatusCode.OK;
                response.Data = token;
                response.Message = "Login successful.";
            }
            else
            {
                response.Status = HttpStatusCode.Unauthorized;
                response.Message = "Invalid OTP.";
            }

            return response;
        }

        [HttpPost("logout")]
        public async Task<ServiceResponse<string>> DriverLogout(DriverLogoutRequest request)
        {
            var response = new ServiceResponse<string>();
            bool isLoggedOut = await _driverDal.LogoutDriver(request.DriverId);

            if (isLoggedOut)
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "Logout successful.";
            }
            else
            {
                response.Status = HttpStatusCode.BadRequest;
                response.Message = "Logout failed.";
            }

            return response;
        }
    }
}
