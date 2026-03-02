using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mahjong.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using X.PagedList.Extensions;

namespace Mahjong.Controllers
{
    public class AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        // Controllers
        // GET: /Admin/
        [Authorize(Roles = "Administrator")]
        #region public ActionResult Index(string searchStringUserNameOrEmail)
        public ActionResult Index(string searchStringUserNameOrEmail, string currentFilter, int? page)
        {
            try
            {
                int intPage = 1;
                int intPageSize = 10;
                int intTotalPageCount = 0;
                if (searchStringUserNameOrEmail != null)
                {
                    intPage = 1;
                }
                else
                {
                    searchStringUserNameOrEmail = currentFilter;
                    intPage = page ?? 1;
                }
                ViewBag.CurrentFilter = searchStringUserNameOrEmail;
                List<ExpandedUser> col_User = [];
                int intSkip = (intPage - 1) * intPageSize;
                List<ApplicationUser> result;
                if (searchStringUserNameOrEmail == null || searchStringUserNameOrEmail == "")
                {
                    intTotalPageCount = UserManager.Users.Count();
                    result = [.. UserManager.Users
                        .OrderBy(x => x.UserName)
                        .Skip(intSkip)
                        .Take(intPageSize)];
                }
                else
                {
                    intTotalPageCount = UserManager.Users
                        .Where(x => x.UserName.Contains(searchStringUserNameOrEmail))
                        .Count();
                    result = [.. UserManager.Users
                        .Where(x => x.UserName.Contains(searchStringUserNameOrEmail))
                        .OrderBy(x => x.UserName)
                        .Skip(intSkip)
                        .Take(intPageSize)];
                }

                foreach (var item in result)
                {
                    ExpandedUser objUser = new()
                    {
                        UserName = item.UserName,
                        LockoutEndDateUtc = item.LockoutEnd?.UtcDateTime
                    };
                    col_User.Add(objUser);
                }
                // Set the number of pages
                var _UserAsIPagedList =
                    new StaticPagedList<ExpandedUser>(col_User, intPage, intPageSize, intTotalPageCount);
                return View(_UserAsIPagedList);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                List<ExpandedUser> col_User = [];
                return View(col_User.ToPagedList(1, 25));
            }
        }
        #endregion

        // Roles *****************************
        // GET: /Admin/ViewAllRoles
        [Authorize(Roles = "Administrator")]
        #region public ActionResult ViewAllRoles()
        public ActionResult ViewAllRoles()
        {
            List<Role> colRole = [.. (from objRole in RoleManager.Roles
                                  select new Role
                                  {
                                      Id = objRole.Id,
                                      RoleName = objRole.Name
                                  })];
            return View(colRole);
        }
        #endregion

        // GET: /Admin/AddRole
        [Authorize(Roles = "Administrator")]
        #region public ActionResult AddRole()
        public ActionResult AddRole()
        {
            Role objRole = new();
            return View(objRole);
        }
        #endregion
        // PUT: /Admin/AddRole
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult AddRole(Role paramRole)
        public ActionResult AddRole(Role paramRole)
        {
            try
            {
                if (paramRole == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }
                var RoleName = paramRole.RoleName.Trim();
                if (RoleName == "")
                {
                    throw new Exception("No RoleName");
                }
                // Create Role
                if (!RoleManager.RoleExistsAsync(RoleName).Result)
                {
                    RoleManager.CreateAsync(new IdentityRole(RoleName)).Wait();
                }
                return Redirect("~/Admin/ViewAllRoles");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("AddRole");
            }
        }
        #endregion
        // DELETE: /Admin/DeleteUserRole?RoleName=TestRole
        [Authorize(Roles = "Administrator")]
        #region public ActionResult DeleteUserRole(string RoleName)
        public ActionResult DeleteUserRole(string RoleName)
        {
            try
            {
                if (RoleName == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }
                if (RoleName.Equals("Administrator", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new Exception(String.Format("Cannot delete {0} Role.", RoleName));
                }
                var UsersInRole = UserManager.GetUsersInRoleAsync(RoleName).Result.Count;
                if (UsersInRole > 0)
                {
                    throw new Exception(
                        String.Format(
                            "Canot delete {0} Role because it still has users.",
                            RoleName)
                            );
                }
                var objRoleToDelete = (from objRole in RoleManager.Roles
                                       where objRole.Name == RoleName
                                       select objRole).FirstOrDefault();
                if (objRoleToDelete != null)
                {
                    RoleManager.DeleteAsync(objRoleToDelete).Wait();
                }
                else
                {
                    throw new Exception(
                        String.Format(
                            "Cannot delete {0} Role does not exist.",
                            RoleName)
                            );
                }
                List<Role> colRole = [.. (from objRole in RoleManager.Roles
                                      select new Role
                                      {
                                          Id = objRole.Id,
                                          RoleName = objRole.Name
                                      })];
                return View("ViewAllRoles", colRole);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                List<Role> colRole = [.. (from objRole in RoleManager.Roles
                                      select new Role
                                      {
                                          Id = objRole.Id,
                                          RoleName = objRole.Name
                                      })];
                return View("ViewAllRoles", colRole);
            }
        }
        #endregion

        // Users *****************************
        // GET: /Admin/Edit/Create 
        [Authorize(Roles = "Administrator")]
        #region public ActionResult Create()
        public ActionResult Create()
        {
            ExpandedUser objExpandedUser = new();
            ViewBag.Roles = GetAllRolesAsSelectList();
            return View(objExpandedUser);
        }
        #endregion
        // PUT: /Admin/Create
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult Create(ExpandedUser paramExpandedUser)
        public ActionResult Create(ExpandedUser paramExpandedUser)
        {
            try
            {
                if (paramExpandedUser == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }
                var Email = paramExpandedUser.Email.Trim();
                var UserName = paramExpandedUser.UserName.Trim();
                var Password = paramExpandedUser.Password.Trim();
                /*if (Email == "")
                {
                    throw new Exception("No Email");
                }*/
                if (Password == "")
                {
                    throw new Exception("No Password");
                }

                // Create user
                var objNewAdminUser = new ApplicationUser { UserName = UserName, Email = Email };
                var AdminUserCreateResult = UserManager.CreateAsync(objNewAdminUser, Password).Result;
                if (AdminUserCreateResult.Succeeded == true)
                {
                    string strNewRole = Convert.ToString(Request.Form["Roles"]);
                    if (strNewRole != "0")
                    {
                        // Put user in role
                        UserManager.AddToRoleAsync(objNewAdminUser, strNewRole).Wait();
                    }
                    return Redirect("~/Admin/Index");
                }
                else
                {
                    ViewBag.Roles = GetAllRolesAsSelectList();
                    ModelState.AddModelError(string.Empty, "Error: Failed to create the user. Check password requirements.");
                    return View(paramExpandedUser);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Roles = GetAllRolesAsSelectList();
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("Create");
            }
        }
        #endregion

        // GET: /Admin/Edit/TestUser 
        [Authorize(Roles = "Administrator")]
        #region public ActionResult EditUser(string UserName)
        public ActionResult EditUser(string UserName)
        {
            if (UserName == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            ExpandedUser objExpandedUser = GetUser(UserName);
            if (objExpandedUser == null)
            {
                return NotFound();
            }
            return View(objExpandedUser);
        }
        #endregion
        // PUT: /Admin/EditUser
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult EditUser(ExpandedUser paramExpandedUser)
        public ActionResult EditUser(ExpandedUser paramExpandedUser)
        {
            try
            {
                if (paramExpandedUser == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }
                ExpandedUser objExpandedUserDTO = UpdateUser(paramExpandedUser);
                if (objExpandedUserDTO == null)
                {
                    return NotFound();
                }
                return Redirect("~/Admin/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("EditUser", GetUser(paramExpandedUser.UserName));
            }
        }
        #endregion
        // DELETE: /Admin/DeleteUser
        [Authorize(Roles = "Administrator")]
        #region public ActionResult DeleteUser(string UserName)
        public ActionResult DeleteUser(string UserName)
        {
            try
            {
                if (UserName == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }
                if (UserName.Equals(this.User.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    ModelState.AddModelError(
                        string.Empty, "Error: Cannot delete the current user");
                    return View("EditUser");
                }
                ExpandedUser objExpandedUser = GetUser(UserName);
                if (objExpandedUser == null)
                {
                    return NotFound();
                }
                else
                {
                    DeleteUser(objExpandedUser);
                }
                return Redirect("~/Admin/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("EditUser", GetUser(UserName));
            }
        }
        #endregion
        // GET: /Admin/EditRoles/TestUser 
        [Authorize(Roles = "Administrator")]
        #region ActionResult EditRoles(string UserName)
        public ActionResult EditRoles(string UserName)
        {
            if (UserName == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            UserName = UserName.ToLower();
            // Check that we have an actual user
            ExpandedUser objExpandedUser = GetUser(UserName);
            if (objExpandedUser == null)
            {
                return NotFound();
            }
            UserAndRoles objUserAndRoles = GetUserAndRoles(UserName);
            return View(objUserAndRoles);
        }
        #endregion
        // PUT: /Admin/EditRoles/TestUser 
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult EditRoles(UserAndRoles paramUserAndRoles)
        public ActionResult EditRoles(UserAndRoles paramUserAndRoles)
        {
            try
            {
                if (paramUserAndRoles == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }
                string UserName = paramUserAndRoles.UserName;
                string strNewRole = Convert.ToString(Request.Form["AddRole"]);
                if (strNewRole != "No Roles Found")
                {
                    // Go get the User
                    ApplicationUser user = UserManager.FindByNameAsync(UserName).Result;
                    // Put user in role
                    UserManager.AddToRoleAsync(user, strNewRole).Wait();
                }
                ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));
                UserAndRoles objUserAndRoles = GetUserAndRoles(UserName);
                return View(objUserAndRoles);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("EditRoles");
            }
        }
        #endregion
        // DELETE: /Admin/DeleteRole?UserName="TestUser&RoleName=Administrator
        [Authorize(Roles = "Administrator")]
        #region public ActionResult DeleteRole(string UserName, string RoleName)
        public ActionResult DeleteRole(string UserName, string RoleName)
        {
            try
            {
                if ((UserName == null) || (RoleName == null))
                {
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }
                UserName = UserName.ToLower();
                // Check that we have an actual user
                ExpandedUser objExpandedUser = GetUser(UserName);
                if (objExpandedUser == null)
                {
                    return NotFound();
                }
                if (UserName.Equals(this.User.Identity.Name, StringComparison.CurrentCultureIgnoreCase) && RoleName == "Administrator")
                {
                    ModelState.AddModelError(string.Empty, "Error: Cannot delete Administrator Role for the current user");
                }
                // Go get the User
                ApplicationUser user = UserManager.FindByNameAsync(UserName).Result;
                // Remove User from role
                UserManager.RemoveFromRoleAsync(user, RoleName).Wait();
                UserManager.UpdateAsync(user).Wait();
                ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));
                return RedirectToAction("EditRoles", new { UserName });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));
                UserAndRoles objUserAndRoles = GetUserAndRoles(UserName);
                return View("EditRoles", objUserAndRoles);
            }
        }
        #endregion

        // Utility
        // Utility
        #region public UserManager<ApplicationUser> UserManager
        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                return userManager;
            }
            private set
            {
                userManager = value;
            }
        }
        #endregion
        #region public RoleManager<IdentityRole> RoleManager
        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return roleManager;
            }
            private set
            {
                roleManager = value;
            }
        }
        #endregion

        #region private List<SelectListItem> GetAllRolesAsSelectList()
        private List<SelectListItem> GetAllRolesAsSelectList()
        {
            List<SelectListItem> SelectRoleListItems = [];
            var colRoleSelectList = RoleManager.Roles.OrderBy(x => x.Name).ToList();
            SelectRoleListItems.Add(
                new SelectListItem
                {
                    Text = "Select",
                    Value = "0"
                });
            foreach (var item in colRoleSelectList)
            {
                SelectRoleListItems.Add(
                    new SelectListItem
                    {
                        Text = item.Name.ToString(),
                        Value = item.Name.ToString()
                    });
            }
            return SelectRoleListItems;
        }
        #endregion

        #region private ExpandedUser GetUser(string paramUserName)
        private ExpandedUser GetUser(string paramUserName)
        {
            ExpandedUser objExpandedUser = new();

            var result = UserManager.FindByNameAsync(paramUserName).Result ?? throw new Exception("Could not find the User");
            objExpandedUser.UserName = result.UserName;
            objExpandedUser.Email = result.Email;
            objExpandedUser.LockoutEndDateUtc = result.LockoutEnd?.UtcDateTime;
            objExpandedUser.AccessFailedCount = result.AccessFailedCount;
            objExpandedUser.PhoneNumber = result.PhoneNumber;

            return objExpandedUser;
        }
        #endregion

        #region private ExpandedUser UpdateUser(ExpandedUser objExpandedUser)
        private ExpandedUser UpdateUser(ExpandedUser paramExpandedUser)
        {
            ApplicationUser result = UserManager.FindByNameAsync(paramExpandedUser.UserName).Result ?? throw new Exception("Could not find the User");
            result.Email = paramExpandedUser.Email;
            // Lets check if the account needs to be unlocked
            if (UserManager.IsLockedOutAsync(result).Result)
            {
                // Unlock user
                UserManager.ResetAccessFailedCountAsync(result).Wait();
            }
            UserManager.UpdateAsync(result).Wait();
            // Was a password sent across?
            if (!string.IsNullOrEmpty(paramExpandedUser.Password))
            {
                // Remove current password
                var removePassword = UserManager.RemovePasswordAsync(result).Result;
                if (removePassword.Succeeded)
                {
                    // Add new password
                    var AddPassword =
                        UserManager.AddPasswordAsync(result, paramExpandedUser.Password).Result;
                    if (AddPassword.Errors.Any())
                    {
                        throw new Exception(AddPassword.Errors.FirstOrDefault().Description);
                    }
                }
            }
            return paramExpandedUser;
        }
        #endregion
        #region private void DeleteUser(ExpandedUser paramExpandedUser)
        private void DeleteUser(ExpandedUser paramExpandedUser)
        {
            ApplicationUser user = UserManager.FindByNameAsync(paramExpandedUser.UserName).Result ?? throw new Exception("Could not find the User");
            UserManager.RemoveFromRolesAsync(user, [.. UserManager.GetRolesAsync(user).Result]).Wait();
            UserManager.UpdateAsync(user).Wait();
            UserManager.DeleteAsync(user).Wait();
        }
        #endregion

        #region private UserAndRoles GetUserAndRoles(string UserName)
        private UserAndRoles GetUserAndRoles(string UserName)
        {
            // Go get the User
            ApplicationUser user = UserManager.FindByNameAsync(UserName).Result;
            List<UserRole> colUserRole = [.. (from objRole in UserManager.GetRolesAsync(user).Result
                                          select new UserRole
                                          {
                                              RoleName = objRole,
                                              UserName = UserName
                                          })];
            if (colUserRole.Count == 0)
            {
                colUserRole.Add(new UserRole { RoleName = "No Roles Found" });
            }
            ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));
            // Create UserRolesAndPermissionsDTO
            UserAndRoles objUserAndRoles = new()
            {
                UserName = UserName,
                colUserRole = colUserRole
            };
            return objUserAndRoles;
        }
        #endregion
        #region private List<string> RolesUserIsNotIn(string UserName)
        private List<string> RolesUserIsNotIn(string UserName)
        {
            // Get roles the user is not in
            var colAllRoles = RoleManager.Roles.Select(x => x.Name).ToList();
            // Go get the roles for an individual
            ApplicationUser user = UserManager.FindByNameAsync(UserName).Result ?? throw new Exception("Could not find the User");
            var colRolesForUser = UserManager.GetRolesAsync(user).Result.ToList();
            var colRolesUserInNotIn = (from objRole in colAllRoles
                                       where !colRolesForUser.Contains(objRole)
                                       select objRole).ToList();
            if (colRolesUserInNotIn.Count == 0)
            {
                colRolesUserInNotIn.Add("No Roles Found");
            }
            return colRolesUserInNotIn;
        }
        #endregion
    }
}