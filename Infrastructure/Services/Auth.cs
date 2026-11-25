using Core.DTO;
using Core.Entities;
using Core.Interfaces;
using Core.Sharing;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class Auth : IAuth
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IGenerateToken generateToken;
        private readonly AppDbContext context;
        public Auth(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, IGenerateToken generateToken, AppDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            this.generateToken = generateToken;
            this.context = context;
        }

        public async Task<string> RegisterAsync(RegisterDTO registerDTO)
        {
            if (registerDTO == null)
            {
                return null;
            }
            if (await userManager.FindByNameAsync(registerDTO.UserName) is not null)
            {
                return "this UserName is already registerd";
            }
            if (await userManager.FindByEmailAsync(registerDTO.Email) is not null)
            {
                return "this email is already registerd";
            }

            AppUser user = new()
            {
                Email = registerDTO.Email,
                UserName = registerDTO.UserName,
                FullName = registerDTO.FullName,
            };

            var result = await userManager.CreateAsync(user, registerDTO.Password);
            if (result.Succeeded is not true)
            {
                return result.Errors.ToList()[0].Description;
            }
            // Send Active Email
            string confirmationBaseUrl =
            "https://localhost:7175/api/Auth/active-account";
            string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await SendEmail(user.Email, token, confirmationBaseUrl, "ActiveEmail", "Please active your email, click on button to active");
            return "Done";
        }


        public async Task SendEmail(string email, string code, string component, string subject, string message)
        {
            var result = new EmailDTO(email,
                "myecom34@gmail.com",
                subject
                , EmailStringBody.Generate(email, code, component, message));
            await emailService.SendEmailAsync(result);
        }

        public async Task<string> LoginAsync(LoginDTO login)
        {
            if (login == null)
            {
                return null;
            }
            var finduser = await userManager.FindByEmailAsync(login.Email);

            if (!finduser.EmailConfirmed)
            {
                string confirmationBaseUrl =
                "https://localhost:7175/api/Auth/active-account";

                string token = await userManager.GenerateEmailConfirmationTokenAsync(finduser);

                await SendEmail(finduser.Email, token, confirmationBaseUrl, "ActiveEmail", "Please active your email, click on button to active");

                return "Please confirem your email first, we have send activat to your E-mail";
            }

            var result = await signInManager.CheckPasswordSignInAsync(finduser, login.Password, true);

            if (result.Succeeded)
            {
                return generateToken.GetAndCreateToken(finduser);
            }

            return "Please check your email and password, something went wrong";
        }

        public async Task<bool> SendEmailForForgetPassword(string email)
        {
            var findUser = await userManager.FindByEmailAsync(email);
            if (findUser is null)
            {
                return false;
            }
            var token = await userManager.GeneratePasswordResetTokenAsync(findUser);
            await SendEmail(findUser.Email, token, "Reset-Password", "Rest pssword", "click on button to Reset your password");

            return true;

        }

        public async Task<string> ResetPassword(RestPasswordDTO restPassword)
        {
            var findUser = await userManager.FindByEmailAsync(restPassword.Email);
            if (findUser is null)
            {
                return null;
            }

            var result = await userManager.ResetPasswordAsync(findUser, restPassword.Token, restPassword.Password);

            if (result.Succeeded)
            {
                return "Done";
            }
            return result.Errors.ToList()[0].Description;
        }
        public async Task<bool> ActiveAccount(ActiveAccountDTO accountDTO)
        {
            var findUser = await userManager.FindByEmailAsync(accountDTO.Email);

            if (findUser is null)
            {
                return false;
            }

            if (findUser.EmailConfirmed)
            {
                return true;
            }

            var result = await userManager.ConfirmEmailAsync(
                findUser,
                accountDTO.Token
            );

            return result.Succeeded;
        }
    }
}
