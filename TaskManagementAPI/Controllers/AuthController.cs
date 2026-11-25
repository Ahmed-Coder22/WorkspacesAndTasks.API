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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string result = await _work.Auth.RegisterAsync(registerDTO);

            if (result != "Done")
            {
                return BadRequest(new ResponseAPI(400, result));
            }
            return StatusCode(201, new ResponseAPI(201, "Registration successful. Please check your email to activate your account."));
        }
        [EnableRateLimiting("IpRateLimit")]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string result = await _work.Auth.LoginAsync(loginDTO);

            if (result.StartsWith("Please"))
            {
                return BadRequest(new ResponseAPI(400, result));
            }

            Response.Cookies.Append("token", result, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddDays(6)
            });
            return Ok(new ResponseAPI(200, "Login successful."));
        }

        [HttpGet("Active-Account")]
        public async Task<IActionResult> ActiveAccount(
    [FromQuery] string email,
    [FromQuery] string code)
        {

            var accountDTO = new ActiveAccountDTO(email, code);

            var result = await _work.Auth.ActiveAccount(accountDTO);

            return result
                ? Ok(new ResponseAPI(200, "Email confirmed successfully"))
                : BadRequest(new ResponseAPI(400, "Invalid confirmation link"));
        }
        
        [EnableRateLimiting("IpRateLimit")]
        [HttpPost("send-email-forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] string email)
        {
            await _work.Auth.SendEmailForForgetPassword(email);

            return Ok(new ResponseAPI(200, "If your email is registered, you will receive a password reset link shortly."));
        }

        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword(RestPasswordDTO restPasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _work.Auth.ResetPassword(restPasswordDTO);
            if (result == "Done")
            {
                return Ok(new ResponseAPI(200, "Password reset successful."));
            }
            return BadRequest(new ResponseAPI(400,result));
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token", new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None });
            return Ok(new ResponseAPI(200, "Logout successful."));
        }
    }
}
