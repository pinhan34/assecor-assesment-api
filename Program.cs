using assecor_assesment_api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register repository
builder.Services.AddSingleton<IPersonRepository, CsvPersonRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// (No OpenAPI/OpenAPI mapping required for these tests)

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
