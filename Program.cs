using assecor_assesment_api.Data;
using assecor_assesment_api.Exceptions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register DbContext for SQLite
builder.Services.AddDbContext<PersonDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repository based on configuration
var useDatabase = builder.Configuration.GetValue<bool>("UseDatabase");

if (useDatabase)
{
    builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
}
else
{
    builder.Services.AddScoped<IPersonRepository, CsvPersonRepository>();
}

var app = builder.Build();

// Initialize database if using database mode
if (useDatabase)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PersonDbContext>();
        dbContext.Database.EnsureCreated(); // Creates database if it doesn't exist
    }
}

// Configure the HTTP request pipeline.

// Exception handler middleware
app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
		var exception = exceptionHandlerPathFeature?.Error;

		// Handle PersonNotFoundException (404)
		if (exception is PersonNotFoundException personNotFound)
		{
			context.Response.StatusCode = StatusCodes.Status404NotFound;
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsJsonAsync(new
			{
				error = personNotFound.Message,
				requestedId = personNotFound.RequestedId
			});
			return;
		}

		// Handle ColorNotFoundInTheListException (404)
		if (exception is ColorNotFoundInTheListException colorException)
		{
			context.Response.StatusCode = StatusCodes.Status404NotFound;
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsJsonAsync(new
			{
				error = colorException.Message,
				requestedColor = colorException.RequestedColor,
				validColors = new[] { "Blau", "Grün", "Violett", "Rot", "Gelb", "Türkis", "Weiß" }
			});
			return;
		}

		// Handle InvalidPersonDataException (400)
		if (exception is InvalidPersonDataException validationException)
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsJsonAsync(new
			{
				error = "Invalid person data",
				validationErrors = validationException.ValidationErrors
			});
			return;
		}

		// Handle CsvFileException (500)
		if (exception is CsvFileException csvException)
		{
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsJsonAsync(new
			{
				error = "Unable to access person data",
				detail = csvException.Message,
				operation = csvException.Operation.ToString(),
				message = "Please contact the administrator"
			});
			return;
		}

		// For other exceptions, return 500
		context.Response.StatusCode = StatusCodes.Status500InternalServerError;
		context.Response.ContentType = "application/json";
		await context.Response.WriteAsJsonAsync(new { error = "An internal server error occurred." });
	});
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
