using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["CSharpCode"] = "";
        ViewData["CSharpInput"] = "";
        ViewData["CppCode"] = "";
        ViewData["CppInput"] = "";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RunCSharpCode(string code, string input)
    {
        if (string.IsNullOrEmpty(code))
        {
            ViewData["CSharpResult"] = "Code cannot be empty!";
            ViewData["CSharpCode"] = code;
            ViewData["CSharpInput"] = input;
            return View("Index");
        }

        if (string.IsNullOrEmpty(input))
        {
            input = string.Empty;
        }

        string scriptCode;
        if (code.Contains("class Program") && code.Contains("static void Main"))
        {
            // Full program structure
            scriptCode = code;
        }
        else
        {
            // Simple script snippet
            scriptCode = $@"
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
        }

        try
        {
            var result = await CSharpScript.EvaluateAsync<string>(
                scriptCode,
                ScriptOptions.Default
                    .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                        .Select(a => MetadataReference.CreateFromFile(a.Location)))
                    .WithImports("System", "System.Linq", "System.IO")
            );

            ViewData["CSharpResult"] = result;
            ViewData["CSharpCode"] = code;
            ViewData["CSharpInput"] = input;
            return View("Index");
        }
        catch (CompilationErrorException ex)
        {
            ViewData["CSharpResult"] = $"Compilation Failed:\n{string.Join("\n", ex.Diagnostics)}";
            ViewData["CSharpCode"] = code;
            ViewData["CSharpInput"] = input;
            return View("Index");
        }
        catch (Exception ex)
        {
            ViewData["CSharpResult"] = $"An error occurred: {ex.Message}";
            ViewData["CSharpCode"] = code;
            ViewData["CSharpInput"] = input;
            return View("Index");
        }
    }

    [HttpPost]
    public IActionResult RunCppCode(string code, string input)
    {
        if (string.IsNullOrEmpty(code))
        {
            ViewData["CppResult"] = "Code cannot be empty!";
            ViewData["CppCode"] = code;
            ViewData["CppInput"] = input;
            return View("Index");
        }

        if (string.IsNullOrEmpty(input))
        {
            input = string.Empty;
        }

        string cppCode = code.Contains("int main") ? code : $@"
#include <iostream>
#include <string>
using namespace std;

int main() {{
    string input = ""{input.Replace("\"", "\\\"")}"";
    {code}
    return 0;
}}
";

        string cppFileName = Path.GetTempFileName() + ".cpp";
        string exeFileName = Path.ChangeExtension(cppFileName, ".exe");
        System.IO.File.WriteAllText(cppFileName, cppCode);

        try
        {
            Process compileProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "g++",
                    Arguments = $"-o \"{exeFileName}\" \"{cppFileName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            compileProcess.Start();
            string compileOutput = compileProcess.StandardOutput.ReadToEnd();
            string compileErrors = compileProcess.StandardError.ReadToEnd();
            compileProcess.WaitForExit();

            if (compileProcess.ExitCode != 0)
            {
                ViewData["CppResult"] = $"Compilation Failed:\n{compileErrors}";
                ViewData["CppCode"] = code;
                ViewData["CppInput"] = input;
                return View("Index");
            }

            Process runProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exeFileName,
                    Arguments = input,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            runProcess.Start();
            string runOutput = runProcess.StandardOutput.ReadToEnd();
            string runErrors = runProcess.StandardError.ReadToEnd();
            runProcess.WaitForExit();

            ViewData["CppResult"] = runOutput + runErrors;
            ViewData["CppCode"] = code;
            ViewData["CppInput"] = input;
            return View("Index");
        }
        catch (Exception ex)
        {
            ViewData["CppResult"] = $"An error occurred: {ex.Message}";
            ViewData["CppCode"] = code;
            ViewData["CppInput"] = input;
            return View("Index");
        }
        finally
        {
            if (System.IO.File.Exists(cppFileName))
            {
                System.IO.File.Delete(cppFileName);
            }
            if (System.IO.File.Exists(exeFileName))
            {
                System.IO.File.Delete(exeFileName);
            }
        }
    }
}