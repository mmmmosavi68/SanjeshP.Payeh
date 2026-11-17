
using System.ComponentModel.DataAnnotations.Schema;
using SanjeshP.Payeh.Entities.Common;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Entities.User;

public class UserUniversity : BaseEntity
{
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public int UniversityId { get; set; }

    [ForeignKey(nameof(UniversityId))]
    public virtual University University { get; set; } = null!;
}
    