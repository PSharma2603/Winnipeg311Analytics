using Microsoft.AspNetCore.Mvc;
using Winnipeg311Analytics.Services;

namespace Winnipeg311Analytics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class City311Controller : ControllerBase
    {
        private readonly City311Service _city311Service;

        public City311Controller(
            City311Service city311Service)
        {
            _city311Service = city311Service;
        }


        [HttpGet]
        public async Task<IActionResult> GetServiceRequests(
            [FromQuery] string? ward,
            [FromQuery] string? serviceArea)
        {
            var result =
                await _city311Service
                    .GetServiceRequestsAsync(
                        ward,
                        serviceArea
                    );

            return Ok(result);
        }


        [HttpGet("wards")]
        public async Task<IActionResult> GetWards()
        {
            var result =
                await _city311Service
                    .GetWardsAsync();

            return Ok(result);
        }


        [HttpGet("service-areas")]
        public async Task<IActionResult> GetServiceAreas()
        {
            var result =
                await _city311Service
                    .GetServiceAreasAsync();

            return Ok(result);
        }


        [HttpGet("ward-count")]
        public async Task<IActionResult> GetWardCount(
            [FromQuery] string ward)
        {
            if (string.IsNullOrWhiteSpace(ward))
            {
                return BadRequest(
                    "Ward is required."
                );
            }

            var count =
                await _city311Service
                    .GetWardRequestCountAsync(
                        ward
                    );

            return Ok(count);
        }
    }
}