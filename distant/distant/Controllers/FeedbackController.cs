using Microsoft.AspNetCore.Mvc;
using distant.Models;
using distant.Data;
using Microsoft.EntityFrameworkCore;

namespace distant.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        // Форма для отправки обратной связи
        [HttpGet]
        public IActionResult SubmitFeedback()
        {
            return View();
        }

        // Обработка отправки обратной связи
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(Feedback model)
        {
            if (ModelState.IsValid)
            {
                model.DateSubmitted = DateTime.Now;

                // Сохраняем отзыв в базе данных
                _context.Feedbacks.Add(model);
                await _context.SaveChangesAsync();

            }

            return View(model);
        }
    }
}
