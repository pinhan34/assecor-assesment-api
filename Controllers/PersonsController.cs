using assecor_assesment_api.Data;
using assecor_assesment_api.Models;
using Microsoft.AspNetCore.Mvc;
using assecor_assesment_api.Exceptions;

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

        [HttpGet("color/{color:int}")]
        public async Task<IActionResult> GetByColor(int color, CancellationToken cancellationToken)
        {
            // Validate color is in the enumerated range (1-7)
            if (color < 1 || color > 7)
            {
                throw new ColorNotFoundInTheListException(color);
            }

            var people = await _repo.GetAllAsync(cancellationToken);
            var filtered = people.Where(p => p.Color == color).ToList();
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