using BenchmarkDotNet.Running;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Perf.BenchMarks;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Perf
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            // Test your "test" code here
            //SubRoutineBenchMarks test = new SubRoutineBenchMarks();
            //Console.WriteLine(test.ImplementsInterface());
            //Console.WriteLine(test.ImplementsInterfaceIsAssignableFrom());
            //Console.WriteLine(test.ImplementsInterfaceGetInterfaces());
            //Console.WriteLine(test.ImplementsInterfaceViaLinq());

            //Console.WriteLine($"OLD: {typeof(IWeb).BaseImplementsInterface(typeof(IQueryableDataModel))} | {typeof(IWeb).ImplementsInterface(typeof(IDataModelGet<IWeb>))} | {typeof(IDataModel<IWeb>).ImplementsInterface(typeof(IDataModelWithContext))} | {typeof(IDataModel<>).ImplementsInterface(typeof(IDataModelWithContext))} | {typeof(Web).ImplementsInterface(typeof(IDataModel<>))} " );
            //Console.WriteLine($"NEW: {typeof(IWeb).NewImplementsInterface(typeof(IQueryableDataModel))} | {typeof(IWeb).NewImplementsInterface(typeof(IDataModelGet<IWeb>))} | {typeof(IDataModel<IWeb>).NewImplementsInterface(typeof(IDataModelWithContext))} | {typeof(IDataModel<>).NewImplementsInterface(typeof(IDataModelWithContext))} | {typeof(Web).NewImplementsInterface(typeof(IDataModel<>))} ");

            string baseValue = JsonSerializer.Serialize(new SubRoutineBenchMarks().BaseAsExpando(), typeof(ExpandoObject));
            Console.WriteLine(baseValue);
            string newValue = JsonSerializer.Serialize(new SubRoutineBenchMarks().NewAsExpando(), typeof(ExpandoObject));
            Console.WriteLine(newValue);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Are equal: {baseValue == newValue}");
            Console.ResetColor();


#else
            // Run a benchmark on your test code
            // dotnet run -c release from a console window

            var summary = BenchmarkRunner.Run<SubRoutineBenchMarks>();
#endif
        }
    }
}
