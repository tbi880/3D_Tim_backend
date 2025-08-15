using _3D_Tim_backend.Controllers;
using _3D_Tim_backend.DTOs;
using _3D_Tim_backend.Entities;
using _3D_Tim_backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

namespace UnitTests.Controllers
{
    public class EmailContactsControllerTests
    {
        private readonly EmailContactsController _controller;
        private readonly Mock<IEmailContactService> _mockService;
        private readonly Mock<IMessageQueueService> _mockQueueService;
        private readonly Mock<ILogger<EmailContactsController>> _mockLogger;

        public EmailContactsControllerTests()
        {
            _mockService = new Mock<IEmailContactService>();
            _mockQueueService = new Mock<IMessageQueueService>();
            _mockLogger = new Mock<ILogger<EmailContactsController>>();
            _controller = new EmailContactsController(_mockService.Object, _mockQueueService.Object, _mockLogger.Object);
        }

#if DEBUG
        [Fact]
        public async Task GetEmailContacts_Returns_Ok_With_Contacts()
        {
            // Arrange
            var contacts = new List<EmailContact>
            {
                new EmailContact { Name = "Tim", Email = "tim@test.com" },
                new EmailContact { Name = "John", Email = "john@test.com" }
            };

            _mockService.Setup(service => service.GetAllContactsAsync()).ReturnsAsync(contacts);

            // Act
            var result = await _controller.GetEmailContacts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedContacts = Assert.IsAssignableFrom<IEnumerable<EmailContact>>(okResult.Value);
            Assert.Equal(2, returnedContacts.Count());
        }
#endif

        [Fact]
        public async Task PostEmailContact_Returns_Created()
        {
            // Arrange
            var dto = new CreateEmailContactDto("Tim", "tim@test.com", "Hello!", true);
            var emailContact = new EmailContact
            {
                Name = dto.Name,
                Email = dto.Email,
                Message = dto.Message,
                VCode = "123456"
            };

            _mockService.Setup(service => service.CreateOrUpdateEmailContactAsync(dto)).Returns(Task.CompletedTask);
            _mockService.Setup(service => service.GetContactByEmailAsync(dto.Email)).ReturnsAsync(emailContact);

            // Act
            var result = await _controller.PostEmailContact(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            _mockQueueService.Verify(q => q.PublishMessage(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PostEmailContactByVCode_Returns_NotFound()
        {
            // Arrange
            var vCodeDto = new VCodeVerifyDto("123456");
            _mockService.Setup(service => service.VerifyContactByVCodeAsync(vCodeDto.VerificationCode))
                        .ReturnsAsync((EmailContact)null);

            // Act
            var result = await _controller.PostEmailContactByVCode(vCodeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<VCodeVerifyReturnDto>(okResult.Value);
            Assert.Equal("notFound", response.Status);
        }

        [Fact]
        public async Task PostEmailContactByVCode_Returns_Success()
        {
            // Arrange
            var vCodeDto = new VCodeVerifyDto("123456");
            var emailContact = new EmailContact
            {
                Name = "Tim",
                Email = "tim@test.com",
                VCode = vCodeDto.VerificationCode
            };

            _mockService.Setup(service => service.VerifyContactByVCodeAsync(vCodeDto.VerificationCode))
                        .ReturnsAsync(emailContact);

            _mockService.Setup(service => service.UpdateContactVerifiedAtAsync(emailContact))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PostEmailContactByVCode(vCodeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<VCodeVerifyReturnDto>(okResult.Value);
            Assert.Equal("success", response.Status);
            Assert.Equal(emailContact.Name, response.Name);
        }

#if DEBUG
        [Fact]
        public async Task DeleteEmailContact_Returns_NoContent()
        {
            // Arrange
            _mockService.Setup(service => service.DeleteAllContactsAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteEmailContact();

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(service => service.DeleteAllContactsAsync(), Times.Once);
        }
#endif
    }
}
