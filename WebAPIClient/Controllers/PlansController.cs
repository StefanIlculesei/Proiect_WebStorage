using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Accessors;
using WebAPIClient.DTOs;

namespace WebAPIClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly PlanAccessor _planAccessor;
        private readonly IMapper _mapper;

        public PlansController(PlanAccessor planAccessor, IMapper mapper)
        {
            _planAccessor = planAccessor;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await _planAccessor.GetAllAsync();
            var response = _mapper.Map<IEnumerable<PlanResponse>>(plans);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlanById(int id)
        {
            var plan = await _planAccessor.GetByIdAsync(id);
            if (plan == null)
            {
                return NotFound(new { Message = "Plan not found" });
            }

            var response = _mapper.Map<PlanResponse>(plan);
            return Ok(response);
        }
    }
}
