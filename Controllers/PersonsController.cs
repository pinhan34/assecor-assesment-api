using assecor_assesment_api.Data;
using assecor_assesment_api.Models;
using assecor_assesment_api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace assecor_assesment_api.Controllers
{
    [ApiController]
    [Route("persons")]
    public class PersonsController : ControllerBase
    {
        private readonly IPersonRepository _repo;

        public PersonsController(IPersonRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var people = await _repo.GetAllAsync(cancellationToken);
            return Ok(people);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var p = await _repo.GetByIdAsync(id, cancellationToken);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpGet("color/{colorName}")]
        public async Task<IActionResult> GetByColorName(string colorName, CancellationToken cancellationToken)
        {
            // Try to parse the color name to enum
            if (!Enum.TryParse<ColorEnum>(colorName, ignoreCase: true, out var colorEnum))
            {
                throw new ColorNotFoundInTheListException(colorName);
            }

            // Convert enum to integer value
            int colorValue = (int)colorEnum;

            var people = await _repo.GetAllAsync(cancellationToken);
            var filtered = people.Where(p => p.Color == colorValue).ToList();
            return Ok(filtered);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePerson([FromBody] CreatePersonRequest request, CancellationToken cancellationToken)
        {
            // Validate request
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) && string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest("At least one of FirstName or LastName is required.");
            }

            if (request.Color.HasValue && (request.Color < 1 || request.Color > 7))
            {
                return BadRequest("Color must be between 1 and 7.");
            }

            try
            {
                var person = new Person
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Address = request.Address,
                    Color = request.Color,
                    Group = null
                };

                var createdPerson = await _repo.AddPersonAsync(person, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = createdPerson.Id }, createdPerson);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}