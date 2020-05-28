using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            //checking if the user token matches the id of the user i am passing in
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            //get the message from the repo
            var messageFromRepo = await _repo.GetMessage(id);
            //just checking if there is a message from our repo
            if (messageFromRepo == null)
                return NotFound();
            //if it is found pass in the message
            return Ok(messageFromRepo);
        }
        
        //this http is not using id as a parametar and it is using messages controller
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, 
            [FromQuery]MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;

            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);
            // we are taking messages from users into a messages to return
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, 
                messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);
            
            return Ok(messages);
        }

        //represents the conversation between 2 users
        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            //because they are returning the message entities we are going to need to map these into our message to return
            var messagesFromRepo = await _repo.GetMessageThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            return Ok(messageThread);
        }

        //dont have to add anything to the route because if is gonna self generate id
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repo.GetUser(userId);
            //authorized check..
            if (sender.id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            //setting parameter messagefor creatindto to a user id
            messageForCreationDto.SenderId = userId;
            //taking recipient from the repo
            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);
            //if recipent exist..
            if (recipient == null)
                return BadRequest("Could not find user");
            //maping our message for creation data into our message class or message//<Message> is destination and we pass in messageforcreationdto
            var message = _mapper.Map<Message>(messageForCreationDto);
            //need maping because i want to map my messageForCreationDto to a message entity
            
            //after maping i add this message to repo (database)
            //dont need to async because add is not async metod it is just sync bit of code
            //because we are not querying and we are not doing anything with our databse at this time
            _repo.Add(message);

            //saving it to the datapase
            if (await _repo.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new {id = message.Id}, messageToReturn);
            }

            throw new Exception("Creating the message failed on save");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();  

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo.SenderId == userId)
                messageFromRepo.SenderDeleted = true;

            if (messageFromRepo.RecipientId == userId)
                messageFromRepo.RecipientDeleted = true;
            //if both of these values are true then delete the message
            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                _repo.Delete(messageFromRepo);
            
            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception("Error deleting the message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();  

            //GET THE MESSAGE FROM THE REPO
            var message = await _repo.GetMessage(id);
            //
            if (message.RecipientId != userId)
                return Unauthorized();

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _repo.SaveAll();

            return NoContent();
        }
    }
}