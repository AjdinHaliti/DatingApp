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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // getting user from repo
            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;
            // if a gender is specified
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                //if not set oposit of users gender
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            return Unauthorized();

            var userForRepo = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userForRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new System.Exception($"Updating user {id} failed on save");
        }
        
        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            //iff the like exist
            var like = await _repo.GetLike(id, recipientId);
            //if the like not equal to null so it alredy exists
            if (like != null)
                return BadRequest("You already like this user");
            
            if (await _repo.GetUser(recipientId) == null)
                return NotFound();
            //creating a new like
            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };
            //passing the like this is not async method
            _repo.Add<Like>(like);
            //not it is savint to database and it is async..
            if (await _repo.SaveAll())
                return Ok();
            //if not saved successfully then...
            return BadRequest("Failed to like user");
        }
    }
}