﻿using AuthAPI.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthAPI.Authentication
{
    public sealed class JwtHeaderProvider : IJwtHeaderProvider
    {
        private readonly IConfiguration _config;

        public JwtHeaderProvider(IConfiguration config)
        {
            _config = config;
        }

        public string Generate(Profile profile, HttpContext context)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, profile.Id.ToString()),
                new Claim(ClaimTypes.Name, profile.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));

            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);  

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                null,
                DateTime.UtcNow.AddHours(1),
                signingCredentials); 

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            context.Response.Headers.Add("Authorization", jwt);

            return jwt;
        }
    }
}
