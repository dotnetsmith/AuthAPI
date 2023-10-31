using AuthAPI.Models;
using AuthAPI.Repositories;
using DadsWebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<Profile>?> Authenticate([FromBody] Profile profile)
        {
            var profiles = await _profileRepository.GetProfile(profile.Username);

            if (profile.Username != profiles.Username || profile.Password != profiles.Password)
            {
                return default(Profile)!;
            }

            return profiles;
        }

        [HttpPost]
        [Route("CreateProfile")]
        public async Task<ActionResult> Create(Profile profiles)
        {
            if (await IsUsernameUnique(profiles.Username) == false)
            {
                return BadRequest("Username already exists");
            }

            await _profileRepository.Create(profiles);

            return CreatedAtAction(nameof(Create), new { Username = profiles.Username }, profiles);
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
