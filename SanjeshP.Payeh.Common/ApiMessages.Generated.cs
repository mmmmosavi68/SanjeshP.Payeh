namespace SanjeshP.Payeh.Common;

/// <summary>پیام‌های تولیدشده از Literalهای Service/Controller؛ هر پیام یک ErrorCode پایدار دارد (AREA_NNN).</summary>
public static partial class ApiMessages
{
    public static class AuditLogs
    {
        /// <summary>تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.</summary>
        public static readonly ApiMessage M001 = new("AUDIT_LOGS_001", "تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.");
    }

    public static class Auth
    {
        /// <summary>کپچا در تنظیمات غیرفعال است.</summary>
        public static readonly ApiMessage M001 = new("AUTH_001", "کپچا در تنظیمات غیرفعال است.");
        /// <summary>با موفقیت از این دستگاه خارج شدید.</summary>
        public static readonly ApiMessage M002 = new("AUTH_002", "با موفقیت از این دستگاه خارج شدید.");
        /// <summary>از تمام دستگاه‌های دیگر خارج شدید.</summary>
        public static readonly ApiMessage M003 = new("AUTH_003", "از تمام دستگاه‌های دیگر خارج شدید.");
        /// <summary>نشست مورد نظر باطل شد.</summary>
        public static readonly ApiMessage M004 = new("AUTH_004", "نشست مورد نظر باطل شد.");
        /// <summary>ورود با شماره موبایل غیرفعال است.</summary>
        public static readonly ApiMessage M005 = new("AUTH_005", "ورود با شماره موبایل غیرفعال است.");
        /// <summary>ورود با نام کاربری و رمز عبور غیرفعال است.</summary>
        public static readonly ApiMessage M006 = new("AUTH_006", "ورود با نام کاربری و رمز عبور غیرفعال است.");
    }

    public static class ExamRegistrations
    {
        /// <summary>اطلاعات داوطلب با موفقیت ثبت شد.</summary>
        public static readonly ApiMessage M001 = new("EXAM_REGISTRATIONS_001", "اطلاعات داوطلب با موفقیت ثبت شد.");
        /// <summary>اطلاعات داوطلب با موفقیت ویرایش شد.</summary>
        public static readonly ApiMessage M002 = new("EXAM_REGISTRATIONS_002", "اطلاعات داوطلب با موفقیت ویرایش شد.");
        /// <summary>ثبت‌نام با موفقیت حذف شد.</summary>
        public static readonly ApiMessage M003 = new("EXAM_REGISTRATIONS_003", "ثبت‌نام با موفقیت حذف شد.");
    }

    public static class ExamSessions
    {
        /// <summary>نوبت آزمون با موفقیت ثبت شد.</summary>
        public static readonly ApiMessage M001 = new("EXAM_SESSIONS_001", "نوبت آزمون با موفقیت ثبت شد.");
        /// <summary>نوبت آزمون با موفقیت ویرایش شد.</summary>
        public static readonly ApiMessage M002 = new("EXAM_SESSIONS_002", "نوبت آزمون با موفقیت ویرایش شد.");
        /// <summary>نوبت آزمون با موفقیت حذف شد.</summary>
        public static readonly ApiMessage M003 = new("EXAM_SESSIONS_003", "نوبت آزمون با موفقیت حذف شد.");
    }

    public static class Exams
    {
        /// <summary>آزمون با موفقیت ثبت شد.</summary>
        public static readonly ApiMessage M001 = new("EXAMS_001", "آزمون با موفقیت ثبت شد.");
        /// <summary>اطلاعات آزمون با موفقیت ویرایش شد.</summary>
        public static readonly ApiMessage M002 = new("EXAMS_002", "اطلاعات آزمون با موفقیت ویرایش شد.");
        /// <summary>آزمون با موفقیت حذف شد.</summary>
        public static readonly ApiMessage M003 = new("EXAMS_003", "آزمون با موفقیت حذف شد.");
    }

    public static class Groups
    {
        /// <summary>گروه با موفقیت ایجاد شد.</summary>
        public static readonly ApiMessage M001 = new("GROUPS_001", "گروه با موفقیت ایجاد شد.");
        /// <summary>اطلاعات گروه با موفقیت ویرایش شد.</summary>
        public static readonly ApiMessage M002 = new("GROUPS_002", "اطلاعات گروه با موفقیت ویرایش شد.");
        /// <summary>گروه با موفقیت حذف شد.</summary>
        public static readonly ApiMessage M003 = new("GROUPS_003", "گروه با موفقیت حذف شد.");
        /// <summary>کاربر با موفقیت به گروه اضافه شد.</summary>
        public static readonly ApiMessage M004 = new("GROUPS_004", "کاربر با موفقیت به گروه اضافه شد.");
        /// <summary>کاربر با موفقیت از گروه خارج شد.</summary>
        public static readonly ApiMessage M005 = new("GROUPS_005", "کاربر با موفقیت از گروه خارج شد.");
        /// <summary>دسترسی منوهای گروه با موفقیت ذخیره شد.</summary>
        public static readonly ApiMessage M006 = new("GROUPS_006", "دسترسی منوهای گروه با موفقیت ذخیره شد.");
    }

    public static class Menus
    {
        /// <summary>منو با موفقیت ایجاد شد.</summary>
        public static readonly ApiMessage M001 = new("MENUS_001", "منو با موفقیت ایجاد شد.");
        /// <summary>منو با موفقیت حذف شد.</summary>
        public static readonly ApiMessage M002 = new("MENUS_002", "منو با موفقیت حذف شد.");
        /// <summary>منوی موردنظر پیدا نشد.</summary>
        public static readonly ApiMessage M003 = new("MENUS_003", "منوی موردنظر پیدا نشد.");
        /// <summary>این منو دارای زیرمنو است و ابتدا باید زیرمنوهای آن حذف شوند.</summary>
        public static readonly ApiMessage M004 = new("MENUS_004", "این منو دارای زیرمنو است و ابتدا باید زیرمنوهای آن حذف شوند.");
        /// <summary>یک منو نمی‌تواند والد خودش باشد.</summary>
        public static readonly ApiMessage M005 = new("MENUS_005", "یک منو نمی‌تواند والد خودش باشد.");
        /// <summary>منوی والد انتخاب‌شده معتبر نیست یا حذف شده است.</summary>
        public static readonly ApiMessage M006 = new("MENUS_006", "منوی والد انتخاب‌شده معتبر نیست یا حذف شده است.");
        /// <summary>امکان انتخاب یکی از زیرمنوهای این منو به‌عنوان والد وجود ندارد.</summary>
        public static readonly ApiMessage M007 = new("MENUS_007", "امکان انتخاب یکی از زیرمنوهای این منو به‌عنوان والد وجود ندارد.");
        /// <summary>عنوان منو الزامی است.</summary>
        public static readonly ApiMessage M008 = new("MENUS_008", "عنوان منو الزامی است.");
    }

    public static class Universities
    {
        /// <summary>دانشگاه با موفقیت ثبت شد.</summary>
        public static readonly ApiMessage M001 = new("UNIVERSITIES_001", "دانشگاه با موفقیت ثبت شد.");
        /// <summary>اطلاعات دانشگاه با موفقیت ویرایش شد.</summary>
        public static readonly ApiMessage M002 = new("UNIVERSITIES_002", "اطلاعات دانشگاه با موفقیت ویرایش شد.");
        /// <summary>دانشگاه با موفقیت حذف شد.</summary>
        public static readonly ApiMessage M003 = new("UNIVERSITIES_003", "دانشگاه با موفقیت حذف شد.");
    }

    public static class UniversityExamCapacities
    {
        /// <summary>ظرفیت دانشگاه برای آزمون با موفقیت ثبت شد.</summary>
        public static readonly ApiMessage M001 = new("UNIVERSITY_EXAM_CAPACITIES_001", "ظرفیت دانشگاه برای آزمون با موفقیت ثبت شد.");
        /// <summary>ظرفیت دانشگاه مهمان با موفقیت ثبت شد.</summary>
        public static readonly ApiMessage M002 = new("UNIVERSITY_EXAM_CAPACITIES_002", "ظرفیت دانشگاه مهمان با موفقیت ثبت شد.");
    }

    public static class UserManagement
    {
        /// <summary>دسترسی اختصاصی کاربر با موفقیت ذخیره شد.</summary>
        public static readonly ApiMessage M001 = new("USER_MANAGEMENT_001", "دسترسی اختصاصی کاربر با موفقیت ذخیره شد.");
        /// <summary>دسترسی دانشگاهی کاربر با موفقیت ذخیره شد.</summary>
        public static readonly ApiMessage M002 = new("USER_MANAGEMENT_002", "دسترسی دانشگاهی کاربر با موفقیت ذخیره شد.");
        /// <summary>کاربر مورد نظر یافت نشد.</summary>
        public static readonly ApiMessage M003 = new("USER_MANAGEMENT_003", "کاربر مورد نظر یافت نشد.");
        /// <summary>فهرست منوها معتبر نیست.</summary>
        public static readonly ApiMessage M004 = new("USER_MANAGEMENT_004", "فهرست منوها معتبر نیست.");
        /// <summary>شناسه یک یا چند منو نامعتبر است.</summary>
        public static readonly ApiMessage M005 = new("USER_MANAGEMENT_005", "شناسه یک یا چند منو نامعتبر است.");
        /// <summary>حداکثر ۵۰۰ منو قابل انتخاب است.</summary>
        public static readonly ApiMessage M006 = new("USER_MANAGEMENT_006", "حداکثر ۵۰۰ منو قابل انتخاب است.");
        /// <summary>یک یا چند منوی انتخاب‌شده نامعتبر یا غیرفعال هستند.</summary>
        public static readonly ApiMessage M007 = new("USER_MANAGEMENT_007", "یک یا چند منوی انتخاب‌شده نامعتبر یا غیرفعال هستند.");
        /// <summary>حداقل یک کاربر باید انتخاب شود.</summary>
        public static readonly ApiMessage M008 = new("USER_MANAGEMENT_008", "حداقل یک کاربر باید انتخاب شود.");
        /// <summary>شناسه یک یا چند کاربر نامعتبر است.</summary>
        public static readonly ApiMessage M009 = new("USER_MANAGEMENT_009", "شناسه یک یا چند کاربر نامعتبر است.");
        /// <summary>حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است.</summary>
        public static readonly ApiMessage M010 = new("USER_MANAGEMENT_010", "حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است.");
        /// <summary>امکان حذف یا غیرفعال‌سازی حساب کاربری خودتان وجود ندارد.</summary>
        public static readonly ApiMessage M011 = new("USER_MANAGEMENT_011", "امکان حذف یا غیرفعال‌سازی حساب کاربری خودتان وجود ندارد.");
        /// <summary>یک یا چند کاربر یافت نشدند یا قبلاً حذف شده‌اند.</summary>
        public static readonly ApiMessage M012 = new("USER_MANAGEMENT_012", "یک یا چند کاربر یافت نشدند یا قبلاً حذف شده‌اند.");
        /// <summary>فهرست دانشگاه‌ها معتبر نیست.</summary>
        public static readonly ApiMessage M013 = new("USER_MANAGEMENT_013", "فهرست دانشگاه‌ها معتبر نیست.");
        /// <summary>شناسه یک یا چند دانشگاه نامعتبر است.</summary>
        public static readonly ApiMessage M014 = new("USER_MANAGEMENT_014", "شناسه یک یا چند دانشگاه نامعتبر است.");
        /// <summary>حداکثر ۵۰۰ دانشگاه قابل انتخاب است.</summary>
        public static readonly ApiMessage M015 = new("USER_MANAGEMENT_015", "حداکثر ۵۰۰ دانشگاه قابل انتخاب است.");
        /// <summary>یک یا چند دانشگاه انتخاب‌شده نامعتبر یا غیرفعال هستند.</summary>
        public static readonly ApiMessage M016 = new("USER_MANAGEMENT_016", "یک یا چند دانشگاه انتخاب‌شده نامعتبر یا غیرفعال هستند.");
    }

    public static class Users
    {
        /// <summary>کاربر با موفقیت ایجاد شد.</summary>
        public static readonly ApiMessage M001 = new("USERS_001", "کاربر با موفقیت ایجاد شد.");
        /// <summary>کاربر با موفقیت حذف شد.</summary>
        public static readonly ApiMessage M002 = new("USERS_002", "کاربر با موفقیت حذف شد.");
    }

    public static class ExamRegistration
    {
        /// <summary>شناسه کاربر معتبر نیست.</summary>
        public static readonly ApiMessage M001 = new("EXAM_REGISTRATION_001", "شناسه کاربر معتبر نیست.");
        /// <summary>دانشگاه انتخاب‌شده معتبر نیست.</summary>
        public static readonly ApiMessage M002 = new("EXAM_REGISTRATION_002", "دانشگاه انتخاب‌شده معتبر نیست.");
        /// <summary>شناسه کارشناس ثبت‌کننده معتبر نیست.</summary>
        public static readonly ApiMessage M003 = new("EXAM_REGISTRATION_003", "شناسه کارشناس ثبت‌کننده معتبر نیست.");
        /// <summary>انتخاب آزمون الزامی است.</summary>
        public static readonly ApiMessage M004 = new("EXAM_REGISTRATION_004", "انتخاب آزمون الزامی است.");
        /// <summary>نام داوطلب الزامی است.</summary>
        public static readonly ApiMessage M005 = new("EXAM_REGISTRATION_005", "نام داوطلب الزامی است.");
        /// <summary>نام خانوادگی داوطلب الزامی است.</summary>
        public static readonly ApiMessage M006 = new("EXAM_REGISTRATION_006", "نام خانوادگی داوطلب الزامی است.");
        /// <summary>آزمون موردنظر پیدا نشد.</summary>
        public static readonly ApiMessage M007 = new("EXAM_REGISTRATION_007", "آزمون موردنظر پیدا نشد.");
        /// <summary>ثبت‌نام در این آزمون غیرفعال است.</summary>
        public static readonly ApiMessage M008 = new("EXAM_REGISTRATION_008", "ثبت‌نام در این آزمون غیرفعال است.");
        /// <summary>در حال حاضر بازه ثبت‌نام این آزمون فعال نیست.</summary>
        public static readonly ApiMessage M009 = new("EXAM_REGISTRATION_009", "در حال حاضر بازه ثبت‌نام این آزمون فعال نیست.");
        /// <summary>دانشگاه محل تحصیل معتبر نیست یا غیرفعال شده است.</summary>
        public static readonly ApiMessage M010 = new("EXAM_REGISTRATION_010", "دانشگاه محل تحصیل معتبر نیست یا غیرفعال شده است.");
        /// <summary>دانشگاه محل آزمون معتبر نیست یا غیرفعال شده است.</summary>
        public static readonly ApiMessage M011 = new("EXAM_REGISTRATION_011", "دانشگاه محل آزمون معتبر نیست یا غیرفعال شده است.");
        /// <summary>تخصیص ظرفیت انتخاب‌شده با آزمون و دانشگاه محل برگزاری مطابقت ندارد.</summary>
        public static readonly ApiMessage M012 = new("EXAM_REGISTRATION_012", "تخصیص ظرفیت انتخاب‌شده با آزمون و دانشگاه محل برگزاری مطابقت ندارد.");
        /// <summary>برای داوطلب دانشگاه برگزارکننده، انتخاب ظرفیت دانشگاه مهمان مجاز نیست.</summary>
        public static readonly ApiMessage M013 = new("EXAM_REGISTRATION_013", "برای داوطلب دانشگاه برگزارکننده، انتخاب ظرفیت دانشگاه مهمان مجاز نیست.");
        /// <summary>ظرفیت داوطلبان دانشگاه برگزارکننده تکمیل شده است.</summary>
        public static readonly ApiMessage M014 = new("EXAM_REGISTRATION_014", "ظرفیت داوطلبان دانشگاه برگزارکننده تکمیل شده است.");
        /// <summary>برای داوطلب دانشگاه مهمان، انتخاب ظرفیت مهمان الزامی است.</summary>
        public static readonly ApiMessage M015 = new("EXAM_REGISTRATION_015", "برای داوطلب دانشگاه مهمان، انتخاب ظرفیت مهمان الزامی است.");
        /// <summary>ظرفیت مهمان انتخاب‌شده با دانشگاه داوطلب مطابقت ندارد.</summary>
        public static readonly ApiMessage M016 = new("EXAM_REGISTRATION_016", "ظرفیت مهمان انتخاب‌شده با دانشگاه داوطلب مطابقت ندارد.");
        /// <summary>ظرفیت دانشگاه مهمان تکمیل شده است.</summary>
        public static readonly ApiMessage M017 = new("EXAM_REGISTRATION_017", "ظرفیت دانشگاه مهمان تکمیل شده است.");
        /// <summary>ظرفیت کل تخصیص‌یافته به دانشگاه محل آزمون تکمیل شده است.</summary>
        public static readonly ApiMessage M018 = new("EXAM_REGISTRATION_018", "ظرفیت کل تخصیص‌یافته به دانشگاه محل آزمون تکمیل شده است.");
        /// <summary>شناسه کارشناس ویرایش‌کننده معتبر نیست.</summary>
        public static readonly ApiMessage M019 = new("EXAM_REGISTRATION_019", "شناسه کارشناس ویرایش‌کننده معتبر نیست.");
        /// <summary>ثبت‌نام موردنظر پیدا نشد.</summary>
        public static readonly ApiMessage M020 = new("EXAM_REGISTRATION_020", "ثبت‌نام موردنظر پیدا نشد.");
        /// <summary>تغییر دانشگاه محل تحصیل در ویرایش ثبت‌نام مجاز نیست.</summary>
        public static readonly ApiMessage M021 = new("EXAM_REGISTRATION_021", "تغییر دانشگاه محل تحصیل در ویرایش ثبت‌نام مجاز نیست.");
        /// <summary>در حال حاضر بازه ویرایش اطلاعات این آزمون فعال نیست.</summary>
        public static readonly ApiMessage M022 = new("EXAM_REGISTRATION_022", "در حال حاضر بازه ویرایش اطلاعات این آزمون فعال نیست.");
        /// <summary>شناسه آزمون معتبر نیست.</summary>
        public static readonly ApiMessage M023 = new("EXAM_REGISTRATION_023", "شناسه آزمون معتبر نیست.");
        /// <summary>شناسه یکی از فیلدهای اختصاصی معتبر نیست.</summary>
        public static readonly ApiMessage M024 = new("EXAM_REGISTRATION_024", "شناسه یکی از فیلدهای اختصاصی معتبر نیست.");
        /// <summary>یک فیلد اختصاصی بیش از یک بار ارسال شده است.</summary>
        public static readonly ApiMessage M025 = new("EXAM_REGISTRATION_025", "یک فیلد اختصاصی بیش از یک بار ارسال شده است.");
        /// <summary>یکی از فیلدهای اختصاصی متعلق به این آزمون نیست یا غیرفعال است.</summary>
        public static readonly ApiMessage M026 = new("EXAM_REGISTRATION_026", "یکی از فیلدهای اختصاصی متعلق به این آزمون نیست یا غیرفعال است.");
        /// <summary>مقدار فیلد اختصاصی نباید بیش از ۴۰۰۰ کاراکتر باشد.</summary>
        public static readonly ApiMessage M027 = new("EXAM_REGISTRATION_027", "مقدار فیلد اختصاصی نباید بیش از ۴۰۰۰ کاراکتر باشد.");
        /// <summary>آزمون انتخاب‌شده معتبر نیست.</summary>
        public static readonly ApiMessage M028 = new("EXAM_REGISTRATION_028", "آزمون انتخاب‌شده معتبر نیست.");
        /// <summary>آزمون فعال یا در بازه ثبت‌نام نیست.</summary>
        public static readonly ApiMessage M029 = new("EXAM_REGISTRATION_029", "آزمون فعال یا در بازه ثبت‌نام نیست.");
    }

    public static class Exam
    {
        /// <summary>آزمون موردنظر پیدا نشد.</summary>
        public static readonly ApiMessage M001 = new("EXAM_001", "آزمون موردنظر پیدا نشد.");
        /// <summary>شناسه کاربر ثبت‌کننده معتبر نیست.</summary>
        public static readonly ApiMessage M002 = new("EXAM_002", "شناسه کاربر ثبت‌کننده معتبر نیست.");
        /// <summary>برای این آزمون ظرفیت، فیلد اختصاصی یا ثبت‌نام ثبت شده است و امکان حذف آن وجود ندارد.</summary>
        public static readonly ApiMessage M003 = new("EXAM_003", "برای این آزمون ظرفیت، فیلد اختصاصی یا ثبت‌نام ثبت شده است و امکان حذف آن وجود ندارد.");
        /// <summary>عنوان آزمون الزامی است.</summary>
        public static readonly ApiMessage M004 = new("EXAM_004", "عنوان آزمون الزامی است.");
        /// <summary>عنوان آزمون نباید بیش از ۲۰۰ کاراکتر باشد.</summary>
        public static readonly ApiMessage M005 = new("EXAM_005", "عنوان آزمون نباید بیش از ۲۰۰ کاراکتر باشد.");
        /// <summary>نوبت آزمون را انتخاب کنید.</summary>
        public static readonly ApiMessage M006 = new("EXAM_006", "نوبت آزمون را انتخاب کنید.");
        /// <summary>نوع آزمون معتبر نیست.</summary>
        public static readonly ApiMessage M007 = new("EXAM_007", "نوع آزمون معتبر نیست.");
        /// <summary>نحوه برگزاری آزمون معتبر نیست.</summary>
        public static readonly ApiMessage M008 = new("EXAM_008", "نحوه برگزاری آزمون معتبر نیست.");
        /// <summary>پایان ثبت‌نام نمی‌تواند قبل از شروع ثبت‌نام باشد.</summary>
        public static readonly ApiMessage M009 = new("EXAM_009", "پایان ثبت‌نام نمی‌تواند قبل از شروع ثبت‌نام باشد.");
        /// <summary>برای بازه ویرایش، هر دو تاریخ شروع و پایان باید مشخص شوند.</summary>
        public static readonly ApiMessage M010 = new("EXAM_010", "برای بازه ویرایش، هر دو تاریخ شروع و پایان باید مشخص شوند.");
        /// <summary>پایان ویرایش نمی‌تواند قبل از شروع ویرایش باشد.</summary>
        public static readonly ApiMessage M011 = new("EXAM_011", "پایان ویرایش نمی‌تواند قبل از شروع ویرایش باشد.");
        /// <summary>توضیحات نباید بیش از ۱۰۰۰ کاراکتر باشد.</summary>
        public static readonly ApiMessage M012 = new("EXAM_012", "توضیحات نباید بیش از ۱۰۰۰ کاراکتر باشد.");
        /// <summary>نوبت انتخاب‌شده وجود ندارد یا غیرفعال است.</summary>
        public static readonly ApiMessage M013 = new("EXAM_013", "نوبت انتخاب‌شده وجود ندارد یا غیرفعال است.");
    }

    public static class ExamSession
    {
        /// <summary>نوبت آزمون موردنظر پیدا نشد.</summary>
        public static readonly ApiMessage M001 = new("EXAM_SESSION_001", "نوبت آزمون موردنظر پیدا نشد.");
        /// <summary>شناسه کاربر ثبت‌کننده معتبر نیست.</summary>
        public static readonly ApiMessage M002 = new("EXAM_SESSION_002", "شناسه کاربر ثبت‌کننده معتبر نیست.");
        /// <summary>برای این نوبت آزمون تعریف شده است و امکان حذف آن وجود ندارد.</summary>
        public static readonly ApiMessage M003 = new("EXAM_SESSION_003", "برای این نوبت آزمون تعریف شده است و امکان حذف آن وجود ندارد.");
        /// <summary>عنوان نوبت الزامی است.</summary>
        public static readonly ApiMessage M004 = new("EXAM_SESSION_004", "عنوان نوبت الزامی است.");
        /// <summary>عنوان نوبت نباید بیشتر از ۱۰۰ کاراکتر باشد.</summary>
        public static readonly ApiMessage M005 = new("EXAM_SESSION_005", "عنوان نوبت نباید بیشتر از ۱۰۰ کاراکتر باشد.");
        /// <summary>ساعت پایان باید بعد از ساعت شروع باشد.</summary>
        public static readonly ApiMessage M006 = new("EXAM_SESSION_006", "ساعت پایان باید بعد از ساعت شروع باشد.");
    }

    public static class UniversityExamCapacity
    {
        /// <summary>شناسه کاربر ثبت‌کننده معتبر نیست.</summary>
        public static readonly ApiMessage M001 = new("UNIVERSITY_EXAM_CAPACITY_001", "شناسه کاربر ثبت‌کننده معتبر نیست.");
        /// <summary>آزمون را انتخاب کنید.</summary>
        public static readonly ApiMessage M002 = new("UNIVERSITY_EXAM_CAPACITY_002", "آزمون را انتخاب کنید.");
        /// <summary>ظرفیت تخصیص‌یافته باید حداقل ۱ باشد.</summary>
        public static readonly ApiMessage M003 = new("UNIVERSITY_EXAM_CAPACITY_003", "ظرفیت تخصیص‌یافته باید حداقل ۱ باشد.");
        /// <summary>ظرفیت داوطلبان دانشگاه معتبر نیست.</summary>
        public static readonly ApiMessage M004 = new("UNIVERSITY_EXAM_CAPACITY_004", "ظرفیت داوطلبان دانشگاه معتبر نیست.");
        /// <summary>ظرفیت داوطلبان دانشگاه نمی‌تواند بیشتر از ظرفیت کل تخصیص‌یافته باشد.</summary>
        public static readonly ApiMessage M005 = new("UNIVERSITY_EXAM_CAPACITY_005", "ظرفیت داوطلبان دانشگاه نمی‌تواند بیشتر از ظرفیت کل تخصیص‌یافته باشد.");
        /// <summary>آزمون موردنظر وجود ندارد یا غیرفعال است.</summary>
        public static readonly ApiMessage M006 = new("UNIVERSITY_EXAM_CAPACITY_006", "آزمون موردنظر وجود ندارد یا غیرفعال است.");
        /// <summary>اطلاعات ظرفیت پس از ثبت پیدا نشد.</summary>
        public static readonly ApiMessage M007 = new("UNIVERSITY_EXAM_CAPACITY_007", "اطلاعات ظرفیت پس از ثبت پیدا نشد.");
        /// <summary>شناسه دانشگاه معتبر نیست.</summary>
        public static readonly ApiMessage M008 = new("UNIVERSITY_EXAM_CAPACITY_008", "شناسه دانشگاه معتبر نیست.");
        /// <summary>شما به این دانشگاه دسترسی ندارید یا دانشگاه غیرفعال است.</summary>
        public static readonly ApiMessage M009 = new("UNIVERSITY_EXAM_CAPACITY_009", "شما به این دانشگاه دسترسی ندارید یا دانشگاه غیرفعال است.");
        /// <summary>شناسه کاربر معتبر نیست.</summary>
        public static readonly ApiMessage M010 = new("UNIVERSITY_EXAM_CAPACITY_010", "شناسه کاربر معتبر نیست.");
        /// <summary>دانشگاه مهمان را انتخاب کنید.</summary>
        public static readonly ApiMessage M011 = new("UNIVERSITY_EXAM_CAPACITY_011", "دانشگاه مهمان را انتخاب کنید.");
        /// <summary>ظرفیت مهمان باید حداقل ۱ باشد.</summary>
        public static readonly ApiMessage M012 = new("UNIVERSITY_EXAM_CAPACITY_012", "ظرفیت مهمان باید حداقل ۱ باشد.");
        /// <summary>دانشگاه میزبان نمی‌تواند به‌عنوان دانشگاه مهمان انتخاب شود.</summary>
        public static readonly ApiMessage M013 = new("UNIVERSITY_EXAM_CAPACITY_013", "دانشگاه میزبان نمی‌تواند به‌عنوان دانشگاه مهمان انتخاب شود.");
        /// <summary>دانشگاه مهمان وجود ندارد یا غیرفعال است.</summary>
        public static readonly ApiMessage M014 = new("UNIVERSITY_EXAM_CAPACITY_014", "دانشگاه مهمان وجود ندارد یا غیرفعال است.");
        /// <summary>ابتدا باید ظرفیت فعال آزمون برای دانشگاه میزبان تعریف شود.</summary>
        public static readonly ApiMessage M015 = new("UNIVERSITY_EXAM_CAPACITY_015", "ابتدا باید ظرفیت فعال آزمون برای دانشگاه میزبان تعریف شود.");
        /// <summary>ظرفیتی برای تخصیص به دانشگاه‌های مهمان باقی نمانده است.</summary>
        public static readonly ApiMessage M016 = new("UNIVERSITY_EXAM_CAPACITY_016", "ظرفیتی برای تخصیص به دانشگاه‌های مهمان باقی نمانده است.");
        /// <summary>ظرفیت دانشگاه مهمان پس از ثبت پیدا نشد.</summary>
        public static readonly ApiMessage M017 = new("UNIVERSITY_EXAM_CAPACITY_017", "ظرفیت دانشگاه مهمان پس از ثبت پیدا نشد.");
        /// <summary>ظرفیت فعال این آزمون برای دانشگاه پیدا نشد.</summary>
        public static readonly ApiMessage M018 = new("UNIVERSITY_EXAM_CAPACITY_018", "ظرفیت فعال این آزمون برای دانشگاه پیدا نشد.");
    }

    public static class Codes
    {
        /// <summary>کد موردنظر پیدا نشد.</summary>
        public static readonly ApiMessage M001 = new("CODES_001", "کد موردنظر پیدا نشد.");
        /// <summary>ساختار اصلی کدها در سیستم پیدا نشد.</summary>
        public static readonly ApiMessage M002 = new("CODES_002", "ساختار اصلی کدها در سیستم پیدا نشد.");
        /// <summary>انتخاب والد برای ایجاد کد الزامی است.</summary>
        public static readonly ApiMessage M003 = new("CODES_003", "انتخاب والد برای ایجاد کد الزامی است.");
        /// <summary>کد اصلی گروه باید بزرگ‌تر از صفر باشد.</summary>
        public static readonly ApiMessage M004 = new("CODES_004", "کد اصلی گروه باید بزرگ‌تر از صفر باشد.");
        /// <summary>این کد اصلی قبلاً برای گروه دیگری ثبت شده است.</summary>
        public static readonly ApiMessage M005 = new("CODES_005", "این کد اصلی قبلاً برای گروه دیگری ثبت شده است.");
        /// <summary>کد فرعی آیتم باید مشخص شود.</summary>
        public static readonly ApiMessage M006 = new("CODES_006", "کد فرعی آیتم باید مشخص شود.");
        /// <summary>این کد فرعی قبلاً در گروه انتخاب‌شده ثبت شده است.</summary>
        public static readonly ApiMessage M007 = new("CODES_007", "این کد فرعی قبلاً در گروه انتخاب‌شده ثبت شده است.");
        /// <summary>ویرایش کد ریشه امکان‌پذیر نیست.</summary>
        public static readonly ApiMessage M008 = new("CODES_008", "ویرایش کد ریشه امکان‌پذیر نیست.");
        /// <summary>انتخاب والد الزامی است.</summary>
        public static readonly ApiMessage M009 = new("CODES_009", "انتخاب والد الزامی است.");
        /// <summary>ساختار فعلی کد معتبر نیست.</summary>
        public static readonly ApiMessage M010 = new("CODES_010", "ساختار فعلی کد معتبر نیست.");
        /// <summary>گروه باید مستقیماً زیرمجموعه ریشه باشد.</summary>
        public static readonly ApiMessage M011 = new("CODES_011", "گروه باید مستقیماً زیرمجموعه ریشه باشد.");
        /// <summary>به دلیل وجود آیتم در این گروه، تغییر کد اصلی امکان‌پذیر نیست.</summary>
        public static readonly ApiMessage M012 = new("CODES_012", "به دلیل وجود آیتم در این گروه، تغییر کد اصلی امکان‌پذیر نیست.");
        /// <summary>حذف کد ریشه امکان‌پذیر نیست.</summary>
        public static readonly ApiMessage M013 = new("CODES_013", "حذف کد ریشه امکان‌پذیر نیست.");
        /// <summary>این کد دارای زیرمجموعه است. ابتدا زیرمجموعه‌های آن را حذف کنید.</summary>
        public static readonly ApiMessage M014 = new("CODES_014", "این کد دارای زیرمجموعه است. ابتدا زیرمجموعه‌های آن را حذف کنید.");
        /// <summary>کد والد انتخاب‌شده معتبر نیست یا غیرفعال شده است.</summary>
        public static readonly ApiMessage M015 = new("CODES_015", "کد والد انتخاب‌شده معتبر نیست یا غیرفعال شده است.");
        /// <summary>آیتم باید مستقیماً زیرمجموعه یک گروه باشد.</summary>
        public static readonly ApiMessage M016 = new("CODES_016", "آیتم باید مستقیماً زیرمجموعه یک گروه باشد.");
        /// <summary>عنوان الزامی است.</summary>
        public static readonly ApiMessage M017 = new("CODES_017", "عنوان الزامی است.");
        /// <summary>عنوان نباید بیشتر از ۲۰۰ کاراکتر باشد.</summary>
        public static readonly ApiMessage M018 = new("CODES_018", "عنوان نباید بیشتر از ۲۰۰ کاراکتر باشد.");
        /// <summary>اطلاعات اضافی نباید بیشتر از ۴۰۰ کاراکتر باشد.</summary>
        public static readonly ApiMessage M019 = new("CODES_019", "اطلاعات اضافی نباید بیشتر از ۴۰۰ کاراکتر باشد.");
        /// <summary>گروه انتخاب‌شده معتبر نیست.</summary>
        public static readonly ApiMessage M020 = new("CODES_020", "گروه انتخاب‌شده معتبر نیست.");
    }

    public static class Group
    {
        /// <summary>گروه مورد نظر یافت نشد.</summary>
        public static readonly ApiMessage M001 = new("GROUP_001", "گروه مورد نظر یافت نشد.");
        /// <summary>عنوان گروه الزامی است.</summary>
        public static readonly ApiMessage M002 = new("GROUP_002", "عنوان گروه الزامی است.");
        /// <summary>گروهی با این عنوان قبلاً ثبت شده است.</summary>
        public static readonly ApiMessage M003 = new("GROUP_003", "گروهی با این عنوان قبلاً ثبت شده است.");
        /// <summary>امکان افزودن کاربر به گروه غیرفعال وجود ندارد.</summary>
        public static readonly ApiMessage M004 = new("GROUP_004", "امکان افزودن کاربر به گروه غیرفعال وجود ندارد.");
        /// <summary>کاربر مورد نظر یافت نشد یا غیرفعال است.</summary>
        public static readonly ApiMessage M005 = new("GROUP_005", "کاربر مورد نظر یافت نشد یا غیرفعال است.");
        /// <summary>این کاربر در حال حاضر عضو گروه است.</summary>
        public static readonly ApiMessage M006 = new("GROUP_006", "این کاربر در حال حاضر عضو گروه است.");
        /// <summary>عضویت مورد نظر یافت نشد.</summary>
        public static readonly ApiMessage M007 = new("GROUP_007", "عضویت مورد نظر یافت نشد.");
        /// <summary>حداقل یک کاربر باید انتخاب شود.</summary>
        public static readonly ApiMessage M008 = new("GROUP_008", "حداقل یک کاربر باید انتخاب شود.");
        /// <summary>شناسه یک یا چند کاربر نامعتبر است.</summary>
        public static readonly ApiMessage M009 = new("GROUP_009", "شناسه یک یا چند کاربر نامعتبر است.");
        /// <summary>حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است.</summary>
        public static readonly ApiMessage M010 = new("GROUP_010", "حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است.");
        /// <summary>یک یا چند کاربر یافت نشدند یا غیرفعال هستند.</summary>
        public static readonly ApiMessage M011 = new("GROUP_011", "یک یا چند کاربر یافت نشدند یا غیرفعال هستند.");
        /// <summary>یک یا چند کاربر در حال حاضر عضو گروه هستند.</summary>
        public static readonly ApiMessage M012 = new("GROUP_012", "یک یا چند کاربر در حال حاضر عضو گروه هستند.");
        /// <summary>یک یا چند مورد از عضویت‌های انتخاب‌شده یافت نشدند.</summary>
        public static readonly ApiMessage M013 = new("GROUP_013", "یک یا چند مورد از عضویت‌های انتخاب‌شده یافت نشدند.");
        /// <summary>گروه موردنظر یافت نشد.</summary>
        public static readonly ApiMessage M014 = new("GROUP_014", "گروه موردنظر یافت نشد.");
        /// <summary>یک یا چند منوی انتخاب‌شده نامعتبر یا غیرفعال هستند.</summary>
        public static readonly ApiMessage M015 = new("GROUP_015", "یک یا چند منوی انتخاب‌شده نامعتبر یا غیرفعال هستند.");
    }

    public static class Otp
    {
        /// <summary>برای این حساب شماره موبایل ثبت نشده است؛ با مدیر سیستم تماس بگیرید.</summary>
        public static readonly ApiMessage M001 = new("OTP_001", "برای این حساب شماره موبایل ثبت نشده است؛ با مدیر سیستم تماس بگیرید.");
        /// <summary>کد وارد شده منقضی شده است؛ کد جدید درخواست کنید.</summary>
        public static readonly ApiMessage M002 = new("OTP_002", "کد وارد شده منقضی شده است؛ کد جدید درخواست کنید.");
        /// <summary>تعداد تلاش‌ها بیش از حد مجاز است؛ کد جدید درخواست کنید.</summary>
        public static readonly ApiMessage M003 = new("OTP_003", "تعداد تلاش‌ها بیش از حد مجاز است؛ کد جدید درخواست کنید.");
    }

    public static class User
    {
        /// <summary>کاربر مورد نظر یافت نشد.</summary>
        public static readonly ApiMessage M001 = new("USER_001", "کاربر مورد نظر یافت نشد.");
        /// <summary>درخواست قبل از اجرای کوئری لغو شده است.</summary>
        public static readonly ApiMessage M002 = new("USER_002", "درخواست قبل از اجرای کوئری لغو شده است.");
        /// <summary>این نام کاربری قبلاً ثبت شده است.</summary>
        public static readonly ApiMessage M003 = new("USER_003", "این نام کاربری قبلاً ثبت شده است.");
        /// <summary>این شماره موبایل قبلاً ثبت شده است.</summary>
        public static readonly ApiMessage M004 = new("USER_004", "این شماره موبایل قبلاً ثبت شده است.");
        /// <summary>یک یا چند نقش ارسالی معتبر نیستند.</summary>
        public static readonly ApiMessage M005 = new("USER_005", "یک یا چند نقش ارسالی معتبر نیستند.");
        /// <summary>این کد ملی قبلاً ثبت شده است.</summary>
        public static readonly ApiMessage M006 = new("USER_006", "این کد ملی قبلاً ثبت شده است.");
        /// <summary>این ایمیل قبلاً ثبت شده است.</summary>
        public static readonly ApiMessage M007 = new("USER_007", "این ایمیل قبلاً ثبت شده است.");
    }

    public static class Jwt
    {
        /// <summary>توکن تازه‌سازی ارسال نشده است.</summary>
        public static readonly ApiMessage M001 = new("JWT_001", "توکن تازه‌سازی ارسال نشده است.");
        /// <summary>توکن تازه‌سازی نامعتبر است. لطفاً دوباره وارد شوید.</summary>
        public static readonly ApiMessage M002 = new("JWT_002", "توکن تازه‌سازی نامعتبر است. لطفاً دوباره وارد شوید.");
        /// <summary>این نشست باطل شده است. لطفاً دوباره وارد شوید.</summary>
        public static readonly ApiMessage M003 = new("JWT_003", "این نشست باطل شده است. لطفاً دوباره وارد شوید.");
        /// <summary>نشست منقضی شده است. لطفاً دوباره وارد شوید.</summary>
        public static readonly ApiMessage M004 = new("JWT_004", "نشست منقضی شده است. لطفاً دوباره وارد شوید.");
        /// <summary>حساب کاربری غیرفعال است.</summary>
        public static readonly ApiMessage M005 = new("JWT_005", "حساب کاربری غیرفعال است.");
        /// <summary>نشست مورد نظر یافت نشد.</summary>
        public static readonly ApiMessage M006 = new("JWT_006", "نشست مورد نظر یافت نشد.");
    }

    public static class University
    {
        /// <summary>دانشگاه موردنظر پیدا نشد.</summary>
        public static readonly ApiMessage M001 = new("UNIVERSITY_001", "دانشگاه موردنظر پیدا نشد.");
        /// <summary>شناسه کاربر ثبت‌کننده معتبر نیست.</summary>
        public static readonly ApiMessage M002 = new("UNIVERSITY_002", "شناسه کاربر ثبت‌کننده معتبر نیست.");
        /// <summary>این دانشگاه دارای دانشگاه یا پردیس زیرمجموعه است. ابتدا زیرمجموعه‌ها را تعیین تکلیف کنید.</summary>
        public static readonly ApiMessage M003 = new("UNIVERSITY_003", "این دانشگاه دارای دانشگاه یا پردیس زیرمجموعه است. ابتدا زیرمجموعه‌ها را تعیین تکلیف کنید.");
        /// <summary>برای این دانشگاه ظرفیت یا ثبت‌نام وابسته ثبت شده است و امکان حذف آن وجود ندارد.</summary>
        public static readonly ApiMessage M004 = new("UNIVERSITY_004", "برای این دانشگاه ظرفیت یا ثبت‌نام وابسته ثبت شده است و امکان حذف آن وجود ندارد.");
        /// <summary>نام دانشگاه الزامی است.</summary>
        public static readonly ApiMessage M005 = new("UNIVERSITY_005", "نام دانشگاه الزامی است.");
        /// <summary>نام دانشگاه نباید بیش از ۲۰۰ کاراکتر باشد.</summary>
        public static readonly ApiMessage M006 = new("UNIVERSITY_006", "نام دانشگاه نباید بیش از ۲۰۰ کاراکتر باشد.");
        /// <summary>نوع دانشگاه معتبر نیست.</summary>
        public static readonly ApiMessage M007 = new("UNIVERSITY_007", "نوع دانشگاه معتبر نیست.");
        /// <summary>منطقه دانشگاه باید بین ۱ تا ۱۱ باشد.</summary>
        public static readonly ApiMessage M008 = new("UNIVERSITY_008", "منطقه دانشگاه باید بین ۱ تا ۱۱ باشد.");
        /// <summary>ظرفیت مصوب نمی‌تواند منفی باشد.</summary>
        public static readonly ApiMessage M009 = new("UNIVERSITY_009", "ظرفیت مصوب نمی‌تواند منفی باشد.");
        /// <summary>دانشگاه مادر انتخاب‌شده معتبر نیست.</summary>
        public static readonly ApiMessage M010 = new("UNIVERSITY_010", "دانشگاه مادر انتخاب‌شده معتبر نیست.");
        /// <summary>یک دانشگاه نمی‌تواند والد خودش باشد.</summary>
        public static readonly ApiMessage M011 = new("UNIVERSITY_011", "یک دانشگاه نمی‌تواند والد خودش باشد.");
        /// <summary>دانشگاه مادر انتخاب‌شده معتبر نیست یا غیرفعال شده است.</summary>
        public static readonly ApiMessage M012 = new("UNIVERSITY_012", "دانشگاه مادر انتخاب‌شده معتبر نیست یا غیرفعال شده است.");
        /// <summary>امکان انتخاب یکی از زیرمجموعه‌های دانشگاه به‌عنوان دانشگاه مادر وجود ندارد.</summary>
        public static readonly ApiMessage M013 = new("UNIVERSITY_013", "امکان انتخاب یکی از زیرمجموعه‌های دانشگاه به‌عنوان دانشگاه مادر وجود ندارد.");
        /// <summary>ساختار والدین دانشگاه‌ها دارای چرخه نامعتبر است.</summary>
        public static readonly ApiMessage M014 = new("UNIVERSITY_014", "ساختار والدین دانشگاه‌ها دارای چرخه نامعتبر است.");
    }

    public static class IdentityValidation
    {
        /// <summary>شناسه آزمون معتبر نیست.</summary>
        public static readonly ApiMessage M001 = new("IDENTITY_VALIDATION_001", "شناسه آزمون معتبر نیست.");
        /// <summary>کد ملی واردشده معتبر نیست.</summary>
        public static readonly ApiMessage M002 = new("IDENTITY_VALIDATION_002", "کد ملی واردشده معتبر نیست.");
        /// <summary>شماره همراه واردشده معتبر نیست.</summary>
        public static readonly ApiMessage M003 = new("IDENTITY_VALIDATION_003", "شماره همراه واردشده معتبر نیست.");
    }

    public static class Base
    {
        /// <summary>شناسه کاربر معتبر نیست.</summary>
        public static readonly ApiMessage M001 = new("BASE_001", "شناسه کاربر معتبر نیست.");
    }

}
