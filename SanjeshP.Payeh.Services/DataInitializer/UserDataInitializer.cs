using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace SanjeshP.Payeh.Services.DataInitializer
{
    public class UserDataInitializer : IDataInitializer
    {
        //private readonly IEFRepository<Role> eFRoleRepository;
        //private readonly IEFRepository<User> eFUserRepository;
        //private readonly IEFRepository<UserProfile> eFUserProfileRepository;
        //private readonly IEFRepository<Menu> eFMenuRepository;
        //private readonly IEFRepository<Group> eFGroupRepository;
        //private readonly IEFRepository<UserGroup> eFUserGroupRepository;
        //private readonly IEFRepository<AccessMenusGroup> eFAccessMenusGroupRepository;
        //private readonly IEFRepository<AccessMenus> eFAccessMenusRepository;
        //private readonly IEFRepository<UserRole> eFUserRoleRepository;

        //public UserDataInitializer(IEFRepository<Role> eFRoleRepository
        //    , IEFRepository<User> eFUserRepository
        //    , IEFRepository<UserProfile> eFUserProfileRepository
        //    , IEFRepository<Menu> eFMenuRepository
        //    , IEFRepository<Group> eFGroupRepository
        //    , IEFRepository<UserGroup> eFUserGroupRepository
        //    , IEFRepository<AccessMenusGroup> eFAccessMenusGroupRepository
        //    , IEFRepository<AccessMenus> eFAccessMenusRepository
        //    , IEFRepository<UserRole> eFUserRoleRepository)
        //{
        //    this.eFRoleRepository = eFRoleRepository;
        //    this.eFUserRepository = eFUserRepository;
        //    this.eFUserProfileRepository = eFUserProfileRepository;
        //    this.eFMenuRepository = eFMenuRepository;
        //    this.eFGroupRepository = eFGroupRepository;
        //    this.eFUserGroupRepository = eFUserGroupRepository;
        //    this.eFAccessMenusGroupRepository = eFAccessMenusGroupRepository;
        //    this.eFAccessMenusRepository = eFAccessMenusRepository;
        //    this.eFUserRoleRepository = eFUserRoleRepository;
        //}

        public void InitializeData()
        {
            //    string path = Path.Combine(Environment.CurrentDirectory, @"DefaultData\");

            //    var jsonUser = File.ReadAllText(path + "User.json");
            //    var jUser = JsonConvert.DeserializeObject<List<User>>(jsonUser);
            //    foreach (var menu in jUser)
            //    {
            //        var existMenu = eFUserRepository.TableNoTracking.Any(p => p.NormalizedUserName == menu.NormalizedUserName);
            //        if (!existMenu)
            //        {
            //            menu.CreateDate = DateTime.Now;
            //            menu.ExpireDate = DateTime.Now.AddYears(20);
            //            eFUserRepository.Add(menu);
            //        }
            //    }

            //    var user = eFUserRepository.TableNoTracking.Where(p => p.NormalizedUserName == "ADMINISTRATOR").SingleOrDefault();

            //    var jsonUserProfile = File.ReadAllText(path + "UserProfile.json");
            //    var jUserProfile = JsonConvert.DeserializeObject<List<UserProfile>>(jsonUserProfile);
            //    foreach (var menu in jUserProfile)
            //    {
            //        var existMenu = eFUserProfileRepository.TableNoTracking.Any(p => p.UserId == menu.UserId);
            //        if (!existMenu)
            //        {
            //            menu.Creator = user.Id;
            //            menu.CreateDate = DateTime.Now;
            //            eFUserProfileRepository.Add(menu);
            //        }
            //    }

            //var jsonMenu = File.ReadAllText(path + "Menu.json");
            //var jMenu = JsonConvert.DeserializeObject<List<Menu>>(jsonMenu).OrderBy(u => u.PageCode);
            //foreach (var menu in jMenu)
            //{
            //    var existMenu = eFMenuRepository.TableNoTracking.Any(p => p.Id == menu.Id);
            //    if (!existMenu)
            //    {
            //        menu.Creator = user.Id;
            //        menu.CreateDate = DateTime.Now;
            //        eFMenuRepository.Add(menu);
            //    }
            //}

            //var jsonGroup = File.ReadAllText(path + "Group.json");
            //var jGroup = JsonConvert.DeserializeObject<List<Group>>(jsonGroup);
            //foreach (var menu in jGroup)
            //{
            //    var existMenu = eFGroupRepository.TableNoTracking.Any(p => p.UserGroupText == menu.UserGroupText);
            //    if (!existMenu)
            //    {
            //        menu.Creator = user.Id;
            //        menu.CreateDate = DateTime.Now;
            //        eFGroupRepository.Add(menu);
            //    }
            //}

            //var jsonUserGroup = File.ReadAllText(path + "UserGroup.json");
            //var jUserGroup = JsonConvert.DeserializeObject<List<UserGroup>>(jsonUserGroup);
            //foreach (var menu in jUserGroup)
            //{
            //    var existMenu = eFUserGroupRepository.TableNoTracking.Any(p => p.Id == menu.Id);
            //    if (!existMenu)
            //    {
            //        menu.Creator = user.Id;
            //        menu.CreateDate = DateTime.Now;
            //        eFUserGroupRepository.Add(menu);
            //    }
            //}

            //var jsonAccessMenusGroup = File.ReadAllText(path + "AccessMenusGroup.json");
            //var jAccessMenusGroup = JsonConvert.DeserializeObject<List<AccessMenusGroup>>(jsonAccessMenusGroup);
            //foreach (var menu in jAccessMenusGroup)
            //{
            //    var existMenu = eFAccessMenusGroupRepository.TableNoTracking.Any(p => p.GroupId == menu.GroupId && p.ListMenuId == menu.ListMenuId);
            //    if (!existMenu)
            //    {
            //        menu.Creator = user.Id;
            //        menu.CreateDate = DateTime.Now;
            //        eFAccessMenusGroupRepository.Add(menu);
            //    }
            //}

            //var jsonAccessMenu = File.ReadAllText(path + "AccessMenus.json");
            //var jAccessMenu = JsonConvert.DeserializeObject<List<AccessMenus>>(jsonAccessMenu);
            //foreach (var menu in jAccessMenu)
            //{
            //    var existMenu = eFAccessMenusRepository.TableNoTracking.Any(p => p.UserId == menu.UserId && p.ListMenuId == menu.ListMenuId);
            //    if (!existMenu)
            //    {
            //        menu.Creator = user.Id;
            //        menu.CreateDate = DateTime.Now;
            //        eFAccessMenusRepository.Add(menu);
            //    }
            //}

            //    var jsonRole = File.ReadAllText(path + "Role.json");
            //    var jRole = JsonConvert.DeserializeObject<List<Role>>(jsonRole);
            //    foreach (var menu in jRole)
            //    {
            //        var existMenu = eFRoleRepository.TableNoTracking.Any(p => p.RoleTitleEn == menu.RoleTitleEn && p.RoleTitleFa == menu.RoleTitleFa);
            //        if (!existMenu)
            //        {
            //            menu.Creator = user.Id;
            //            menu.CreateDate = DateTime.Now;
            //            eFRoleRepository.Add(menu);
            //        }
            //    }

            //    var role = eFRoleRepository.TableNoTracking.Where(p => p.NormalizedRoleTitleEn == "ADMIN").SingleOrDefault();
            //    var existuserRole = eFUserRoleRepository.TableNoTracking.Any(p => p.UserId == user.Id && p.RoleId == role.Id);
            //    if (!existuserRole)
            //    {
            //        UserRole userRole = new UserRole
            //        {
            //            UserId = user.Id,
            //            RoleId = role.Id,
            //            IsActive = true,
            //            IsDelete = false,
            //            CreateDate = DateTime.Now,
            //            Creator = user.Id,
            //            HostIp = "::1"
            //        };
            //        eFUserRoleRepository.Add(userRole);
            //    }
        }
    }
}