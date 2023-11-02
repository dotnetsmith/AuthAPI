using AuthAPI.Entities;
using AuthAPI.Models;
using AuthAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ProfileRepository _profileRepository;

        public AuthController(ProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }
        
        [HttpPost]
        public async Task<ActionResult<UserViewModel>?> Authenticate([FromBody] User user)
        {            
            var profile = await _profileRepository.GetProfile(user.Username);            

            if (profile == null)
            {
                return Unauthorized("Account not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(user.Password, profile.PasswordHash))
            {
                return Unauthorized("Account not found");
            }

            var userViewModel = new UserViewModel 
            {
                Id = profile.Id,
                Username = profile.Username,
                AuthToken = GenerateToken(profile)            
            };

            return userViewModel;
        }

        private string GenerateToken(Profile profile)
        {
            return Guid.NewGuid().ToString();
        }

        [HttpPost]
        [Route("CreateProfile")]
        public async Task<ActionResult> Create(User user)
        {
            if (await IsUsernameUnique(user.Username) == false)
            {
                return BadRequest("Username already exists");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, 13);

            user.Password = hashedPassword;

            await _profileRepository.Create(user);

            var profile = await _profileRepository.GetProfile(user.Username);

            var userViewModel = new UserViewModel
            {
                Id = profile.Id,
                Username = profile.Username,
                AuthToken = GenerateToken(profile)
            };

            return CreatedAtAction(nameof(Create), new { Username = userViewModel.Username }, userViewModel);
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
