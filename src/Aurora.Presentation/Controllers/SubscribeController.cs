using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Aurora.Presentation.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class SubscribeController : ControllerBase
    {
        [HttpGet("subscribe")]
        public async Task<ActionResult> Subscribe()
        {
            Guid id = Guid.NewGuid();
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString())
            };

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(claimsIdentity);
            AuthenticationProperties props = new()
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

            return Ok();
        }
    }
}
