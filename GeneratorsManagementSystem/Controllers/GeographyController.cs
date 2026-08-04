using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class GeographyController : Controller
    {
        private readonly IGeographyService _service;

        public GeographyController(IGeographyService service)
        {
            _service = service;
        }

        // GET: /Geography/GetDistricts?governorateId=1
        [HttpGet]
        public async Task<IActionResult> GetDistricts(int governorateId)
        {
            var districts = await _service.GetDistrictsByGovernorateAsync(governorateId);
            return Json(districts.Select(d => new
            {
                id = d.Id,
                name = d.Name
            }));
        }

        // GET: /Geography/GetNeighborhoods?districtId=1
        [HttpGet]
        public async Task<IActionResult> GetNeighborhoods(int districtId)
        {
            var neighborhoods = await _service.GetNeighborhoodsByDistrictAsync(districtId);
            return Json(neighborhoods.Select(n => new
            {
                id = n.Id,
                name = n.Name
            }));
        }

        // GET: /Geography/GetAlleys?neighborhoodId=1
        [HttpGet]
        public async Task<IActionResult> GetAlleys(int neighborhoodId)
        {
            var alleys = await _service.GetAlleysByNeighborhoodAsync(neighborhoodId);
            return Json(alleys.Select(a => new
            {
                id = a.Id,
                name = a.Name
            }));
        }
    }
}