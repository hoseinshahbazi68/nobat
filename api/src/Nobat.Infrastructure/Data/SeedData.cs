using Microsoft.EntityFrameworkCore;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Entities.Clinics;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Insurances;
using Nobat.Domain.Entities.Services;
using Nobat.Domain.Entities.Locations;
using System.Security.Cryptography;
using System.Text;

namespace Nobat.Infrastructure.Data;

public static class SeedData
{
    public static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Administrator role", CreatedAt = DateTime.UtcNow },
            new Role { Id = 2, Name = "User", Description = "Regular user role", CreatedAt = DateTime.UtcNow },
            new Role { Id = 3, Name = "Support", Description = "Support staff role", CreatedAt = DateTime.UtcNow },
            new Role { Id = 4, Name = "ClinicManager", Description = "مدیر کلینیک", CreatedAt = DateTime.UtcNow }
        );
    }

    public static void SeedUsers(ModelBuilder modelBuilder)
    {
        var adminPasswordHash = HashPassword("123456");
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                NationalCode = "4670038893",
                Email = "admin@nobat.com",
                PasswordHash = adminPasswordHash,
                FirstName = "مدیر",
                LastName = "سیستم",
                IsActive = true,
                CreatedAt = createdAt,
                PhoneNumber = "09134215104"
            }
        );

        // اتصال کاربر admin به نقش Admin
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                Id = 1,
                UserId = 1,
                RoleId = 1,
                CreatedAt = createdAt
            },
            new UserRole
            {
                Id = 2,
                UserId = 1,
                RoleId = 2,
                CreatedAt = createdAt
            }
            ,
            new UserRole
            {
                Id = 3,
                UserId = 1,
                RoleId = 3,
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedShifts(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<Shift>().HasData(
            new Shift
            {
                Id = 1,
                Name = "صبح",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                Description = "شیفت صبح از ساعت 8 تا 12",
                CreatedAt = createdAt
            },
            new Shift
            {
                Id = 2,
                Name = "ظهر",
                StartTime = new TimeSpan(12, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                Description = "شیفت ظهر از ساعت 12 تا 16",
                CreatedAt = createdAt
            },
            new Shift
            {
                Id = 3,
                Name = "عصر",
                StartTime = new TimeSpan(16, 0, 0),
                EndTime = new TimeSpan(20, 0, 0),
                Description = "شیفت عصر از ساعت 16 تا 20",
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedClinics(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<Clinic>().HasData(
            new Clinic
            {
                Id = 1,
                Name = "کلینیک تست",
                Address = "تهران، خیابان ولیعصر، پلاک 100",
                Phone = "021-12345678",
                Email = "test@clinic.com",
                IsActive = true,
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedClinicUsers(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<ClinicUser>().HasData(
            new ClinicUser
            {
                Id = 1,
                ClinicId = 1,
                UserId = 1,
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedDoctors(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                Id = 1,
                MedicalCode = "12345",
                UserId = 1,
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedDoctorClinics(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<DoctorClinic>().HasData(
            new DoctorClinic
            {
                Id = 1,
                DoctorId = 1,
                ClinicId = 1,
                IsActive = true,
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedInsurances(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<Insurance>().HasData(
            new Insurance
            {
                Id = 1,
                Name = "بیمه سلامت",
                Code = "SALAMAT",
                IsActive = true,
                CreatedAt = createdAt
            },
            new Insurance
            {
                Id = 2,
                Name = "تأمین اجتماعی",
                Code = "TAAMIN",
                IsActive = true,
                CreatedAt = createdAt
            },
            new Insurance
            {
                Id = 3,
                Name = "خدمات درمانی",
                Code = "KHADAMAT",
                IsActive = true,
                CreatedAt = createdAt
            },
            new Insurance
            {
                Id = 4,
                Name = "بیمه ایران",
                Code = "IRAN",
                IsActive = true,
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedServices(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<Service>().HasData(
            new Service
            {
                Id = 1,
                Name = "ویزیت",
                Description = "ویزیت عمومی پزشک",
                CreatedAt = createdAt
            },
            new Service
            {
                Id = 2,
                Name = "معاینه",
                Description = "معاینه فیزیکی بیمار",
                CreatedAt = createdAt
            },
            new Service
            {
                Id = 3,
                Name = "آزمایش",
                Description = "درخواست آزمایش",
                CreatedAt = createdAt
            },
            new Service
            {
                Id = 4,
                Name = "سونوگرافی",
                Description = "سونوگرافی",
                CreatedAt = createdAt
            },
            new Service
            {
                Id = 5,
                Name = "نوار قلب",
                Description = "نوار قلب (ECG)",
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedSpecialties(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<Specialty>().HasData(
            new Specialty
            {
                Id = 1,
                Name = "قلب و عروق",
                Description = "تخصص قلب و عروق",
                CreatedAt = createdAt
            },
            new Specialty
            {
                Id = 2,
                Name = "داخلی",
                Description = "تخصص داخلی",
                CreatedAt = createdAt
            },
            new Specialty
            {
                Id = 3,
                Name = "اطفال",
                Description = "تخصص اطفال",
                CreatedAt = createdAt
            },
            new Specialty
            {
                Id = 4,
                Name = "جراحی عمومی",
                Description = "تخصص جراحی عمومی",
                CreatedAt = createdAt
            },
            new Specialty
            {
                Id = 5,
                Name = "زنان و زایمان",
                Description = "تخصص زنان و زایمان",
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedDoctorSpecialties(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<DoctorSpecialty>().HasData(
            new DoctorSpecialty
            {
                Id = 1,
                DoctorId = 1,
                SpecialtyId = 1,
                SortOrder = 1,
                CreatedAt = createdAt
            },
            new DoctorSpecialty
            {
                Id = 2,
                DoctorId = 1,
                SpecialtyId = 2,
                SortOrder = 2,
                CreatedAt = createdAt
            }
        );
    }

    public static void SeedProvinces(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<Province>().HasData(
            new Province { Id = 1, Name = "آذربایجان شرقی", Code = "03", CreatedAt = createdAt },
            new Province { Id = 2, Name = "آذربایجان غربی", Code = "04", CreatedAt = createdAt },
            new Province { Id = 3, Name = "اردبیل", Code = "05", CreatedAt = createdAt },
            new Province { Id = 4, Name = "اصفهان", Code = "10", CreatedAt = createdAt },
            new Province { Id = 5, Name = "البرز", Code = "30", CreatedAt = createdAt },
            new Province { Id = 6, Name = "ایلام", Code = "06", CreatedAt = createdAt },
            new Province { Id = 7, Name = "بوشهر", Code = "07", CreatedAt = createdAt },
            new Province { Id = 8, Name = "تهران", Code = "23", CreatedAt = createdAt },
            new Province { Id = 9, Name = "چهارمحال و بختیاری", Code = "14", CreatedAt = createdAt },
            new Province { Id = 10, Name = "خراسان جنوبی", Code = "29", CreatedAt = createdAt },
            new Province { Id = 11, Name = "خراسان رضوی", Code = "09", CreatedAt = createdAt },
            new Province { Id = 12, Name = "خراسان شمالی", Code = "28", CreatedAt = createdAt },
            new Province { Id = 13, Name = "خوزستان", Code = "06", CreatedAt = createdAt },
            new Province { Id = 14, Name = "زنجان", Code = "11", CreatedAt = createdAt },
            new Province { Id = 15, Name = "سمنان", Code = "20", CreatedAt = createdAt },
            new Province { Id = 16, Name = "سیستان و بلوچستان", Code = "16", CreatedAt = createdAt },
            new Province { Id = 17, Name = "فارس", Code = "07", CreatedAt = createdAt },
            new Province { Id = 18, Name = "قزوین", Code = "26", CreatedAt = createdAt },
            new Province { Id = 19, Name = "قم", Code = "25", CreatedAt = createdAt },
            new Province { Id = 20, Name = "کردستان", Code = "12", CreatedAt = createdAt },
            new Province { Id = 21, Name = "کرمان", Code = "08", CreatedAt = createdAt },
            new Province { Id = 22, Name = "کرمانشاه", Code = "13", CreatedAt = createdAt },
            new Province { Id = 23, Name = "کهگیلویه و بویراحمد", Code = "17", CreatedAt = createdAt },
            new Province { Id = 24, Name = "گلستان", Code = "27", CreatedAt = createdAt },
            new Province { Id = 25, Name = "گیلان", Code = "01", CreatedAt = createdAt },
            new Province { Id = 26, Name = "لرستان", Code = "15", CreatedAt = createdAt },
            new Province { Id = 27, Name = "مازندران", Code = "02", CreatedAt = createdAt },
            new Province { Id = 28, Name = "مرکزی", Code = "00", CreatedAt = createdAt },
            new Province { Id = 29, Name = "هرمزگان", Code = "22", CreatedAt = createdAt },
            new Province { Id = 30, Name = "همدان", Code = "13", CreatedAt = createdAt },
            new Province { Id = 31, Name = "یزد", Code = "21", CreatedAt = createdAt }
        );
    }

    public static void SeedCities(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;
        var cityId = 1;

        // تهران
        modelBuilder.Entity<City>().HasData(
            new City { Id = cityId++, Name = "تهران", ProvinceId = 8, Code = "021", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "اسلامشهر", ProvinceId = 8, Code = "021", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "شهریار", ProvinceId = 8, Code = "021", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "کرج", ProvinceId = 5, Code = "026", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "مشهد", ProvinceId = 11, Code = "051", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "اصفهان", ProvinceId = 4, Code = "031", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "شیراز", ProvinceId = 17, Code = "071", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "تبریز", ProvinceId = 1, Code = "041", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "قم", ProvinceId = 19, Code = "025", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "اهواز", ProvinceId = 13, Code = "061", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "کرمانشاه", ProvinceId = 22, Code = "083", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "رشت", ProvinceId = 25, Code = "013", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "ارومیه", ProvinceId = 2, Code = "044", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "زاهدان", ProvinceId = 16, Code = "054", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "کرمان", ProvinceId = 21, Code = "034", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "یزد", ProvinceId = 31, Code = "035", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "همدان", ProvinceId = 30, Code = "081", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "اردبیل", ProvinceId = 3, Code = "045", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "بندرعباس", ProvinceId = 29, Code = "076", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "اراک", ProvinceId = 28, Code = "086", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "زنجان", ProvinceId = 14, Code = "024", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "ساری", ProvinceId = 27, Code = "011", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "گرگان", ProvinceId = 24, Code = "017", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "قزوین", ProvinceId = 18, Code = "028", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "خرم‌آباد", ProvinceId = 26, Code = "066", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "سنندج", ProvinceId = 20, Code = "087", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "بوشهر", ProvinceId = 7, Code = "077", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "بجنورد", ProvinceId = 12, Code = "058", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "بیرجند", ProvinceId = 10, Code = "056", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "ایلام", ProvinceId = 6, Code = "084", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "شهرکرد", ProvinceId = 9, Code = "038", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "یاسوج", ProvinceId = 23, Code = "074", CreatedAt = createdAt },
            new City { Id = cityId++, Name = "سمنان", ProvinceId = 15, Code = "023", CreatedAt = createdAt }
        );
    }

    public static void SeedMedicalConditions(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        modelBuilder.Entity<MedicalCondition>().HasData(
            // علائم گوارشی
            new MedicalCondition { Id = 1, Name = "درد معده", Description = "درد در ناحیه معده", CreatedAt = createdAt },
            new MedicalCondition { Id = 2, Name = "شکم درد", Description = "درد در ناحیه شکم", CreatedAt = createdAt },
            new MedicalCondition { Id = 3, Name = "سوء هاضمه", Description = "مشکل در هضم غذا", CreatedAt = createdAt },
            new MedicalCondition { Id = 4, Name = "تهوع", Description = "حالت تهوع", CreatedAt = createdAt },
            new MedicalCondition { Id = 5, Name = "استفراغ", Description = "استفراغ", CreatedAt = createdAt },
            new MedicalCondition { Id = 6, Name = "اسهال", Description = "اسهال", CreatedAt = createdAt },
            new MedicalCondition { Id = 7, Name = "یبوست", Description = "یبوست", CreatedAt = createdAt },
            new MedicalCondition { Id = 8, Name = "سوزش سر دل", Description = "سوزش در ناحیه سینه", CreatedAt = createdAt },

            // علائم پوستی
            new MedicalCondition { Id = 9, Name = "کبودی پوست", Description = "کبودی و تغییر رنگ پوست", CreatedAt = createdAt },
            new MedicalCondition { Id = 10, Name = "خارش پوست", Description = "خارش در ناحیه پوست", CreatedAt = createdAt },
            new MedicalCondition { Id = 11, Name = "جوش و آکنه", Description = "جوش و آکنه روی پوست", CreatedAt = createdAt },
            new MedicalCondition { Id = 12, Name = "قرمزی پوست", Description = "قرمزی و التهاب پوست", CreatedAt = createdAt },
            new MedicalCondition { Id = 13, Name = "خارش سر", Description = "خارش در ناحیه سر", CreatedAt = createdAt },
            new MedicalCondition { Id = 14, Name = "ریزش مو", Description = "ریزش مو", CreatedAt = createdAt },

            // علائم قلبی
            new MedicalCondition { Id = 15, Name = "درد قفسه سینه", Description = "درد در ناحیه قفسه سینه", CreatedAt = createdAt },
            new MedicalCondition { Id = 16, Name = "تنگی نفس", Description = "مشکل در تنفس", CreatedAt = createdAt },
            new MedicalCondition { Id = 17, Name = "تپش قلب", Description = "تپش قلب نامنظم", CreatedAt = createdAt },
            new MedicalCondition { Id = 18, Name = "فشار خون بالا", Description = "فشار خون بالا", CreatedAt = createdAt },
            new MedicalCondition { Id = 19, Name = "فشار خون پایین", Description = "فشار خون پایین", CreatedAt = createdAt },

            // علائم عمومی
            new MedicalCondition { Id = 20, Name = "سردرد", Description = "درد در ناحیه سر", CreatedAt = createdAt },
            new MedicalCondition { Id = 21, Name = "تب", Description = "افزایش دمای بدن", CreatedAt = createdAt },
            new MedicalCondition { Id = 22, Name = "خستگی", Description = "خستگی و بی‌حالی", CreatedAt = createdAt },
            new MedicalCondition { Id = 23, Name = "درد مفاصل", Description = "درد در ناحیه مفاصل", CreatedAt = createdAt },
            new MedicalCondition { Id = 24, Name = "کمردرد", Description = "درد در ناحیه کمر", CreatedAt = createdAt },
            new MedicalCondition { Id = 25, Name = "گردن درد", Description = "درد در ناحیه گردن", CreatedAt = createdAt },

            // علائم اطفال
            new MedicalCondition { Id = 26, Name = "گریه مداوم نوزاد", Description = "گریه مداوم در نوزادان", CreatedAt = createdAt },
            new MedicalCondition { Id = 27, Name = "بی‌اشتهایی", Description = "کاهش اشتها", CreatedAt = createdAt },
            new MedicalCondition { Id = 28, Name = "رشد ناکافی", Description = "مشکل در رشد کودک", CreatedAt = createdAt },

            // علائم زنان
            new MedicalCondition { Id = 29, Name = "درد قاعدگی", Description = "درد در دوران قاعدگی", CreatedAt = createdAt },
            new MedicalCondition { Id = 30, Name = "اختلالات قاعدگی", Description = "مشکلات در چرخه قاعدگی", CreatedAt = createdAt },
            new MedicalCondition { Id = 31, Name = "ترشحات واژن", Description = "ترشحات غیرعادی", CreatedAt = createdAt }
        );
    }

    public static void SeedSpecialtyMedicalConditions(ModelBuilder modelBuilder)
    {
        var createdAt = DateTime.UtcNow;

        // قلب و عروق (Id = 1)
        modelBuilder.Entity<SpecialtyMedicalCondition>().HasData(
            new SpecialtyMedicalCondition { Id = 1, SpecialtyId = 1, MedicalConditionId = 15, CreatedAt = createdAt }, // درد قفسه سینه
            new SpecialtyMedicalCondition { Id = 2, SpecialtyId = 1, MedicalConditionId = 16, CreatedAt = createdAt }, // تنگی نفس
            new SpecialtyMedicalCondition { Id = 3, SpecialtyId = 1, MedicalConditionId = 17, CreatedAt = createdAt }, // تپش قلب
            new SpecialtyMedicalCondition { Id = 4, SpecialtyId = 1, MedicalConditionId = 18, CreatedAt = createdAt }, // فشار خون بالا
            new SpecialtyMedicalCondition { Id = 5, SpecialtyId = 1, MedicalConditionId = 19, CreatedAt = createdAt }  // فشار خون پایین
        );

        // داخلی (Id = 2)
        modelBuilder.Entity<SpecialtyMedicalCondition>().HasData(
            new SpecialtyMedicalCondition { Id = 6, SpecialtyId = 2, MedicalConditionId = 1, CreatedAt = createdAt },  // درد معده
            new SpecialtyMedicalCondition { Id = 7, SpecialtyId = 2, MedicalConditionId = 2, CreatedAt = createdAt },  // شکم درد
            new SpecialtyMedicalCondition { Id = 8, SpecialtyId = 2, MedicalConditionId = 3, CreatedAt = createdAt },  // سوء هاضمه
            new SpecialtyMedicalCondition { Id = 9, SpecialtyId = 2, MedicalConditionId = 4, CreatedAt = createdAt },  // تهوع
            new SpecialtyMedicalCondition { Id = 10, SpecialtyId = 2, MedicalConditionId = 5, CreatedAt = createdAt }, // استفراغ
            new SpecialtyMedicalCondition { Id = 11, SpecialtyId = 2, MedicalConditionId = 6, CreatedAt = createdAt }, // اسهال
            new SpecialtyMedicalCondition { Id = 12, SpecialtyId = 2, MedicalConditionId = 7, CreatedAt = createdAt }, // یبوست
            new SpecialtyMedicalCondition { Id = 13, SpecialtyId = 2, MedicalConditionId = 8, CreatedAt = createdAt }, // سوزش سر دل
            new SpecialtyMedicalCondition { Id = 14, SpecialtyId = 2, MedicalConditionId = 20, CreatedAt = createdAt }, // سردرد
            new SpecialtyMedicalCondition { Id = 15, SpecialtyId = 2, MedicalConditionId = 21, CreatedAt = createdAt }, // تب
            new SpecialtyMedicalCondition { Id = 16, SpecialtyId = 2, MedicalConditionId = 22, CreatedAt = createdAt }, // خستگی
            new SpecialtyMedicalCondition { Id = 17, SpecialtyId = 2, MedicalConditionId = 10, CreatedAt = createdAt }, // خارش پوست
            new SpecialtyMedicalCondition { Id = 18, SpecialtyId = 2, MedicalConditionId = 12, CreatedAt = createdAt }  // قرمزی پوست
        );

        // اطفال (Id = 3)
        modelBuilder.Entity<SpecialtyMedicalCondition>().HasData(
            new SpecialtyMedicalCondition { Id = 19, SpecialtyId = 3, MedicalConditionId = 26, CreatedAt = createdAt }, // گریه مداوم نوزاد
            new SpecialtyMedicalCondition { Id = 20, SpecialtyId = 3, MedicalConditionId = 27, CreatedAt = createdAt }, // بی‌اشتهایی
            new SpecialtyMedicalCondition { Id = 21, SpecialtyId = 3, MedicalConditionId = 28, CreatedAt = createdAt }, // رشد ناکافی
            new SpecialtyMedicalCondition { Id = 22, SpecialtyId = 3, MedicalConditionId = 21, CreatedAt = createdAt }, // تب
            new SpecialtyMedicalCondition { Id = 23, SpecialtyId = 3, MedicalConditionId = 4, CreatedAt = createdAt },  // تهوع
            new SpecialtyMedicalCondition { Id = 24, SpecialtyId = 3, MedicalConditionId = 5, CreatedAt = createdAt },  // استفراغ
            new SpecialtyMedicalCondition { Id = 25, SpecialtyId = 3, MedicalConditionId = 6, CreatedAt = createdAt },  // اسهال
            new SpecialtyMedicalCondition { Id = 26, SpecialtyId = 3, MedicalConditionId = 2, CreatedAt = createdAt }   // شکم درد
        );

        // جراحی عمومی (Id = 4)
        modelBuilder.Entity<SpecialtyMedicalCondition>().HasData(
            new SpecialtyMedicalCondition { Id = 27, SpecialtyId = 4, MedicalConditionId = 2, CreatedAt = createdAt },  // شکم درد
            new SpecialtyMedicalCondition { Id = 28, SpecialtyId = 4, MedicalConditionId = 24, CreatedAt = createdAt }, // کمردرد
            new SpecialtyMedicalCondition { Id = 29, SpecialtyId = 4, MedicalConditionId = 25, CreatedAt = createdAt },  // گردن درد
            new SpecialtyMedicalCondition { Id = 30, SpecialtyId = 4, MedicalConditionId = 23, CreatedAt = createdAt }, // درد مفاصل
            new SpecialtyMedicalCondition { Id = 31, SpecialtyId = 4, MedicalConditionId = 9, CreatedAt = createdAt }   // کبودی پوست
        );

        // زنان و زایمان (Id = 5)
        modelBuilder.Entity<SpecialtyMedicalCondition>().HasData(
            new SpecialtyMedicalCondition { Id = 32, SpecialtyId = 5, MedicalConditionId = 29, CreatedAt = createdAt }, // درد قاعدگی
            new SpecialtyMedicalCondition { Id = 33, SpecialtyId = 5, MedicalConditionId = 30, CreatedAt = createdAt }, // اختلالات قاعدگی
            new SpecialtyMedicalCondition { Id = 34, SpecialtyId = 5, MedicalConditionId = 31, CreatedAt = createdAt }, // ترشحات واژن
            new SpecialtyMedicalCondition { Id = 35, SpecialtyId = 5, MedicalConditionId = 2, CreatedAt = createdAt },    // شکم درد
            new SpecialtyMedicalCondition { Id = 36, SpecialtyId = 5, MedicalConditionId = 20, CreatedAt = createdAt }  // سردرد
        );
    }

    /// <summary>
    /// هش کردن رمز عبور با استفاده از SHA256
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
