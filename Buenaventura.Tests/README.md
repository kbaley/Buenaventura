# Buenaventura Unit Tests

This test suite provides comprehensive coverage for the Buenaventura financial management application's core functionality.

## Test Structure

### Domain Tests (`Domain/`)
- **TransactionTests.cs**: Tests for Transaction entity business logic
- **InvestmentTests.cs**: Tests for Investment entity calculations and operations

### Service Tests (`Services/`)
- **ServerAccountServiceTests.cs**: Tests for account management service
- **ServerCategoryServiceTests.cs**: Tests for category management service  
- **ServerCustomerServiceTests.cs**: Tests for customer management service

### Repository Tests (`Data/`)
- **TransactionRepositoryTests.cs**: Tests for transaction data access operations

### API Tests (`Api/`)
- **InvestmentsControllerTests.cs**: Tests for investment API endpoints

### Integration Tests (`Integration/`)
- **BuenaventuraWebApplicationFactory.cs**: Test application factory for integration tests
- **AccountsControllerIntegrationTests.cs**: End-to-end API integration tests

### Performance Tests (`Performance/`)
- **PerformanceTests.cs**: Performance and load testing for critical operations

### Test Utilities (`Helpers/`, `Utilities/`)
- **TestDataFactory.cs**: Fake data generators using Bogus
- **TestDbContextFactory.cs**: In-memory database context factory
- **TestUtilities.cs**: Helper methods and assertion utilities

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Category
```bash
# Unit tests only
dotnet test --filter Category=Unit

# Integration tests only  
dotnet test --filter Category=Integration

# Performance tests only
dotnet test --filter Category=Performance
```

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Verbose Output
```bash
dotnet test --verbosity normal
```

## Test Features

### Test Data Generation
- Uses **Bogus** library for realistic fake data generation
- Consistent test data across all test classes
- Configurable data volumes for performance testing

### Database Testing
- **In-memory database** for fast, isolated tests
- **SQLite** option for more realistic database behavior
- Automatic database cleanup between tests

### Mocking
- **Moq** framework for service and dependency mocking
- Clean separation between unit and integration tests

### Assertions
- **FluentAssertions** for readable and maintainable test assertions
- Custom assertion helpers for domain-specific validations

### Integration Testing
- **ASP.NET Core Test Host** for full application testing
- Test-specific configuration and seeding
- HTTP client testing for API endpoints

## Test Configuration

### Parallel Execution
Tests are configured to run in parallel for faster execution:
- `parallelizeTestCollections`: true
- `maxParallelThreads`: 4

### Test Categories
Tests are organized into logical categories:
- **Unit**: Fast, isolated tests
- **Integration**: End-to-end functionality tests
- **Performance**: Load and performance tests

## Coverage Goals

The test suite aims for:
- **90%+ code coverage** for business logic
- **100% coverage** for critical financial calculations
- **Integration test coverage** for all API endpoints
- **Performance benchmarks** for data-heavy operations

## Adding New Tests

### For New Domain Models
1. Create test class in `Domain/` folder
2. Use `TestDataFactory` for data generation
3. Focus on business logic and calculations
4. Add assertion helpers to `TestUtilities`

### For New Services
1. Create test class in `Services/` folder
2. Use dependency injection and mocking
3. Test both success and error scenarios
4. Include edge cases and validation

### For New Controllers
1. Create test class in `Api/` folder
2. Mock service dependencies
3. Test HTTP responses and status codes
4. Add integration tests for end-to-end scenarios

### For Data Access
1. Create test class in `Data/` folder
2. Use in-memory database for isolation
3. Test CRUD operations thoroughly
4. Include performance tests for heavy operations

## Best Practices

1. **AAA Pattern**: Arrange, Act, Assert
2. **Single Responsibility**: One assertion per test
3. **Descriptive Names**: Test names should describe the scenario
4. **Independent Tests**: No dependencies between tests
5. **Fast Execution**: Keep unit tests under 100ms
6. **Realistic Data**: Use meaningful test data
7. **Clean Code**: Refactor test code as production code
8. **Documentation**: Comment complex test scenarios

## Continuous Integration

Tests are designed to run in CI/CD pipelines:
- No external dependencies
- Deterministic results
- Parallel execution support
- Detailed failure reporting
- Performance regression detection
