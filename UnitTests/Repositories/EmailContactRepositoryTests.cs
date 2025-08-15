using Microsoft.EntityFrameworkCore;
using _3D_Tim_backend.Data;
using _3D_Tim_backend.Entities;
using _3D_Tim_backend.Repositories;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;


namespace UnitTests.repositories;

public class EmailContactRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly EmailContactRepository _repository;
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogger<EmailContactRepository>> _mockLogger = new();


    public EmailContactRepositoryTests(ITestOutputHelper output)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _dbContext = new AppDbContext(options);
        _repository = new EmailContactRepository(_dbContext, _mockLogger.Object);
        _output = output;


        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_Returns_All_EmailContacts()
    {
        // Arrange
        _dbContext.EmailContacts.Add(new EmailContact { Name = "Tim", Email = "tim@test.com" });
        _dbContext.EmailContacts.Add(new EmailContact { Name = "John", Email = "john@test.com" });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Correct_EmailContact()
    {
        // Arrange
        _dbContext.EmailContacts.Add(new EmailContact { Name = "John", Email = "john@test.com" });
        var contact = new EmailContact { Name = "Tim", Email = "tim@test.com" };
        await _dbContext.EmailContacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(contact.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Tim", result.Name);
        Assert.Equal("tim@test.com", result.Email);
    }

    [Fact]
    public async Task GetByNameAndEmailAsync_Returns_Correct_EmailContact()
    {
        // Arrange
        _dbContext.EmailContacts.Add(new EmailContact { Name = "John", Email = "john@test.com" });
        var contact = new EmailContact { Name = "Tim", Email = "tim@test.com" };
        await _dbContext.EmailContacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAndEmailAsync(contact.Name, contact.Email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Tim", result.Name);
        Assert.Equal("tim@test.com", result.Email);
    }

    [Fact]
    public async Task GenerateUniqueVCodeAsync_Generates_Unique_Code()
    {
        // Act
        var vCode1 = await _repository.GenerateUniqueVCodeAsync();
        var vCode2 = await _repository.GenerateUniqueVCodeAsync();

        // Assert
        Assert.NotEqual(vCode1, vCode2);
    }

    [Fact]
    public async Task GetByVCodeAsync_Returns_Correct_EmailContact()
    {
        // Arrange
        _dbContext.EmailContacts.Add(new EmailContact { Name = "John", Email = "john@test.com", VCode = "12345678" });
        var contact = new EmailContact { Name = "Tim", Email = "tim@test.com", VCode = "87654321" };
        await _dbContext.EmailContacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByVCodeAsync(contact.VCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Tim", result.Name);

    }

    [Fact]
    public async Task GetByEmailAsync_Returns_Correct_EmailContact()
    {
        // Arrange
        _dbContext.EmailContacts.Add(new EmailContact { Name = "John", Email = "john@test.com", VCode = "12345678" });
        var contact = new EmailContact { Name = "Tim", Email = "tim@test.com", VCode = "87654321" };
        await _dbContext.EmailContacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(contact.Email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Tim", result.Name);
    }

    [Fact]
    public async Task UpdateContactAsync_Updates_Contact()
    {
        // Arrange
        _dbContext.EmailContacts.Add(new EmailContact { Name = "John", Email = "john@test.com", VCode = "12345678" });
        var contact = new EmailContact { Name = "Tim", Email = "tim@test.com", VCode = "87654321", AllowSaveEmail = false };
        await _dbContext.EmailContacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        // Act
        contact.Name = "Tom";
        contact.AllowSaveEmail = true;
        await _repository.UpdateContactAsync(contact);

        // Assert
        var result = await _repository.GetByIdAsync(contact.Id);
        Assert.Equal("Tom", result.Name);
        Assert.True(result.AllowSaveEmail);
        Assert.Equal(true, result.AllowSaveEmail);
    }

    [Fact]
    public async Task UpdateVerifiedAtAsync_Updates_VerifiedAt()
    {
        // Arrange
        _dbContext.EmailContacts.Add(new EmailContact { Name = "John", Email = "john@test.com", VCode = "12345678" });
        var contact = new EmailContact { Name = "Tim", Email = "tim@test.com", VCode = "87654321", AllowSaveEmail = false };
        await _dbContext.EmailContacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        // Act
        var verifiedAt = DateTime.Now;
        await _repository.UpdateVerifiedAtAsync(verifiedAt, contact);

        // Assert
        var result = await _repository.GetByIdAsync(contact.Id);
        Assert.Equal(verifiedAt, result.VerifiedAt);
    }

}

