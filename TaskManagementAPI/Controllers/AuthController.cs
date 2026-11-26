using Core.DTO;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagementAPI.Helper;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _work;

        public AuthController(IUnitOfWork work)
        {
            _work = work;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _work.Auth.RegisterAsync(registerDTO);

            return StatusCode(
                result.StatusCode,
                new ResponseAPI(result.StatusCode, result.Message)
            );
        }

        [EnableRateLimiting("IpRateLimit")]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _work.Auth.LoginAsync(loginDTO);

            if (!result.Success)
            {
                return StatusCode(
                    result.StatusCode,
                    new ResponseAPI(result.StatusCode, result.Message)
                );
            }

            Response.Cookies.Append("token", result.Data!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddDays(6)
            });

            return Ok(new ResponseAPI(200, result.Message));
        }



        [HttpGet("Active-Account")]
        public async Task<IActionResult> ActiveAccount(
            [FromQuery] string email,
            [FromQuery] string code)
        {
            var accountDTO = new ActiveAccountDTO(email, code);

            var result = await _work.Auth.ActiveAccount(accountDTO);

            return StatusCode(
                result.StatusCode,
                new ResponseAPI(result.StatusCode, result.Message)
            );
        }

        [EnableRateLimiting("IpRateLimit")]
        [HttpPost("send-email-forget-password")]
        public async Task<IActionResult> ForgetPassword(
            [FromBody] string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _work.Auth.SendEmailForForgetPassword(
                email
            );

            return Ok(new ResponseAPI(200, result.Message));
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] RestPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _work.Auth.ResetPassword(resetPasswordDTO);

            return StatusCode(
                result.StatusCode,
                new ResponseAPI(result.StatusCode, result.Message)
            );
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token", new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None });
            return Ok(new ResponseAPI(200, "Logout successful."));
        }
    }
}
