﻿using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase  
    {
        private readonly IAuthRepository repo;
        private readonly IConfiguration config;

        public AuthController(IAuthRepository repo,  IConfiguration config)
        {
            this.repo = repo;
            this.config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserToRegister userToRegis)
        {
            // validate request 

            userToRegis.Username = userToRegis.Username.ToLower();

            if (await repo.UserExists(userToRegis.Username))
                return BadRequest("The user alredy exists");

            var userToCreate = new User
            {
                Username = userToRegis.Username
            };

            var createdUser = await repo.Register(userToCreate, userToRegis.Password);
            return StatusCode(201);        
        }

        [HttpPost("login")] 
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {          
                

                var userFromRepo = await repo.Login(userForLoginDto.Username, userForLoginDto.Password);

                if (userFromRepo == null)
                    return Unauthorized();

                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)

            };

                var keyToken = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("AppSettings:Token").Value));

                var creds = new SigningCredentials(keyToken, SecurityAlgorithms.HmacSha512Signature);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };

                var tokenhandler = new JwtSecurityTokenHandler();
                var token = tokenhandler.CreateToken(tokenDescriptor);

                return Ok(new
                {
                    token = tokenhandler.WriteToken(token)
                });   

        }
    }
}
