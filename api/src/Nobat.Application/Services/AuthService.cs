using AutoMapper;
using Microsoft.Extensions.Logging;
using Nobat.Application.Interfaces;
using Nobat.Application.Jwt;
using Nobat.Application.Repositories;
using Nobat.Application.Users.Dto;
using Nobat.Domain.Entities;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Nobat.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IJwtService jwtService,
        IMapper mapper,
        ILogger<AuthService> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByNationalCodeAsync(loginDto.NationalCode, cancellationToken);

        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid national code or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive.");
        }

        var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
        var token = _jwtService.GenerateToken(user, roles);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        return new AuthResponseDto
        {
            Token = token,
            User = userDto,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        var existingUserByNationalCode = await _userRepository.GetByNationalCodeAsync(registerDto.NationalCode, cancellationToken);
        var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email, cancellationToken);

        if (existingUserByNationalCode != null || existingUserByEmail != null)
        {
            throw new InvalidOperationException("National code or email already exists.");
        }

        var user = _mapper.Map<User>(registerDto);
        user.PasswordHash = HashPassword(registerDto.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign default User role
        var defaultRole = (await _roleRepository.FindAsync(r => r.Name == "User", cancellationToken)).FirstOrDefault();
        if (defaultRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id
            };
            await _userRoleRepository.AddAsync(userRole, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var roles = new List<string> { "User" };
        var token = _jwtService.GenerateToken(user, roles);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        _logger.LogInformation("User registered with national code: {NationalCode}", user.NationalCode);

        return new AuthResponseDto
        {
            Token = token,
            User = userDto,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == passwordHash;
    }
}
