
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data.Contracts.Universities;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Services.Universities;

public sealed class UniversityService : IUniversityService, IScopedDependency
{
    private readonly IUniversityRepository _repository;
    private readonly ICodesService _codes;

    public UniversityService(IUniversityRepository repository, ICodesService codes)
    {
        _codes = codes;
        _repository = repository;
    }

    #region Read

    public Task<List<University>> GetAllAsync(
        string? search,
        int? universityTypeId,
        int? region,
        int? parentUniversityId,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(
            search,
            universityTypeId,
            region,
            parentUniversityId,
            isActive,
            cancellationToken);
    }

    public async Task<University> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var university = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (university is null)
            throw new NotFoundException("دانشگاه موردنظر پیدا نشد.");

        return university;
    }

    #endregion

    #region Create

    public async Task<University> CreateAsync(
        University university,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (createdBy == Guid.Empty)
            throw new BadRequestException("شناسه کاربر ثبت‌کننده معتبر نیست.");

        ValidateFields(university);

        await ValidateUniversityTypeAsync(
            university.UniversityTypeId,
            cancellationToken);

        await ValidateParentAsync(
            university.ParentUniversityId,
            null,
            cancellationToken);

        university.Title = university.Title.Trim();

        university.CreatedBy = createdBy;
        university.CreatedDate = DateTime.UtcNow;
        university.UpdatedDate = null;
        university.IsActive = true;
        university.IsDeleted = false;

        await _repository.AddAsync(
            university,
            cancellationToken);

        return await GetByIdAsync(
            university.Id,
            cancellationToken);
    }

    #endregion

    #region Update

    public async Task<University> UpdateAsync(
        int id,
        University updatedUniversity,
        CancellationToken cancellationToken)
    {
        var university = await GetByIdAsync(
            id,
            cancellationToken);

        ValidateFields(updatedUniversity);

        await ValidateUniversityTypeAsync(
            updatedUniversity.UniversityTypeId,
            cancellationToken);

        await ValidateParentAsync(
            updatedUniversity.ParentUniversityId,
            id,
            cancellationToken);

        university.Title = updatedUniversity.Title.Trim();
        university.UniversityTypeId = updatedUniversity.UniversityTypeId;
        university.Region = updatedUniversity.Region;
        university.CapacityApproved = updatedUniversity.CapacityApproved;
        university.ParentUniversityId = updatedUniversity.ParentUniversityId;
        university.IsActive = updatedUniversity.IsActive;
        university.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(
            university,
            cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    #endregion

    #region Delete

    public async Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var university = await GetByIdAsync(
            id,
            cancellationToken);

        var hasChildren = await _repository.HasChildrenAsync(
            id,
            cancellationToken);

        if (hasChildren)
        {
            throw new BadRequestException(
                "این دانشگاه دارای دانشگاه یا پردیس زیرمجموعه است. ابتدا زیرمجموعه‌ها را تعیین تکلیف کنید.");
        }

        var hasDependencies = await _repository.HasDependenciesAsync(
            id,
            cancellationToken);

        if (hasDependencies)
        {
            throw new BadRequestException(
                "برای این دانشگاه ظرفیت یا ثبت‌نام وابسته ثبت شده است و امکان حذف آن وجود ندارد.");
        }

        university.IsDeleted = true;
        university.IsActive = false;
        university.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(
            university,
            cancellationToken);
    }

    #endregion

    #region Validation

    private static void ValidateFields(University university)
    {
        if (string.IsNullOrWhiteSpace(university.Title))
            throw new BadRequestException("نام دانشگاه الزامی است.");

        if (university.Title.Trim().Length > 200)
            throw new BadRequestException("نام دانشگاه نباید بیش از ۲۰۰ کاراکتر باشد.");

        if (university.UniversityTypeId <= 0)
            throw new BadRequestException("نوع دانشگاه معتبر نیست.");

        if (university.Region is < 1 or > 11)
            throw new BadRequestException("منطقه دانشگاه باید بین ۱ تا ۱۱ باشد.");

        if (university.CapacityApproved < 0)
            throw new BadRequestException("ظرفیت مصوب نمی‌تواند منفی باشد.");
    }

    private async Task ValidateUniversityTypeAsync(int universityTypeId, CancellationToken cancellationToken)
    {
        await _codes.ResolveIdAsync(universityTypeId, null, null, null, ApiMessages.Lookup.UniversityTypeInvalid, cancellationToken);
    }

    private async Task ValidateParentAsync(
        int? parentUniversityId,
        int? currentUniversityId,
        CancellationToken cancellationToken)
    {
        if (!parentUniversityId.HasValue)
            return;

        if (parentUniversityId.Value <= 0)
            throw new BadRequestException("دانشگاه مادر انتخاب‌شده معتبر نیست.");

        if (currentUniversityId.HasValue &&
            parentUniversityId.Value == currentUniversityId.Value)
        {
            throw new BadRequestException(
                "یک دانشگاه نمی‌تواند والد خودش باشد.");
        }

        var parentExists = await _repository.ParentExistsAsync(
            parentUniversityId.Value,
            cancellationToken);

        if (!parentExists)
        {
            throw new BadRequestException(
                "دانشگاه مادر انتخاب‌شده معتبر نیست یا غیرفعال شده است.");
        }

        if (!currentUniversityId.HasValue)
            return;

        // جلوگیری از ایجاد چرخه در ساختار دانشگاه و پردیس‌ها
        var universities = await _repository.GetAllAsync(
            null, null, null, null, null,
            cancellationToken);

        var universitiesById = universities.ToDictionary(x => x.Id);

        var visited = new HashSet<int>();
        int? ancestorId = parentUniversityId.Value;

        while (ancestorId.HasValue)
        {
            if (ancestorId.Value == currentUniversityId.Value)
            {
                throw new BadRequestException(
                    "امکان انتخاب یکی از زیرمجموعه‌های دانشگاه به‌عنوان دانشگاه مادر وجود ندارد.");
            }

            if (!visited.Add(ancestorId.Value))
            {
                throw new BadRequestException(
                    "ساختار والدین دانشگاه‌ها دارای چرخه نامعتبر است.");
            }

            if (!universitiesById.TryGetValue(
                    ancestorId.Value,
                    out var ancestor))
            {
                break;
            }

            ancestorId = ancestor.ParentUniversityId;
        }
    }

    #endregion

    public Task<PagedItems<TDto>> GetPageAsync<TDto>(string? search, int? universityTypeId, int? universityTypePrimaryCode, int? universityTypeSubCode, int? region, int? parentUniversityId, bool? isActive, int? page, int? pageSize, Func<IQueryable<University>, IQueryable<TDto>> project, CancellationToken cancellationToken)
    {
        return _repository.GetPageAsync(search, universityTypeId, universityTypePrimaryCode, universityTypeSubCode, region, parentUniversityId, isActive, page, pageSize, project, cancellationToken);
    }
}
