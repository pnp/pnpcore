using BenchmarkDotNet.Attributes;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Perf.BenchMarks
{
    /// <summary>
    /// Note: uses span/readonlyspan extensions coming from here
    /// https://github.com/ServiceStack/ServiceStack.Text/blob/master/src/ServiceStack.Text/StringSpanExtensions.cs
    /// </summary>
    [MemoryDiagnoser]
    public class SubRoutineBenchMarks
    {
        

        public SubRoutineBenchMarks()
        {
            
        }



        #region Test done, clear improvements and candidates to implement in PnP.Core

        /*
         
        |                  Method |        Mean |     Error |    StdDev | Ratio |  Gen 0 | Allocated |
        |------------------------ |------------:|----------:|----------:|------:|-------:|----------:|
        | BaseImplementsInterface | 40,035.4 ns | 314.50 ns | 294.19 ns | 1.000 | 0.1221 |     913 B |
        |  NewImplementsInterface |    173.1 ns |   1.01 ns |   0.95 ns | 0.004 | 0.0598 |     376 B |

        */
        //[Benchmark(Baseline = true)]
        //public void OldImplementsInterface()
        //{
        //    typeof(IWeb).BaseImplementsInterface(typeof(IQueryableDataModel));
        //    typeof(IWeb).BaseImplementsInterface(typeof(IDataModelGet<IWeb>));
        //    typeof(IDataModel<IWeb>).BaseImplementsInterface(typeof(IDataModelWithContext));
        //    typeof(IDataModel<>).BaseImplementsInterface(typeof(IDataModelWithContext));
        //    typeof(Web).BaseImplementsInterface(typeof(IDataModel<>));
        //}

        //[Benchmark]
        //public void NewImplementsInterface()
        //{
        //    typeof(IWeb).NewImplementsInterface(typeof(IQueryableDataModel));
        //    typeof(IWeb).NewImplementsInterface(typeof(IDataModelGet<IWeb>));
        //    typeof(IDataModel<IWeb>).NewImplementsInterface(typeof(IDataModelWithContext));
        //    typeof(IDataModel<>).NewImplementsInterface(typeof(IDataModelWithContext));
        //    typeof(Web).NewImplementsInterface(typeof(IDataModel<>));
        //}

        /*
        |        Method |     Mean |    Error |   StdDev | Ratio |  Gen 0 | Allocated |
        |-------------- |---------:|---------:|---------:|------:|-------:|----------:|
        |     CamelCase | 25.28 ns | 0.162 ns | 0.151 ns |  1.00 | 0.0191 |     120 B |
        | CamelCaseSpan | 17.94 ns | 0.204 ns | 0.181 ns |  0.71 | 0.0166 |     104 B |
        */
        //[Benchmark(Baseline = true)]
        //public string CamelCase()
        //{
        //    string str = "ThisIsAString";
        //    if (!string.IsNullOrEmpty(str) && str.Length > 1)
        //    {
        //        return char.ToLowerInvariant(str[0]) + str.Substring(1);
        //    }
        //    return str;
        //}

        //[Benchmark]
        //public string CamelCaseSpan()
        //{
        //    Span<char> toCamelCase = "ThisIsAString".ToCharArray();
        //    if (toCamelCase.Length > 1)
        //    {
        //        toCamelCase[0] = char.ToLowerInvariant(toCamelCase[0]);
        //    }

        //    return toCamelCase.ToString();
        //}

        /*
        |    Method |      Mean |    Error |   StdDev | Ratio |  Gen 0 | Allocated |
        |---------- |----------:|---------:|---------:|------:|-------:|----------:|
        |     Split | 198.87 ns | 2.051 ns | 1.918 ns |  1.00 | 0.0548 |     344 B |
        | SplitSpan |  82.65 ns | 0.518 ns | 0.484 ns |  0.42 |      - |         - |
        */
        //[Benchmark(Baseline = true)]
        //public Guid Split()
        //{
        //    string idFieldValue = "bertonline.sharepoint.com,cf1ed1cb-4a3c-43ed-bb3f-4ced4ce69ecf,1de385e4-e441-4448-8443-77680dfd845e";
        //    if (!string.IsNullOrEmpty(idFieldValue))
        //    {
        //        string id = idFieldValue.Split(new char[] { ',' })[2];

        //        return Guid.Parse(id);
        //    }

        //    return Guid.Empty;
        //}

        //[Benchmark]
        //public Guid SplitSpan()
        //{
        //    ReadOnlySpan<char> idFieldValue = "bertonline.sharepoint.com,cf1ed1cb-4a3c-43ed-bb3f-4ced4ce69ecf,1de385e4-e441-4448-8443-77680dfd845e";
        //    if (!idFieldValue.IsNullOrEmpty())
        //    {
        //        var id = idFieldValue.LastRightPart(',');
        //        return Guid.Parse(id);
        //    }
        //    return Guid.Empty;
        //}

        #endregion

        #region Nah, does not seem to help
        /*
        Add: 
        private readonly Func<object> _expression;
        _expression = Expression.Lambda<Func<object>>(Expression.New(typeof(PnP.Core.Model.SharePoint.Web))).Compile(); to constuctor

        |              Method |      Mean |    Error |   StdDev | Ratio |  Gen 0 |  Gen 1 | Allocated |
        |-------------------- |----------:|---------:|---------:|------:|-------:|-------:|----------:|
        | ReflectionActivator | 117.58 ns | 0.842 ns | 0.787 ns |  1.00 | 0.0994 |      - |     624 B |
        |  CompiledExpression |  94.98 ns | 1.216 ns | 1.137 ns |  0.81 | 0.0994 | 0.0001 |     624 B |
        
        */
        //[Benchmark(Baseline = true)]
        //public object ReflectionActivator()
        //{
        //    return Activator.CreateInstance(typeof(PnP.Core.Model.SharePoint.Web));
        //}

        //[Benchmark]
        //public object CompiledExpression()
        //{
        //    return _expression();
        //}
        #endregion

    }
}
