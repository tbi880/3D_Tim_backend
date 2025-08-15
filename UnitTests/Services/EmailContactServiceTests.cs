using _3D_Tim_backend.Services;
using _3D_Tim_backend.Repositories;
using _3D_Tim_backend.Entities;
using _3D_Tim_backend.DTOs;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace UnitTests.Services;

public class EmailContactServiceTests
{
    private readonly EmailContactService _service;
    private readonly Mock<IEmailContactRepository> _mockRepo;
    private readonly Mock<ILogger<EmailContactService>> _mockLogger;
    private readonly ILogger<EmailContactServiceTests> _logger;

    public EmailContactServiceTests()
    {
        _mockRepo = new Mock<IEmailContactRepository>();
        _mockLogger = new Mock<ILogger<EmailContactService>>();
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EmailContactServiceTests>();
        _service = new EmailContactService(_mockRepo.Object, null, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllContactsAsync_Returns_All_Contacts()
    {
        // Arrange
        var contacts = new List<EmailContact>
            {
                new EmailContact { Name = "Tim", Email = "tim@test.com" },
                new EmailContact { Name = "John", Email = "john@test.com" }
            };
        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(contacts);

        // Act
        var result = await _service.GetAllContactsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetContactByEmailAsync_Returns_Contact()
    {
        // Arrange
        var contacts = new List<EmailContact>
    {
        new EmailContact { Name = "Tim", Email = "tim@test.com" },
        new EmailContact { Name = "John", Email = "john@test.com" }
    };
        _mockRepo.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                 .ReturnsAsync((string email) => contacts.FirstOrDefault(c => c.Email == email));

        var email = contacts[0].Email;

        // Act
        var result = await _service.GetContactByEmailAsync(email);

        // Assert
        Assert.Equal(email, result?.Email);
        _logger.LogInformation("{Name}", result?.Name);
    }
    [Fact]
    public async Task CreateOrUpdateEmailContactAsync_Updates_Existing_Contact()
    {
        // Arrange
        var existingContact = new EmailContact { Name = "Old Name", Email = "existing@test.com", Message = "Old Message" };
        var dto = new CreateEmailContactDto("New Name", "existing@test.com", "New Message", true);

        _mockRepo.Setup(repo => repo.GetByEmailAsync(dto.Email)).ReturnsAsync(existingContact);
        _mockRepo.Setup(repo => repo.UpdateContactAsync(It.IsAny<EmailContact>())).Returns(Task.CompletedTask);

        // Act
        await _service.CreateOrUpdateEmailContactAsync(dto);

        // Assert
        Assert.Equal(dto.Name, existingContact.Name);
        Assert.Equal(dto.Message, existingContact.Message);
        _mockRepo.Verify(repo => repo.UpdateContactAsync(It.IsAny<EmailContact>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateEmailContactAsync_Creates_New_Contact()
    {
        // Arrange
        var dto = new CreateEmailContactDto
        ("New Name", "new@test.com", "New Message", true);

        _mockRepo.Setup(repo => repo.GetByEmailAsync(dto.Email)).ReturnsAsync((EmailContact?)null);
        _mockRepo.Setup(repo => repo.GenerateUniqueVCodeAsync()).ReturnsAsync("UNIQUE_VCODE");
        _mockRepo.Setup(repo => repo.AddAsync(It.IsAny<EmailContact>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.CreateOrUpdateEmailContactAsync(dto);

        // Assert
        _mockRepo.Verify(repo => repo.AddAsync(It.Is<EmailContact>(c =>
            c.Name == dto.Name &&
            c.Email == dto.Email &&
            c.Message == dto.Message &&
            c.VCode == "UNIQUE_VCODE"
        )), Times.Once);
        _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task VerifyContactByVCodeAsync_Returns_Contact()
    {
        // Arrange
        var vCode = "VALID_VCODE";
        var contact = new EmailContact { Name = "Tim", Email = "tim@test.com", VCode = vCode };

        _mockRepo.Setup(repo => repo.GetByVCodeAsync(vCode)).ReturnsAsync(contact);

        // Act
        var result = await _service.VerifyContactByVCodeAsync(vCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(contact.VCode, result?.VCode);
    }

    [Fact]
    public async Task UpdateContactVerifiedAtAsync_Updates_VerifiedAt()
    {
        // Arrange
        var contact = new EmailContact { Name = "Tim", Email = "tim@test.com" };

        _mockRepo.Setup(repo => repo.UpdateVerifiedAtAsync(It.IsAny<DateTime>(), contact)).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateContactVerifiedAtAsync(contact);

        // Assert
        _mockRepo.Verify(repo => repo.UpdateVerifiedAtAsync(It.IsAny<DateTime>(), contact), Times.Once);
    }

    [Fact]
    public async Task DeleteAllContactsAsync_Deletes_All_Contacts()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.DeleteAllAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAllContactsAsync();

        // Assert
        _mockRepo.Verify(repo => repo.DeleteAllAsync(), Times.Once);
    }

}

