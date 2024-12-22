using distant.Data;
using distant.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace distant.Controllers
{
    public class TestController : Controller
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tests = _context.Tests.Include(t => t.Lesson).ToList();
            return View(tests);
        }

        public IActionResult Start(int id)
        {
            var test = _context.Tests.Include(t => t.Questions).FirstOrDefault(t => t.Id == id);
            return View(test);
        }

        [HttpPost]
        public IActionResult Submit(int testId, Dictionary<int, string> answers)
        {
            var test = _context.Tests.Include(t => t.Questions).FirstOrDefault(t => t.Id == testId);
            var score = 0;

            foreach (var question in test.Questions)
            {
                if (answers.TryGetValue(question.Id, out var answer) && answer == question.CorrectAnswer)
                {
                    score++;
                }
            }

            var studentId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var result = new TestResult
            {
                StudentId = studentId,
                TestId = testId,
                Score = score
            };
            _context.TestResults.Add(result);
            _context.SaveChanges();

            return RedirectToAction("Result", new { id = result.Id });
        }

        public IActionResult Result(int id)
        {
            var result = _context.TestResults.Include(r => r.Test).FirstOrDefault(r => r.Id == id);
            return View(result);
        }
    }
}
