using AutoMapper;
using HopInBE.DAL;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.Database_Model;
using HopInBE.ResponseModel;
using HopInBE.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using HopInBE.RequestModel;

namespace HopInBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RideRequestController : ControllerBase
    {
        private readonly RideRequestDal _rideRequestDal;
        private readonly IMapper _mapper;
        private readonly FareCalculator _fareCalculator;

        public RideRequestController(IDbSettings dbSettings, IMapper mapperObj, IHttpContextAccessor httpContextAccessor)
        {
            _rideRequestDal = new RideRequestDal(dbSettings, mapperObj, httpContextAccessor);
            _mapper = mapperObj;
            _fareCalculator = new FareCalculator();
        }

        [HttpPost("request")]
        public async Task<ServiceResponse<FareEstimateResponse>> RequestRide(RideRequestDto requestDto)
        {
            ServiceResponse<FareEstimateResponse> serviceResponse = new ServiceResponse<FareEstimateResponse>();

            try
            {
                // Calculate Estimated Fare
                double estimatedFare = _fareCalculator.CalculateFare(requestDto.DistanceInKm, DateTime.UtcNow);

                // Map DTO to RideRequest Model
                var rideRequest = _mapper.Map<RideRequest>(requestDto);
                rideRequest.EstimatedFare = estimatedFare;
                rideRequest.Status = "Pending"; // Initial status

                // Store in Database
                var fareEstimate = await _rideRequestDal.RequestRide(rideRequest);

                if (fareEstimate != null)
                {
                    serviceResponse.Status = HttpStatusCode.OK;
                    serviceResponse.Data = fareEstimate;
                    serviceResponse.Message = "Ride request submitted successfully";
                }
                else
                {
                    serviceResponse.Status = HttpStatusCode.BadRequest;
                    serviceResponse.Message = "Failed to submit ride request";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = HttpStatusCode.InternalServerError;
                serviceResponse.Message = $"Error processing ride request: {ex.Message}";
            }

            return serviceResponse;
        }
    }
}
