using Core.DTO;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAuth
    {
        Task<ServiceResponse<bool>> RegisterAsync(RegisterDTO registerDTO);
        Task<ServiceResponse<string>> LoginAsync(LoginDTO login);
        Task<ServiceResponse<bool>> SendEmailForForgetPassword(string email);
        Task<ServiceResponse<bool>> ResetPassword(RestPasswordDTO restPassword);
        Task<ServiceResponse<bool>> ActiveAccount(ActiveAccountDTO accountDTO);

    }
}
