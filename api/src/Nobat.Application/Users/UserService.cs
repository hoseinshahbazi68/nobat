using AutoMapper;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Jwt;
using Nobat.Application.Repositories;
using Nobat.Application.Schedules;
using Nobat.Application.Users.Dto;
using Nobat.Domain.Entities.Clinics;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;
using System.Security.Cryptography;
using System.Text;

namespace Nobat.Application.Users;

/// <summary>
/// سرویس مدیریت کاربران
/// </summary>
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IRepository<ClinicUser> _clinicUserRepository;
    private readonly IRepository<Clinic> _clinicRepository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<UserService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IFileUploadService _fileUploadService;

    /// <summary>
    /// سازنده سرویس کاربر
    /// </summary>
    public UserService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IRepository<ClinicUser> clinicUserRepository,
        IRepository<Clinic> clinicRepository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<UserService> logger,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IFileUploadService fileUploadService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _clinicUserRepository = clinicUserRepository;
        _clinicRepository = clinicRepository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// دریافت کاربر بر اساس شناسه
    /// </summary>
    public async Task<ApiResponse<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetWithRolesAsync(id, cancellationToken);
            if (user == null)
            {
                return ApiResponse<UserDto>.Error("کاربر با شناسه مشخص شده یافت نشد", 404);
            }

            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;
            userDto.CityName = user.City?.Name;
            userDto.ProvinceName = user.City?.Province?.Name;

            return ApiResponse<UserDto>.Success(userDto, "کاربر با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id: {Id}", id);
            return ApiResponse<UserDto>.Error("خطا در دریافت کاربر", ex);
        }
    }

    /// <summary>
    /// دریافت کاربر بر اساس کد ملی
    /// </summary>
    public async Task<ApiResponse<UserDto>> GetByNationalCodeAsync(string nationalCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
            {
                return ApiResponse<UserDto>.Error("کد ملی نمی‌تواند خالی باشد", 400);
            }

            var user = await _userRepository.GetByNationalCodeAsync(nationalCode.Trim(), cancellationToken);
            if (user == null)
            {
                return ApiResponse<UserDto>.Error("کاربری با کد ملی مشخص شده یافت نشد", 404);
            }

            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;
            userDto.CityName = user.City?.Name;
            userDto.ProvinceName = user.City?.Province?.Name;

            return ApiResponse<UserDto>.Success(userDto, "کاربر با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by national code: {NationalCode}", nationalCode);
            return ApiResponse<UserDto>.Error("خطا در دریافت کاربر", ex);
        }
    }

    /// <summary>
    /// دریافت لیست کاربران با فیلتر و مرتب‌سازی
    /// </summary>
    public async Task<ApiResponse<PagedResult<UserDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _userRepository.GetQueryableAsync(cancellationToken);

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var filteredUsers = filteredQuery.ToList();

            var userDtos = new List<UserDto>();
            foreach (var user in filteredUsers)
            {
                var userWithRoles = await _userRepository.GetWithRolesAsync(user.Id, cancellationToken);
                if (userWithRoles != null)
                {
                    var roles = userWithRoles.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
                    var userDto = _mapper.Map<UserDto>(userWithRoles);
                    userDto.Roles = roles;
                    userDto.CityName = userWithRoles.City?.Name;
                    userDto.ProvinceName = userWithRoles.City?.Province?.Name;
                    userDtos.Add(userDto);
                }
            }

            var result = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<UserDto>>.Success(result, "لیست کاربران با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return ApiResponse<PagedResult<UserDto>>.Error("خطا در دریافت لیست کاربران", ex);
        }
    }

    /// <summary>
    /// ایجاد کاربر جدید
    /// </summary>
    public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی تکراری نبودن کد ملی
            var existingUserByNationalCode = await _userRepository.GetByNationalCodeAsync(dto.NationalCode, cancellationToken);
            if (existingUserByNationalCode != null)
            {
                return ApiResponse<UserDto>.Error("کد ملی قبلاً استفاده شده است", 400, "کد ملی باید یکتا باشد");
            }

            // بررسی تکراری نبودن ایمیل
            var existingUserByEmail = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingUserByEmail != null)
            {
                return ApiResponse<UserDto>.Error("ایمیل قبلاً استفاده شده است", 400, "ایمیل باید یکتا باشد");
            }

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = HashPassword(dto.Password);
            user.IsActive = true;

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // اختصاص نقش‌ها
            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                foreach (var roleId in dto.RoleIds)
                {
                    var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
                    if (role != null)
                    {
                        var userRole = new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roleId
                        };
                        await _userRoleRepository.AddAsync(userRole, cancellationToken);
                    }
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // بارگذاری مجدد با نقش‌ها
            var createdUser = await _userRepository.GetWithRolesAsync(user.Id, cancellationToken);
            var roles = createdUser?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var userDto = _mapper.Map<UserDto>(createdUser!);
            userDto.Roles = roles;

            _logger.LogInformation("User created with ID: {UserId}", user.Id);

            return ApiResponse<UserDto>.Success(userDto, "کاربر جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return ApiResponse<UserDto>.Error("خطا در ایجاد کاربر", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی کاربر
    /// </summary>
    public async Task<ApiResponse<UserDto>> UpdateAsync(UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetWithRolesAsync(dto.Id, cancellationToken);
            if (user == null)
            {
                return ApiResponse<UserDto>.Error($"کاربر با شناسه {dto.Id} یافت نشد", 404);
            }

            // بررسی تکراری نبودن کد ملی
            if (dto.NationalCode != user.NationalCode)
            {
                var existingUserByNationalCode = await _userRepository.GetByNationalCodeAsync(dto.NationalCode, cancellationToken);
                if (existingUserByNationalCode != null && existingUserByNationalCode.Id != dto.Id)
                {
                    return ApiResponse<UserDto>.Error("کد ملی قبلاً استفاده شده است", 400, "کد ملی باید یکتا باشد");
                }
            }

            // بررسی تکراری نبودن ایمیل
            if (dto.Email != user.Email)
            {
                var existingUserByEmail = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
                if (existingUserByEmail != null && existingUserByEmail.Id != dto.Id)
                {
                    return ApiResponse<UserDto>.Error("ایمیل قبلاً استفاده شده است", 400, "ایمیل باید یکتا باشد");
                }
            }

            // به‌روزرسانی اطلاعات کاربر
            user.NationalCode = dto.NationalCode;
            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Gender = dto.Gender;
            user.BirthDate = dto.BirthDate;
            user.CityId = dto.CityId;
            user.IsActive = dto.IsActive;

            await _userRepository.UpdateAsync(user, cancellationToken);

            // به‌روزرسانی نقش‌ها
            if (dto.RoleIds != null)
            {
                // حذف نقش‌های قبلی که در لیست جدید نیستند
                var existingUserRoles = user.UserRoles?.ToList() ?? new List<UserRole>();
                var roleIdsToKeep = dto.RoleIds.ToHashSet();

                foreach (var userRole in existingUserRoles)
                {
                    if (!roleIdsToKeep.Contains(userRole.RoleId))
                    {
                        await _userRoleRepository.DeleteAsync(userRole.Id, cancellationToken);
                    }
                }

                // افزودن نقش‌های جدید
                foreach (var roleId in dto.RoleIds)
                {
                    var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
                    if (role != null)
                    {
                        // بررسی اینکه آیا این نقش قبلاً وجود داشته یا نه
                        var existingUserRole = existingUserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
                        if (existingUserRole == null)
                        {
                            // اگر وجود نداشت، جدید ایجاد می‌کنیم
                            var userRole = new UserRole
                            {
                                UserId = user.Id,
                                RoleId = roleId
                            };
                            await _userRoleRepository.AddAsync(userRole, cancellationToken);
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // بارگذاری مجدد با نقش‌ها
            var updatedUser = await _userRepository.GetWithRolesAsync(user.Id, cancellationToken);
            var roles = updatedUser?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var userDto = _mapper.Map<UserDto>(updatedUser!);
            userDto.Roles = roles;
            userDto.CityName = updatedUser?.City?.Name;
            userDto.ProvinceName = updatedUser?.City?.Province?.Name;

            _logger.LogInformation("User updated with ID: {UserId}", user.Id);

            return ApiResponse<UserDto>.Success(userDto, "کاربر با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return ApiResponse<UserDto>.Error("خطا در به‌روزرسانی کاربر", ex);
        }
    }

    /// <summary>
    /// حذف کاربر
    /// </summary>
    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return ApiResponse.Error("کاربر با شناسه مشخص شده یافت نشد", 404);
            }

            await _userRepository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User deleted with ID: {UserId}", id);

            return ApiResponse.Success("کاربر با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {Id}", id);
            return ApiResponse.Error("خطا در حذف کاربر", ex);
        }
    }

    /// <summary>
    /// ورود کاربر
    /// </summary>
    /// <param name="loginDto">اطلاعات ورود</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ احراز هویت شامل توکن و اطلاعات کاربر</returns>
    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByNationalCodeAsync(loginDto.NationalCode, cancellationToken);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return ApiResponse<AuthResponseDto>.Error("کد ملی یا رمز عبور صحیح نیست", 401, "اطلاعات ورود نامعتبر است");
            }

            if (!user.IsActive)
            {
                return ApiResponse<AuthResponseDto>.Error("حساب کاربری غیرفعال است", 401, "لطفاً با مدیر سیستم تماس بگیرید");
            }

            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var token = _jwtService.GenerateToken(user, roles);

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;

            var authResponse = new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };

            return ApiResponse<AuthResponseDto>.Success(authResponse, "ورود با موفقیت انجام شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return ApiResponse<AuthResponseDto>.Error("خطا در فرآیند ورود", ex);
        }
    }

    /// <summary>
    /// ثبت‌نام کاربر جدید
    /// </summary>
    /// <param name="registerDto">اطلاعات ثبت‌نام</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ احراز هویت شامل توکن و اطلاعات کاربر</returns>
    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUserByNationalCode = await _userRepository.GetByNationalCodeAsync(registerDto.NationalCode, cancellationToken);
            var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email, cancellationToken);

            if (existingUserByNationalCode != null)
            {
                return ApiResponse<AuthResponseDto>.Error("کد ملی قبلاً استفاده شده است", 400, "لطفاً کد ملی دیگری وارد کنید");
            }

            if (existingUserByEmail != null)
            {
                return ApiResponse<AuthResponseDto>.Error("ایمیل قبلاً استفاده شده است", 400, "لطفاً ایمیل دیگری وارد کنید");
            }

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = HashPassword(registerDto.Password);
            user.IsActive = true;

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

            var authResponse = new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };

            return ApiResponse<AuthResponseDto>.Success(authResponse, "ثبت‌نام با موفقیت انجام شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return ApiResponse<AuthResponseDto>.Error("خطا در فرآیند ثبت‌نام", ex);
        }
    }

    /// <summary>
    /// دریافت اطلاعات کاربر فعلی
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>اطلاعات کاربر</returns>
    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetWithRolesAsync(userId, cancellationToken);

            if (user == null)
            {
                return ApiResponse<UserDto>.Error("کاربر یافت نشد", 404);
            }

            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;

            return ApiResponse<UserDto>.Success(userDto, "اطلاعات کاربر با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return ApiResponse<UserDto>.Error("خطا در دریافت اطلاعات کاربر", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی پروفایل کاربر
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="updateDto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>اطلاعات کاربر به‌روز شده</returns>
    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(int userId, UpdateProfileDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return ApiResponse<UserDto>.Error("کاربر یافت نشد", 404);
            }

            // Check if email is already taken by another user
            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(updateDto.Email, cancellationToken);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return ApiResponse<UserDto>.Error("ایمیل قبلاً توسط کاربر دیگری استفاده شده است", 400, "لطفاً ایمیل دیگری وارد کنید");
                }
            }

            // Update user properties
            // اگر فیلدها خالی نیستند، آنها را به‌روزرسانی می‌کنیم
            if (!string.IsNullOrWhiteSpace(updateDto.Email))
            {
                user.Email = updateDto.Email;
            }
            if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
            {
                user.FirstName = updateDto.FirstName;
            }
            if (!string.IsNullOrWhiteSpace(updateDto.LastName))
            {
                user.LastName = updateDto.LastName;
            }
            if (updateDto.PhoneNumber != null)
            {
                user.PhoneNumber = updateDto.PhoneNumber;
            }
            if (updateDto.Gender.HasValue)
            {
                user.Gender = updateDto.Gender;
            }
            if (updateDto.BirthDate.HasValue)
            {
                user.BirthDate = updateDto.BirthDate;
            }
            if (updateDto.CityId.HasValue)
            {
                user.CityId = updateDto.CityId;
            }

            // به‌روزرسانی عکس پروفایل اگر ارسال شده باشد
            if (!string.IsNullOrEmpty(updateDto.ProfilePicture))
            {
                // حذف عکس قبلی اگر وجود داشته باشد
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    await _fileUploadService.DeleteFileAsync(user.ProfilePicture, cancellationToken);
                }
                user.ProfilePicture = updateDto.ProfilePicture;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with roles
            var updatedUser = await _userRepository.GetWithRolesAsync(userId, cancellationToken);
            var roles = updatedUser?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var userDto = _mapper.Map<UserDto>(updatedUser!);
            userDto.Roles = roles;

            _logger.LogInformation("User profile updated for userId: {UserId}", userId);

            return ApiResponse<UserDto>.Success(userDto, "پروفایل با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return ApiResponse<UserDto>.Error("خطا در به‌روزرسانی پروفایل", ex);
        }
    }

    /// <summary>
    /// تغییر رمز عبور کاربر
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="changePasswordDto">اطلاعات تغییر رمز عبور</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    public async Task<ApiResponse> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return ApiResponse.Error("کاربر یافت نشد", 404);
            }

            // Verify current password
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse.Error("رمز عبور فعلی صحیح نیست", 400, "لطفاً رمز عبور فعلی را صحیح وارد کنید");
            }

            // Update password
            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed for userId: {UserId}", userId);

            return ApiResponse.Success("رمز عبور با موفقیت تغییر کرد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return ApiResponse.Error("خطا در تغییر رمز عبور", ex);
        }
    }

    /// <summary>
    /// هش کردن رمز عبور
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// بررسی صحت رمز عبور
    /// </summary>
    /// <param name="password">رمز عبور</param>
    /// <param name="passwordHash">رمز عبور هش شده</param>
    /// <returns>true در صورت صحیح بودن</returns>
    private bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == passwordHash;
    }

    /// <summary>
    /// دریافت لیست کلینیک‌های کاربر
    /// </summary>
    public async Task<ApiResponse<List<Clinics.Dto.ClinicDto>>> GetUserClinicsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return ApiResponse<List<Clinics.Dto.ClinicDto>>.Error("کاربر یافت نشد", 404);
            }

            var clinicUsers = await _clinicUserRepository.FindAsync(cu => cu.UserId == userId, cancellationToken);
            var clinicIds = clinicUsers.Select(cu => cu.ClinicId).ToList();

            var clinics = new List<Clinics.Dto.ClinicDto>();
            foreach (var clinicId in clinicIds)
            {
                var clinic = await _clinicRepository.GetByIdAsync(clinicId, cancellationToken);
                if (clinic != null)
                {
                    clinics.Add(_mapper.Map<Clinics.Dto.ClinicDto>(clinic));
                }
            }

            return ApiResponse<List<Clinics.Dto.ClinicDto>>.Success(clinics, "لیست کلینیک‌های کاربر با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user clinics for userId: {UserId}", userId);
            return ApiResponse<List<Clinics.Dto.ClinicDto>>.Error("خطا در دریافت لیست کلینیک‌های کاربر", ex);
        }
    }

    /// <summary>
    /// اختصاص کلینیک به کاربر
    /// </summary>
    public async Task<ApiResponse> AssignClinicToUserAsync(int userId, AssignClinicToUserDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return ApiResponse.Error("کاربر یافت نشد", 404);
            }

            var clinic = await _clinicRepository.GetByIdAsync(dto.ClinicId, cancellationToken);
            if (clinic == null)
            {
                return ApiResponse.Error("کلینیک یافت نشد", 404);
            }

            // بررسی اینکه آیا قبلاً این کلینیک به کاربر اختصاص داده شده است
            var existingClinicUser = (await _clinicUserRepository.FindAsync(
                cu => cu.UserId == userId && cu.ClinicId == dto.ClinicId, cancellationToken)).FirstOrDefault();

            if (existingClinicUser != null)
            {
                return ApiResponse.Error("این کلینیک قبلاً به کاربر اختصاص داده شده است", 400);
            }

            var clinicUser = new ClinicUser
            {
                UserId = userId,
                ClinicId = dto.ClinicId
            };

            await _clinicUserRepository.AddAsync(clinicUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Clinic {ClinicId} assigned to user {UserId}", dto.ClinicId, userId);

            return ApiResponse.Success("کلینیک با موفقیت به کاربر اختصاص داده شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning clinic to user");
            return ApiResponse.Error("خطا در اختصاص کلینیک به کاربر", ex);
        }
    }

    /// <summary>
    /// حذف دسترسی کاربر به کلینیک
    /// </summary>
    public async Task<ApiResponse> RemoveClinicFromUserAsync(int userId, int clinicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var clinicUser = (await _clinicUserRepository.FindAsync(
                cu => cu.UserId == userId && cu.ClinicId == clinicId , cancellationToken)).FirstOrDefault();

            if (clinicUser == null)
            {
                return ApiResponse.Error("رابطه کاربر و کلینیک یافت نشد", 404);
            }

            await _clinicUserRepository.DeleteAsync(clinicUser.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Clinic {ClinicId} removed from user {UserId}", clinicId, userId);

            return ApiResponse.Success("دسترسی کاربر به کلینیک با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing clinic from user");
            return ApiResponse.Error("خطا در حذف دسترسی کاربر به کلینیک", ex);
        }
    }
}
