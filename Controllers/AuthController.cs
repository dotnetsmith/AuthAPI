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
        private readonly IJwtProvider _jwtProvider;
        private readonly IRefreshTokenProvider _refreshTokenProvider;

        public AuthController(
            IProfileRepository profileRepository, 
            IJwtProvider jwtProvider,
            IRefreshTokenProvider refreshTokenProvider)
        {
            _profileRepository = profileRepository;
            _jwtProvider = jwtProvider;
            _refreshTokenProvider = refreshTokenProvider;
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
            
            var token = _jwtProvider.Generate(profile);

            Response.Headers.Add("Authorization", token);

            var (refreshToken, refreshTokenExpiration) = _refreshTokenProvider.Generate();

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshTokenExpiration
            });

            await _profileRepository.UpdateRefeshToken(refreshToken!, refreshTokenExpiration, profile.Id);

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

            var token = _jwtProvider.Generate(profile);

            Response.Headers.Add("Authorization", token);

            var (newRefreshToken, newRefreshTokenExpiration) = _refreshTokenProvider.Generate();

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshTokenExpiration
            });

            await _profileRepository.UpdateRefeshToken(newRefreshToken!, newRefreshTokenExpiration, profile.Id);

            return Ok("Token refreshed");
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
