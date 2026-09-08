// RegisterCommandHandler.cs
using Library.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Library.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    public RegisterCommandHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        // UserManager gère AUTOMATIQUEMENT le hashing du mot de passe (jamais stocké en clair)
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new RegisterResult(false, errors);
        }

        return new RegisterResult(true, null);
    }
}