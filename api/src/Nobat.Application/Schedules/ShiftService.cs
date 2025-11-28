using AutoMapper;
using Microsoft.Extensions.Logging;
using Nobat.Application.Schedules.Dto;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Schedules;

/// <summary>
/// سرویس شیفت
/// </summary>
public class ShiftService : IShiftService
{
    /// <summary>
    /// ریپازیتوری شیفت
    /// </summary>
    private readonly IRepository<Shift> _repository;

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
    private readonly ILogger<ShiftService> _logger;

    /// <summary>
    /// واحد کار
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// سازنده سرویس شیفت
    /// </summary>
    /// <param name="repository">ریپازیتوری شیفت</param>
    /// <param name="mapper">مپر AutoMapper</param>
    /// <param name="sieveProcessor">پردازشگر Sieve</param>
    /// <param name="logger">لاگر</param>
    /// <param name="unitOfWork">واحد کار</param>
    public ShiftService(
        IRepository<Shift> repository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<ShiftService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// دریافت شیفت بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت یافت شده یا null</returns>
    public async Task<ShiftDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var shift = await _repository.GetByIdAsync(id, cancellationToken);
        return shift != null ? _mapper.Map<ShiftDto>(shift) : null;
    }

    /// <summary>
    /// دریافت لیست صفحه‌بندی شده شیفت‌ها
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه صفحه‌بندی شده</returns>
    public async Task<PagedResult<ShiftDto>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        var query = await _repository.GetQueryableAsync(cancellationToken);

        var totalCount = query.Count();
        var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
        var shifts = filteredQuery.ToList();

        var result = new PagedResult<ShiftDto>
        {
            Items = _mapper.Map<List<ShiftDto>>(shifts),
            TotalCount = totalCount,
            Page = sieveModel.Page ?? 1,
            PageSize = sieveModel.PageSize ?? 10
        };

        return result;
    }

    /// <summary>
    /// ایجاد شیفت جدید
    /// </summary>
    /// <param name="dto">اطلاعات شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت ایجاد شده</returns>
    public async Task<ShiftDto> CreateAsync(CreateShiftDto dto, CancellationToken cancellationToken = default)
    {
        var shift = _mapper.Map<Shift>(dto);
        await _repository.AddAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shift created with ID: {ShiftId}", shift.Id);
        return _mapper.Map<ShiftDto>(shift);
    }

    /// <summary>
    /// به‌روزرسانی شیفت
    /// </summary>
    /// <param name="dto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت به‌روز شده</returns>
    public async Task<ShiftDto> UpdateAsync(UpdateShiftDto dto, CancellationToken cancellationToken = default)
    {
        var shift = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (shift == null)
        {
            throw new KeyNotFoundException($"Shift with ID {dto.Id} not found.");
        }

        _mapper.Map(dto, shift);
        await _repository.UpdateAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shift updated with ID: {ShiftId}", shift.Id);
        return _mapper.Map<ShiftDto>(shift);
    }

    /// <summary>
    /// حذف شیفت
    /// </summary>
    /// <param name="id">شناسه شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shift deleted with ID: {ShiftId}", id);
    }
}
