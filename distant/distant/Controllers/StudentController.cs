using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using distant.Data;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using distant.Models;
using distant.ViewModels;

namespace distant.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StudentController> _logger;

        public StudentController(AppDbContext context, ILogger<StudentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Отображение доступных уроков
        public async Task<IActionResult> AvailableLessons()
        {
            // Получаем ID текущего пользователя
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Проверяем, авторизован ли пользователь
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Ошибка: UserId не найден.");
                return Unauthorized("Не удалось идентифицировать пользователя.");
            }

            // Пробуем преобразовать ID в целое число
            if (!int.TryParse(userId, out int studentId))
            {
                _logger.LogError("Ошибка: UserId не является допустимым числом.");
                return BadRequest("Некорректный формат идентификатора пользователя.");
            }

            // Находим студента и его группу
            var student = await _context.Students
                .Include(s => s.Group) // Загружаем группу студента
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                _logger.LogWarning($"Студент с ID: {studentId} не найден.");
                return NotFound("Студент не найден.");
            }

            // Проверяем, есть ли группа у студента
            if (student.Group == null)
            {
                _logger.LogWarning($"У студента с ID: {studentId} нет группы.");
                return BadRequest("У студента не указана группа.");
            }

            _logger.LogInformation($"Студент {student.Id} принадлежит группе {student.Group.Name}");

            // Получаем уроки, доступные группе студента
            var lessons = await _context.Lessons
                .Where(l => l.Groups.Any(g => g.Id == student.GroupId))
                .Include(l => l.Lecturer)  // Подгружаем информацию о преподавателе
                .Include(l => l.Groups)    // Подгружаем связанные группы
                .ToListAsync();

            _logger.LogInformation($"Найдено {lessons.Count} уроков для группы {student.Group.Name}");

            return View(lessons);
        }



        // Страница урока
        public async Task<IActionResult> LessonPage(int id)
        {
            _logger.LogInformation($"Запрос к странице урока с ID: {id}");

            if (id == 0)
            {
                _logger.LogError("Ошибка: Передан неверный ID урока (ID = 0).");
                return NotFound("ID урока не может быть 0.");
            }

            var lesson = await _context.Lessons
                .Include(l => l.Materials)
                .Include(l => l.Tests)
                .Include(l => l.Lecturer)  // Подгружаем преподавателя
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                _logger.LogWarning($"Урок с ID: {id} не найден.");
                return NotFound("Урок не найден.");
            }

            _logger.LogInformation($"Урок с ID: {id} найден. Количество материалов: {lesson.Materials.Count}, Количество тестов: {lesson.Tests.Count}");

            return View(lesson);
        }


        // Страница материала
        public async Task<IActionResult> MaterialPage(int id)
        {
            _logger.LogInformation($"Запрос к странице материала с ID: {id}");

            if (id == 0)
            {
                _logger.LogError("Ошибка: Передан неверный ID материала (ID = 0).");
                return NotFound("ID материала не может быть 0.");
            }

            var material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                _logger.LogWarning($"Материал с ID: {id} не найден.");
                return NotFound("Материал не найден.");
            }

            _logger.LogInformation($"Материал с ID: {id} найден.");

            return View(material);
        }


        // Страница теста
        public async Task<IActionResult> TestPage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int studentId;

            if (!int.TryParse(userId, out studentId))
            {
                return BadRequest("Неверный формат ID студента.");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("Студент не найден.");
            }

            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound("Тест не найден.");
            }

            // Проверка состояния теста
            if (!test.IsOpen)
            {
                // Тест закрыт, показываем страницу с уведомлением
                return View("TestClosed");
            }

            // Проверка, есть ли уже результат теста у студента
            var existingResult = await _context.TestResults
                .FirstOrDefaultAsync(r => r.StudentId == student.Id && r.TestId == id);

            if (existingResult != null)
            {
                // Если результат уже существует, перенаправляем на страницу с уведомлением
                return RedirectToAction("TestAlreadyTaken");
            }

            return View(test);
        }




        [HttpPost]
        public async Task<IActionResult> SubmitTest(TestAnswerViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == int.Parse(userId));

            if (student == null)
            {
                return NotFound("Студент не найден.");
            }

            var test = await _context.Tests
                .Include(t => t.Questions) // Загружаем связанные вопросы
                .FirstOrDefaultAsync(t => t.Id == model.TestId);

            if (test == null)
            {
                return NotFound("Тест не найден.");
            }

            if (model.TestId <= 0)
            {
                return BadRequest("Неверный ID теста.");
            }

            // Подсчет баллов
            int score = 0;
            foreach (var question in test.Questions)
            {
                if (model.Answers.ContainsKey(question.Id) && model.Answers[question.Id] == question.CorrectAnswer)
                {
                    score++;
                }
            }

            // Сохранение результата
            var testResult = new TestResult
            {
                StudentId = student.Id,
                TestId = test.Id,
                Score = score
            };

            _context.TestResults.Add(testResult);
            await _context.SaveChangesAsync();

            return RedirectToAction("Result", new { testId = test.Id });
        }

        public async Task<IActionResult> Result(int testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == int.Parse(userId));

            if (student == null)
            {
                return NotFound("Студент не найден.");
            }

            // Получаем результат теста для студента
            var testResult = await _context.TestResults
                .Include(r => r.Test)
                .ThenInclude(t => t.Questions) // Загружаем связанные вопросы
                .Include(r => r.Test.Lesson)  // Загружаем урок, связанный с тестом
                .FirstOrDefaultAsync(r => r.StudentId == student.Id && r.TestId == testId);

            if (testResult == null)
            {
                return NotFound("Результат теста не найден.");
            }

            // Создаем ViewModel для передачи в представление
            var viewModel = new StudentTestResultViewModel
            {
                TestTitle = testResult.Test.Title,
                LessonName = testResult.Test.Lesson.Name,
                Score = testResult.Score,
                MaxScore = testResult.Test?.Questions?.Count ?? 0 // Используем null-оператор для безопасности
            };

            // Логирование для отладки
            _logger.LogInformation($"Test ID: {testId}, Score: {testResult.Score}, MaxScore: {viewModel.MaxScore}");

            return View(viewModel);
        }


        public IActionResult TestAlreadyTaken()
        {
            return View(); // Страница уведомления, что тест уже пройден
        }



        public async Task<IActionResult> MyResults()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == int.Parse(userId));

            if (student == null)
            {
                return NotFound("Студент не найден.");
            }

            // Извлекаем результаты тестов студента
            var results = await _context.TestResults
                .Include(r => r.Test)
                .ThenInclude(t => t.Questions) // Загружаем вопросы теста
                .Include(r => r.Test.Lesson)  // Загружаем уроки
                .Where(r => r.StudentId == student.Id)
                .ToListAsync();


            // Создаем список для передачи в представление
            var viewModel = results.Select(r => new StudentTestResultViewModel
            {
                TestTitle = r.Test.Title,
                LessonName = r.Test.Lesson.Name,
                Score = r.Score,
                MaxScore = r.Test.Questions.Count // Здесь мы берем количество вопросов
            }).ToList();


            return View(viewModel);
        }


    }
}
