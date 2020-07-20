using MathNet.Numerics;
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
                FinancialFormulaTest();
                PolynomialTest();
                MathNetTest();
                Console.WriteLine("Analysis completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        private static void FinancialFormulaTest()
        {
            //var source = "5.857486213 6.882938361 5.938018732 7.418233591 8.102131936 10.02132021 11.30071133 13.07283147 15.03172119 17.62765193 20.33497935 23.3258717 26.49699319 29.9783162 32.92265494 35.10837941 36.3501758 36.53764154 34.1757566 30.89822951 26.63771337 22.32763029 18.38308201 14.90463905 12.35598901 9.741861183 7.720235644 6.033749358 4.565486063 3.445948379 2.494921555 1.82715694 1.368045582 0.965480591 0.722276257 0.577920119 0.448848566 0.361900586 0.29215714 0.242289617 0.202792727 0.166097863 0.069075582 0.002831501 0.000875038 0.043841538";
            var datas = new Datas();
            var sourceDatas = datas.LoadSourceDatas();
            foreach (var data in sourceDatas)
            {
                var values = data.Skip(32).Take(6);

                var range = Enumerable.Range(1, 6).Select(r => (double)r);
                var forecaster = new Forecaster();
                var result = forecaster.GetPredictData(8, values.ToArray());

                for (int i = 0; i < 8; i++)
                {
                    data[data.Count - 1 - i] = result[result.Length - 1 - i];
                }

                SetUnitary(data);
            }

            datas.SaveResultDatas(sourceDatas, "PredictData.txt");
        }

        private static void PolynomialTest()
        {
            //var source = "5.857486213 6.882938361 5.938018732 7.418233591 8.102131936 10.02132021 11.30071133 13.07283147 15.03172119 17.62765193 20.33497935 23.3258717 26.49699319 29.9783162 32.92265494 35.10837941 36.3501758 36.53764154 34.1757566 30.89822951 26.63771337 22.32763029 18.38308201 14.90463905 12.35598901 9.741861183 7.720235644 6.033749358 4.565486063 3.445948379 2.494921555 1.82715694 1.368045582 0.965480591 0.722276257 0.577920119 0.448848566 0.361900586 0.29215714 0.242289617 0.202792727 0.166097863 0.069075582 0.002831501 0.000875038 0.043841538";
            //var range = string.Join("\r\n", Enumerable.Range(1, 14));
            var datas = new Datas();
            var sourceDatas = datas.LoadSourceDatas();
            foreach (var data in sourceDatas)
            {
                var values = data.Skip(32).Take(6);

                double[] X = Enumerable.Range(1, 6).Select(r => (double)r).ToArray();
                double[] Y = values.ToArray();

                // f(x) = A*x*x + B*x + C
                GaussNewton.F f = delegate (double[] coefficients, double x)
                {
                    return coefficients[0] * x * x + coefficients[1] * x + coefficients[2];
                };

                GaussNewton gaussNewton = new GaussNewton(3);
                gaussNewton.Initialize(Y, X, f);
                double[] answer = gaussNewton.Coefficients;

                List<double> result = new List<double>();
                for (int i = 1; i < 15; i++)
                {
                    result.Add(answer[0] * i * i + answer[1] * i + answer[2]);
                }

                for (int i = 0; i < 8; i++)
                {
                    data[data.Count - 1 - i] = result[result.Count - 1 - i];
                }

                SetUnitary(data);
            }

            datas.SaveResultDatas(sourceDatas, "PolynomialData.txt");
        }

        private static void MathNetTest()
        {
            var datas = new Datas();
            var sourceDatas = datas.LoadSourceDatas();
            foreach (var data in sourceDatas)
            {
                var values = data.Skip(32).Take(6);

                double[] X = Enumerable.Range(1, 6).Select(r => (double)r).ToArray();
                double[] Y = values.ToArray();

                double[] parameters = Fit.Polynomial(X, Y, 2);


                List<double> result = new List<double>();
                for (int i = 1; i < 15; i++)
                {
                    result.Add(parameters[0] + parameters[1] * i + parameters[2] * i * i );
                }

                for (int i = 0; i < 8; i++)
                {
                    data[data.Count - 1 - i] = result[result.Count - 1 - i];
                }

                SetUnitary(data);
            }

            datas.SaveResultDatas(sourceDatas, "MathNetData.txt");
        }

        private static void SetUnitary(List<double> dataToSet)
        {
            if (dataToSet != null
                && dataToSet.Count > 0)
            {
                var totalDistribution = dataToSet.Sum();

                if (totalDistribution > 0)
                    for (var i = 0; i < dataToSet.Count; i++)
                        dataToSet[i] = dataToSet[i] / totalDistribution;
            }
        }
    }
}
