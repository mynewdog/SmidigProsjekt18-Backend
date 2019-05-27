using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using smidigprosjekt.Logic.Services;
using smidigprosjekt.Models;

namespace smidigprosjekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterUser : ControllerBase
    {
        private IUserService _userService;

        public RegisterUser(IUserService UserService)
        {
            _userService = UserService;
        }
        //Todo: legg inn Password field
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] NewUserClass newUser)
        {
            var user = new User()
            {
                Username = newUser.Name,
                Institutt = newUser.Institutt,
                Studie = newUser.Studie,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Lobbies = new HashSet<Lobby>(),
            };
            user.storePassword(newUser.Password);

            //todo:add class validation for all fields
            if (
                string.IsNullOrEmpty(user.Email) ||
                string.IsNullOrEmpty(user.LastName) ||
                string.IsNullOrEmpty(user.Institutt) ||
                string.IsNullOrEmpty(user.Studie) ||
                string.IsNullOrEmpty(user.Username)) return BadRequest("InvalidForm");
            
            if (await _userService.RegisterUser(user))
            {
                return Ok(user);
            }
            else
            {
                return BadRequest("User allready exists");
            }
        }
    }
}
