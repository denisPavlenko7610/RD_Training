using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using RD_Training.Models;

namespace RD_Training.Controllers
{
    public class CppController : Controller
    {
        private readonly TaskService _taskService;
        private readonly CodeExecutor _codeExecutor;
        private readonly ResultValidator _resultValidator;
        private readonly ApplicationDbContext _context;

        private const string DemoUserId = "demo-user"; // Fixed user ID for demonstration purposes

        public CppController(TaskService taskService, CodeExecutor codeExecutor, ResultValidator resultValidator, ApplicationDbContext context)
        {
            _taskService = taskService;
            _codeExecutor = codeExecutor;
            _resultValidator = resultValidator;
            _context = context;
        }

        public async Task<IActionResult> Index(int taskId = 0)
        {
            // Retrieve the last completed task ID from the database
            var userProgress = await _context.UserProgresses
                .Where(u => u.UserId == DemoUserId)
                .FirstOrDefaultAsync();

            if (userProgress != null)
            {
                taskId = userProgress.LastCompletedTaskId + 1;
            }

            var task = _taskService.GetCppTask(taskId);
            if (task == null)
            {
                return RedirectToAction("Complete");
            }

            var userTaskProgress = await _context.UserTaskProgresses
                .Where(u => u.UserId == DemoUserId && u.TaskId == taskId)
                .FirstOrDefaultAsync();

            ViewData["TaskDescription"] = task.Description;
            ViewData["TaskId"] = taskId;
            ViewData["CppCode"] = "";
            ViewData["CppInput"] = "";
            ViewData["IsCppCorrect"] = userTaskProgress?.IsCompleted ?? false;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RunCppCode(int taskId, string code, string input)
        {
            var task = _taskService.GetCppTask(taskId);
            if (task == null)
            {
                return RedirectToAction("Complete");
            }

            ViewData["TaskDescription"] = task.Description;
            ViewData["TaskId"] = taskId;
            ViewData["CppCode"] = code;
            ViewData["CppInput"] = input;

            if (string.IsNullOrEmpty(code))
            {
                ViewData["CppResult"] = "Code cannot be empty!";
                return View("Index");
            }

            var result = _codeExecutor.ExecuteCppCode(code, input);
            var isCorrect = _resultValidator.Validate(result, task.ExpectedOutput);

            ViewData["CppResult"] = result;
            ViewData["IsCppCorrect"] = isCorrect;

            var userTaskProgress = await _context.UserTaskProgresses
                .Where(u => u.UserId == DemoUserId && u.TaskId == taskId)
                .FirstOrDefaultAsync();

            if (userTaskProgress == null)
            {
                userTaskProgress = new UserTaskProgress
                {
                    UserId = DemoUserId,
                    TaskId = taskId,
                    IsCompleted = isCorrect
                };
                _context.UserTaskProgresses.Add(userTaskProgress);
            }
            else
            {
                userTaskProgress.IsCompleted = isCorrect;
                _context.UserTaskProgresses.Update(userTaskProgress);
            }

            if (isCorrect)
            {
                // Update the last completed task ID in the database
                var userProgress = await _context.UserProgresses
                    .Where(u => u.UserId == DemoUserId)
                    .FirstOrDefaultAsync();

                if (userProgress == null)
                {
                    userProgress = new UserProgress
                    {
                        UserId = DemoUserId,
                        LastCompletedTaskId = taskId
                    };
                    _context.UserProgresses.Add(userProgress);
                }
                else
                {
                    userProgress.LastCompletedTaskId = taskId;
                    _context.UserProgresses.Update(userProgress);
                }

                await _context.SaveChangesAsync();

                // Redirect to the Index action to check for the next task
                return RedirectToAction("Index");
            }

            await _context.SaveChangesAsync();
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
            var userProgress = await _context.UserProgresses.Where(u => u.UserId == DemoUserId).FirstOrDefaultAsync();

            if (userProgress != null)
            {
                userProgress.LastCompletedTaskId = -1;
                _context.UserProgresses.Update(userProgress);
            }

            _context.UserTaskProgresses.RemoveRange(userProgresses);
            await _context.SaveChangesAsync();

            // Redirect to the Index action to show the first task
            return RedirectToAction("Index", new { taskId = 0 });
        }
    }
}