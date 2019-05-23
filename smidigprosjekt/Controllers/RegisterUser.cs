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

    public RegisterUser(IUserService UserService) {
      _userService = UserService;
    }
    //Todo: legg inn Password field
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] NewUserClass newUser)
    {
      var user = new User() {
        Username = newUser.Name,
      };
            user.storePassword(newUser.Password);
      //Ny klasse for hver post
      if (await _userService.RegisterUser(user))
      {
        return Ok(user);
      }
      else {
        return BadRequest();
      }
    }
  }
}
