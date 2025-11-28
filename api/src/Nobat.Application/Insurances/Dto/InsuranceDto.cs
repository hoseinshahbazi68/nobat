namespace Nobat.Application.Insurances.Dto;

/// <summary>
/// DTO بیمه
/// </summary>
public class InsuranceDto
{
    /// <summary>
    /// شناسه بیمه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام بیمه
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// کد بیمه
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// وضعیت فعال بودن
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد بیمه
/// </summary>
public class CreateInsuranceDto
{
    /// <summary>
    /// نام بیمه
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// کد بیمه
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// وضعیت فعال بودن
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO به‌روزرسانی بیمه
/// </summary>
public class UpdateInsuranceDto
{
    /// <summary>
    /// شناسه بیمه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام بیمه
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// کد بیمه
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// وضعیت فعال بودن
    /// </summary>
    public bool IsActive { get; set; }
}
