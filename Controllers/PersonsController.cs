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
        public async Task<IActionResult> GetAllPersons(CancellationToken cancellationToken)
        {
            var people = await _repo.GetAllPersonsAsync(cancellationToken);
            var dtos = people.Select(PersonResponseDto.FromPerson).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPersonById(int id, CancellationToken cancellationToken)
        {
            var p = await _repo.GetPersonByIdAsync(id, cancellationToken) ?? throw new PersonNotFoundException(id);
            var dto = PersonResponseDto.FromPerson(p);
            return Ok(dto);
        }

        [HttpGet("color/{colorName}")]
        public async Task<IActionResult> GetPersonsByColorName(string colorName, CancellationToken cancellationToken)
        {
            // Try to parse the color name to enum
            if (!Enum.TryParse<ColorEnum>(colorName, ignoreCase: true, out var colorEnum))
            {
                throw new ColorNotFoundInTheListException(colorName);
            }

            // Convert enum to integer value
            int colorValue = (int)colorEnum;

            var people = await _repo.GetAllPersonsAsync(cancellationToken);
            var filtered = people.Where(p => p.Color == colorValue).ToList();
            var dtos = filtered.Select(PersonResponseDto.FromPerson).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePerson([FromBody] CreatePersonRequest request, CancellationToken cancellationToken)
        {
            // Validate request
            var validationErrors = new List<string>();

            if (request == null)
            {
                throw new InvalidPersonDataException("Request body is empty!");
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) && string.IsNullOrWhiteSpace(request.LastName))
            {
                validationErrors.Add("At least one of FirstName or LastName is required.");
            }

            if (request.Color.HasValue && (request.Color < 1 || request.Color > 7))
            {
                validationErrors.Add("Color must be between 1 and 7.");
            }

            if (validationErrors.Any())
            {
                throw new InvalidPersonDataException(validationErrors);
            }

            var person = new Person
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
                Color = request.Color
            };

            var createdPerson = await _repo.AddPersonAsync(person, cancellationToken);
            var dto = PersonResponseDto.FromPerson(createdPerson);
            return CreatedAtAction(nameof(GetPersonById), new { id = createdPerson.Id }, dto);
        }
    }
}