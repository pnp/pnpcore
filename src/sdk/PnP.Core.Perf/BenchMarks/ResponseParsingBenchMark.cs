using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Perf.BenchMarks
{
    [MemoryDiagnoser]
    public class ResponseParsingBenchMark
    {

        public ResponseParsingBenchMark()
        {

        }

        [Benchmark(Baseline = true)]
        public void Current()
        {

        }

        [Benchmark]
        public void Tuned()
        {

        }
    }
}
