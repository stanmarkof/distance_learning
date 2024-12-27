using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using distant.Models;
using distant.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using distant.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace distant.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;  // Логгер для записи логов

        public AdminController(UserManager<User> userManager,
                               RoleManager<IdentityRole<int>> roleManager,
                               AppDbContext context,
                               ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;  // Инициализация логгера
        }



        // Страница управления пользователями
        public async Task<IActionResult> ManageUsers(string searchUserId = null, string searchRole=null)
        {
            var users = _userManager.Users.ToList();


            if (!string.IsNullOrEmpty(searchUserId))
            {
                users = users.Where(u => u.Id.ToString().Contains(searchUserId)).ToList();
            }
            // Фильтрация по роли
            if (!string.IsNullOrEmpty(searchRole))
            {
                var filteredUsers = new List<User>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains(searchRole))
                    {
                        filteredUsers.Add(user);
                    }
                }

                users = filteredUsers;
            }

            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Получаем группу, к которой привязан студент, если это студент
                var group = user is Student student
                            ? await _context.Groups.FirstOrDefaultAsync(g => g.Id == student.GroupId)
                            : null;

                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = roles.ToList(),
                    GroupId = group?.Id
                });
            }

            ViewBag.Groups = new SelectList(await _context.Groups.ToListAsync(), "Id", "Name");

            ViewData["SearchUserId"] = searchUserId;

            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> ChangeRole(UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return RedirectToAction("ManageUsers");
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var roleClaims = claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            foreach (var roleClaim in roleClaims)
            {
                await _userManager.RemoveClaimAsync(user, roleClaim);
            }

            if (!string.IsNullOrEmpty(model.NewRole))
            {
                var addResult = await _userManager.AddToRoleAsync(user, model.NewRole);

                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    var roleClaim = new Claim(ClaimTypes.Role, model.NewRole);
                    await _userManager.AddClaimAsync(user, roleClaim);
                }
            }

            // Привязка к группе, если это студент
            if (model.NewRole == "Student" && model.GroupId.HasValue)
            {
                var group = await _context.Groups
                                          .FirstOrDefaultAsync(g => g.Id == model.GroupId); // Используем GroupId напрямую

                if (group != null)
                {
                    // Получаем студента из базы данных и обновляем его привязку к группе
                    var student = await _context.Users
                                                .OfType<Student>()
                                                .FirstOrDefaultAsync(s => s.Id == user.Id);

                    if (student != null)
                    {
                        // Обновляем привязку к группе
                        student.GroupId = group.Id;
                        _context.Entry(student).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ManageUsers");
        }







        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            return RedirectToAction("ManageUsers");
        }





        // Просмотр всех отзывов
        public async Task<IActionResult> ViewFeedbacks()
        {
            var feedbacks = await _context.Feedbacks.ToListAsync();
            return View(feedbacks);
        }

        // Удаление отзыва
        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ViewFeedbacks");
        }





        public IActionResult CreateGroup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(Group group)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Логируем получение данных
                    _logger.LogInformation("Attempting to add a new group with name: " + group.Name);

                    // Добавляем группу в контекст
                    _context.Groups.Add(group);

                    // Сохраняем изменения в базе данных
                    var result = await _context.SaveChangesAsync();

                    // Логируем результат
                    if (result > 0)
                    {
                        _logger.LogInformation("Group saved successfully: " + group.Name);
                    }
                    else
                    {
                        _logger.LogWarning("Group not saved. SaveChanges returned 0.");
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Логируем исключение
                    _logger.LogError($"Error while saving group: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the group.");
                }
            }
            else
            {
                // Логируем ошибки модели
                _logger.LogWarning("ModelState is not valid. Please check the form fields.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError(error.ErrorMessage);
                }
            }

            return View(group);
        }






        // Страница для изменения кода
        public async Task<IActionResult> ChangeVerificationCode()
        {
            var appSetting = await _context.AppSettings.FindAsync(1); // Получаем текущий код из базы данных
            if (appSetting == null)
            {
                return NotFound();
            }

            var model = new ChangeVerificationCodeViewModel
            {
                CurrentVerificationCode = appSetting.VerificationCode
            };

            return View(model);
        }

        // Обработка изменения кода
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeVerificationCode(ChangeVerificationCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model state is not valid.");
            }
            _logger.LogInformation("ChangeVerificationCode method called.");

            if (ModelState.IsValid)
            {
                // Получаем текущий код из базы данных
                var appSetting = await _context.AppSettings.FindAsync(1);
                if (appSetting == null)
                {
                    _logger.LogError("AppSetting not found.");
                    return NotFound();
                }

                // Проверка, совпадает ли подтвержденный новый код с введенным
                if (model.NewVerificationCode != model.ConfirmNewVerificationCode)
                {
                    _logger.LogError("New verification code does not match the confirmation.");
                    ModelState.AddModelError(string.Empty, "Новый код и подтвержденный код не совпадают.");
                    return View(model);
                }

                // Обновляем код в базе данных
                appSetting.VerificationCode = model.NewVerificationCode;
                _context.Entry(appSetting).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Verification code updated successfully.");
                return RedirectToAction("Index", "Home");
            }

            _logger.LogError("ModelState is not valid.");
            return View(model);
        }



        public IActionResult Function()
        {
            return View();
        }




    }
}