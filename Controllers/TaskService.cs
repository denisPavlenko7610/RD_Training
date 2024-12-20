using RD_Training.Models;

namespace RD_Training.Controllers;

using System.Collections.Generic;
using System.Linq;

public class TaskService
{
    private readonly List<TaskModel> CSharpTasks = new List<TaskModel>
    {
        new TaskModel { Id = 0, Description = "Write a function that returns the string 'Hello, World!'.", ExpectedOutput = "Hello, World!" },
        new TaskModel { Id = 1, Description = "Write a function that returns the sum of two integers 5 + 4.", ExpectedOutput = "9" },
        new TaskModel { Id = 2, Description = "Write a function that returns the product of two integers 5 * 4.", ExpectedOutput = "20" },
        new TaskModel { Id = 3, Description = "Write a function that returns the quotient of two integers 10 / 2.", ExpectedOutput = "5" },
        new TaskModel { Id = 4, Description = "Write a function that returns the remainder of two integers 10 % 3.", ExpectedOutput = "1" },
        new TaskModel { Id = 5, Description = "Write a function that returns the result of 2 raised to the power of 5.", ExpectedOutput = "32" },
        new TaskModel { Id = 6, Description = "Write a function that returns the sum of all integers from 1 to 100.", ExpectedOutput = "5050" },
        new TaskModel { Id = 7, Description = "Write a function that returns the sum of all even integers from 1 to 100.", ExpectedOutput = "2550" },
        new TaskModel { Id = 8, Description = "Write a function that returns the sum of all odd integers from 1 to 100.", ExpectedOutput = "2500" },
        new TaskModel { Id = 9, Description = "Write a function that returns the factorial of 5.", ExpectedOutput = "120" },
        new TaskModel { Id = 10, Description = "Write a function that returns the Fibonacci number at index 10.", ExpectedOutput = "55" },
        new TaskModel { Id = 11, Description = "Write a function that returns the greatest common divisor of 30 and 45.", ExpectedOutput = "15" },
        new TaskModel { Id = 12, Description = "Write a function that returns the least common multiple of 12 and 18.", ExpectedOutput = "36" },
        new TaskModel { Id = 13, Description = "Write a function that returns the square root of 144.", ExpectedOutput = "12" },
        new TaskModel { Id = 14, Description = "Write a function that returns the sine of 30 degrees.", ExpectedOutput = "0.5" },
        new TaskModel { Id = 15, Description = "Write a function that returns the cosine of 60 degrees.", ExpectedOutput = "0.5" },
        // Add more C# tasks here
    };

    private readonly List<TaskModel> CppTasks = new List<TaskModel>
    {
        new TaskModel { Id = 0, Description = "Write a function that returns the string 'Hello, World!'.", ExpectedOutput = "Hello, World!" },
        new TaskModel { Id = 1, Description = "Write a function that returns the product of two integers.", ExpectedOutput = "Function to return the product of two integers" },
        new TaskModel { Id = 2, Description = "Write a function that returns the quotient of two integers.", ExpectedOutput = "Function to return the quotient of two integers" },
        new TaskModel { Id = 3, Description = "Write a function that returns the remainder of two integers.", ExpectedOutput = "Function to return the remainder of two integers" },
        new TaskModel { Id = 4, Description = "Write a function that returns the result of 2 raised to the power of 5.", ExpectedOutput = "32" },
        new TaskModel { Id = 5, Description = "Write a function that returns the sum of all integers from 1 to 100.", ExpectedOutput = "5050" },
        new TaskModel { Id = 6, Description = "Write a function that returns the sum of all even integers from 1 to 100.", ExpectedOutput = "2550" },
        new TaskModel { Id = 7, Description = "Write a function that returns the sum of all odd integers from 1 to 100.", ExpectedOutput = "2500" },
        new TaskModel { Id = 8, Description = "Write a function that returns the factorial of 5.", ExpectedOutput = "120" },
        new TaskModel { Id = 9, Description = "Write a function that returns the Fibonacci number at index 10.", ExpectedOutput = "55" },
        new TaskModel { Id = 10, Description = "Write a function that returns the greatest common divisor of 30 and 45.", ExpectedOutput = "15" },
        new TaskModel { Id = 11, Description = "Write a function that returns the least common multiple of 12 and 18.", ExpectedOutput = "36" },
        new TaskModel { Id = 12, Description = "Write a function that returns the square root of 144.", ExpectedOutput = "12" },
        new TaskModel { Id = 13, Description = "Write a function that returns the sine of 30 degrees.", ExpectedOutput = "0.5" },
        new TaskModel { Id = 14, Description = "Write a function that returns the cosine of 60 degrees.", ExpectedOutput = "0.5" },
        // Add more C++ tasks here
    };

    public TaskModel GetCSharpTask(int id)
    {
        return CSharpTasks.FirstOrDefault(t => t.Id == id);
    }

    public TaskModel GetCppTask(int id)
    {
        return CppTasks.FirstOrDefault(t => t.Id == id);
    }
}