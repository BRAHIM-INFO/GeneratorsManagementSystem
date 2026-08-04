using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Models.Settings;
using GeneratorsManagementSystem.Models.ViewModels.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GeneratorsManagementSystem.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region General Settings

        public async Task<GeneralSettingsViewModel> GetGeneralSettingsAsync()
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Category == "General")
                .ToListAsync();

            var model = new GeneralSettingsViewModel();

            foreach (var setting in settings)
            {
                switch (setting.SettingKey)
                {
                    case "SystemName": model.SystemName = setting.SettingValue ?? model.SystemName; break;
                    case "SystemShortName": model.SystemShortName = setting.SettingValue ?? model.SystemShortName; break;
                    case "Version": model.Version = setting.SettingValue ?? model.Version; break;
                    case "SupportEmail": model.SupportEmail = setting.SettingValue; break;
                    case "SupportPhone": model.SupportPhone = setting.SettingValue; break;
                    case "Currency": model.Currency = setting.SettingValue ?? model.Currency; break;
                    case "CurrencySymbol": model.CurrencySymbol = setting.SettingValue ?? model.CurrencySymbol; break;
                    case "CurrencyPosition": model.CurrencyPosition = setting.SettingValue ?? model.CurrencyPosition; break;
                    case "DecimalPlaces": model.DecimalPlaces = int.TryParse(setting.SettingValue, out var dp) ? dp : model.DecimalPlaces; break;
                    case "ThousandSeparator": model.ThousandSeparator = setting.SettingValue ?? model.ThousandSeparator; break;
                    case "DecimalSeparator": model.DecimalSeparator = setting.SettingValue ?? model.DecimalSeparator; break;
                    case "DefaultLanguage": model.DefaultLanguage = setting.SettingValue ?? model.DefaultLanguage; break;
                    case "TextDirection": model.TextDirection = setting.SettingValue ?? model.TextDirection; break;
                    case "TimeZone": model.TimeZone = setting.SettingValue ?? model.TimeZone; break;
                    case "DateFormat": model.DateFormat = setting.SettingValue ?? model.DateFormat; break;
                    case "TimeFormat": model.TimeFormat = setting.SettingValue ?? model.TimeFormat; break;
                    case "Calendar": model.Calendar = setting.SettingValue ?? model.Calendar; break;
                    case "SessionTimeout": model.SessionTimeout = int.TryParse(setting.SettingValue, out var st) ? st : model.SessionTimeout; break;
                    case "AutoLogout": model.AutoLogout = bool.TryParse(setting.SettingValue, out var al) ? al : model.AutoLogout; break;
                    case "AllowSelfRegistration": model.AllowSelfRegistration = bool.TryParse(setting.SettingValue, out var asr) ? asr : model.AllowSelfRegistration; break;
                    case "AutoActivateAccounts": model.AutoActivateAccounts = bool.TryParse(setting.SettingValue, out var aaa) ? aaa : model.AutoActivateAccounts; break;
                    case "MinPasswordLength": model.MinPasswordLength = int.TryParse(setting.SettingValue, out var mpl) ? mpl : model.MinPasswordLength; break;
                    case "PageSize": model.PageSize = int.TryParse(setting.SettingValue, out var ps) ? ps : model.PageSize; break;
                }
            }

            return model;
        }

        public async Task<bool> SaveGeneralSettingsAsync(GeneralSettingsViewModel model, string userId)
        {
            try
            {
                var settingsMap = new Dictionary<string, (string Value, string DataType)>
                {
                    { "SystemName", (model.SystemName, "string") },
                    { "SystemShortName", (model.SystemShortName, "string") },
                    { "Version", (model.Version, "string") },
                    { "SupportEmail", (model.SupportEmail ?? "", "string") },
                    { "SupportPhone", (model.SupportPhone ?? "", "string") },
                    { "Currency", (model.Currency, "string") },
                    { "CurrencySymbol", (model.CurrencySymbol, "string") },
                    { "CurrencyPosition", (model.CurrencyPosition, "string") },
                    { "DecimalPlaces", (model.DecimalPlaces.ToString(), "int") },
                    { "ThousandSeparator", (model.ThousandSeparator, "string") },
                    { "DecimalSeparator", (model.DecimalSeparator, "string") },
                    { "DefaultLanguage", (model.DefaultLanguage, "string") },
                    { "TextDirection", (model.TextDirection, "string") },
                    { "TimeZone", (model.TimeZone, "string") },
                    { "DateFormat", (model.DateFormat, "string") },
                    { "TimeFormat", (model.TimeFormat, "string") },
                    { "Calendar", (model.Calendar, "string") },
                    { "SessionTimeout", (model.SessionTimeout.ToString(), "int") },
                    { "AutoLogout", (model.AutoLogout.ToString(), "bool") },
                    { "AllowSelfRegistration", (model.AllowSelfRegistration.ToString(), "bool") },
                    { "AutoActivateAccounts", (model.AutoActivateAccounts.ToString(), "bool") },
                    { "MinPasswordLength", (model.MinPasswordLength.ToString(), "int") },
                    { "PageSize", (model.PageSize.ToString(), "int") }
                };

                foreach (var kvp in settingsMap)
                {
                    await SetSettingAsync(kvp.Key, kvp.Value.Value, userId, "General", kvp.Value.DataType);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Organization Settings

        public async Task<OrganizationSettingsViewModel> GetOrganizationSettingsAsync()
        {
            var org = await _context.OrganizationSettings.FirstOrDefaultAsync();

            if (org == null)
            {
                return new OrganizationSettingsViewModel
                {
                    OrganizationName = "شركة إدارة المولدات",
                    Country = "ليبيا"
                };
            }

            return new OrganizationSettingsViewModel
            {
                Id = org.Id,
                OrganizationName = org.OrganizationName,
                OrganizationNameEn = org.OrganizationNameEn,
                Slogan = org.Slogan,
                LogoPath = org.LogoPath,
                FaviconPath = org.FaviconPath,
                Email = org.Email,
                Phone1 = org.Phone1,
                Phone2 = org.Phone2,
                Fax = org.Fax,
                Website = org.Website,
                Country = org.Country,
                City = org.City,
                District = org.District,
                Address = org.Address,
                PostalCode = org.PostalCode,
                TaxNumber = org.TaxNumber,
                CommercialRegister = org.CommercialRegister,
                LicenseNumber = org.LicenseNumber,
                Facebook = org.Facebook,
                Twitter = org.Twitter,
                Instagram = org.Instagram,
                LinkedIn = org.LinkedIn,
                WhatsApp = org.WhatsApp,
                AboutUs = org.AboutUs,
                Notes = org.Notes
            };
        }

        public async Task<bool> SaveOrganizationSettingsAsync(OrganizationSettingsViewModel model, string userId)
        {
            try
            {
                var org = await _context.OrganizationSettings.FirstOrDefaultAsync();
                bool isNew = false;

                if (org == null)
                {
                    org = new OrganizationSettings();
                    isNew = true;
                }

                org.OrganizationName = model.OrganizationName;
                org.OrganizationNameEn = model.OrganizationNameEn;
                org.Slogan = model.Slogan;

                if (!string.IsNullOrEmpty(model.LogoPath))
                    org.LogoPath = model.LogoPath;

                if (!string.IsNullOrEmpty(model.FaviconPath))
                    org.FaviconPath = model.FaviconPath;

                org.Email = model.Email;
                org.Phone1 = model.Phone1;
                org.Phone2 = model.Phone2;
                org.Fax = model.Fax;
                org.Website = model.Website;
                org.Country = model.Country;
                org.City = model.City;
                org.District = model.District;
                org.Address = model.Address;
                org.PostalCode = model.PostalCode;
                org.TaxNumber = model.TaxNumber;
                org.CommercialRegister = model.CommercialRegister;
                org.LicenseNumber = model.LicenseNumber;
                org.Facebook = model.Facebook;
                org.Twitter = model.Twitter;
                org.Instagram = model.Instagram;
                org.LinkedIn = model.LinkedIn;
                org.WhatsApp = model.WhatsApp;
                org.AboutUs = model.AboutUs;
                org.Notes = model.Notes;
                org.UpdatedAt = DateTime.Now;
                org.UpdatedBy = userId;

                if (isNew)
                {
                    org.CreatedAt = DateTime.Now;
                    _context.OrganizationSettings.Add(org);
                }
                else
                {
                    _context.OrganizationSettings.Update(org);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Generic Settings

        public async Task<string?> GetSettingAsync(string key)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            return setting?.SettingValue;
        }

        public async Task<T?> GetSettingAsync<T>(string key)
        {
            var value = await GetSettingAsync(key);
            if (string.IsNullOrEmpty(value)) return default;

            try
            {
                if (typeof(T) == typeof(string)) return (T)(object)value;
                if (typeof(T) == typeof(int)) return (T)(object)int.Parse(value);
                if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(value);
                if (typeof(T) == typeof(decimal)) return (T)(object)decimal.Parse(value);

                return JsonSerializer.Deserialize<T>(value);
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> SetSettingAsync(string key, string value, string userId, string? category = null, string dataType = "string")
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);

            if (setting == null)
            {
                setting = new SystemSettings
                {
                    SettingKey = key,
                    SettingValue = value,
                    Category = category,
                    DataType = dataType,
                    CreatedAt = DateTime.Now,
                    UpdatedBy = userId
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = value;
                setting.UpdatedAt = DateTime.Now;
                setting.UpdatedBy = userId;
                if (!string.IsNullOrEmpty(category)) setting.Category = category;
                _context.SystemSettings.Update(setting);
            }

            return true;
        }

        public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category)
        {
            return await _context.SystemSettings
                .Where(s => s.Category == category)
                .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue ?? "");
        }

        #endregion

        #region Dashboard

        public async Task<SettingsDashboardViewModel> GetDashboardAsync()
        {
            var org = await _context.OrganizationSettings.FirstOrDefaultAsync();
            var users = await _userManager.Users.ToListAsync();
            var totalSettings = await _context.SystemSettings.CountAsync();

            var model = new SettingsDashboardViewModel
            {
                OrganizationName = org?.OrganizationName ?? "غير محدد",
                LogoPath = org?.LogoPath,
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                TotalRoles = await _context.Roles.CountAsync(),
                SystemVersion = await GetSettingAsync("Version") ?? "1.0.0",
                LastBackup = DateTime.Now.AddDays(-1),
                TotalSettings = totalSettings,
                Cards = GetSettingsCards()
            };

            return model;
        }

        private List<SettingCard> GetSettingsCards()
        {
            return new List<SettingCard>
            {
                new() { Title = "الإعدادات العامة", Description = "معلومات النظام، العملة، اللغة والمنطقة الزمنية", Icon = "fa-cog", Color = "primary", Url = "/Settings/General" },
                new() { Title = "بيانات المؤسسة", Description = "معلومات الشركة، الشعار والعنوان", Icon = "fa-building", Color = "success", Url = "/Settings/Organization" },
                new() { Title = "إدارة المستخدمين", Description = "إضافة، تعديل وحذف مستخدمي النظام", Icon = "fa-users", Color = "info", Url = "/Settings/Users" },
                new() { Title = "الأدوار والصلاحيات", Description = "إدارة أدوار المستخدمين والصلاحيات", Icon = "fa-user-shield", Color = "warning", Url = "/Settings/Roles" },
                //new() { Title = "إعدادات المولدات", Description = "القيم الافتراضية وتنبيهات المولدات", Icon = "fa-bolt", Color = "purple", Url = "/Settings/Generators" },
                //new() { Title = "إعدادات الاشتراكات", Description = "الرسوم الافتراضية وفترات الفوترة", Icon = "fa-file-invoice", Color = "danger", Url = "/Settings/Subscriptions" },
                //new() { Title = "إعدادات الفوترة", Description = "الضرائب، الخصومات وطرق الدفع", Icon = "fa-money-bill", Color = "success", Url = "/Settings/Billing" },
                new() { Title = "إعدادات التنبيهات", Description = "تنبيهات البريد والرسائل النصية", Icon = "fa-bell", Color = "warning", Url = "/Settings/Notifications" },
                new() { Title = "النسخ الاحتياطي", Description = "نسخ واستعادة قاعدة البيانات", Icon = "fa-database", Color = "dark", Url = "/Settings/Backup" },
                new() { Title = "سجل النشاطات", Description = "تتبع كافة العمليات في النظام", Icon = "fa-history", Color = "secondary", Url = "/AuditLog/Index" },
                new() { Title = "SMS و Email", Description = "إعدادات خوادم البريد والرسائل", Icon = "fa-envelope", Color = "info", Url = "/Settings/Communications" }
            };
        }

        #endregion

        #region Generator Settings

        public async Task<GeneratorSettingsViewModel> GetGeneratorSettingsAsync()
        {
            var settings = await _context.GeneratorSettings.FirstOrDefaultAsync();
            if (settings == null)
                return new GeneratorSettingsViewModel();

            return new GeneratorSettingsViewModel
            {
                Id = settings.Id,
                GeneratorNumberPrefix = settings.GeneratorNumberPrefix,
                GeneratorNumberLength = settings.GeneratorNumberLength,
                GeneratorNumberStart = settings.GeneratorNumberStart,
                DefaultFuelType = settings.DefaultFuelType,
                DefaultFuelTankCapacity = settings.DefaultFuelTankCapacity,
                DefaultFuelConsumptionRate = settings.DefaultFuelConsumptionRate,
                FuelPricePerLiter = settings.FuelPricePerLiter,
                LowFuelAlertPercentage = settings.LowFuelAlertPercentage,
                CriticalFuelAlertPercentage = settings.CriticalFuelAlertPercentage,
                EnableFuelAlerts = settings.EnableFuelAlerts,
                EnableEmailFuelAlerts = settings.EnableEmailFuelAlerts,
                EnableSmsFuelAlerts = settings.EnableSmsFuelAlerts,
                DefaultMaintenanceIntervalHours = settings.DefaultMaintenanceIntervalHours,
                MaintenanceAlertBeforeHours = settings.MaintenanceAlertBeforeHours,
                EnableMaintenanceAlerts = settings.EnableMaintenanceAlerts,
                DefaultVoltage = settings.DefaultVoltage,
                DefaultFrequency = settings.DefaultFrequency,
                MaxTemperature = settings.MaxTemperature,
                MinOilPressure = settings.MinOilPressure,
                MaxLoadPercentage = settings.MaxLoadPercentage,
                EnableRealTimeMonitoring = settings.EnableRealTimeMonitoring,
                MonitoringIntervalSeconds = settings.MonitoringIntervalSeconds,
                AutoLogGeneratorEvents = settings.AutoLogGeneratorEvents
            };
        }

        public async Task<bool> SaveGeneratorSettingsAsync(GeneratorSettingsViewModel model, string userId)
        {
            try
            {
                var settings = await _context.GeneratorSettings.FirstOrDefaultAsync();
                bool isNew = false;

                if (settings == null)
                {
                    settings = new Models.Settings.GeneratorSettings();
                    isNew = true;
                }

                settings.GeneratorNumberPrefix = model.GeneratorNumberPrefix;
                settings.GeneratorNumberLength = model.GeneratorNumberLength;
                settings.GeneratorNumberStart = model.GeneratorNumberStart;
                settings.DefaultFuelType = model.DefaultFuelType;
                settings.DefaultFuelTankCapacity = model.DefaultFuelTankCapacity;
                settings.DefaultFuelConsumptionRate = model.DefaultFuelConsumptionRate;
                settings.FuelPricePerLiter = model.FuelPricePerLiter;
                settings.LowFuelAlertPercentage = model.LowFuelAlertPercentage;
                settings.CriticalFuelAlertPercentage = model.CriticalFuelAlertPercentage;
                settings.EnableFuelAlerts = model.EnableFuelAlerts;
                settings.EnableEmailFuelAlerts = model.EnableEmailFuelAlerts;
                settings.EnableSmsFuelAlerts = model.EnableSmsFuelAlerts;
                settings.DefaultMaintenanceIntervalHours = model.DefaultMaintenanceIntervalHours;
                settings.MaintenanceAlertBeforeHours = model.MaintenanceAlertBeforeHours;
                settings.EnableMaintenanceAlerts = model.EnableMaintenanceAlerts;
                settings.DefaultVoltage = model.DefaultVoltage;
                settings.DefaultFrequency = model.DefaultFrequency;
                settings.MaxTemperature = model.MaxTemperature;
                settings.MinOilPressure = model.MinOilPressure;
                settings.MaxLoadPercentage = model.MaxLoadPercentage;
                settings.EnableRealTimeMonitoring = model.EnableRealTimeMonitoring;
                settings.MonitoringIntervalSeconds = model.MonitoringIntervalSeconds;
                settings.AutoLogGeneratorEvents = model.AutoLogGeneratorEvents;
                settings.UpdatedAt = DateTime.Now;
                settings.UpdatedBy = userId;

                if (isNew)
                {
                    settings.CreatedAt = DateTime.Now;
                    _context.GeneratorSettings.Add(settings);
                }
                else
                {
                    _context.GeneratorSettings.Update(settings);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region Subscription Settings

        public async Task<SubscriptionSettingsViewModel> GetSubscriptionSettingsAsync()
        {
            var settings = await _context.SubscriptionSettings.FirstOrDefaultAsync();
            if (settings == null)
                return new SubscriptionSettingsViewModel();

            return new SubscriptionSettingsViewModel
            {
                Id = settings.Id,
                SubscriberNumberPrefix = settings.SubscriberNumberPrefix,
                SubscriberNumberLength = settings.SubscriberNumberLength,
                IncludeYearInNumber = settings.IncludeYearInNumber,
                DefaultInstallationFee = settings.DefaultInstallationFee,
                DefaultMonthlyFee = settings.DefaultMonthlyFee,
                DefaultPricePerAmpere = settings.DefaultPricePerAmpere,
                DefaultPricePerKwh = settings.DefaultPricePerKwh,
                DefaultSubscriptionType = settings.DefaultSubscriptionType,
                BillingDayOfMonth = settings.BillingDayOfMonth,
                GracePeriodDays = settings.GracePeriodDays,
                AutoSuspendOverdue = settings.AutoSuspendOverdue,
                SuspendAfterDays = settings.SuspendAfterDays,
                ApplyLateFees = settings.ApplyLateFees,
                LateFeeAmount = settings.LateFeeAmount,
                LateFeePercentage = settings.LateFeePercentage,
                AllowEarlyPaymentDiscount = settings.AllowEarlyPaymentDiscount,
                EarlyPaymentDiscountPercentage = settings.EarlyPaymentDiscountPercentage,
                EarlyPaymentDaysBeforeDue = settings.EarlyPaymentDaysBeforeDue,
                DefaultContractDurationMonths = settings.DefaultContractDurationMonths,
                RequireDeposit = settings.RequireDeposit,
                DefaultDepositAmount = settings.DefaultDepositAmount,
                EnablePaymentReminders = settings.EnablePaymentReminders,
                ReminderBeforeDueDays = settings.ReminderBeforeDueDays,
                ReminderAfterDueDays = settings.ReminderAfterDueDays
            };
        }

        public async Task<bool> SaveSubscriptionSettingsAsync(SubscriptionSettingsViewModel model, string userId)
        {
            try
            {
                var settings = await _context.SubscriptionSettings.FirstOrDefaultAsync();
                bool isNew = false;

                if (settings == null)
                {
                    settings = new Models.Settings.SubscriptionSettings();
                    isNew = true;
                }

                settings.SubscriberNumberPrefix = model.SubscriberNumberPrefix;
                settings.SubscriberNumberLength = model.SubscriberNumberLength;
                settings.IncludeYearInNumber = model.IncludeYearInNumber;
                settings.DefaultInstallationFee = model.DefaultInstallationFee;
                settings.DefaultMonthlyFee = model.DefaultMonthlyFee;
                settings.DefaultPricePerAmpere = model.DefaultPricePerAmpere;
                settings.DefaultPricePerKwh = model.DefaultPricePerKwh;
                settings.DefaultSubscriptionType = model.DefaultSubscriptionType;
                settings.BillingDayOfMonth = model.BillingDayOfMonth;
                settings.GracePeriodDays = model.GracePeriodDays;
                settings.AutoSuspendOverdue = model.AutoSuspendOverdue;
                settings.SuspendAfterDays = model.SuspendAfterDays;
                settings.ApplyLateFees = model.ApplyLateFees;
                settings.LateFeeAmount = model.LateFeeAmount;
                settings.LateFeePercentage = model.LateFeePercentage;
                settings.AllowEarlyPaymentDiscount = model.AllowEarlyPaymentDiscount;
                settings.EarlyPaymentDiscountPercentage = model.EarlyPaymentDiscountPercentage;
                settings.EarlyPaymentDaysBeforeDue = model.EarlyPaymentDaysBeforeDue;
                settings.DefaultContractDurationMonths = model.DefaultContractDurationMonths;
                settings.RequireDeposit = model.RequireDeposit;
                settings.DefaultDepositAmount = model.DefaultDepositAmount;
                settings.EnablePaymentReminders = model.EnablePaymentReminders;
                settings.ReminderBeforeDueDays = model.ReminderBeforeDueDays;
                settings.ReminderAfterDueDays = model.ReminderAfterDueDays;
                settings.UpdatedAt = DateTime.Now;
                settings.UpdatedBy = userId;

                if (isNew)
                {
                    settings.CreatedAt = DateTime.Now;
                    _context.SubscriptionSettings.Add(settings);
                }
                else
                {
                    _context.SubscriptionSettings.Update(settings);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region Billing Settings

        public async Task<BillingSettingsViewModel> GetBillingSettingsAsync()
        {
            var settings = await _context.BillingSettings.FirstOrDefaultAsync();
            if (settings == null)
                return new BillingSettingsViewModel();

            return new BillingSettingsViewModel
            {
                Id = settings.Id,
                InvoiceNumberPrefix = settings.InvoiceNumberPrefix,
                InvoiceNumberLength = settings.InvoiceNumberLength,
                IncludeYearInInvoice = settings.IncludeYearInInvoice,
                IncludeMonthInInvoice = settings.IncludeMonthInInvoice,
                ResetInvoiceNumberYearly = settings.ResetInvoiceNumberYearly,
                ReceiptNumberPrefix = settings.ReceiptNumberPrefix,
                ReceiptNumberLength = settings.ReceiptNumberLength,
                EnableTax = settings.EnableTax,
                TaxName = settings.TaxName,
                TaxPercentage = settings.TaxPercentage,
                TaxIncludedInPrice = settings.TaxIncludedInPrice,
                EnableDiscounts = settings.EnableDiscounts,
                AllowPercentageDiscount = settings.AllowPercentageDiscount,
                AllowFixedDiscount = settings.AllowFixedDiscount,
                MaxDiscountPercentage = settings.MaxDiscountPercentage,
                AllowCashPayment = settings.AllowCashPayment,
                AllowBankTransfer = settings.AllowBankTransfer,
                AllowCreditCard = settings.AllowCreditCard,
                AllowCheque = settings.AllowCheque,
                AllowOnlinePayment = settings.AllowOnlinePayment,
                AllowMobilePayment = settings.AllowMobilePayment,
                InvoiceHeader = settings.InvoiceHeader,
                InvoiceFooter = settings.InvoiceFooter,
                PaymentTerms = settings.PaymentTerms,
                BankDetails = settings.BankDetails,
                ShowLogoOnInvoice = settings.ShowLogoOnInvoice,
                ShowSignatureOnInvoice = settings.ShowSignatureOnInvoice,
                ShowStampOnInvoice = settings.ShowStampOnInvoice,
                Currency = settings.Currency,
                CurrencySymbol = settings.CurrencySymbol,
                RoundingMethod = settings.RoundingMethod,
                RoundingDecimals = settings.RoundingDecimals,
                AutoSendInvoiceEmail = settings.AutoSendInvoiceEmail,
                AutoSendReceiptEmail = settings.AutoSendReceiptEmail
            };
        }

        public async Task<bool> SaveBillingSettingsAsync(BillingSettingsViewModel model, string userId)
        {
            try
            {
                var settings = await _context.BillingSettings.FirstOrDefaultAsync();
                bool isNew = false;

                if (settings == null)
                {
                    settings = new Models.Settings.BillingSettings();
                    isNew = true;
                }

                settings.InvoiceNumberPrefix = model.InvoiceNumberPrefix;
                settings.InvoiceNumberLength = model.InvoiceNumberLength;
                settings.IncludeYearInInvoice = model.IncludeYearInInvoice;
                settings.IncludeMonthInInvoice = model.IncludeMonthInInvoice;
                settings.ResetInvoiceNumberYearly = model.ResetInvoiceNumberYearly;
                settings.ReceiptNumberPrefix = model.ReceiptNumberPrefix;
                settings.ReceiptNumberLength = model.ReceiptNumberLength;
                settings.EnableTax = model.EnableTax;
                settings.TaxName = model.TaxName;
                settings.TaxPercentage = model.TaxPercentage;
                settings.TaxIncludedInPrice = model.TaxIncludedInPrice;
                settings.EnableDiscounts = model.EnableDiscounts;
                settings.AllowPercentageDiscount = model.AllowPercentageDiscount;
                settings.AllowFixedDiscount = model.AllowFixedDiscount;
                settings.MaxDiscountPercentage = model.MaxDiscountPercentage;
                settings.AllowCashPayment = model.AllowCashPayment;
                settings.AllowBankTransfer = model.AllowBankTransfer;
                settings.AllowCreditCard = model.AllowCreditCard;
                settings.AllowCheque = model.AllowCheque;
                settings.AllowOnlinePayment = model.AllowOnlinePayment;
                settings.AllowMobilePayment = model.AllowMobilePayment;
                settings.InvoiceHeader = model.InvoiceHeader;
                settings.InvoiceFooter = model.InvoiceFooter;
                settings.PaymentTerms = model.PaymentTerms;
                settings.BankDetails = model.BankDetails;
                settings.ShowLogoOnInvoice = model.ShowLogoOnInvoice;
                settings.ShowSignatureOnInvoice = model.ShowSignatureOnInvoice;
                settings.ShowStampOnInvoice = model.ShowStampOnInvoice;
                settings.Currency = model.Currency;
                settings.CurrencySymbol = model.CurrencySymbol;
                settings.RoundingMethod = model.RoundingMethod;
                settings.RoundingDecimals = model.RoundingDecimals;
                settings.AutoSendInvoiceEmail = model.AutoSendInvoiceEmail;
                settings.AutoSendReceiptEmail = model.AutoSendReceiptEmail;
                settings.UpdatedAt = DateTime.Now;
                settings.UpdatedBy = userId;

                if (isNew)
                {
                    settings.CreatedAt = DateTime.Now;
                    _context.BillingSettings.Add(settings);
                }
                else
                {
                    _context.BillingSettings.Update(settings);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        #endregion
    }
}