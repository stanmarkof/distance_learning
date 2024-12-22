using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using distant.Data;
using distant.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace distant.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly AppDbContext _context;

        public LecturerController(AppDbContext context)
        {
            _context = context;
        }

        // Просмотр уроков лектора
        public async Task<IActionResult> Lessons()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var lessons = await _context.Lessons
                .Include(l => l.Groups)  // Загружаем связанные группы
                .Where(l => l.LecturerId == int.Parse(userId))
                .ToListAsync();

            return View(lessons);
        }



        // Создание урока (GET)
        public IActionResult CreateLesson()
        {
            var groups = _context.Groups.ToList();
            ViewBag.Groups = new SelectList(groups, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateLesson(Lesson lesson, int[] groupIds)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                lesson.LecturerId = userId;

                // Связываем группы с уроком
                if (groupIds != null && groupIds.Length > 0)
                {
                    lesson.Groups = await _context.Groups
                        .Where(g => groupIds.Contains(g.Id))
                        .ToListAsync();
                }

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Lessons));
            }

            // Если модель невалидна, возвращаем форму с ошибками
            var groups = _context.Groups.ToList();
            ViewBag.Groups = new SelectList(groups, "Id", "Name");
            return View(lesson);
        }






        // Редактирование урока (GET)
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.LecturerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            // Передаем lesson в представление для редактирования
            ViewBag.Groups = new MultiSelectList(_context.Groups.ToList(), "Id", "Name", lesson.Groups.Select(g => g.Id));
            return View(lesson);  // Передаем объект lesson в представление
        }






        [HttpPost]
        public async Task<IActionResult> EditLesson(int id, Lesson updatedLesson, int[] groupIds)
        {
            // Логирование ошибок ModelState
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // Это выведет ошибку в консоль
                }

                // Выводим ошибки в случае неверной модели
                ViewBag.Groups = new MultiSelectList(_context.Groups.ToList(), "Id", "Name", groupIds);
                return View(updatedLesson);  // Возвращаем обновленную модель в форму
            }

            // Продолжение работы с моделью
            var lesson = await _context.Lessons
                .Include(l => l.Groups)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.LecturerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            lesson.Name = updatedLesson.Name;  // Обновляем имя
            lesson.Groups = await _context.Groups.Where(g => groupIds.Contains(g.Id)).ToListAsync();

            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Lessons));
        }














        // Удаление урока
        [HttpPost]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.LecturerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lessons));
        }




        // Добавление материалов к уроку
        public async Task<IActionResult> AddMaterial(int lessonId)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null || lesson.LecturerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            return View(new Material { LessonId = lessonId });
        }



        [HttpPost]
        public async Task<IActionResult> AddMaterial(Material material, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                material.FilePath = filePath;
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Lessons));
            }

            return View(material);
        }





        // Управление тестами
        public async Task<IActionResult> Tests(int lessonId)
        {
            var tests = await _context.Tests
                .Where(t => t.LessonId == lessonId)
                .ToListAsync();

            return View(tests);
        }

        public IActionResult CreateTest(int lessonId)
        {
            return View(new Test { LessonId = lessonId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTest(Test test)
        {
            if (ModelState.IsValid)
            {
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Tests), new { lessonId = test.LessonId });
            }
            return View(test);
        }

        public async Task<IActionResult> EditTest(int id)
        {
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.Id == id);
            return test != null ? View(test) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditTest(Test test)
        {
            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Tests), new { lessonId = test.LessonId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTest(int id)
        {
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.Id == id);
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Tests), new { lessonId = test.LessonId });
        }
    }
}
