using Microsoft.CodeAnalysis;

namespace RD_Training.Controllers;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class CodeExecutor
{
    private static readonly string CppFileName = Path.Combine(Path.GetTempPath(), "user_code.cpp");
    private static readonly string ExeFileName = Path.Combine(Path.GetTempPath(), "user_code.exe");

    public async Task<string> ExecuteCSharpCode(string code, string input)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));
        if (input == null) input = string.Empty;

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
                return ""An error occurred: "" + ex.Message + ""\n"" + ex.StackTrace;
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

            return result?.Trim() ?? "No output was produced.";
        }
        catch (CompilationErrorException ex)
        {
            return $"Compilation Failed:\n{string.Join("\n", ex.Diagnostics)}";
        }
        catch (Exception ex)
        {
            return $"An error occurred while executing the code:\n{ex.Message}\n{ex.StackTrace}";
        }
    }

    public string ExecuteCppCode(string code, string input)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));
        if (input == null) input = string.Empty;

        // Wrap user code in a main function if it doesn't already contain one
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

        try
        {
            // Clear the contents of the C++ file
            System.IO.File.WriteAllText(CppFileName, cppCode);

            // Compile the C++ code using g++
            Process compileProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "g++",
                    Arguments = $"-o \"{ExeFileName}\" \"{CppFileName}\"",
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
                return $"Compilation Failed:\n{compileErrors}";
            }

            // Run the compiled executable
            Process runProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ExeFileName,
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

            return runOutput.Trim() + runErrors.Trim();
        }
        catch (Exception ex)
        {
            return $"An error occurred while executing the code:\n{ex.Message}\n{ex.StackTrace}";
        }
    }
}