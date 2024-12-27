using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using distant.ViewModels;
using distant.Models;
using System.Security.Claims;
using distant.Data;
using Microsoft.EntityFrameworkCore;

namespace distant.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly AppDbContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        // Регистрация
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Создаем пользователя (или студента)
                // Получаем текущий код из таблицы AppSettings
                var appSetting = await _context.AppSettings.FirstOrDefaultAsync(a => a.Id == 1);
                if (appSetting == null || appSetting.VerificationCode != model.VerificationCode)
                {
                    ModelState.AddModelError(string.Empty, "Неверный код, выданный дирекцией.");
                    return View(model);
                }
                var user = new Student
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    GroupId = 2  // Устанавливаем GroupId = 2 при регистрации
                };

                // Создаем пользователя с паролем
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Назначение роли "Student"
                    await _userManager.AddToRoleAsync(user, "Student");

                    // Добавление claim для роли
                    var roleClaim = new Claim(ClaimTypes.Role, "Student");
                    await _userManager.AddClaimAsync(user, roleClaim);

                    // Выполнение входа после регистрации
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Перенаправление на главную страницу
                    return RedirectToAction("Index", "Home");
                }

                // Обработка ошибок при регистрации
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }




        // Вход
        public IActionResult Login() => View();
        // Метод для входа
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Попытка входа: Логин = {UserName}", model.UserName);

                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Получаем пользователя
                    var user = await _userManager.FindByNameAsync(model.UserName);

                    // Получаем роли и добавляем claims для них
                    var roles = await _userManager.GetRolesAsync(user);
                    var claims = new List<Claim>();
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    // Добавляем claims в текущую сессию
                    var identity = new ClaimsIdentity(claims, "local");
                    await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

                    _logger.LogInformation("Вход выполнен успешно для пользователя {UserName}", model.UserName);
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError("Ошибка валидации: {ErrorMessage}", error.ErrorMessage);
                }
            }

            return View(model);
        }

        // Личный кабинет
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Проверяем, является ли пользователь студентом
            var student = await _context.Users
                .OfType<Student>()
                .Include(s => s.Group) // Загружаем информацию о группе
                .FirstOrDefaultAsync(s => s.Id == user.Id);

            // Создаем модель для отображения данных пользователя
            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                GroupName = student?.Group?.Name // Если пользователь студент, выводим название группы
            };

            return View(model);
        }



        // Редактирование данных пользователя (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string userId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userToEdit = currentUser;

            // Если userId передан в запросе, то это значит, что администратор редактирует чужой профиль
            if (userId != null && User.IsInRole("Admin"))
            {
                userToEdit = await _userManager.FindByIdAsync(userId);
                if (userToEdit == null)
                {
                    return NotFound();
                }
            }

            var model = new EditProfileViewModel
            {
                UserName = userToEdit.UserName,
                Email = userToEdit.Email,
                FirstName = userToEdit.FirstName,
                LastName = userToEdit.LastName,
                MiddleName = userToEdit.MiddleName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model, string userId = null)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userToEdit = currentUser;

                // Если userId передан и это администратор, редактируем чужой профиль
                if (userId != null && User.IsInRole("Admin"))
                {
                    userToEdit = await _userManager.FindByIdAsync(userId);
                    if (userToEdit == null)
                    {
                        return NotFound();
                    }
                }

                // Изменяем данные пользователя
                userToEdit.UserName = model.UserName;
                userToEdit.Email = model.Email;
                userToEdit.FirstName = model.FirstName;
                userToEdit.LastName = model.LastName;
                userToEdit.MiddleName = model.MiddleName;

                var result = await _userManager.UpdateAsync(userToEdit);

                if (result.Succeeded)
                {
                    // Если это администратор, возвращаем на страницу ManageUsers
                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ManageUsers", "Admin");
                    }
                    // Если это не администратор, перенаправляем на профиль
                    return RedirectToAction("Profile", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }







        // Выход
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
