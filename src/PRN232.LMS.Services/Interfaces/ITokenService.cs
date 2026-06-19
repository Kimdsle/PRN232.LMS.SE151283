using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Auth;

namespace PRN232.LMS.Services.Interfaces;

public interface ITokenService
{
    TokenGenerationResult GenerateTokens(User user);
}
