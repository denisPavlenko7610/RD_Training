using Microsoft.AspNetCore.Mvc;

namespace RD_Training.Controllers;

using Microsoft.AspNetCore.Mvc;

public class CppController : Controller
{
    private readonly TaskService _taskService;
    private readonly CodeExecutor _codeExecutor;
    private readonly ResultValidator _resultValidator;

    public CppController(TaskService taskService, CodeExecutor codeExecutor, ResultValidator resultValidator)
    {
        _taskService = taskService;
        _codeExecutor = codeExecutor;
        _resultValidator = resultValidator;
    }

    public IActionResult Index(int taskId = 0)
    {
        var task = _taskService.GetCppTask(taskId);
        if (task == null)
        {
            return RedirectToAction("Complete");
        }

        ViewData["TaskDescription"] = task.Description;
        ViewData["TaskId"] = taskId;
        ViewData["CppCode"] = "";
        ViewData["CppInput"] = "";
        return View();
    }

    [HttpPost]
    public IActionResult RunCppCode(int taskId, string code, string input)
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

        if (isCorrect)
        {
            return RedirectToAction("Index", new { taskId = taskId + 1 });
        }

        return View("Index");
    }

    public IActionResult Complete()
    {
        return View();
    }
}