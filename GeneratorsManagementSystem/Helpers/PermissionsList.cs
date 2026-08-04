namespace GeneratorsManagementSystem.Helpers
{
    public static class PermissionsList
    {
        public static class Dashboard
        {
            public const string View = "Dashboard.View";
        }

        public static class Subscribers
        {
            public const string View = "Subscribers.View";
            public const string Create = "Subscribers.Create";
            public const string Edit = "Subscribers.Edit";
            public const string Delete = "Subscribers.Delete";
            public const string Export = "Subscribers.Export";
        }

        public static class Generators
        {
            public const string View = "Generators.View";
            public const string Create = "Generators.Create";
            public const string Edit = "Generators.Edit";
            public const string Delete = "Generators.Delete";
            public const string Monitor = "Generators.Monitor";
        }

        public static class Fuel
        {
            public const string View = "Fuel.View";
            public const string Create = "Fuel.Create";
            public const string Edit = "Fuel.Edit";
            public const string Delete = "Fuel.Delete";
        }

        public static class Maintenance
        {
            public const string View = "Maintenance.View";
            public const string Create = "Maintenance.Create";
            public const string Edit = "Maintenance.Edit";
            public const string Delete = "Maintenance.Delete";
        }

        public static class Accounting
        {
            public const string View = "Accounting.View";
            public const string CreateInvoice = "Accounting.CreateInvoice";
            public const string ReceivePayment = "Accounting.ReceivePayment";
            public const string DeleteInvoice = "Accounting.DeleteInvoice";
        }

        public static class Reports
        {
            public const string View = "Reports.View";
            public const string Export = "Reports.Export";
        }

        public static class Settings
        {
            public const string View = "Settings.View";
            public const string EditGeneral = "Settings.EditGeneral";
            public const string EditOrganization = "Settings.EditOrganization";
            public const string ManageUsers = "Settings.ManageUsers";
            public const string ManageRoles = "Settings.ManageRoles";
            public const string ManageBackup = "Settings.ManageBackup";
            public const string ViewAuditLog = "Settings.ViewAuditLog";
        }

        // مجموعات الصلاحيات
        public static List<PermissionGroup> GetAllPermissions()
        {
            return new List<PermissionGroup>
            {
                new PermissionGroup
                {
                    GroupName = "لوحة التحكم",
                    GroupKey = "Dashboard",
                    Icon = "fa-tachometer-alt",
                    Color = "primary",
                    Permissions = new List<PermissionItem>
                    {
                        new(Dashboard.View, "عرض لوحة التحكم", "الوصول للوحة التحكم الرئيسية")
                    }
                },
                new PermissionGroup
                {
                    GroupName = "المشتركين",
                    GroupKey = "Subscribers",
                    Icon = "fa-users",
                    Color = "success",
                    Permissions = new List<PermissionItem>
                    {
                        new(Subscribers.View, "عرض المشتركين", "عرض قائمة المشتركين"),
                        new(Subscribers.Create, "إضافة مشترك", "إضافة مشتركين جدد"),
                        new(Subscribers.Edit, "تعديل المشترك", "تعديل بيانات المشتركين"),
                        new(Subscribers.Delete, "حذف المشترك", "حذف المشتركين"),
                        new(Subscribers.Export, "تصدير المشتركين", "تصدير قائمة المشتركين")
                    }
                },
                new PermissionGroup
                {
                    GroupName = "المولدات",
                    GroupKey = "Generators",
                    Icon = "fa-bolt",
                    Color = "warning",
                    Permissions = new List<PermissionItem>
                    {
                        new(Generators.View, "عرض المولدات", "عرض قائمة المولدات"),
                        new(Generators.Create, "إضافة مولد", "إضافة مولدات جديدة"),
                        new(Generators.Edit, "تعديل المولد", "تعديل بيانات المولدات"),
                        new(Generators.Delete, "حذف المولد", "حذف المولدات"),
                        new(Generators.Monitor, "مراقبة التشغيل", "مراقبة تشغيل المولدات المباشر")
                    }
                },
                new PermissionGroup
                {
                    GroupName = "الوقود والتشغيل",
                    GroupKey = "Fuel",
                    Icon = "fa-gas-pump",
                    Color = "danger",
                    Permissions = new List<PermissionItem>
                    {
                        new(Fuel.View, "عرض الوقود", "عرض سجلات الوقود"),
                        new(Fuel.Create, "إضافة تعبئة", "إضافة تعبئة وقود"),
                        new(Fuel.Edit, "تعديل السجل", "تعديل سجلات الوقود"),
                        new(Fuel.Delete, "حذف السجل", "حذف سجلات الوقود")
                    }
                },
                new PermissionGroup
                {
                    GroupName = "الصيانة",
                    GroupKey = "Maintenance",
                    Icon = "fa-tools",
                    Color = "info",
                    Permissions = new List<PermissionItem>
                    {
                        new(Maintenance.View, "عرض الصيانة", "عرض سجلات الصيانة"),
                        new(Maintenance.Create, "إضافة صيانة", "إضافة سجل صيانة"),
                        new(Maintenance.Edit, "تعديل الصيانة", "تعديل سجلات الصيانة"),
                        new(Maintenance.Delete, "حذف الصيانة", "حذف سجلات الصيانة")
                    }
                },
                new PermissionGroup
                {
                    GroupName = "المحاسبة المالية",
                    GroupKey = "Accounting",
                    Icon = "fa-money-bill",
                    Color = "success",
                    Permissions = new List<PermissionItem>
                    {
                        new(Accounting.View, "عرض المحاسبة", "عرض السجلات المالية"),
                        new(Accounting.CreateInvoice, "إنشاء فاتورة", "إنشاء فواتير جديدة"),
                        new(Accounting.ReceivePayment, "استلام دفعة", "تسجيل استلام الدفعات"),
                        new(Accounting.DeleteInvoice, "حذف فاتورة", "حذف الفواتير")
                    }
                },
                new PermissionGroup
                {
                    GroupName = "التقارير",
                    GroupKey = "Reports",
                    Icon = "fa-chart-pie",
                    Color = "purple",
                    Permissions = new List<PermissionItem>
                    {
                        new(Reports.View, "عرض التقارير", "الوصول لصفحة التقارير"),
                        new(Reports.Export, "تصدير التقارير", "تصدير التقارير PDF/Excel")
                    }
                },
                new PermissionGroup
                {
                    GroupName = "إعدادات النظام",
                    GroupKey = "Settings",
                    Icon = "fa-cogs",
                    Color = "dark",
                    Permissions = new List<PermissionItem>
                    {
                        new(Settings.View, "عرض الإعدادات", "الوصول لصفحة الإعدادات"),
                        new(Settings.EditGeneral, "تعديل الإعدادات العامة", "تعديل الإعدادات العامة للنظام"),
                        new(Settings.EditOrganization, "تعديل بيانات المؤسسة", "تعديل بيانات المؤسسة"),
                        new(Settings.ManageUsers, "إدارة المستخدمين", "إضافة/تعديل/حذف المستخدمين"),
                        new(Settings.ManageRoles, "إدارة الأدوار", "إدارة الأدوار والصلاحيات"),
                        new(Settings.ManageBackup, "النسخ الاحتياطي", "إدارة النسخ الاحتياطية"),
                        new(Settings.ViewAuditLog, "عرض سجل النشاطات", "عرض سجل نشاطات النظام")
                    }
                }
            };
        }

        public static List<string> GetAllPermissionKeys()
        {
            return GetAllPermissions()
                .SelectMany(g => g.Permissions.Select(p => p.Key))
                .ToList();
        }
    }

    public class PermissionGroup
    {
        public string GroupName { get; set; } = string.Empty;
        public string GroupKey { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<PermissionItem> Permissions { get; set; } = new();
    }

    public class PermissionItem
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public PermissionItem(string key, string name, string description)
        {
            Key = key;
            Name = name;
            Description = description;
        }
    }
}