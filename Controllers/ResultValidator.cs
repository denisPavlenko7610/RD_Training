namespace RD_Training.Controllers;
public class ResultValidator
{
    public bool Validate(string result, string expectedOutput)
    {
        return result.Trim() == expectedOutput.Trim();
    }
}