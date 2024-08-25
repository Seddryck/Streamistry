using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Testing;
internal class ConsoleOutput : IDisposable
{
    private readonly StringWriter stringWriter = new();
    private readonly TextWriter originalOutput = Console.Out;

    public ConsoleOutput()
        => Console.SetOut(stringWriter);

    public string GetOuput()
        => stringWriter.ToString();

    public int CountSubstring(string value)
    {
        var text = GetOuput();
        int count = 0, minIndex = text.IndexOf(value, 0);
        while (minIndex != -1)
        {
            minIndex = text.IndexOf(value, minIndex + value.Length);
            count++;
        }
        return count;
    }

    public void Dispose()
    {
        Console.SetOut(originalOutput);
        stringWriter.Dispose();
    }
}
