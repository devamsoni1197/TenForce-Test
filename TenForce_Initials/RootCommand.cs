//var builder = WebApplication.CreateBuilder(args);

using Microsoft.CodeAnalysis.Options;

internal class RootCommand
{
    private string v;

    public RootCommand(string v)
    {
        this.v = v;
    }

    internal void AddOption(Option<string> outputOption)
    {
        throw new NotImplementedException();
    }

    internal async Task InvokeAsync(string[] args)
    {
        throw new NotImplementedException();
    }

    internal void SetHandler(Func<string, bool, Task> value, Option<string> outputOption, Option<bool> estimateFlag)
    {
        throw new NotImplementedException();
    }
}