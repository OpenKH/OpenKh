using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenKh.Research.Kh2Anim.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        public IActionResult Index()
        {
            return Ok(new { Message = "We will be running now." });
        }
    }
}
