using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Geography;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Models.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GeneratorsManagementSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<ApplicationDbContext>();
            // إنشاء الأدوار الأساسية
            //var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] defaultRoles = { "Admin", "Manager", "Employee", "Subscriber" };


            // Ensure DB created
            await context.Database.EnsureCreatedAsync();

            // Create Roles
            string[] roles = { "Admin", "Manager", "Operator", "Viewer", "Subscriber" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }


            //المحافظات والاقضية والأحياء والأزقة
            var data = new List<(string Gov, string District, string Hood, string Alley)>
            {
                // بغداد
                ("بغداد", "العامرية", "حي العامرية", "زقاق 12"),
                ("بغداد", "المنصور", "حي اليرموك", "زقاق 5"),
                ("بغداد", "المنصور", "حي الداودي", "زقاق 14"),
                ("بغداد", "الكرادة", "الكرادة الداخل", "زقاق 21"),
                ("بغداد", "الكرادة", "العرصات", "زقاق 8"),
                ("بغداد", "الرشيد", "حي البياع", "زقاق 30"),
                ("بغداد", "الرشيد", "حي العامل", "زقاق 19"),
                ("بغداد", "الاعظمية", "حي السفينة", "زقاق 4"),
                ("بغداد", "الاعظمية", "راغبة خاتون", "زقاق 11"),
                ("بغداد", "الكاظمية", "الاسكان", "زقاق 2"),
                ("بغداد", "الكاظمية", "حي الطوبجي", "زقاق 15"),
                ("بغداد", "الصدر الأولى", "سداسية الصدر", "قطاع 10 زقاق 3"),
                ("بغداد", "الصدر الثانية", "القطاع الثامن", "قطاع 8 زقاق 12"),

                // البصرة
                ("البصرة", "البصرة", "الجبيلة", "زقاق 3"),
                ("البصرة", "البصرة", "شارع الاستقلال المعقل", ""),
                ("البصرة", "البصرة", "الطويسة", "زقاق 5"),
                ("البصرة", "الزبير", "البرجسية", "زقاق 1"),
                ("البصرة", "الزبير", "حي الزهراء", "زقاق 7"),
                ("البصرة", "أبو الخصيب", "محلة نهر الخوز السراجي", ""),
                ("البصرة", "شط العرب", "البلدية", "زقاق 4"),

                // نينوى
                ("نينوى", "الموصل", "حي الغابات", "زقاق 1"),
                ("نينوى", "الموصل", "حي الزهور", "زقاق 10"),
                ("نينوى", "الموصل", "حي النور", "زقاق 15"),
                ("نينوى", "الموصل", "العربي", "زقاق 6"),
                ("نينوى", "تلعفر", "العبرة", "زقاق 2"),
                ("نينوى", "الحمدانية", "قرقوش الشمالي", ""),

                // أربيل
                ("أربيل", "أربيل", "شارع الكنيسة عنكاوا", "زقاق الكنيسة"),
                ("أربيل", "أربيل", "حي البكر", "زقاق 12"),
                ("أربيل", "أربيل", "محلة التسجيل قلعة أربيل", ""),
                ("أربيل", "سوران", "دينا", "زقاق السوق"),

                // السليمانية
                ("السليمانية", "السليمانية", "سرجنار", "زقاق 5"),
                ("السليمانية", "السليمانية", "بختياري", "زقاق 12"),
                ("السليمانية", "السليمانية", "سلطاني", "زقاق 3"),

                // دهوك
                ("دهوك", "دهوك", "نزاركي", "زقاق 7"),
                ("دهوك", "دهوك", "مشتك", "زقاق 2"),
                ("دهوك", "زاخو", "إبراهيم خليل", "زقاق الحدود"),

                // كركوك
                ("كركوك", "كركوك", "حي الواسطي", "زقاق 14"),
                ("كركوك", "كركوك", "حي النصر", "زقاق 8"),
                ("كركوك", "كركوك", "شورجة", "زقاق 3"),
                ("كركوك", "دقوق", "حي القادسية", "زقاق 1"),

                // ديالى
                ("ديالى", "بعقوبة", "حي التحرير", "زقاق 5"),
                ("ديالى", "بعقوبة", "المعلمين", "زقاق 12"),
                ("ديالى", "خالص", "العامري", "زقاق 2"),

                // الأنبار
                ("الأنبار", "الرمادي", "حوز", "زقاق 4"),
                ("الأنبار", "الرمادي", "الملعب", "زقاق 9"),
                ("الأنبار", "الفلوجة", "الشرطة", "زقاق 3"),
                ("الأنبار", "الفلوجة", "الجولان", "زقاق 6"),

                // بابل
                ("بابل", "الحلة", "حي الإسكان", "زقاق 11"),
                ("بابل", "الحلة", "الأكرمين", "زقاق 4"),
                ("بابل", "المسيب", "الزهور", "زقاق 2"),

                // كربلاء
                ("كربلاء", "كربلاء", "الإصلاح الغربي", "زقاق 7"),
                ("كربلاء", "كربلاء", "حي العباسية", "زقاق 1"),
                ("كربلاء", "كربلاء", "حي المعلمين", "زقاق 10"),

                // النجف
                ("النجف", "النجف", "محلة الحويش المدينة القديمة", ""),
                ("النجف", "النجف", "حي الأمير", "زقاق 15"),
                ("النجف", "الكوفة", "الصدر", "زقاق 4"),

                // واسط
                ("واسط", "الكوت", "حي الجهاد", "زقاق 8"),
                ("واسط", "الكوت", "العزة", "زقاق 2"),
                ("واسط", "الحي", "العسكري", "زقاق 5"),

                // ميسان
                ("ميسان", "العمارة", "حي الحسين", "زقاق 6"),
                ("ميسان", "العمارة", "المعلمين", "زقاق 11"),
                ("ميسان", "المجر الكبير", "الشارع العام", "زقاق 1"),

                // ذي قار
                ("ذي قار", "الناصرية", "حي الشورى", "زقاق 5"),
                ("ذي قار", "الناصرية", "الإسكان الصناعي", "زقاق 12"),
                ("ذي قار", "الشطرة", "العباسية", "زقاق 3"),

                // المثنى
                ("المثنى", "السماوة", "الحيدرية", "زقاق 4"),
                ("المثنى", "السماوة", "العسكري", "زقاق 7"),
                ("المثنى", "الرميثة", "المركز", "زقاق 1"),

                // القادسية
                ("القادسية", "الديوانية", "حي العونية", "زقاق 9"),
                ("القادسية", "الديوانية", "حي الجزائر", "زقاق 3"),
                ("القادسية", "عفك", "الزهور", "زقاق 2"),

                // صلاح الدين
                ("صلاح الدين", "تكريت", "الشرطة", "زقاق 5"),
                ("صلاح الدين", "تكريت", "القادسية", "زقاق 10"),
                ("صلاح الدين", "سامراء", "الاعتصام", "زقاق 5"),
                ("صلاح الدين", "بيجي", "المصفى", "زقاق 1"),
            };

            // ═══ إنشاء المحافظات ═══
            var governorates = data.Select(x => x.Gov).Distinct().ToList();
            var govMap = new Dictionary<string, Governorate>();

            int order = 1;
            foreach (var govName in governorates)
            {
                var gov = new Governorate
                {
                    Name = govName,
                    DisplayOrder = order++,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Governorates.Add(gov);
                govMap[govName] = gov;
            }

            // ═══ إنشاء الأقضية ═══
            var districts = data.Select(x => new { x.Gov, x.District }).Distinct().ToList();
            var distMap = new Dictionary<string, District>();

            foreach (var d in districts)
            {
                var district = new District
                {
                    GovernorateId = govMap[d.Gov].Id,
                    Name = d.District,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Districts.Add(district);
                distMap[$"{d.Gov}|{d.District}"] = district;
            }

            // ═══ إنشاء الأحياء ═══
            var hoods = data.Select(x => new { x.Gov, x.District, x.Hood }).Distinct().ToList();
            var hoodMap = new Dictionary<string, Neighborhood>();

            foreach (var h in hoods)
            {
                if (string.IsNullOrWhiteSpace(h.Hood)) continue;

                var hood = new Neighborhood
                {
                    DistrictId = distMap[$"{h.Gov}|{h.District}"].Id,
                    Name = h.Hood,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Neighborhoods.Add(hood);
                hoodMap[$"{h.Gov}|{h.District}|{h.Hood}"] = hood;
            }

            // ═══ إنشاء الأزقة ═══
            foreach (var a in data)
            {
                if (string.IsNullOrWhiteSpace(a.Alley) || string.IsNullOrWhiteSpace(a.Hood)) continue;

                var key = $"{a.Gov}|{a.District}|{a.Hood}";
                if (!hoodMap.ContainsKey(key)) continue;

                var alley = new Alley
                {
                    NeighborhoodId = hoodMap[key].Id,
                    Name = a.Alley,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Alleys.Add(alley);
            }

          


                

                    // Create Admin User
                    var adminEmail = "admin@gms.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "مدير النظام",
                    JobTitle = "مدير عام",
                    Department = "الإدارة",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");

                    // Default Theme Settings
                    context.ThemeSettings.Add(new ThemeSettings
                    {
                        UserId = admin.Id,
                        LayoutMode = "light",
                        NavbarType = "fixed",
                        SidebarColor = "gradient-purple",
                        SidebarWidth = "medium",
                        CompactMenu = false,
                        PrimaryColor = "#7367F0"
                    });
                }
            }

            // System Settings
            if (!context.SystemSettings.Any())
            {
                context.SystemSettings.AddRange(
                    new SystemSettings { SettingKey = "SystemName", SettingValue = "نظام إدارة المولدات الكهربائية" },
                    new SystemSettings { SettingKey = "Currency", SettingValue = "د.ع" },
                    new SystemSettings { SettingKey = "DefaultLanguage", SettingValue = "ar" },
                    new SystemSettings { SettingKey = "TimeZone", SettingValue = "Asia/Baghdad" }
                );
            }
             

            foreach (var roleName in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // إعطاء المستخدم Admin دور Admin
            var adminUser = await userManager.FindByEmailAsync("admin@gms.com");
            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");

                // إعطاء صلاحيات كاملة لدور Admin
                var adminRole = await roleManager.FindByNameAsync("Admin");
                if (adminRole != null)
                {
                    var allPermissions = GeneratorsManagementSystem.Helpers.PermissionsList.GetAllPermissionKeys();
                    var existingClaims = await roleManager.GetClaimsAsync(adminRole);

                    foreach (var permission in allPermissions)
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            await roleManager.AddClaimAsync(adminRole, new System.Security.Claims.Claim("Permission", permission));
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}