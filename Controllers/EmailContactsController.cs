using _3D_Tim_backend.Entities;
using _3D_Tim_backend.Repositories;
using _3D_Tim_backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using _3D_Tim_backend.Services;
using System.Text.Json;
using _3D_Tim_backend.Data;

namespace _3D_Tim_backend.Controllers
{
    [Route("email_contacts")]
    [ApiController]
    public class EmailContactsController : ControllerBase
    {
        private readonly IEmailContactService _emailContactService;

        private readonly IMessageQueueService _messageQueueService;



        public EmailContactsController(IEmailContactService emailContactService, IMessageQueueService messageQueueService)
        {
            _emailContactService = emailContactService;
            _messageQueueService = messageQueueService;
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

            return Created();
        }

        [HttpPost("verify")]
        public async Task<ActionResult<EmailContact>> PostEmailContactByVCode([FromBody] VCodeVerifyDto vCodeVerifyDto)
        {
            var emailContactInDb = await _emailContactService.VerifyContactByVCodeAsync(vCodeVerifyDto.VerificationCode);
            if (emailContactInDb == null)
            {
                return Ok(new VCodeVerifyReturnDto("notFound", "None"));
            }
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
