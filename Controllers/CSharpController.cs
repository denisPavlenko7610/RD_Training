using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace RD_Training.Controllers
{
    public class CSharpController : Controller
    {
        private readonly TaskService _taskService;
        private readonly CodeExecutor _codeExecutor;
        private readonly ResultValidator _resultValidator;
        private readonly ApplicationDbContext _context;

        private const string DemoUserId = "demo-user"; // Fixed user ID for demonstration purposes

        public CSharpController(TaskService taskService, CodeExecutor codeExecutor, ResultValidator resultValidator, ApplicationDbContext context)
        {
            _taskService = taskService;
            _codeExecutor = codeExecutor;
            _resultValidator = resultValidator;
            _context = context;
        }

        public async Task<IActionResult> Index(int taskId = 0)
        {
            // Find the first incomplete task for the user
            var incompleteTask = await _context.UserTaskProgresses
                .Where(u => u.UserId == DemoUserId && !u.IsCompleted)
                .OrderBy(u => u.TaskId)
                .FirstOrDefaultAsync();

            if (incompleteTask != null)
            {
                taskId = incompleteTask.TaskId;
            }

            var task = _taskService.GetCSharpTask(taskId);
            if (task == null)
            {
                return RedirectToAction("Complete");
            }

            var userProgress = await _context.UserTaskProgresses
                .Where(u => u.UserId == DemoUserId && u.TaskId == taskId)
                .FirstOrDefaultAsync();

            ViewData["TaskDescription"] = task.Description;
            ViewData["TaskId"] = taskId;
            ViewData["CSharpCode"] = "";
            ViewData["CSharpInput"] = "";
            ViewData["IsCSharpCorrect"] = userProgress?.IsCompleted ?? false;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RunCSharpCode(int taskId, string code, string input)
        {
            var task = _taskService.GetCSharpTask(taskId);
            if (task == null)
            {
                return RedirectToAction("Complete");
            }

            ViewData["TaskDescription"] = task.Description;
            ViewData["TaskId"] = taskId;
            ViewData["CSharpCode"] = code;
            ViewData["CSharpInput"] = input;

            if (string.IsNullOrEmpty(code))
            {
                ViewData["CSharpResult"] = "Code cannot be empty!";
                return View("Index");
            }

            var result = await _codeExecutor.ExecuteCSharpCode(code, input);
            var isCorrect = _resultValidator.Validate(result, task.ExpectedOutput);

            ViewData["CSharpResult"] = result;
            ViewData["IsCSharpCorrect"] = isCorrect;

            var userProgress = await _context.UserTaskProgresses
                .Where(u => u.UserId == DemoUserId && u.TaskId == taskId)
                .FirstOrDefaultAsync();

            if (userProgress == null)
            {
                userProgress = new UserTaskProgress
                {
                    UserId = DemoUserId,
                    TaskId = taskId,
                    IsCompleted = isCorrect
                };
                _context.UserTaskProgresses.Add(userProgress);
            }
            else
            {
                userProgress.IsCompleted = isCorrect;
                _context.UserTaskProgresses.Update(userProgress);
            }

            await _context.SaveChangesAsync();

            if (isCorrect)
            {
                // Redirect to the Index action to check for the next task
                return RedirectToAction("Index", new { taskId = taskId + 1 });
            }

            return View("Index");
        }

        public IActionResult Complete()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Restart()
        {
            var userProgresses = _context.UserTaskProgresses.Where(u => u.UserId == DemoUserId);
            _context.UserTaskProgresses.RemoveRange(userProgresses);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}