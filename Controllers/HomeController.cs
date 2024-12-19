using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Initialize ViewData to retain input after submission
        ViewData["Code"] = "";
        ViewData["Input"] = "";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RunCode(string code, string input)
    {
        // Step 1: Null check for code and input
        if (string.IsNullOrEmpty(code))
        {
            ViewData["Result"] = "Code cannot be empty!";
            ViewData["Code"] = code; // Retain code input
            ViewData["Input"] = input; // Retain input field
            return View("Index");
        }

        if (string.IsNullOrEmpty(input))
        {
            input = string.Empty;
        }

        // Step 2: Build the script code dynamically
        string scriptCode = $@"
using System;
using System.IO;
public class UserCode {{
    public static string Run(string input) {{
        using (var sw = new StringWriter())
        {{
            Console.SetOut(sw);
            try
            {{
                {code}
            }}
            catch (Exception ex)
            {{
                return ""An error occurred: "" + ex.Message;
            }}
            return sw.ToString();
        }}
    }}
}}
return UserCode.Run(""{input.Replace("\"", "\\\"")}"");
";

        try
        {
            // Step 3: Execute the code using Roslyn
            var result = await CSharpScript.EvaluateAsync<string>(
                scriptCode,
                ScriptOptions.Default
                    .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                        .Select(a => MetadataReference.CreateFromFile(a.Location)))
                    .WithImports("System", "System.Linq", "System.IO")
            );

            // Step 4: Return the result
            ViewData["Result"] = result;
            ViewData["Code"] = code; // Retain code input
            ViewData["Input"] = input; // Retain input field
            return View("Index");
        }
        catch (CompilationErrorException ex)
        {
            // Handle errors from the compilation process
            ViewData["Result"] = $"Compilation Failed:\n{string.Join("\n", ex.Diagnostics)}";
            ViewData["Code"] = code; // Retain code input
            ViewData["Input"] = input; // Retain input field
            return View("Index");
        }
        catch (Exception ex)
        {
            // Catch any other unexpected errors
            ViewData["Result"] = $"An error occurred: {ex.Message}";
            ViewData["Code"] = code; // Retain code input
            ViewData["Input"] = input; // Retain input field
            return View("Index");
        }
    }
}