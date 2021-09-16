namespace PnP.Core.Perf
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            // Test your "test" code here

            //JsonMapping jm = new JsonMapping();           
            //Console.WriteLine(jm.CamelCase());
            //Console.WriteLine(jm.CamelCaseSpan());

#else
            // Run a benchmark on your test code
            // dotnet run -c release from a console window

            var summary = BenchmarkRunner.Run<SubRoutineBenchMarks>();
#endif
        }
    }
}
