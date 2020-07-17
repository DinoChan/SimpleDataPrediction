using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataPrediction
{
   public class Datas
    {
        public List<List<double>> LoadSourceDatas()
        {
            var allLines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "data.txt"));

            var sourceDatas = new List<List<double>>();

            foreach (var line in allLines)
            {
                sourceDatas.Add(line.Split('\t').Select(l => double.Parse(l)).ToList());
            }

            return sourceDatas;
        }

        public void SaveResultDatas( List<List<double>> resultDatas,string fileName)
        {
            var result = string.Empty;

            foreach (var line in resultDatas)
            {
                result += string.Join("\t", line);
                result += Environment.NewLine;
            }

            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, fileName), result);
        }

    }
}
