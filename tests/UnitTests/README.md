# Unit Tests

Comprehensive unit tests for the Assecor Assessment API.

## Overview

This test project contains 31 unit tests covering all major components of the API, including controllers, repositories, and exception handling.

## Test Structure

### PersonsControllerTests (10 tests)
Tests for the API controller endpoints using a fake repository implementation.

**Coverage:**
- `GetAllPersons_ReturnsAllPersons` - Verify all persons are returned
- `GetPersonById_ReturnsPerson_WhenExists` - Verify person lookup by ID
- `GetPersonById_ThrowsPersonNotFoundException_WhenMissing` - Verify 404 exception
- `GetByColorName_ReturnsPersonsWithThatColor` - Verify color-based filtering
- `GetByColorName_IsCaseInsensitive` - Verify case-insensitive color names
- `GetByColorName_ReturnsBadRequestForInvalidColorName` - Verify invalid color exception
- `CreatePerson_CreatesNewPerson` - Verify person creation
- `CreatePerson_ThrowsInvalidPersonDataException_ForMissingNames` - Verify validation
- `CreatePerson_ThrowsInvalidPersonDataException_ForInvalidColor` - Verify color validation
- `CreatePerson_AllowsNullOptionalFields` - Verify optional fields handling

### CsvPersonRepositoryTests (8 tests)
Tests for CSV file-based data access with file exception handling.

**Coverage:**
- `ParseLine_AssignsLineNumberAsId` - Verify ID assignment from line numbers
- `ParseLine_HandlesRowsWithMissingOptionalFields` - Verify nullable fields
- `ParseLine_SkipsRowWithMissingBothNames` - Verify validation logic
- `GetByIdAsync_ReturnsPersonByLineNumber` - Verify person retrieval
- `GetByIdAsync_ReturnsNullForNonexistentId` - Verify null returns
- `ParseLine_HandlesEmptyLines` - Verify empty line handling
- `GetPersonsByColorAsync_ReturnsOnlyMatchingColor` - Verify color filtering
- `GetPersonsByColorAsync_ReturnsEmpty_WhenNoMatches` - Verify empty results
- `GetAllPersonsAsync_ThrowsCsvFileException_WhenFileNotFound` - Exception test
- `GetPersonByIdAsync_ThrowsCsvFileException_WhenFileNotFound` - Exception test
- `GetPersonsByColorAsync_ThrowsCsvFileException_WhenFileNotFound` - Exception test

### DatabasePersonRepositoryTests (10 tests)
Tests for database-based data access using in-memory SQLite.

**Coverage:**
- `GetAllPersonsAsync_ReturnsAllPersons_OrderedById` - Verify retrieval with ordering
- `GetAllPersonsAsync_ReturnsEmptyList_WhenDatabaseIsEmpty` - Verify empty database
- `GetPersonByIdAsync_ReturnsCorrectPerson_WhenPersonExists` - Verify lookup
- `GetPersonByIdAsync_ReturnsNull_WhenPersonDoesNotExist` - Verify null returns
- `AddPersonAsync_AddsPersonToDatabase_AndReturnsPersonWithId` - Verify insertion
- `AddPersonAsync_WithMinimalData_SavesSuccessfully` - Verify nullable fields
- `AddPersonAsync_IncreasesTotalCount` - Verify count changes
- `GetPersonsByColorAsync_ReturnsOnlyMatchingColor` - Verify color filtering
- `GetPersonsByColorAsync_ReturnsEmpty_WhenNoMatches` - Verify empty results
- `GetPersonsByColorAsync_ReturnsOrderedById` - Verify ordering

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~PersonsControllerTests"
dotnet test --filter "FullyQualifiedName~CsvPersonRepositoryTests"
dotnet test --filter "FullyQualifiedName~DatabasePersonRepositoryTests"
```

### Run with Detailed Output
```bash
dotnet test --verbosity normal
```

### List All Tests
```bash
dotnet test --list-tests
```

## Test Patterns

### Fake Repository
The `FakeRepo` class in `PersonsControllerTests` implements `IPersonRepository` with in-memory data, allowing controller tests to run without dependencies on actual data sources.

### Temporary Files
`CsvPersonRepositoryTests` creates temporary CSV files for each test using `CreateTestConfiguration()`, ensuring tests are isolated and clean up automatically.

### In-Memory Database
`DatabasePersonRepositoryTests` uses SQLite in-memory mode (`DataSource=:memory:`), providing fast, isolated database tests without affecting actual database files.

## Dependencies

- **xUnit 2.4.2** - Test framework
- **Microsoft.NET.Test.Sdk 18.0.1** - Test runner
- **Microsoft.EntityFrameworkCore 9.0.0** - For database tests
- **Microsoft.EntityFrameworkCore.Sqlite 9.0.0** - SQLite provider

## Test Coverage Summary

✅ **31 total tests** - all passing  
✅ Controller validation and exception handling  
✅ CSV file operations and error cases  
✅ Database CRUD operations  
✅ Edge cases (null values, empty results, missing data)  
✅ Color-based filtering across all repositories  

## Adding New Tests

1. Create test method with `[Fact]` attribute
2. Follow Arrange-Act-Assert pattern
3. Use descriptive test names: `MethodName_ExpectedBehavior_WhenCondition`
4. Ensure tests are isolated (no shared state)
5. Run tests to verify they pass

## CI/CD Integration

These tests can be integrated into CI/CD pipelines:

```yaml
# Example for Azure Pipelines
- task: DotNetCoreCLI@2
  displayName: 'Run Unit Tests'
  inputs:
    command: test
    projects: '**/UnitTests.csproj'
    arguments: '--configuration Release'
```
