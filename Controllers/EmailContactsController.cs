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
        private readonly IEmailContactRepository _emailContactRepository;

        private readonly MessageQueueService _messageQueueService;

        private readonly AppDbContext _context;



        public EmailContactsController(IEmailContactRepository emailContactRepository, MessageQueueService messageQueueService, AppDbContext context)
        {
            _emailContactRepository = emailContactRepository;
            _messageQueueService = messageQueueService;
            _context = context;
        }

#if DEBUG // only in development environment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailContact>>> GetEmailContacts()
        {
            var contacts = await _emailContactRepository.GetAllAsync();
            return Ok(contacts);
        }
#endif

        [HttpPost]
        public async Task<ActionResult> PostEmailContact([FromBody] CreateEmailContactDto emailContactDto)
        {



            var emailContactInDb = await _emailContactRepository.GetByNameAndEmailAsync(emailContactDto.Name, emailContactDto.Email);
            if (emailContactInDb != null)
            {
                var messageObject = new
                {
                    recipientEmail = emailContactDto.Email,
                    recipientName = emailContactDto.Name,
                    vCode = emailContactInDb.VCode,
                };
                var message = JsonSerializer.Serialize(messageObject);
                _messageQueueService.PublishMessage(message);
                return Created();
            }
            var newEmailContact = new EmailContact
            {
                Name = emailContactDto.Name,
                Email = emailContactDto.Email,
                Message = emailContactDto.Message,
                AllowSaveEmail = emailContactDto.AllowSaveEmail,
                VCode = await _emailContactRepository.GenerateUniqueVCodeAsync()
            };

            await _emailContactRepository.AddAsync(newEmailContact);
            await _emailContactRepository.SaveChangesAsync();

            // Send email with the VCode
            var messageObj = new
            {
                recipientEmail = newEmailContact.Email,
                recipientName = newEmailContact.Name,
                vCode = newEmailContact.VCode,
            };
            var msg = JsonSerializer.Serialize(messageObj);
            _messageQueueService.PublishMessage(msg);

            return Created();
        }

        [HttpPost("verify")]
        public async Task<ActionResult<EmailContact>> PostEmailContactByVCode([FromBody] VCodeVerifyDto vCodeVerifyDto)
        {
            var emailContactInDb = await _emailContactRepository.GetByVCodeAsync(vCodeVerifyDto.VerificationCode);
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
            await _emailContactRepository.DeleteAll();
            return NoContent();
        }
#endif
    }
}
