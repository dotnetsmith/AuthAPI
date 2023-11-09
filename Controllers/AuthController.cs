using AuthAPI.Authentication;
using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ProfileRepository _profileRepository;
        private readonly IJwtProvider _jwtProvider;

        public AuthController(ProfileRepository profileRepository, IJwtProvider jwtProvider)
        {
            _profileRepository = profileRepository;
            _jwtProvider = jwtProvider;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromBody] UserRequest user)
        {            
            var profile = await _profileRepository.GetProfile(user.Username); 

            if (profile == null)
            {
                return NotFound("Account not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(user.Password, profile.PasswordHash))
            {
                return Unauthorized("Incorrect credentials");
            }
            
            var token = _jwtProvider.Generate(profile);

            Response.Headers.Add("Authorization", token);

            return Ok(token);
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult> Register(UserRequest user)
        {
            if (await IsUsernameUnique(user.Username) == false)
            {
                return BadRequest("Username already exists");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, 13);

            user.Password = hashedPassword;

            await _profileRepository.Create(user);

            var profile = await _profileRepository.GetProfile(user.Username);

            var userDto = profile.ToDto();

            //var userDto = new UserDto
            //{
            //    Id = profile.Id,
            //    Username = profile.Username
            //};

            return CreatedAtAction(nameof(Register), new { Username = userDto.Username }, userDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetProfileById(int id)
        {
            var profile = await _profileRepository.GetProfileById(id);

            if (profile.Id == 0)
            {
                return NotFound("Profile not found");
            }

            var userDto = new UserDto
            {
                Id = profile.Id,
                Username = profile.Username
            };

            return userDto;
        }

        //Get Identity of current user
        [HttpGet("getMe"), Authorize] 
        public ActionResult<string> GetMe()
        {
            var username = User?.Identity?.Name;
            return Ok(username);
        }

        private async Task<bool> IsUsernameUnique(string Username)
        {
            var profiles = await _profileRepository.GetProfile(Username);

            if (profiles == null)
            {
                return true;
            }

            return false;
        }
    }
}
