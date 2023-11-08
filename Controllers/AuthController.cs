using AuthAPI.Abastractions;
using AuthAPI.Models;
using AuthAPI.Repositories;
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

        [HttpPost]
        public async Task<ActionResult<string>> Authenticate([FromBody] UserRequest user)
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

            var userResponse = new UserResponse
            {
                Id = profile.Id,
                Username = profile.Username,
                AuthToken = _jwtProvider.Generate(profile)
            };

            Response.Headers.Add("Authorization", userResponse.AuthToken);

            return Ok(userResponse);
        }

        [HttpPost]
        [Route("CreateProfile")]
        public async Task<ActionResult> Create(UserRequest user)
        {
            if (await IsUsernameUnique(user.Username) == false)
            {
                return BadRequest("Username already exists");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, 13);

            user.Password = hashedPassword;

            await _profileRepository.Create(user);

            var profile = await _profileRepository.GetProfile(user.Username);

            var userResponse = new UserResponse
            {
                Id = profile.Id,
                Username = profile.Username,
                AuthToken = null!
            };

            return CreatedAtAction(nameof(Create), new { Username = userResponse.Username }, userResponse);
        }

        //Get Identity of current user
        [HttpGet("getMe"), Authorize] 
        public ActionResult<string> GetMe()
        {
            var username = User?.Identity?.Name;
            return Ok(username);
        }

        [Authorize (Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<UserResponse>> GetProfileById(int id)
        {
            var profile = await _profileRepository.GetProfileById(id);

            if (profile.Id == 0)
            {
                return NotFound("Profile not found");
            }

            var userResponse = new UserResponse
            {
                Id = profile.Id,
                Username = profile.Username,
                AuthToken = null!
            };

            return userResponse;
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
