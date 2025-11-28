using AutoMapper;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Services;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Entities.Insurances;
using Nobat.Domain.Entities.Clinics;
using Nobat.Application.Users.Dto;
using Nobat.Application.Schedules.Dto;
using Nobat.Application.Doctors.Dto;
using Nobat.Application.Services.Dto;
using Nobat.Application.Insurances.Dto;
using Nobat.Application.Clinics.Dto;
using Nobat.Application.Chat.Dto;
using Nobat.Application.Locations.Dto;
using Nobat.Domain.Entities.Chat;
using Nobat.Domain.Entities.Locations;

namespace Nobat.Application.Mappings;

/// <summary>
/// پروفایل مپینگ AutoMapper
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// سازنده پروفایل مپینگ
    /// </summary>
    public MappingProfile()
    {
        // Shift mappings
        CreateMap<Shift, ShiftDto>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")));

        CreateMap<CreateShiftDto, Shift>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.EndTime)))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateShiftDto, Shift>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.EndTime)))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()))
            .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicture));

        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Role mappings
        CreateMap<Role, RoleDto>();

        CreateMap<CreateRoleDto, Role>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateRoleDto, Role>()
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Doctor mappings
        CreateMap<Doctor, DoctorDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : string.Empty))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User != null ? src.User.LastName : string.Empty))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User != null ? (src.User.PhoneNumber ?? string.Empty) : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.NationalCode, opt => opt.MapFrom(src => src.User != null ? src.User.NationalCode : string.Empty))
            .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePicture : null))
            .ForMember(dest => dest.Clinics, opt => opt.Ignore()); // Clinics will be mapped manually

        // DoctorVisitInfo mappings
        CreateMap<DoctorVisitInfo, DoctorVisitInfoDto>()
            .ForMember(dest => dest.DoctorName, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.MedicalCode, opt => opt.Ignore()); // Will be set manually

        CreateMap<CreateDoctorVisitInfoDto, DoctorVisitInfo>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Doctor, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateDoctorVisitInfoDto, DoctorVisitInfo>()
            .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
            .ForMember(dest => dest.Doctor, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<CreateDoctorDto, Doctor>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DoctorSchedules, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceTariffs, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<CreateDoctorWithFileDto, CreateDoctorDto>();
        CreateMap<UpdateDoctorDto, Doctor>()
            .ForMember(dest => dest.DoctorSchedules, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceTariffs, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateDoctorWithFileDto, UpdateDoctorDto>();

        // Insurance mappings
        CreateMap<Insurance, InsuranceDto>();
        CreateMap<CreateInsuranceDto, Insurance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceTariffs, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateInsuranceDto, Insurance>()
            .ForMember(dest => dest.ServiceTariffs, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Service mappings
        CreateMap<Service, ServiceDto>();
        CreateMap<CreateServiceDto, Service>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceTariffs, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateServiceDto, Service>()
            .ForMember(dest => dest.ServiceTariffs, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Specialty mappings
        CreateMap<Specialty, SpecialtyDto>();

        CreateMap<CreateSpecialtyDto, Specialty>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateSpecialtyDto, Specialty>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // MedicalCondition mappings
        CreateMap<MedicalCondition, MedicalConditionDto>();

        CreateMap<CreateMedicalConditionDto, MedicalCondition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SpecialtyMedicalConditions, opt => opt.Ignore())
            .ForMember(dest => dest.DoctorMedicalConditions, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateMedicalConditionDto, MedicalCondition>()
            .ForMember(dest => dest.SpecialtyMedicalConditions, opt => opt.Ignore())
            .ForMember(dest => dest.DoctorMedicalConditions, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // SpecialtyMedicalCondition mappings
        CreateMap<SpecialtyMedicalCondition, SpecialtyMedicalConditionDto>();

        // UserActivityLog mappings
        CreateMap<UserActivityLog, UserActivityLogDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.Ignore());

        // ChatMessage mappings
        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(dest => dest.ChatSessionId, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.PhoneNumber) ? string.Empty : $"chat-{src.PhoneNumber}"))
            .ForMember(dest => dest.SupportUserName, opt => opt.MapFrom(src =>
                src.SenderType == "Support" && src.SenderName != "سیستم هوشمند" ? src.SenderName : null));

        // Clinic mappings
        CreateMap<Clinic, ClinicDto>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null));
        CreateMap<CreateClinicDto, Clinic>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DoctorClinics, opt => opt.Ignore())
            .ForMember(dest => dest.ClinicUsers, opt => opt.Ignore())
            .ForMember(dest => dest.City, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateClinicDto, Clinic>()
            .ForMember(dest => dest.DoctorClinics, opt => opt.Ignore())
            .ForMember(dest => dest.ClinicUsers, opt => opt.Ignore())
            .ForMember(dest => dest.City, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Province mappings
        CreateMap<Province, ProvinceDto>();
        CreateMap<CreateProvinceDto, Province>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Cities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateProvinceDto, Province>()
            .ForMember(dest => dest.Cities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // City mappings
        CreateMap<City, CityDto>()
            .ForMember(dest => dest.ProvinceName, opt => opt.Ignore()); // Will be set manually
        CreateMap<CreateCityDto, City>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Province, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateCityDto, City>()
            .ForMember(dest => dest.Province, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
