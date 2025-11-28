using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Doctors.Dto;
using Nobat.Application.Schedules;
using Nobat.Application.Users;
using Nobat.Application.Users.Dto;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Doctors;

/// <summary>
/// سرویس پزشک
/// این سرویس عملیات CRUD مربوط به پزشکان را مدیریت می‌کند
/// شامل دریافت، ایجاد، به‌روزرسانی و حذف پزشکان
/// از Sieve برای فیلتر و مرتب‌سازی استفاده می‌کند
/// </summary>
public class DoctorService : IDoctorService
{
    private readonly IRepository<Doctor> _repository;
    private readonly IRepository<DoctorSpecialty> _doctorSpecialtyRepository;
    private readonly IRepository<DoctorMedicalCondition> _doctorMedicalConditionRepository;
    private readonly IRepository<SpecialtyMedicalCondition> _specialtyMedicalConditionRepository;
    private readonly IRepository<MedicalCondition> _medicalConditionRepository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<DoctorService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IRepository<Domain.Entities.Users.User> _userRepository;

    /// <summary>
    /// سازنده سرویس پزشک
    /// </summary>
    /// <param name="repository">ریپازیتوری پزشک</param>
    /// <param name="mapper">مپر AutoMapper</param>
    /// <param name="sieveProcessor">پردازشگر Sieve برای فیلتر و مرتب‌سازی</param>
    /// <param name="logger">لاگر</param>
    /// <param name="unitOfWork">واحد کار</param>
    /// <param name="userService">سرویس کاربر</param>
    /// <param name="fileUploadService">سرویس آپلود فایل</param>
    /// <param name="userRepository">ریپازیتوری کاربر</param>
    public DoctorService(
        IRepository<Doctor> repository,
        IRepository<DoctorSpecialty> doctorSpecialtyRepository,
        IRepository<DoctorMedicalCondition> doctorMedicalConditionRepository,
        IRepository<SpecialtyMedicalCondition> specialtyMedicalConditionRepository,
        IRepository<MedicalCondition> medicalConditionRepository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<DoctorService> logger,
        IUnitOfWork unitOfWork,
        IUserService userService,
        IFileUploadService fileUploadService,
        IRepository<Domain.Entities.Users.User> userRepository)
    {
        _repository = repository;
        _doctorSpecialtyRepository = doctorSpecialtyRepository;
        _doctorMedicalConditionRepository = doctorMedicalConditionRepository;
        _specialtyMedicalConditionRepository = specialtyMedicalConditionRepository;
        _medicalConditionRepository = medicalConditionRepository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _userService = userService;
        _fileUploadService = fileUploadService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// دریافت پزشک بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه پزشک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ API شامل پزشک یافت شده</returns>
    public async Task<ApiResponse<DoctorDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            var doctor = await query
                .Include(d => d.User)
                .Include(d => d.DoctorClinics)
                    .ThenInclude(dc => dc.Clinic)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .Include(d => d.DoctorMedicalConditions)
                    .ThenInclude(dmc => dmc.MedicalCondition)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

            if (doctor == null)
            {
                return ApiResponse<DoctorDto>.Error("پزشک با شناسه مشخص شده یافت نشد", 404);
            }

            var doctorDto = _mapper.Map<DoctorDto>(doctor);

            // Map User info including CityId
            if (doctor.User != null)
            {
                doctorDto.FirstName = doctor.User.FirstName;
                doctorDto.LastName = doctor.User.LastName;
                doctorDto.Phone = doctor.User.PhoneNumber ?? string.Empty;
                doctorDto.Email = doctor.User.Email;
                doctorDto.NationalCode = doctor.User.NationalCode;
                doctorDto.ProfilePicture = doctor.User.ProfilePicture;
                doctorDto.CityId = doctor.User.CityId;
                doctorDto.Gender = doctor.User.Gender;
                doctorDto.BirthDate = doctor.User.BirthDate;
            }

            // Map clinics
            doctorDto.Clinics = doctor.DoctorClinics
                .Where(dc => dc.Clinic != null)
                .Select(dc => _mapper.Map<Clinics.Dto.ClinicDto>(dc.Clinic))
                .ToList();
            // Map specialties
            if (doctor.DoctorSpecialties != null && doctor.DoctorSpecialties.Any())
            {
                doctorDto.Specialties = doctor.DoctorSpecialties
                    .OrderBy(ds => ds.SortOrder)
                    .Select(ds => new Dto.DoctorSpecialtyDto
                    {
                        Id = ds.Id,
                        DoctorId = ds.DoctorId,
                        SpecialtyId = ds.SpecialtyId,
                        Specialty = ds.Specialty != null ? _mapper.Map<Dto.SpecialtyDto>(ds.Specialty) : null,
                        SortOrder = ds.SortOrder
                    })
                    .ToList();
            }
            // Map medical conditions
            if (doctor.DoctorMedicalConditions != null && doctor.DoctorMedicalConditions.Any())
            {
                doctorDto.MedicalConditions = doctor.DoctorMedicalConditions
                    .Select(dmc => new Dto.DoctorMedicalConditionDto
                    {
                        Id = dmc.Id,
                        DoctorId = dmc.DoctorId,
                        MedicalConditionId = dmc.MedicalConditionId,
                        MedicalCondition = dmc.MedicalCondition != null ? _mapper.Map<Dto.MedicalConditionDto>(dmc.MedicalCondition) : null
                    })
                    .ToList();
            }
            return ApiResponse<DoctorDto>.Success(doctorDto, "پزشک با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor by id: {Id}", id);
            return ApiResponse<DoctorDto>.Error("خطا در دریافت پزشک", ex);
        }
    }

    /// <summary>
    /// دریافت لیست پزشکان با فیلتر و مرتب‌سازی
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="clinicId">شناسه مرکز (اختیاری - برای فیلتر بر اساس مرکز)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ API شامل نتیجه صفحه‌بندی شده</returns>
    public async Task<ApiResponse<PagedResult<DoctorListDto>>> GetAllAsync(SieveModel sieveModel, int? clinicId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            //query = query
            //    .Include(d => d.User)
            //    .Include(d => d.DoctorClinics)
            //        .ThenInclude(dc => dc.Clinic);

            // فیلتر بر اساس مرکز اگر clinicId مشخص شده باشد
            if (clinicId.HasValue)
            {
                query = query.Where(d => d.DoctorClinics.Any(dc => dc.ClinicId == clinicId.Value));
            }

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var doctorDtos = await filteredQuery.Select(x => new DoctorListDto()
            {
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                MedicalCode = x.MedicalCode,
                NationalCode = x.User.NationalCode,
                Phone = x.User.PhoneNumber,
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                UserId = x.UserId
            }).ToListAsync(cancellationToken);

            //var doctorDtos = doctors.Select(d =>
            //{
            //    var dto = _mapper.Map<DoctorDto>(d);
            //    dto.Clinics = d.DoctorClinics
            //        .Where(dc => dc.Clinic != null)
            //        .Select(dc => _mapper.Map<Clinics.Dto.ClinicDto>(dc.Clinic))
            //        .ToList();
            //    return dto;
            //}).ToList();

            var result = new PagedResult<DoctorListDto>
            {
                Items = doctorDtos,
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<DoctorListDto>>.Success(result, "لیست پزشکان با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all doctors");
            return ApiResponse<PagedResult<DoctorListDto>>.Error("خطا در دریافت لیست پزشکان", ex);
        }
    }

    /// <summary>
    /// ایجاد پزشک جدید
    /// </summary>
    /// <param name="dto">اطلاعات پزشک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ API شامل پزشک ایجاد شده</returns>
    public async Task<ApiResponse<DoctorDto>> CreateAsync(CreateDoctorDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // اگر کد ملی مشخص شده و UserId مشخص نشده، کاربر را جستجو یا ایجاد کن
            if (!string.IsNullOrWhiteSpace(dto.NationalCode) && !dto.UserId.HasValue)
            {
                var userResponse = await _userService.GetByNationalCodeAsync(dto.NationalCode.Trim(), cancellationToken);

                if (userResponse.Status && userResponse.Data != null)
                {
                    // کاربر پیدا شد
                    dto.UserId = userResponse.Data.Id;
                    _logger.LogInformation("User found with national code: {NationalCode}, UserId: {UserId}", dto.NationalCode, dto.UserId);
                }
                else
                {
                    // کاربر پیدا نشد، ایجاد می‌کنیم
                    var createUserDto = new CreateUserDto
                    {
                        NationalCode = dto.NationalCode.Trim(),
                        Email = dto.Email ?? $"{dto.NationalCode.Trim()}@temp.com",
                        Password = dto.NationalCode.Trim(), // رمز موقت (باید تغییر کند)
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        PhoneNumber = dto.Phone,
                        CityId = dto.CityId
                    };

                    var createUserResponse = await _userService.CreateAsync(createUserDto, cancellationToken);

                    if (createUserResponse.Status && createUserResponse.Data != null)
                    {
                        dto.UserId = createUserResponse.Data.Id;
                        _logger.LogInformation("User created with national code: {NationalCode}, UserId: {UserId}", dto.NationalCode, dto.UserId);
                    }
                    else
                    {
                        return ApiResponse<DoctorDto>.Error(
                            createUserResponse.Message ?? "خطا در ایجاد کاربر",
                            createUserResponse.StatusCode != 0 ? createUserResponse.StatusCode : 400,
                            createUserResponse.Description);
                    }
                }
            }

            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);

            // بررسی وجود پزشک با کد نظام پزشکی مشابه
            var existingDoctor = await query
                .Include(d => d.DoctorClinics)
                .FirstOrDefaultAsync(d => d.MedicalCode == dto.MedicalCode, cancellationToken);

            Doctor doctor;
            bool isNewDoctor = false;

            if (existingDoctor != null)
            {
                // پزشک با این کد نظام پزشکی وجود دارد
                doctor = existingDoctor;

                // اگر ClinicId مشخص شده باشد، بررسی می‌کنیم که آیا رابطه DoctorClinic وجود دارد یا نه
                if (dto.ClinicId.HasValue)
                {
                    var existingRelation = doctor.DoctorClinics
                        .FirstOrDefault(dc => dc.ClinicId == dto.ClinicId.Value);

                    if (existingRelation == null)
                    {
                        // رابطه وجود ندارد، ایجاد می‌کنیم
                        doctor.DoctorClinics.Add(new DoctorClinic
                        {
                            DoctorId = doctor.Id,
                            ClinicId = dto.ClinicId.Value,
                            IsActive = true
                        });
                        await _repository.UpdateAsync(doctor, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Doctor {DoctorId} added to clinic {ClinicId}", doctor.Id, dto.ClinicId.Value);
                    }
                    else
                    {
                        _logger.LogInformation("Doctor {DoctorId} already exists in clinic {ClinicId}", doctor.Id, dto.ClinicId.Value);
                    }
                }

                // به‌روزرسانی تخصص‌ها برای پزشک موجود
                if (dto.SpecialtyIds != null && dto.SpecialtyIds.Any())
                {
                    await SaveDoctorSpecialties(doctor.Id, dto.SpecialtyIds, cancellationToken);
                }

                // ذخیره علائم پزشکی برای پزشک موجود
                if (dto.MedicalConditionIds != null && dto.MedicalConditionIds.Any())
                {
                    await SaveDoctorMedicalConditions(doctor.Id, dto.MedicalConditionIds, cancellationToken);
                }
            }
            else
            {
                // پزشک جدید ایجاد می‌کنیم
                isNewDoctor = true;
                doctor = _mapper.Map<Doctor>(dto);
                await _repository.AddAsync(doctor, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // اگر ClinicId مشخص شده باشد، رابطه DoctorClinic را ایجاد می‌کنیم
                if (dto.ClinicId.HasValue)
                {
                    doctor.DoctorClinics.Add(new DoctorClinic
                    {
                        DoctorId = doctor.Id,
                        ClinicId = dto.ClinicId.Value,
                        IsActive = true
                    });
                    await _repository.UpdateAsync(doctor, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // ذخیره تخصص‌ها برای پزشک جدید
                if (dto.SpecialtyIds != null && dto.SpecialtyIds.Any())
                {
                    await SaveDoctorSpecialties(doctor.Id, dto.SpecialtyIds, cancellationToken);
                }

                // ذخیره علائم پزشکی برای پزشک جدید
                if (dto.MedicalConditionIds != null && dto.MedicalConditionIds.Any())
                {
                    await SaveDoctorMedicalConditions(doctor.Id, dto.MedicalConditionIds, cancellationToken);
                }

                _logger.LogInformation("Doctor created with ID: {DoctorId}", doctor.Id);
            }

            // بارگذاری مجدد با شامل کردن روابط
            var reloadedQuery = await _repository.GetQueryableAsync(cancellationToken);
            doctor = await reloadedQuery
                .Include(d => d.User)
                .Include(d => d.DoctorClinics)
                    .ThenInclude(dc => dc.Clinic)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .Include(d => d.DoctorMedicalConditions)
                    .ThenInclude(dmc => dmc.MedicalCondition)
                .FirstOrDefaultAsync(d => d.Id == doctor.Id, cancellationToken);

            var doctorDto = _mapper.Map<DoctorDto>(doctor);

            // Map User info including ProfilePicture
            if (doctor.User != null)
            {
                doctorDto.FirstName = doctor.User.FirstName;
                doctorDto.LastName = doctor.User.LastName;
                doctorDto.Phone = doctor.User.PhoneNumber ?? string.Empty;
                doctorDto.Email = doctor.User.Email;
                doctorDto.NationalCode = doctor.User.NationalCode;
                doctorDto.ProfilePicture = doctor.User.ProfilePicture;
            }

            doctorDto.Clinics = doctor.DoctorClinics
                .Where(dc => dc.Clinic != null)
                .Select(dc => _mapper.Map<Clinics.Dto.ClinicDto>(dc.Clinic))
                .ToList();
            // Map specialties
            if (doctor.DoctorSpecialties != null && doctor.DoctorSpecialties.Any())
            {
                doctorDto.Specialties = doctor.DoctorSpecialties
                    .OrderBy(ds => ds.SortOrder)
                    .Select(ds => new Dto.DoctorSpecialtyDto
                    {
                        Id = ds.Id,
                        DoctorId = ds.DoctorId,
                        SpecialtyId = ds.SpecialtyId,
                        Specialty = ds.Specialty != null ? _mapper.Map<Dto.SpecialtyDto>(ds.Specialty) : null,
                        SortOrder = ds.SortOrder
                    })
                    .ToList();
            }

            var message = isNewDoctor
                ? "پزشک جدید با موفقیت ایجاد شد"
                : "پزشک به کلینیک اضافه شد";

            return ApiResponse<DoctorDto>.Success(doctorDto, message, isNewDoctor ? 201 : 200);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating doctor");
            if (ex.InnerException?.Message.Contains("UNIQUE") == true ||
                ex.InnerException?.Message.Contains("duplicate") == true)
            {
                return ApiResponse<DoctorDto>.Error("این پزشک قبلاً به این کلینیک اضافه شده است", 400);
            }
            return ApiResponse<DoctorDto>.Error("خطا در ایجاد پزشک", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor");
            return ApiResponse<DoctorDto>.Error("خطا در ایجاد پزشک", ex);
        }
    }

    /// <summary>
    /// ایجاد پزشک جدید با فایل
    /// </summary>
    public async Task<ApiResponse<DoctorDto>> CreateWithFileAsync(CreateDoctorWithFileDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // تبدیل به CreateDoctorDto و فراخوانی متد اصلی
            var createDto = _mapper.Map<CreateDoctorDto>(dto);
            var result = await CreateAsync(createDto, cancellationToken);

            // اگر پزشک با موفقیت ایجاد شد و فایل وجود دارد، عکس را آپلود کن
            if (result.Status && result.Data != null && dto.ProfilePictureFile != null && dto.ProfilePictureFile.Length > 0)
            {
                var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
                var doctor = await query
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == result.Data.Id, cancellationToken);

                if (doctor?.User != null)
                {
                    // حذف عکس قبلی اگر وجود داشته باشد
                    if (!string.IsNullOrEmpty(doctor.User.ProfilePicture))
                    {
                        await _fileUploadService.DeleteFileAsync(doctor.User.ProfilePicture, cancellationToken);
                    }

                    // آپلود عکس جدید
                    var profilePicturePath = await _fileUploadService.UploadProfilePictureAsync(
                        dto.ProfilePictureFile,
                        doctor.User.Id,
                        cancellationToken);

                    if (!string.IsNullOrEmpty(profilePicturePath))
                    {
                        doctor.User.ProfilePicture = profilePicturePath;
                        await _userRepository.UpdateAsync(doctor.User, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        // به‌روزرسانی DTO
                        result.Data.ProfilePicture = profilePicturePath;
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor with file");
            return ApiResponse<DoctorDto>.Error("خطا در ایجاد پزشک با فایل", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی پزشک
    /// </summary>
    /// <param name="dto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ API شامل پزشک به‌روز شده</returns>
    public async Task<ApiResponse<DoctorDto>> UpdateAsync(UpdateDoctorDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var doctor = await query
                .Include(d => d.User)
                .Include(d => d.DoctorSpecialties)
                .FirstOrDefaultAsync(d => d.Id == dto.Id, cancellationToken);

            if (doctor == null)
            {
                return ApiResponse<DoctorDto>.Error($"پزشک با شناسه {dto.Id} یافت نشد", 404);
            }

            _mapper.Map(dto, doctor);

            // به‌روزرسانی اطلاعات کاربر (شهر، جنسیت، تاریخ تولد)
            if (doctor.User != null)
            {
                doctor.User.CityId = dto.CityId;
                doctor.User.BirthDate = dto.BirthDate;
                doctor.User.Gender = dto.Gender;
            }

            await _repository.UpdateAsync(doctor, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // به‌روزرسانی تخصص‌ها
            if (dto.SpecialtyIds != null && dto.SpecialtyIds.Any())
            {
                await SaveDoctorSpecialties(doctor.Id, dto.SpecialtyIds, cancellationToken);
            }

            // به‌روزرسانی علائم پزشکی
            if (dto.MedicalConditionIds != null)
            {
                await SaveDoctorMedicalConditions(doctor.Id, dto.MedicalConditionIds, cancellationToken);
            }

            // بارگذاری مجدد با شامل کردن روابط
            var reloadedQuery = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            doctor = await reloadedQuery
                .Include(d => d.User)
                .Include(d => d.DoctorClinics)
                    .ThenInclude(dc => dc.Clinic)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .Include(d => d.DoctorMedicalConditions)
                    .ThenInclude(dmc => dmc.MedicalCondition)
                .FirstOrDefaultAsync(d => d.Id == doctor.Id, cancellationToken);

            _logger.LogInformation("Doctor updated with ID: {DoctorId}", doctor.Id);
            var doctorDto = _mapper.Map<DoctorDto>(doctor);

            // Map User info including ProfilePicture, CityId, Gender, BirthDate
            if (doctor.User != null)
            {
                doctorDto.FirstName = doctor.User.FirstName;
                doctorDto.LastName = doctor.User.LastName;
                doctorDto.Phone = doctor.User.PhoneNumber ?? string.Empty;
                doctorDto.Email = doctor.User.Email;
                doctorDto.NationalCode = doctor.User.NationalCode;
                doctorDto.ProfilePicture = doctor.User.ProfilePicture;
                doctorDto.CityId = doctor.User.CityId;
                doctorDto.Gender = doctor.User.Gender;
                doctorDto.BirthDate = doctor.User.BirthDate;
            }

            doctorDto.Clinics = doctor.DoctorClinics
                .Where(dc => dc.Clinic != null)
                .Select(dc => _mapper.Map<Clinics.Dto.ClinicDto>(dc.Clinic))
                .ToList();
            // Map specialties
            if (doctor.DoctorSpecialties != null && doctor.DoctorSpecialties.Any())
            {
                doctorDto.Specialties = doctor.DoctorSpecialties
                    .OrderBy(ds => ds.SortOrder)
                    .Select(ds => new Dto.DoctorSpecialtyDto
                    {
                        Id = ds.Id,
                        DoctorId = ds.DoctorId,
                        SpecialtyId = ds.SpecialtyId,
                        Specialty = ds.Specialty != null ? _mapper.Map<Dto.SpecialtyDto>(ds.Specialty) : null,
                        SortOrder = ds.SortOrder
                    })
                    .ToList();
            }
            // Map medical conditions
            if (doctor.DoctorMedicalConditions != null && doctor.DoctorMedicalConditions.Any())
            {
                doctorDto.MedicalConditions = doctor.DoctorMedicalConditions
                    .Select(dmc => new Dto.DoctorMedicalConditionDto
                    {
                        Id = dmc.Id,
                        DoctorId = dmc.DoctorId,
                        MedicalConditionId = dmc.MedicalConditionId,
                        MedicalCondition = dmc.MedicalCondition != null ? _mapper.Map<Dto.MedicalConditionDto>(dmc.MedicalCondition) : null
                    })
                    .ToList();
            }
            return ApiResponse<DoctorDto>.Success(doctorDto, "پزشک با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor");
            return ApiResponse<DoctorDto>.Error("خطا در به‌روزرسانی پزشک", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی پزشک با فایل
    /// </summary>
    public async Task<ApiResponse<DoctorDto>> UpdateWithFileAsync(UpdateDoctorWithFileDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var doctor = await query
                .Include(d => d.User)
                .Include(d => d.DoctorSpecialties)
                .FirstOrDefaultAsync(d => d.Id == dto.Id, cancellationToken);

            if (doctor == null)
            {
                return ApiResponse<DoctorDto>.Error($"پزشک با شناسه {dto.Id} یافت نشد", 404);
            }

            // آپلود فایل عکس پروفایل (اگر وجود داشته باشد)
            string? profilePicturePath = null;
            if (dto.ProfilePictureFile != null && dto.ProfilePictureFile.Length > 0)
            {
                if (doctor.User != null)
                {
                    // حذف عکس قبلی اگر وجود داشته باشد
                    if (!string.IsNullOrEmpty(doctor.User.ProfilePicture))
                    {
                        await _fileUploadService.DeleteFileAsync(doctor.User.ProfilePicture, cancellationToken);
                    }

                    // آپلود عکس جدید
                    profilePicturePath = await _fileUploadService.UploadProfilePictureAsync(
                        dto.ProfilePictureFile,
                        doctor.User.Id,
                        cancellationToken);
                }
            }

            // تبدیل به UpdateDoctorDto و به‌روزرسانی
            var updateDto = _mapper.Map<UpdateDoctorDto>(dto);
            _mapper.Map(updateDto, doctor);

            // به‌روزرسانی عکس پروفایل کاربر
            if (doctor.User != null && !string.IsNullOrEmpty(profilePicturePath))
            {
                doctor.User.ProfilePicture = profilePicturePath;
            }
            doctor.User.CityId = dto.CityId;
            doctor.User.BirthDate = dto.BirthDate;
            doctor.User.Gender = dto.Gender;

            await _repository.UpdateAsync(doctor, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // به‌روزرسانی تخصص‌ها
            if (dto.SpecialtyIds != null && dto.SpecialtyIds.Any())
            {
                await SaveDoctorSpecialties(doctor.Id, dto.SpecialtyIds, cancellationToken);
            }

            // به‌روزرسانی علائم پزشکی
            if (dto.MedicalConditionIds != null)
            {
                await SaveDoctorMedicalConditions(doctor.Id, dto.MedicalConditionIds, cancellationToken);
            }

            // بارگذاری مجدد با شامل کردن روابط
            var reloadedQuery = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            doctor = await reloadedQuery
                .Include(d => d.User)
                .Include(d => d.DoctorClinics)
                    .ThenInclude(dc => dc.Clinic)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .Include(d => d.DoctorMedicalConditions)
                    .ThenInclude(dmc => dmc.MedicalCondition)
                .FirstOrDefaultAsync(d => d.Id == doctor.Id, cancellationToken);

            _logger.LogInformation("Doctor updated with ID: {DoctorId}", doctor.Id);
            var doctorDto = _mapper.Map<DoctorDto>(doctor);

            // Map User info including ProfilePicture
            if (doctor.User != null)
            {
                doctorDto.FirstName = doctor.User.FirstName;
                doctorDto.LastName = doctor.User.LastName;
                doctorDto.Phone = doctor.User.PhoneNumber ?? string.Empty;
                doctorDto.Email = doctor.User.Email;
                doctorDto.NationalCode = doctor.User.NationalCode;
                doctorDto.ProfilePicture = doctor.User.ProfilePicture;
            }

            doctorDto.Clinics = doctor.DoctorClinics
                .Where(dc => dc.Clinic != null)
                .Select(dc => _mapper.Map<Clinics.Dto.ClinicDto>(dc.Clinic))
                .ToList();
            // Map specialties
            if (doctor.DoctorSpecialties != null && doctor.DoctorSpecialties.Any())
            {
                doctorDto.Specialties = doctor.DoctorSpecialties
                    .OrderBy(ds => ds.SortOrder)
                    .Select(ds => new Dto.DoctorSpecialtyDto
                    {
                        Id = ds.Id,
                        DoctorId = ds.DoctorId,
                        SpecialtyId = ds.SpecialtyId,
                        Specialty = ds.Specialty != null ? _mapper.Map<Dto.SpecialtyDto>(ds.Specialty) : null,
                        SortOrder = ds.SortOrder
                    })
                    .ToList();
            }
            // Map medical conditions
            if (doctor.DoctorMedicalConditions != null && doctor.DoctorMedicalConditions.Any())
            {
                doctorDto.MedicalConditions = doctor.DoctorMedicalConditions
                    .Select(dmc => new Dto.DoctorMedicalConditionDto
                    {
                        Id = dmc.Id,
                        DoctorId = dmc.DoctorId,
                        MedicalConditionId = dmc.MedicalConditionId,
                        MedicalCondition = dmc.MedicalCondition != null ? _mapper.Map<Dto.MedicalConditionDto>(dmc.MedicalCondition) : null
                    })
                    .ToList();
            }
            return ApiResponse<DoctorDto>.Success(doctorDto, "پزشک با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor with file");
            return ApiResponse<DoctorDto>.Error("خطا در به‌روزرسانی پزشک با فایل", ex);
        }
    }

    /// <summary>
    /// حذف پزشک
    /// </summary>
    /// <param name="id">شناسه پزشک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ API</returns>
    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Doctor deleted with ID: {DoctorId}", id);
            return ApiResponse.Success("پزشک با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor");
            return ApiResponse.Error("خطا در حذف پزشک", ex);
        }
    }

    /// <summary>
    /// جستجوی پزشکان بر اساس نام پزشک یا نام کلینیک
    /// </summary>
    /// <param name="query">متن جستجو</param>
    /// <param name="clinicName">نام کلینیک (اختیاری)</param>
    /// <param name="doctorName">نام پزشک (اختیاری)</param>
    /// <param name="page">شماره صفحه</param>
    /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ API شامل نتیجه جستجو</returns>
    public async Task<ApiResponse<PagedResult<DoctorDto>>> SearchAsync(
        string? query = null,
        string? clinicName = null,
        string? doctorName = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dbQuery = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            dbQuery = dbQuery
                .Include(d => d.User)
                .Include(d => d.DoctorClinics)
                    .ThenInclude(dc => dc.Clinic);

            // فیلتر بر اساس نام پزشک
            if (!string.IsNullOrWhiteSpace(doctorName))
            {
                var doctorNameLower = doctorName.ToLower();
                dbQuery = dbQuery.Where(d =>
                    (d.User != null && (d.User.FirstName.ToLower().Contains(doctorNameLower) ||
                                      d.User.LastName.ToLower().Contains(doctorNameLower) ||
                                      (d.User.FirstName + " " + d.User.LastName).ToLower().Contains(doctorNameLower))));
            }

            // فیلتر بر اساس نام کلینیک
            if (!string.IsNullOrWhiteSpace(clinicName))
            {
                var clinicNameLower = clinicName.ToLower();
                dbQuery = dbQuery.Where(d =>
                    d.DoctorClinics.Any(dc => dc.Clinic != null && dc.Clinic.Name.ToLower().Contains(clinicNameLower)));
            }

            // جستجوی عمومی
            if (!string.IsNullOrWhiteSpace(query))
            {
                var queryLower = query.ToLower();
                dbQuery = dbQuery.Where(d =>
                    (d.User != null && (d.User.FirstName.ToLower().Contains(queryLower) ||
                                      d.User.LastName.ToLower().Contains(queryLower) ||
                                      (d.User.FirstName + " " + d.User.LastName).ToLower().Contains(queryLower) ||
                                      d.User.PhoneNumber != null && d.User.PhoneNumber.Contains(query) ||
                                      d.User.Email.ToLower().Contains(queryLower) ||
                                      d.MedicalCode.ToLower().Contains(queryLower))) ||
                    d.DoctorClinics.Any(dc => dc.Clinic != null && dc.Clinic.Name.ToLower().Contains(queryLower)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            // صفحه‌بندی با OrderBy برای جلوگیری از نتایج غیرقابل پیش‌بینی
            var doctors = await dbQuery
                .OrderBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var doctorDtos = doctors.Select(d =>
            {
                var dto = _mapper.Map<DoctorDto>(d);
                // Map User info
                if (d.User != null)
                {
                    dto.FirstName = d.User.FirstName;
                    dto.LastName = d.User.LastName;
                    dto.Phone = d.User.PhoneNumber ?? string.Empty;
                    dto.Email = d.User.Email;
                    dto.NationalCode = d.User.NationalCode;
                }
                // Map clinics
                dto.Clinics = d.DoctorClinics
                    .Where(dc => dc.Clinic != null)
                    .Select(dc => _mapper.Map<Clinics.Dto.ClinicDto>(dc.Clinic))
                    .ToList();
                return dto;
            }).ToList();

            var result = new PagedResult<DoctorDto>
            {
                Items = doctorDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<DoctorDto>>.Success(result, "جستجو با موفقیت انجام شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching doctors");
            return ApiResponse<PagedResult<DoctorDto>>.Error("خطا در جستجوی پزشکان", ex);
        }
    }

    /// <summary>
    /// جستجوی پزشک بر اساس کد نظام پزشکی
    /// </summary>
    /// <param name="medicalCode">کد نظام پزشکی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ API شامل پزشک یافت شده</returns>
    public async Task<ApiResponse<DoctorDto>> GetByMedicalCodeAsync(string medicalCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(medicalCode))
            {
                return ApiResponse<DoctorDto>.Error("کد نظام پزشکی نمی‌تواند خالی باشد", 400);
            }

            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            var doctor = await query
                .Include(d => d.User)
                .Include(d => d.DoctorClinics)
                    .ThenInclude(dc => dc.Clinic)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .Include(d => d.DoctorMedicalConditions)
                    .ThenInclude(dmc => dmc.MedicalCondition)
                .FirstOrDefaultAsync(d => d.MedicalCode == medicalCode, cancellationToken);

            if (doctor == null)
            {
                return ApiResponse<DoctorDto>.Error("پزشکی با این کد نظام پزشکی یافت نشد", 404);
            }

            var doctorDto = _mapper.Map<DoctorDto>(doctor);
            doctorDto.Clinics = doctor.DoctorClinics
                .Where(dc => dc.Clinic != null)
                .Select(dc => _mapper.Map<Clinics.Dto.ClinicDto>(dc.Clinic))
                .ToList();
            // Map specialties
            if (doctor.DoctorSpecialties != null && doctor.DoctorSpecialties.Any())
            {
                doctorDto.Specialties = doctor.DoctorSpecialties
                    .OrderBy(ds => ds.SortOrder)
                    .Select(ds => new Dto.DoctorSpecialtyDto
                    {
                        Id = ds.Id,
                        DoctorId = ds.DoctorId,
                        SpecialtyId = ds.SpecialtyId,
                        Specialty = ds.Specialty != null ? _mapper.Map<Dto.SpecialtyDto>(ds.Specialty) : null,
                        SortOrder = ds.SortOrder
                    })
                    .ToList();
            }
            // Map medical conditions
            if (doctor.DoctorMedicalConditions != null && doctor.DoctorMedicalConditions.Any())
            {
                doctorDto.MedicalConditions = doctor.DoctorMedicalConditions
                    .Select(dmc => new Dto.DoctorMedicalConditionDto
                    {
                        Id = dmc.Id,
                        DoctorId = dmc.DoctorId,
                        MedicalConditionId = dmc.MedicalConditionId,
                        MedicalCondition = dmc.MedicalCondition != null ? _mapper.Map<Dto.MedicalConditionDto>(dmc.MedicalCondition) : null
                    })
                    .ToList();
            }

            return ApiResponse<DoctorDto>.Success(doctorDto, "پزشک با موفقیت یافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor by medical code: {MedicalCode}", medicalCode);
            return ApiResponse<DoctorDto>.Error("خطا در جستجوی پزشک", ex);
        }
    }

    /// <summary>
    /// ذخیره تخصص‌های پزشک در جدول DoctorSpecialties
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="specialtyIds">لیست شناسه تخصص‌ها</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    private async Task SaveDoctorSpecialties(int doctorId, List<int> specialtyIds, CancellationToken cancellationToken)
    {
        try
        {
            // دریافت تخصص‌های موجود پزشک
            var query = await _doctorSpecialtyRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var existingSpecialties = await query
                .Where(ds => ds.DoctorId == doctorId)
                .ToListAsync(cancellationToken);

            var existingSpecialtyIds = existingSpecialties.Select(ds => ds.SpecialtyId).ToList();

            // حذف تخصص‌هایی که دیگر انتخاب نشده‌اند
            var specialtiesToRemove = existingSpecialties
                .Where(ds => !specialtyIds.Contains(ds.SpecialtyId))
                .ToList();

            foreach (var specialtyToRemove in specialtiesToRemove)
            {
                await _doctorSpecialtyRepository.DeleteAsync(specialtyToRemove.Id, cancellationToken);
            }

            // اضافه کردن تخصص‌های جدید
            var newSpecialtyIds = specialtyIds
                .Where(id => !existingSpecialtyIds.Contains(id))
                .ToList();

            for (int i = 0; i < newSpecialtyIds.Count; i++)
            {
                var specialtyId = newSpecialtyIds[i];
                var doctorSpecialty = new DoctorSpecialty
                {
                    DoctorId = doctorId,
                    SpecialtyId = specialtyId,
                    SortOrder = specialtyIds.IndexOf(specialtyId)
                };
                await _doctorSpecialtyRepository.AddAsync(doctorSpecialty, cancellationToken);
            }

            // به‌روزرسانی ترتیب تخصص‌های موجود
            var specialtiesToUpdate = existingSpecialties
                .Where(ds => specialtyIds.Contains(ds.SpecialtyId))
                .ToList();

            foreach (var existingSpecialty in specialtiesToUpdate)
            {
                var newSortOrder = specialtyIds.IndexOf(existingSpecialty.SpecialtyId);
                if (existingSpecialty.SortOrder != newSortOrder)
                {
                    existingSpecialty.SortOrder = newSortOrder;
                    await _doctorSpecialtyRepository.UpdateAsync(existingSpecialty, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Doctor specialties saved for doctor {DoctorId}", doctorId);

            // اضافه کردن علائم مرتبط با تخصص‌ها به صورت خودکار
            await AddMedicalConditionsFromSpecialties(doctorId, specialtyIds, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving doctor specialties for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    /// <summary>
    /// ذخیره علائم پزشکی پزشک در جدول DoctorMedicalConditions
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="medicalConditionIds">لیست شناسه علائم پزشکی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    private async Task SaveDoctorMedicalConditions(int doctorId, List<int> medicalConditionIds, CancellationToken cancellationToken)
    {
        try
        {
            // دریافت علائم موجود پزشک
            var query = await _doctorMedicalConditionRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var existingConditions = await query
                .Where(dmc => dmc.DoctorId == doctorId)
                .ToListAsync(cancellationToken);

            var existingConditionIds = existingConditions.Select(dmc => dmc.MedicalConditionId).ToList();

            // حذف علائمی که دیگر انتخاب نشده‌اند
            var conditionsToRemove = existingConditions
                .Where(dmc => !medicalConditionIds.Contains(dmc.MedicalConditionId))
                .ToList();

            foreach (var conditionToRemove in conditionsToRemove)
            {
                await _doctorMedicalConditionRepository.DeleteAsync(conditionToRemove.Id, cancellationToken);
            }

            // اضافه کردن علائم جدید
            var newConditionIds = medicalConditionIds
                .Where(id => !existingConditionIds.Contains(id))
                .ToList();

            foreach (var conditionId in newConditionIds)
            {
                var doctorMedicalCondition = new DoctorMedicalCondition
                {
                    DoctorId = doctorId,
                    MedicalConditionId = conditionId
                };
                await _doctorMedicalConditionRepository.AddAsync(doctorMedicalCondition, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Doctor medical conditions saved for doctor {DoctorId}", doctorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving doctor medical conditions for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    /// <summary>
    /// اضافه کردن علائم مرتبط با تخصص‌ها به صورت خودکار
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="specialtyIds">لیست شناسه تخصص‌ها</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    private async Task AddMedicalConditionsFromSpecialties(int doctorId, List<int> specialtyIds, CancellationToken cancellationToken)
    {
        try
        {
            // دریافت علائم مرتبط با تخصص‌ها
            var specialtyConditionQuery = await _specialtyMedicalConditionRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var specialtyConditions = await specialtyConditionQuery
                .Where(smc => specialtyIds.Contains(smc.SpecialtyId))
                .Select(smc => smc.MedicalConditionId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (specialtyConditions.Any())
            {
                // دریافت علائم موجود پزشک
                var doctorConditionQuery = await _doctorMedicalConditionRepository.GetQueryableNoTrackingAsync(cancellationToken);
                var existingDoctorConditions = await doctorConditionQuery
                    .Where(dmc => dmc.DoctorId == doctorId)
                    .Select(dmc => dmc.MedicalConditionId)
                    .ToListAsync(cancellationToken);

                // اضافه کردن علائم جدید که از طریق تخصص‌ها آمده‌اند
                var conditionsToAdd = specialtyConditions
                    .Where(mcId => !existingDoctorConditions.Contains(mcId))
                    .ToList();

                foreach (var conditionId in conditionsToAdd)
                {
                    var doctorMedicalCondition = new DoctorMedicalCondition
                    {
                        DoctorId = doctorId,
                        MedicalConditionId = conditionId
                    };
                    await _doctorMedicalConditionRepository.AddAsync(doctorMedicalCondition, cancellationToken);
                }

                if (conditionsToAdd.Any())
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Added {Count} medical conditions from specialties for doctor {DoctorId}", conditionsToAdd.Count, doctorId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding medical conditions from specialties for doctor {DoctorId}", doctorId);
            // این خطا را throw نمی‌کنیم چون یک عملیات اختیاری است
        }
    }

    /// <summary>
    /// جستجوی پزشکان بر اساس علائم پزشکی
    /// </summary>
    /// <param name="medicalConditionName">نام علائم پزشکی</param>
    /// <param name="page">شماره صفحه</param>
    /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ API شامل نتیجه جستجو</returns>
    public async Task<ApiResponse<PagedResult<DoctorDto>>> SearchByMedicalConditionAsync(
        string medicalConditionName,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(medicalConditionName))
            {
                return ApiResponse<PagedResult<DoctorDto>>.Error("نام علائم پزشکی نمی‌تواند خالی باشد", 400);
            }

            // جستجوی علائم پزشکی
            var medicalConditionQuery = await _medicalConditionRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var medicalConditionNameLower = medicalConditionName.ToLower();
            var medicalConditions = await medicalConditionQuery
                .Where(mc => mc.Name.ToLower().Contains(medicalConditionNameLower))
                .Select(mc => mc.Id)
                .ToListAsync(cancellationToken);

            if (!medicalConditions.Any())
            {
                return ApiResponse<PagedResult<DoctorDto>>.Success(
                    new PagedResult<DoctorDto>
                    {
                        Items = new List<DoctorDto>(),
                        TotalCount = 0,
                        Page = page,
                        PageSize = pageSize
                    },
                    "هیچ پزشکی برای این علائم یافت نشد");
            }

            // جستجوی پزشکان مرتبط با این علائم
            var dbQuery = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            dbQuery = dbQuery
                .Include(d => d.User)
                .Include(d => d.DoctorClinics)
                    .ThenInclude(dc => dc.Clinic)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .Include(d => d.DoctorMedicalConditions)
                    .ThenInclude(dmc => dmc.MedicalCondition)
                .Where(d => d.DoctorMedicalConditions.Any(dmc => medicalConditions.Contains(dmc.MedicalConditionId)));

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            // صفحه‌بندی با OrderBy برای جلوگیری از نتایج غیرقابل پیش‌بینی
            var doctors = await dbQuery
                .OrderBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var doctorDtos = doctors.Select(d =>
            {
                var dto = _mapper.Map<DoctorDto>(d);
                // Map User info
                if (d.User != null)
                {
                    dto.FirstName = d.User.FirstName;
                    dto.LastName = d.User.LastName;
                    dto.Phone = d.User.PhoneNumber ?? string.Empty;
                    dto.Email = d.User.Email;
                    dto.NationalCode = d.User.NationalCode;
                }
                // Map clinics
                dto.Clinics = d.DoctorClinics
                    .Where(dc => dc.Clinic != null)
                    .Select(dc => _mapper.Map<Clinics.Dto.ClinicDto>(dc.Clinic))
                    .ToList();
                // Map specialties
                if (d.DoctorSpecialties != null && d.DoctorSpecialties.Any())
                {
                    dto.Specialties = d.DoctorSpecialties
                        .OrderBy(ds => ds.SortOrder)
                        .Select(ds => new Dto.DoctorSpecialtyDto
                        {
                            Id = ds.Id,
                            DoctorId = ds.DoctorId,
                            SpecialtyId = ds.SpecialtyId,
                            Specialty = ds.Specialty != null ? _mapper.Map<Dto.SpecialtyDto>(ds.Specialty) : null,
                            SortOrder = ds.SortOrder
                        })
                        .ToList();
                }
                // Map medical conditions
                if (d.DoctorMedicalConditions != null && d.DoctorMedicalConditions.Any())
                {
                    dto.MedicalConditions = d.DoctorMedicalConditions
                        .Select(dmc => new Dto.DoctorMedicalConditionDto
                        {
                            Id = dmc.Id,
                            DoctorId = dmc.DoctorId,
                            MedicalConditionId = dmc.MedicalConditionId,
                            MedicalCondition = dmc.MedicalCondition != null ? _mapper.Map<Dto.MedicalConditionDto>(dmc.MedicalCondition) : null
                        })
                        .ToList();
                }
                return dto;
            }).ToList();

            var result = new PagedResult<DoctorDto>
            {
                Items = doctorDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<DoctorDto>>.Success(result, "جستجو با موفقیت انجام شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching doctors by medical condition: {MedicalConditionName}", medicalConditionName);
            return ApiResponse<PagedResult<DoctorDto>>.Error("خطا در جستجوی پزشکان", ex);
        }
    }
}
