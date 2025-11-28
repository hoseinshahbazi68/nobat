using Nobat.Application.Common;
using Nobat.Application.Locations.Dto;
using Sieve.Models;

namespace Nobat.Application.Locations;

/// <summary>
/// اینترفیس سرویس شهر
/// </summary>
public interface ICityService
{
    Task<ApiResponse<CityDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<CityDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);
    Task<ApiResponse<CityDto>> CreateAsync(CreateCityDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CityDto>> UpdateAsync(UpdateCityDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<CityDto>>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default);
}
