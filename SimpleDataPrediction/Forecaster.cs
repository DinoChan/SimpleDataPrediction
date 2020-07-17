using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;

namespace SimpleDataPrediction
{
    public class Forecaster
    {
        public double[] GetPredictData(int forecastingPoints, double[] points)
        {
            var tempChart = new Chart();

            tempChart.ChartAreas.Add(new ChartArea());
            tempChart.ChartAreas[0].AxisX = new Axis();
            tempChart.ChartAreas[0].AxisY = new Axis();
            tempChart.Series.Add(new Series());

            for (int i = 0; i < points.Length; i++)
            {
                tempChart.Series[0].Points.AddXY(i, points[i]);
            }

            var trendSeries = new Series();
            tempChart.Series.Add(trendSeries);

            var typeRegression = "Exponential";
            var forecasting = forecastingPoints.ToString();
            var error = "false";
            var forecastingError = "false";
            var parameters = typeRegression + ',' + forecasting + ',' + error + ',' + forecastingError;

            tempChart.DataManipulator.FinancialFormula(FinancialFormula.Forecasting, parameters, tempChart.Series[0], trendSeries);

            var result = new List<double>();
            for (int i = 0; i < trendSeries.Points.Count; i++)
            {
                result.Add(trendSeries.Points[i].YValues[0]);
            }

            return result.ToArray();
        }
    }
}
