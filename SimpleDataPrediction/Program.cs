using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataPrediction
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var source = "5.857486213 6.882938361 5.938018732 7.418233591 8.102131936 10.02132021 11.30071133 13.07283147 15.03172119 17.62765193 20.33497935 23.3258717 26.49699319 29.9783162 32.92265494 35.10837941 36.3501758 36.53764154 34.1757566 30.89822951 26.63771337 22.32763029 18.38308201 14.90463905 12.35598901 9.741861183 7.720235644 6.033749358 4.565486063 3.445948379 2.494921555 1.82715694 1.368045582 0.965480591 0.722276257 0.577920119 0.448848566 0.361900586 0.29215714 0.242289617 0.202792727 0.166097863 0.069075582 0.002831501 0.000875038 0.043841538";

                var values = source.Split(' ').Skip(8).Take(6).Select(t => double.Parse(t)).Reverse();

                var range = Enumerable.Range(1, 6).Select(r => (double)r);
                var forecaster = new Forecaster();
                var result = forecaster.GetPredictData(8, values.ToArray());
                Console.WriteLine("Analysis completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
