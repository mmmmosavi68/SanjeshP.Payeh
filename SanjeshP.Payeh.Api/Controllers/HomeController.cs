using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers
{
    public class TestRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }




    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                Success = true,
                Message = "API is working!",
                Data = new
                {
                    Id = 1,
                    Name = "Test User",
                    Email = "test@example.com"
                }
            });
        }

        [HttpPost("Test")]
        public IActionResult TestPost([FromBody] TestRequest request)
        {
            return Ok(new
            {
                Success = true,
                Message = "Data received successfully!",
                Data = request
            });
        }




        [HttpGet("TestRoles")]
        public ApiResult<List<Role>> TestRoles()
        {
            var roles = new List<Role>
                            {
                                new Role
                                {
                                    Id = 1,
                                    RoleNameEn = "Admin",
                                    NormalizedRoleNameEn = "ADMIN",
                                    RoleNameFa = "مدیر",
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedDate = null,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedBy = Guid.NewGuid(),
                                    HostIp = "127.0.0.1",
                                    UserRoles = new List<UserRole>()
                                },
                                new Role
                                {
                                    Id = 2,
                                    RoleNameEn = "User",
                                    NormalizedRoleNameEn = "USER",
                                    RoleNameFa = "کاربر",
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedDate = null,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedBy = Guid.NewGuid(),
                                    HostIp = "127.0.0.1",
                                    UserRoles = new List<UserRole>()
                                },
                                new Role
                                {
                                    Id = 3,
                                    RoleNameEn = "Support",
                                    NormalizedRoleNameEn = "SUPPORT",
                                    RoleNameFa = "پشتیبان",
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedDate = null,
                                    IsActive = false,
                                    IsDeleted = false,
                                    CreatedBy = Guid.NewGuid(),
                                    HostIp = null,
                                    UserRoles = new List<UserRole>()
                                } };

            return Ok(roles);
        }

    }
}