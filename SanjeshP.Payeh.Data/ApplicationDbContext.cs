using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.Entities.Common;
using SanjeshP.Payeh.Entities.Menu;
using System.Linq.Expressions;
using System.Reflection;

namespace SanjeshP.Payeh.Data
{
    /// <summary>
    /// کانتکست اصلی EF Core.
    /// تغییر این نسخه: افزودن Global Query Filter برای حذف نرم (IsDeleted)
    /// تا هیچ کوئری‌ای به‌صورت پیش‌فرض رکوردهای حذف‌شده را برنگرداند.
    /// برای دسترسی به رکوردهای حذف‌شده از IgnoreQueryFilters() استفاده کنید.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entitiesAssembly = typeof(IEntity).Assembly;

            // این View در دیتابیس ساخته می‌شود و نباید Migration تولید کند.
            modelBuilder.Entity<View_UserMenubar>().ToTable(nameof(View_UserMenubar), t => t.ExcludeFromMigrations());

            modelBuilder.RegisterAllEntities<IEntity>(entitiesAssembly);
            modelBuilder.RegisterEntityTypeConfiguration(entitiesAssembly);
            modelBuilder.AddRestrictDeleteBehaviorConvention();
            modelBuilder.AddSequentialGuidForIdConvention();
            modelBuilder.AddPluralizingTableNameConvention();

            ApplySoftDeleteQueryFilter(modelBuilder);
        }

        /// <summary>اعمال فیلتر سراسری IsDeleted روی همه‌ی موجودیت‌های ارث‌برده از BaseEntity.</summary>
        private static void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType.GetProperty("IsDeleted", BindingFlags.Public | BindingFlags.Instance) is null)
                    continue;

                // معادل: entity => !entity.IsDeleted
                var parameter = Expression.Parameter(clrType, "entity");
                var property = Expression.Property(parameter, "IsDeleted");
                var body = Expression.Not(property);
                var lambda = Expression.Lambda(body, parameter);

                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }

        public override int SaveChanges()
        {
            CleanPersianStrings();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            CleanPersianStrings();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            CleanPersianStrings();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            CleanPersianStrings();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// یکسان‌سازی ارقام فارسی و حروف ي/ك عربی در همه‌ی رشته‌های ورودی قبل از ذخیره.
        /// (تغییر نام از _cleanString به CleanPersianStrings طبق استاندارد نام‌گذاری)
        /// </summary>
        private void CleanPersianStrings()
        {
            var changedEntities = ChangeTracker.Entries()
                .Where(x => x.State is EntityState.Added or EntityState.Modified);

            foreach (var item in changedEntities)
            {
                var properties = item.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));

                foreach (var property in properties)
                {
                    var value = (string?)property.GetValue(item.Entity);
                    if (value.HasValue())
                    {
                        var newValue = value!.Fa2En().FixPersianChars();
                        if (newValue != value)
                            property.SetValue(item.Entity, newValue);
                    }
                }
            }
        }
    }
}