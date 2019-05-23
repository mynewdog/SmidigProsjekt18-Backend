using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace smidigprosjekt.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RegisterUser : ControllerBase
  {
    [HttpPost]
    public void Post([FromBody] NewUserClass newUser)
    {

    }
  }
}
