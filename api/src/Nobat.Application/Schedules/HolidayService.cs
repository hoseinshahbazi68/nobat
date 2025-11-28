using AutoMapper;
using Microsoft.Extensions.Logging;
using Nobat.Application.Schedules.Dto;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Schedules;

/// <summary>
/// سرویس روز تعطیل
/// این سرویس عملیات CRUD مربوط به روزهای تعطیل را مدیریت می‌کند
/// شامل دریافت، ایجاد، به‌روزرسانی و حذف روزهای تعطیل
/// از Sieve برای فیلتر و مرتب‌سازی استفاده می‌کند
/// </summary>
public class HolidayService : IHolidayService
{
    /// <summary>
    /// ریپازیتوری روز تعطیل
    /// </summary>
    private readonly IRepository<Holiday> _repository;

    /// <summary>
    /// مپر AutoMapper
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// پردازشگر Sieve برای فیلتر و مرتب‌سازی
    /// </summary>
    private readonly ISieveProcessor _sieveProcessor;

    /// <summary>
    /// لاگر
    /// </summary>
    private readonly ILogger<HolidayService> _logger;

    /// <summary>
    /// واحد کار
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// سازنده سرویس روز تعطیل
    /// </summary>
    /// <param name="repository">ریپازیتوری روز تعطیل</param>
    /// <param name="mapper">مپر AutoMapper</param>
    /// <param name="sieveProcessor">پردازشگر Sieve</param>
    /// <param name="logger">لاگر</param>
    /// <param name="unitOfWork">واحد کار</param>
    public HolidayService(
        IRepository<Holiday> repository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<HolidayService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// دریافت روز تعطیل بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه روز تعطیل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>روز تعطیل یافت شده یا null</returns>
    public async Task<HolidayDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var holiday = await _repository.GetByIdAsync(id, cancellationToken);
        if (holiday == null)
            return null;

        var dto = _mapper.Map<HolidayDto>(holiday);
        dto.Date = holiday.Date.ToString("yyyy-MM-dd");
        return dto;
    }

    /// <summary>
    /// دریافت لیست روزهای تعطیل با فیلتر و مرتب‌سازی
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه صفحه‌بندی شده</returns>
    public async Task<PagedResult<HolidayDto>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        var query = await _repository.GetQueryableAsync(cancellationToken);

        var totalCount = query.Count();
        var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
        var holidays = filteredQuery.ToList();

        var result = new PagedResult<HolidayDto>
        {
            Items = holidays.Select(h => new HolidayDto
            {
                Id = h.Id,
                Date = h.Date.ToString("yyyy-MM-dd"),
                Name = h.Name,
                Description = h.Description,
                CreatedAt = h.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = sieveModel.Page ?? 1,
            PageSize = sieveModel.PageSize ?? 10
        };

        return result;
    }

    /// <summary>
    /// ایجاد روز تعطیل جدید
    /// </summary>
    /// <param name="dto">اطلاعات روز تعطیل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>روز تعطیل ایجاد شده</returns>
    public async Task<HolidayDto> CreateAsync(CreateHolidayDto dto, CancellationToken cancellationToken = default)
    {
        var holiday = new Holiday
        {
            Date = DateTime.Parse(dto.Date),
            Name = dto.Name,
            Description = dto.Description
        };

        await _repository.AddAsync(holiday, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Holiday created with ID: {HolidayId}", holiday.Id);

        return new HolidayDto
        {
            Id = holiday.Id,
            Date = holiday.Date.ToString("yyyy-MM-dd"),
            Name = holiday.Name,
            Description = holiday.Description,
            CreatedAt = holiday.CreatedAt
        };
    }

    /// <summary>
    /// به‌روزرسانی روز تعطیل
    /// </summary>
    /// <param name="dto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>روز تعطیل به‌روز شده</returns>
    /// <exception cref="KeyNotFoundException">در صورت عدم یافتن روز تعطیل</exception>
    public async Task<HolidayDto> UpdateAsync(UpdateHolidayDto dto, CancellationToken cancellationToken = default)
    {
        var holiday = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (holiday == null)
        {
            throw new KeyNotFoundException($"Holiday with ID {dto.Id} not found.");
        }

        holiday.Date = DateTime.Parse(dto.Date);
        holiday.Name = dto.Name;
        holiday.Description = dto.Description;

        await _repository.UpdateAsync(holiday, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Holiday updated with ID: {HolidayId}", holiday.Id);

        return new HolidayDto
        {
            Id = holiday.Id,
            Date = holiday.Date.ToString("yyyy-MM-dd"),
            Name = holiday.Name,
            Description = holiday.Description,
            CreatedAt = holiday.CreatedAt
        };
    }

    /// <summary>
    /// حذف روز تعطیل
    /// </summary>
    /// <param name="id">شناسه روز تعطیل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Holiday deleted with ID: {HolidayId}", id);
    }
}
