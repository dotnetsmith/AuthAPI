using AuthAPI.Authentication;
using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IJwtHeaderProvider _jwtHeaderProvider;
        private readonly IRefreshTokenCookieProvider _refreshTokenSessionProvider;

        public AuthController(
            IProfileRepository profileRepository, 
            IJwtHeaderProvider jwtHeaderProvider,
            IRefreshTokenCookieProvider refreshTokenSessionProvider)
        {
            _profileRepository = profileRepository;
            _jwtHeaderProvider = jwtHeaderProvider;
            _refreshTokenSessionProvider = refreshTokenSessionProvider;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            
            var (refreshToken, refreshTokenExpiration) = _refreshTokenSessionProvider.Generate(HttpContext);

            await _profileRepository.UpdateRefeshToken(refreshToken!, refreshTokenExpiration, profile.Id);

            var token = _jwtHeaderProvider.Generate(profile, HttpContext);

            return Ok(token);
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            return CreatedAtAction(
                nameof(Register), 
                new { Username = userDto.Username }, 
                userDto);
        }

        [HttpGet("refresh")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (refreshToken == null)
            {
                return Unauthorized("Refresh token not found");
            }

            var profile = await _profileRepository.GetProfileByRefreshToken(refreshToken);

            if (profile == null)
            {
                return Unauthorized("Refresh token not found");
            }

            if (profile.RefreshTokenExpiration < DateTime.UtcNow)
            {
                return Unauthorized("Refresh token expired");
            }

            var (newRefreshToken, newRefreshTokenExpiration) = _refreshTokenSessionProvider.Generate(HttpContext);

            await _profileRepository.UpdateRefeshToken(newRefreshToken!, newRefreshTokenExpiration, profile.Id);

            var token = _jwtHeaderProvider.Generate(profile, HttpContext);

            return Ok("Token refreshed");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryTokenAttribute]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (refreshToken == null)
            {
                return Unauthorized("Refresh token not found");
            }

            var profile = await _profileRepository.GetProfileByRefreshToken(refreshToken);

            if (profile == null)
            {
                return Unauthorized("Refresh token not found");
            }

            await _profileRepository.UpdateRefeshToken(null!, null!, profile.Id);

            return NoContent();
        }

        [HttpPut("updatePassword")]
        [Authorize]
        [ValidateAntiForgeryTokenAttribute]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePassword(PasswordReset passwordReset)
        {
            var profile = await _profileRepository.GetProfile(passwordReset.Username);

            if (profile == null)
            {
                return NotFound("Profile not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(passwordReset.OldPassword, profile.PasswordHash))
            {
                return Unauthorized("Incorrect credentials");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(passwordReset.NewPassword, 13);

            passwordReset.NewPassword = hashedPassword;

            await _profileRepository.UpdatePassword(passwordReset);

            return NoContent();
        }
                
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]        
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetProfileById(int id)
        {
            var profile = await _profileRepository.GetProfileById(id);

            if (profile == null)
            {
                return NotFound("Profile not found");
            }

            var userDto = profile.ToDto();

            return userDto;
        }

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
