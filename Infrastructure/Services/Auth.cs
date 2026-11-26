using Core.DTO;
using Core.Entities;
using Core.Interfaces;
using Core.Sharing;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public async Task<ServiceResponse<bool>> RegisterAsync(RegisterDTO registerDTO)
        {
            if (registerDTO is null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Invalid registration data.",
                    StatusCode = 400
                };
            }

            // Check Username
            var existingUser = await userManager.FindByNameAsync(registerDTO.UserName);

            if (existingUser is not null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "This username is already registered.",
                    StatusCode = 409
                };
            }

            // Check Email
            var existingEmail = await userManager.FindByEmailAsync(registerDTO.Email);

            if (existingEmail is not null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "This email is already registered.",
                    StatusCode = 409
                };
            }

            // Create User
            AppUser user = new()
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                FullName = registerDTO.FullName
            };

            var result = await userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = string.Join(" | ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400
                };
            }

            // Generate Confirmation Token
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send Confirmation Email
            string confirmationBaseUrl = "https://workspacesandtasks.runasp.net/api/Auth/active-account";


            await SendEmail(
                user.Email!,
                token,
                confirmationBaseUrl,
                "ActiveEmail",
                "Please activate your email by clicking the button below."
            );

            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Registration successful. Please check your email to activate your account.",
                Data = true,
                StatusCode = 201
            };
        }

        public async Task SendEmail(string email, string code, string component, string subject, string message)
        {
            var result = new EmailDTO(email,
                "myecom34@gmail.com",
                subject
                , EmailStringBody.Generate(email, code, component, message));
            await emailService.SendEmailAsync(result);
        }

        public async Task<ServiceResponse<string>> LoginAsync(LoginDTO login)
        {
            if (login is null)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Invalid login data.",
                    StatusCode = 400
                };
            }

            var user = await userManager.FindByEmailAsync(login.Email);

            if (user is null)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    StatusCode = 401
                };
            }

            if (!user.EmailConfirmed)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Please confirm your email before logging in.",
                    StatusCode = 403
                };
            }

            var result = await signInManager.CheckPasswordSignInAsync(
                user,
                login.Password,
                lockoutOnFailure: true
            );

            if (result.IsLockedOut)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Your account is temporarily locked. Please try again later.",
                    StatusCode = 423
                };
            }

            if (!result.Succeeded)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    StatusCode = 401
                };
            }

            var token = generateToken.GetAndCreateToken(user);

            return new ServiceResponse<string>
            {
                Success = true,
                Message = "Login successful.",
                Data = token,
                StatusCode = 200
            };
        }
        public async Task<ServiceResponse<bool>> SendEmailForForgetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Email is required.",
                    StatusCode = 400
                };
            }

            var user = await userManager.FindByEmailAsync(email);

            // Don't reveal whether the email exists.
            if (user is null)
            {
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link shortly.",
                    Data = true,
                    StatusCode = 200
                };
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);


            string resetPasswordBaseUrl =
           "https://workspacesandtasks.runasp.net/api/Auth/reset-password";

            await SendEmail(
                user.Email!,
                token,
                resetPasswordBaseUrl,
                "Reset Password",
                "Click the button below to reset your password."
            );

            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "If your email is registered, you will receive a password reset link shortly.",
                Data = true,
                StatusCode = 200
            };
        }
        public async Task<ServiceResponse<bool>> ResetPassword(RestPasswordDTO resetPassword)
        {
            if (resetPassword is null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Invalid reset password data.",
                    StatusCode = 400
                };
            }

            var user = await userManager.FindByEmailAsync(resetPassword.Email);

            if (user is null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Invalid password reset request.",
                    StatusCode = 400
                };
            }

            var result = await userManager.ResetPasswordAsync(
                user,
                resetPassword.Token,
                resetPassword.Password
            );

            if (!result.Succeeded)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = string.Join(" | ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400
                };
            }

            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Password reset successful.",
                Data = true,
                StatusCode = 200
            };
        }
        public async Task<ServiceResponse<bool>> ActiveAccount(ActiveAccountDTO accountDTO)
        {
            if (accountDTO is null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Invalid confirmation data.",
                    StatusCode = 400
                };
            }

            var user = await userManager.FindByEmailAsync(accountDTO.Email);

            if (user is null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Invalid confirmation link.",
                    StatusCode = 400
                };
            }

            if (user.EmailConfirmed)
            {
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Message = "Email is already confirmed.",
                    Data = true,
                    StatusCode = 200
                };
            }

            var result = await userManager.ConfirmEmailAsync(
                user,
                accountDTO.Token
            );

            if (!result.Succeeded)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Invalid or expired confirmation link.",
                    StatusCode = 400
                };
            }

            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Email confirmed successfully.",
                Data = true,
                StatusCode = 200
            };
        }
    }
}
