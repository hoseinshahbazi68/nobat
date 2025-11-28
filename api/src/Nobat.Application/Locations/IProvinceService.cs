using Nobat.Application.Common;
using Nobat.Application.Locations.Dto;
using Sieve.Models;

namespace Nobat.Application.Locations;

/// <summary>
/// اینترفیس سرویس استان
/// </summary>
public interface IProvinceService
{
    Task<ApiResponse<ProvinceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<ProvinceDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProvinceDto>> CreateAsync(CreateProvinceDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProvinceDto>> UpdateAsync(UpdateProvinceDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
