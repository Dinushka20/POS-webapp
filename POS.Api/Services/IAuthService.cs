using POS.Api.DTOs;

namespace POS.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    }
}
