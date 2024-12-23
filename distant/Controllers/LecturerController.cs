using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using distant.Data;
using distant.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using distant.ViewModels;

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



        public async Task<IActionResult> LessonPage(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Groups)
                .Include(l => l.Materials)
                .Include(l => l.Tests)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.LecturerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            ViewBag.Groups = new MultiSelectList(_context.Groups.ToList(), "Id", "Name", lesson.Groups.Select(g => g.Id));
            return View(lesson);
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














        [HttpPost]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try
            {
                var lesson = await _context.Lessons.FindAsync(id);

                if (lesson == null)
                {
                    return NotFound(); // Если урок не найден
                }

                // Удаляем урок
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Lesson with ID {id} successfully deleted.");
                return RedirectToAction(nameof(Lessons));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting lesson with ID {id}: {ex.Message}");
                ModelState.AddModelError("", "Failed to delete the lesson.");
                return RedirectToAction(nameof(Lessons));
            }
        }

















        public async Task<IActionResult> MaterialPage(int id)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }





        // Добавление материалов к уроку
        // Добавление материала (GET)
        public IActionResult AddMaterial(int lessonId)
        {
            // Передаем lessonId в представление через ViewBag
            ViewBag.LessonId = lessonId;
            return View(new Material { LessonId = lessonId });
        }



        // Добавление материала (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(Material material)
        {
            // Логирование для отладки
            Console.WriteLine($"Title: {material.Title}, Content: {material.Content}, LessonId: {material.LessonId}");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Ошибка: {error.ErrorMessage}");
                }
                return View(material); // Возвращаем представление с ошибками
            }

            // Находим урок по переданному ID
            var lesson = await _context.Lessons.FindAsync(material.LessonId);
            if (lesson == null)
            {
                ModelState.AddModelError("LessonId", "Урок не найден.");
                return View(material); // Если урок не найден, возвращаем ошибку
            }

            // Добавляем материал
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            // Перенаправляем на страницу урока
            return RedirectToAction(nameof(LessonPage), new { id = material.LessonId });
        }







        // Редактирование материала (GET)
        public async Task<IActionResult> EditMaterial(int id)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }



        // Редактирование материала (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(Material material)
        {
            if (!ModelState.IsValid)
            {
                // Если модель невалидна, логируем ошибки и возвращаем форму с ошибками
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Ошибка: {error.ErrorMessage}");
                }

                // Возвращаем материал в представление с ошибками
                return View(material);
            }

            // Если все хорошо, обновляем материал
            _context.Materials.Update(material);
            await _context.SaveChangesAsync();

            return RedirectToAction("LessonPage", new { id = material.LessonId });
        }




        [HttpPost]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(LessonPage), new { id = material?.LessonId });
        }






        public async Task<IActionResult> TestPage(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
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
            Console.WriteLine($"Received Title: {test.Title}, LessonId: {test.LessonId}");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Tests.Add(test);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Test added successfully to the database.");
                    return RedirectToAction(nameof(LessonPage), new { id = test.LessonId });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving test: {ex.Message}");
                    ModelState.AddModelError("", "Failed to save test to the database.");
                }
            }
            else
            {
                Console.WriteLine("Model state is invalid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
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
            if (ModelState.IsValid)
            {
                _context.Tests.Update(test);
                await _context.SaveChangesAsync();
                return RedirectToAction("TestPage", new { id = test.Id });
            }

            return View(test);
        }




        [HttpPost]
        public async Task<IActionResult> DeleteTest(int id)
        {
            try
            {
                var test = await _context.Tests.FindAsync(id);

                if (test == null)
                {
                    return NotFound(); // Если тест не найден
                }

                // Удаляем тест
                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Test with ID {id} successfully deleted.");
                return RedirectToAction(nameof(LessonPage), new { id = test.LessonId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting test with ID {id}: {ex.Message}");
                ModelState.AddModelError("", "Failed to delete the test.");
                return RedirectToAction(nameof(LessonPage), new { id = (await _context.Tests.FindAsync(id))?.LessonId });
            }
        }






        public IActionResult AddQuestion(int testId)
        {
            return View(new Question { TestId = testId });
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(Question question)
        {
            if (!ModelState.IsValid)
            {
                // Возвращаем форму с ошибками
                return View(question);
            }

            try
            {
                // Добавление вопроса в базу данных
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                // Перенаправляем на страницу теста
                return RedirectToAction("TestPage", new { id = question.TestId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                ModelState.AddModelError("", "Не удалось сохранить вопрос. Повторите попытку.");
                return View(question);
            }
        }



        public async Task<IActionResult> TestResults(int testId)
        {
            // Получаем тест по ID
            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
            {
                return NotFound();
            }

            // Получаем результаты студентов для этого теста
            var results = await _context.TestResults
                .Include(tr => tr.Student)
                .Include(tr => tr.Student.Group)  // Загружаем информацию о группе студента
                .Where(tr => tr.TestId == testId)
                .ToListAsync();

            // Создаем ViewModel для отображения
            var viewModel = results.Select(r => new TestResultViewModel
            {
                StudentId = r.Student.Id,
                FirstName = r.Student.FirstName,
                LastName = r.Student.LastName,
                GroupName = r.Student.Group?.Name,
                Score = r.Score,
                MaxScore = test.Questions.Count // Количество вопросов в тесте
            }).ToList();

            // Передаем данные в представление
            ViewBag.TestTitle = test.Title;
            return View(viewModel);
        }







    }
}
