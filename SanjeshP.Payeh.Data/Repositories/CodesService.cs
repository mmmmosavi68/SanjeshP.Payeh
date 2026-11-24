
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Repositories;

public sealed class CodesService : ICodesService, IScopedDependency
{
    private const int RootPrimaryCode = 0;
    private const int RootSubCode = 0;

    private readonly IEFRepository<Code> _repository;

    public CodesService(IEFRepository<Code> repository)
    {
        _repository = repository;
    }

    #region Read

    public async Task<List<Code>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _repository.TableNoTracking
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.PrimaryCode)
            .ThenBy(x => x.SubCode)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Code> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var code = await _repository.Table
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (code is null)
            throw new NotFoundException("کد موردنظر پیدا نشد.");

        return code;
    }

    public async Task<List<Code>> GetGroupsAsync(
        CancellationToken cancellationToken)
    {
        var root = await GetRootAsync(cancellationToken);

        if (root is null)
            return [];

        return await _repository.TableNoTracking
            .Where(x =>
                x.ParentId == root.Id &&
                x.SubCode == 0 &&
                x.IsActive)
            .OrderBy(x => x.PrimaryCode)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Code>> GetItemsAsync(
        int primaryCode,
        CancellationToken cancellationToken)
    {
        var group = await _repository.TableNoTracking
            .FirstOrDefaultAsync(x =>
                x.PrimaryCode == primaryCode &&
                x.SubCode == 0 &&
                x.ParentId != null &&
                x.IsActive,
                cancellationToken);

        if (group is null)
            return [];

        return await _repository.TableNoTracking
            .Where(x =>
                x.ParentId == group.Id &&
                x.PrimaryCode == primaryCode &&
                x.SubCode > 1 &&
                x.IsActive)
            .OrderBy(x => x.SubCode)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Create

    public async Task<Code> CreateAsync(
        Code code,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        ValidateTitle(code.Title);
        ValidateAdditionalInformation(code.AdditionalInformation);

        var root = await GetRootAsync(cancellationToken);

        if (root is null)
        {
            throw new BadRequestException(
                "ساختار اصلی کدها در سیستم پیدا نشد.");
        }

        if (!code.ParentId.HasValue)
        {
            throw new BadRequestException(
                "انتخاب والد برای ایجاد کد الزامی است.");
        }

        var parent = await GetActiveCodeAsync(
            code.ParentId.Value,
            cancellationToken);

        if (parent.Id == root.Id)
        {
            // ایجاد گروه
            if (code.PrimaryCode is null or <= 0)
            {
                throw new BadRequestException(
                    "کد اصلی گروه باید بزرگ‌تر از صفر باشد.");
            }

            var duplicateGroup = await _repository.TableNoTracking
                .AnyAsync(x =>
                    x.ParentId == root.Id &&
                    x.PrimaryCode == code.PrimaryCode &&
                    x.SubCode == 0,
                    cancellationToken);

            if (duplicateGroup)
            {
                throw new BadRequestException(
                    "این کد اصلی قبلاً برای گروه دیگری ثبت شده است.");
            }

            code.SubCode = 0;
            code.StandardCode = null;
        }
        else
        {
            // ایجاد آیتم
            await ValidateGroupAsync(
                parent,
                root.Id,
                cancellationToken);

            if (!code.SubCode.HasValue || code.SubCode.Value < 1)
            {
                throw new BadRequestException(
                    "کد فرعی آیتم باید مشخص شود.");
            }

            var duplicateItem = await _repository.TableNoTracking
                .AnyAsync(x =>
                    x.ParentId == parent.Id &&
                    x.SubCode == code.SubCode,
                    cancellationToken);

            if (duplicateItem)
            {
                throw new BadRequestException(
                    "این کد فرعی قبلاً در گروه انتخاب‌شده ثبت شده است.");
            }

            // کد اصلی آیتم از گروه والد گرفته می‌شود.
            code.PrimaryCode = parent.PrimaryCode;
        }

        code.Title = code.Title.Trim();
        code.AdditionalInformation =
            string.IsNullOrWhiteSpace(code.AdditionalInformation)
                ? null
                : code.AdditionalInformation.Trim();

        code.CreatedBy = createdBy;
        code.CreatedDate = DateTime.UtcNow;
        code.UpdatedDate = null;
        code.IsActive = true;
        code.IsDeleted = false;

        await _repository.AddAsync(
            code,
            cancellationToken);

        return code;
    }

    #endregion

    #region Update


    public async Task<Code> UpdateAsync(
        int id,
        Code updatedCode,
        CancellationToken cancellationToken)
    {
        var code = await GetByIdAsync(id, cancellationToken);

        var root = await GetRootAsync(cancellationToken);

        if (root is null)
            throw new BadRequestException("ساختار اصلی کدها در سیستم پیدا نشد.");

        if (code.Id == root.Id)
            throw new BadRequestException("ویرایش کد ریشه امکان‌پذیر نیست.");

        ValidateTitle(updatedCode.Title);
        ValidateAdditionalInformation(updatedCode.AdditionalInformation);

        if (!updatedCode.ParentId.HasValue)
            throw new BadRequestException("انتخاب والد الزامی است.");

        // تشخیص نوع رکورد فعلی
        var isGroup = code.ParentId == root.Id && code.SubCode == 0;
        var isItem = code.ParentId != root.Id && code.SubCode > 0;

        if (!isGroup && !isItem)
            throw new BadRequestException("ساختار فعلی کد معتبر نیست.");

        var parent = await GetActiveCodeAsync(
            updatedCode.ParentId.Value,
            cancellationToken);

        if (isGroup)
        {
            // گروه باید همچنان مستقیماً زیرمجموعه ریشه باشد.
            if (parent.Id != root.Id)
                throw new BadRequestException("گروه باید مستقیماً زیرمجموعه ریشه باشد.");

            if (updatedCode.PrimaryCode is null or <= 0)
                throw new BadRequestException("کد اصلی گروه باید بزرگ‌تر از صفر باشد.");

            var hasChildren = await _repository.TableNoTracking
                .AnyAsync(x => x.ParentId == id, cancellationToken);

            // تغییر PrimaryCode گروه دارای آیتم مجاز نیست.
            if (hasChildren && code.PrimaryCode != updatedCode.PrimaryCode)
                throw new BadRequestException(
                    "به دلیل وجود آیتم در این گروه، تغییر کد اصلی امکان‌پذیر نیست.");

            var duplicateGroup = await _repository.TableNoTracking
                .AnyAsync(x =>
                    x.Id != id &&
                    x.ParentId == root.Id &&
                    x.PrimaryCode == updatedCode.PrimaryCode &&
                    x.SubCode == 0,
                    cancellationToken);

            if (duplicateGroup)
                throw new BadRequestException(
                    "این کد اصلی قبلاً برای گروه دیگری ثبت شده است.");

            code.ParentId = root.Id;
            code.PrimaryCode = updatedCode.PrimaryCode;
            code.SubCode = 0;
            code.StandardCode = null;
        }
        else
        {
            // آیتم باید مستقیماً زیرمجموعه یک گروه باشد.
            await ValidateGroupAsync(parent, root.Id, cancellationToken);

            if (!updatedCode.SubCode.HasValue || updatedCode.SubCode.Value < 1)
                throw new BadRequestException("کد فرعی آیتم باید مشخص شود.");

            var duplicateItem = await _repository.TableNoTracking
                .AnyAsync(x =>
                    x.Id != id &&
                    x.ParentId == parent.Id &&
                    x.SubCode == updatedCode.SubCode,
                    cancellationToken);

            if (duplicateItem)
                throw new BadRequestException(
                    "این کد فرعی قبلاً در گروه انتخاب‌شده ثبت شده است.");

            code.ParentId = parent.Id;
            code.PrimaryCode = parent.PrimaryCode;
            code.SubCode = updatedCode.SubCode;
            code.StandardCode = updatedCode.StandardCode;
        }

        code.Title = updatedCode.Title.Trim();

        code.AdditionalInformation =
            string.IsNullOrWhiteSpace(updatedCode.AdditionalInformation)
                ? null
                : updatedCode.AdditionalInformation.Trim();

        code.IsActive = updatedCode.IsActive;
        code.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(code, cancellationToken);

        return code;
    }


    #endregion

    #region Delete

    public async Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var code = await GetByIdAsync(
            id,
            cancellationToken);

        var root = await GetRootAsync(cancellationToken);

        if (root is not null && code.Id == root.Id)
        {
            throw new BadRequestException(
                "حذف کد ریشه امکان‌پذیر نیست.");
        }

        var hasChildren = await _repository.TableNoTracking
            .AnyAsync(
                x => x.ParentId == id,
                cancellationToken);

        if (hasChildren)
        {
            throw new BadRequestException(
                "این کد دارای زیرمجموعه است. ابتدا زیرمجموعه‌های آن را حذف کنید.");
        }

        code.IsDeleted = true;
        code.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(
            code,
            cancellationToken);
    }

    #endregion

    #region Validation

    private async Task<Code?> GetRootAsync(
        CancellationToken cancellationToken)
    {
        return await _repository.TableNoTracking
            .FirstOrDefaultAsync(x =>
                x.ParentId == null &&
                x.PrimaryCode == RootPrimaryCode &&
                x.SubCode == RootSubCode,
                cancellationToken);
    }

    private async Task<Code> GetActiveCodeAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var code = await _repository.TableNoTracking
            .FirstOrDefaultAsync(
                x => x.Id == id && x.IsActive,
                cancellationToken);

        if (code is null)
        {
            throw new BadRequestException(
                "کد والد انتخاب‌شده معتبر نیست یا غیرفعال شده است.");
        }

        return code;
    }

    private async Task ValidateGroupAsync(
        Code parent,
        int rootId,
        CancellationToken cancellationToken)
    {
        var isGroup = parent.ParentId == rootId &&
                      parent.SubCode == 0;

        if (!isGroup)
        {
            throw new BadRequestException(
                "آیتم باید مستقیماً زیرمجموعه یک گروه باشد.");
        }

        await Task.CompletedTask;
    }

    private static void ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BadRequestException(
                "عنوان الزامی است.");
        }

        if (title.Trim().Length > 200)
        {
            throw new BadRequestException(
                "عنوان نباید بیشتر از ۲۰۰ کاراکتر باشد.");
        }
    }

    private static void ValidateAdditionalInformation(
        string? additionalInformation)
    {
        if (additionalInformation?.Length > 400)
        {
            throw new BadRequestException(
                "اطلاعات اضافی نباید بیشتر از ۴۰۰ کاراکتر باشد.");
        }
    }

    #endregion


    public async Task<List<Code>> GetManagementGroupsAsync(
    CancellationToken cancellationToken)
    {
        var root = await GetRootAsync(cancellationToken);

        if (root is null)
            return [];

        return await _repository.TableNoTracking
            .Where(x =>
                x.ParentId == root.Id &&
                x.SubCode == 0)
            .OrderBy(x => x.PrimaryCode)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Code>> GetManagementItemsAsync(
        int groupId,
        CancellationToken cancellationToken)
    {
        var root = await GetRootAsync(cancellationToken);

        if (root is null)
        {
            throw new BadRequestException(
                "ساختار اصلی کدها در سیستم پیدا نشد.");
        }

        var group = await GetByIdAsync(
            groupId,
            cancellationToken);

        var isGroup =
            group.ParentId == root.Id &&
            group.SubCode == 0;

        if (!isGroup)
        {
            throw new BadRequestException(
                "گروه انتخاب‌شده معتبر نیست.");
        }

        return await _repository.TableNoTracking
            .Where(x =>
                x.ParentId == groupId &&
                x.SubCode > 0)
            .OrderBy(x => x.SubCode)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }
}
