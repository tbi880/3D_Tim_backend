using _3D_Tim_backend.Entities;
using _3D_Tim_backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace _3D_Tim_backend.Controllers
{
    [Route("email_contacts")]
    [ApiController]
    public class EmailContactsController : ControllerBase
    {
        private readonly IEmailContactService _emailContactService;

        private readonly IMessageQueueService _messageQueueService;

        private readonly ILogger<EmailContactsController> _logger;


        public EmailContactsController(IEmailContactService emailContactService, IMessageQueueService messageQueueService, ILogger<EmailContactsController> logger)
        {
            _emailContactService = emailContactService;
            _messageQueueService = messageQueueService;
            _logger = logger;
        }

#if DEBUG // only in development environment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailContact>>> GetEmailContacts()
        {
            var contacts = await _emailContactService.GetAllContactsAsync();
            return Ok(contacts);
        }
#endif

        [HttpPost]
        public async Task<ActionResult> PostEmailContact([FromBody] CreateEmailContactDto emailContactDto)
        {
            _logger.LogInformation("Received email contact creation request for {Email}", emailContactDto.Email);
            await _emailContactService.CreateOrUpdateEmailContactAsync(emailContactDto);
            var emailContactInDb = await _emailContactService.GetContactByEmailAsync(emailContactDto.Email);

            var messageObj = new
            {
                recipientEmail = emailContactInDb!.Email,
                recipientName = emailContactInDb!.Name,
                vCode = emailContactInDb!.VCode,
            };
            var msg = JsonSerializer.Serialize(messageObj);
            _messageQueueService.PublishMessage(msg);
            _logger.LogInformation("Email contact created successfully and message published for {Email}", emailContactDto.Email);
            return Created();
        }

        [HttpPost("verify")]
        public async Task<ActionResult<EmailContact>> PostEmailContactByVCode([FromBody] VCodeVerifyDto vCodeVerifyDto)
        {
            _logger.LogInformation("Received email contact verification request for {VerificationCode}", vCodeVerifyDto.VerificationCode);
            var emailContactInDb = await _emailContactService.VerifyContactByVCodeAsync(vCodeVerifyDto.VerificationCode);
            if (emailContactInDb == null)
            {
                return Ok(new VCodeVerifyReturnDto("notFound", "None"));
            }
            await _emailContactService.UpdateContactVerifiedAtAsync(emailContactInDb);
            _logger.LogInformation("Email contact verified successfully for {VerificationCode}", vCodeVerifyDto.VerificationCode);
            return Ok(new VCodeVerifyReturnDto("success", emailContactInDb.Name));
        }


#if DEBUG // only in development environment
        [HttpDelete]
        public async Task<ActionResult> DeleteEmailContact()
        {
            await _emailContactService.DeleteAllContactsAsync();
            return NoContent();
        }
#endif
    }
}
