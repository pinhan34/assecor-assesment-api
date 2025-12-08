# Assecor Assessment API

A RESTful API for managing person records with support for both CSV file storage and SQLite database.

## Features

- **Dual Data Sources**: Switch between CSV file and SQLite database storage via "UseDatabase": true in the appsettings.Development.json
- **RESTful Endpoints**: Full CRUD operations for person management
- **Color-based Filtering**: Query persons by German color names
- **Comprehensive Exception Handling**: Custom exceptions with detailed error responses
- **Data Transfer Objects (DTOs)**: Clean API responses with proper data formatting
- **Address Parsing**: Automatic extraction of zip code and city from addresses

## Tech Stack

- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 9.0**: ORM for database operations
- **SQLite**: Lightweight database
- **xUnit**: Testing framework

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/pinhan34/assecor-assesment-api.git
cd assecor-assesment-api
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

### Configuration

Edit `appsettings.Development.json` to configure the data source:

```json
{
  "UseDatabase": true,  // Set to false to use CSV file
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=persons.db"
  },
  "PersonsCsvPath": "data/sample-input.csv"
}
```

### Running the Application

```bash
dotnet run
```

The API will be available at `http://localhost:5033`

## API Endpoints

### Get All Persons
```http
GET /persons
```

### Get Person by ID
```http
GET /persons/{id}
```

### Get Persons by Color
```http
GET /persons/color/{colorName}
```
**Valid color names**: Blau, Grün, Violett, Rot, Gelb, Türkis, Weiß (case-insensitive)

### Create Person
```http
POST /persons
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "address": "12345 Main Street",
  "color": 1
}
```

**Color values**: 1=Blau, 2=Grün, 3=Violett, 4=Rot, 5=Gelb, 6=Türkis, 7=Weiß

## Response Format

```json
{
  "id": 1,
  "name": "John",
  "lastname": "Doe",
  "zipcode": "12345",
  "city": "Main Street",
  "color": "blau"
}
```

## Testing

Run all tests:
```bash
dotnet test
```

Run tests with detailed output:
```bash
dotnet test --verbosity normal
```

**Test Coverage**:
- 31 unit tests
- Controller tests (10 tests)
- CSV repository tests (8 tests)
- Database repository tests (10 tests)
- Exception handling tests (3 tests)

## Project Structure

```
assecor-assesment-api/
├── Controllers/          # API controllers
├── Data/                # Data access layer (repositories, DbContext)
├── Exceptions/          # Custom exception classes
├── Models/              # Domain models and DTOs
├── Properties/          # Launch settings
├── data/                # Sample CSV data
├── tests/               # Unit tests
│   └── UnitTests/       # xUnit test project
├── appsettings.json     # Configuration
└── Program.cs           # Application entry point
```

## Exception Handling

The API provides detailed error responses:

- **404 Not Found**: Person or color not found
- **400 Bad Request**: Invalid person data (validation errors)
- **500 Internal Server Error**: CSV file access errors or database errors

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is part of an assessment for Assecor.
