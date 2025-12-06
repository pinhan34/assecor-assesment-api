using assecor_assesment_api.Data;
using assecor_assesment_api.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register repository
builder.Services.AddSingleton<IPersonRepository, CsvPersonRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Exception handler for ColorNotFoundInTheListException
app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
		var exception = exceptionHandlerPathFeature?.Error;

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

		// For other exceptions, return 500
		context.Response.StatusCode = StatusCodes.Status500InternalServerError;
		context.Response.ContentType = "application/json";
		await context.Response.WriteAsJsonAsync(new { error = "An internal server error occurred." });
	});
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
