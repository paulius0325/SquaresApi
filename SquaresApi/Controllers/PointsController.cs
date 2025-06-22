using Microsoft.AspNetCore.Mvc;
using SquaresApi.Dtos.PointDtos;
using SquaresApi.Service;

namespace SquaresApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointsController : ControllerBase
    {
        private readonly IPointService _pointService;
        private readonly ILogger<PointsController> _logger;

        public PointsController(IPointService pointService, ILogger<PointsController> logger)
        {
            _pointService = pointService;
            _logger = logger;
        }

        // Imports a batch of 2D points to the system from the provided list 
        [HttpPost("import")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportPointsAsync([FromBody] List<ImportPointDto> importPoints)
        {
            var response = await _pointService.ImportListOfPointsAsync(importPoints);

            if (!response.Success)
                return Conflict(new { Message = response.Error }); // 409 for duplicates

            return Ok(response);
        }

        // Adds a single 2D point to the database
        [HttpPost("add")]
        public async Task<IActionResult> AddPointAsync([FromBody] ImportPointDto newPoint)
        {
            var response = await _pointService.AddPointAsync(newPoint);

            if (!response.Success)
                return Conflict(new { Message = response.Error }); // 409 for duplicates

            return Ok(response);
        }

        // Returns all valid squares formed by existing 2D points
        [HttpGet]
        [ProducesResponseType(typeof(List<List<PointDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPossibleSquaresAsync()
        {
            try
            {
                var squares = await _pointService.GetSquaresAsync();
                return Ok(squares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve squares.");
                return StatusCode(500, ex.Message);
            }
        }

        // Deletes a specific point by its coordinates, if it exists
        [HttpDelete("coordinates")] 
        public async Task<IActionResult> DeletePointByCoordinatesAsync([FromQuery] int x, [FromQuery] int y)
        {
            try
            {
                var isDeleted = await _pointService.DeletePointByCoordinatesAsync(x, y);
                if (!isDeleted.Success)
                    return NotFound(new { Message = isDeleted.Error ?? $"Point with coordinates ({x},{y}) does not exist in database" });

                return Ok(isDeleted);
            }
            catch (Exception ex)
            {
                return BadRequest(new{Message = ex.Message});
            }
        }
    }
}
