using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IGenerateToken token;

        public IAuth Auth { get; }

        public UnitOfWork(AppDbContext context , UserManager<AppUser> userManager,
            IEmailService emailService, SignInManager<AppUser> signInManager
, IGenerateToken token)
        {
            _context = context;
            this.userManager = userManager;
            this.emailService = emailService;
            this.signInManager = signInManager;
            this.token = token;
            Auth = new Auth(userManager, signInManager, emailService, token, context);
        }
    }
}
