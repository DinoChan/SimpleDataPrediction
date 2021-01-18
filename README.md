以前在工作中遇到了一个数据错误的问题，顺便写下解决的思路。

## 1. 错误的数据

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103132349601-1590932660.png)

上图是同一组探测器在同一天采集到的 19 次数据，总体来说重复性不错，但很明显最后 8 个探测器出了问题，导致采集到的数据在最后八个点一片混乱。即使把其中看起来最好的一组数据拿出来使用多项式拟合，也可以看出最后几个点没有落在拟合曲线上（只拟合最后 14 个点）：

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103150114506-421520061.png)


虽然我知道这是硬件问题，但是**遇到事情不能坐以待毙**，软件方面也许可以做些什么。既然我从上图中得知出了最后几个点之外，其它数据都在拟合曲线上，那我可以使用前面几个点的拟合结果预测后面几个点并替换掉出错的数据，从而得到一组看起来正常的数据。


## 2. 曲线拟合与数据预测

曲线拟合（curve fitting）是指选择适当的曲线类型来拟合观测数据，以便观察两组数据之间的内在联系，了解数据之间的变化趋势。

在数据分析时，我们有时需要通过已有数据来预测未来数据。在一些复杂的数据模型中，数据维度很多，数据之间的关系很复杂，我们可能会用到深度学习的算法。但是在一些简单的数据模型中，数据之间有很明显的相关性，那我们就可以使用简单的曲线拟合来预测未来的数据。

这些工作都可以使用 Excel 完成，先来尝试一下。把某组数据最后14个点（只选取峰值右边的14个点是因为容易计算）放进Excel中，插入一个散点图，右键点击其中的蓝色散点，选择`添加趋势线`：

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103150823211-708970034.png)





然后在右侧出现的`设置趋势线格式`中选择`多项式`，阶数为 3，勾选`显示公式`：

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103132425872-1484095861.png)



可以看到，曲线图中出现了一条虚线的曲线，并显示了对应的公式为 <CODE>y = 6E-07x^3^ + 0.0002x^2^ - 0.0072x + 0.0637</CODE>：

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103132451456-662217764.png)

如果需要预测数据，可以修改`前推`数字以得到后面几个周期的数据。

## 3. 使用 Math.Net 进行曲线拟合

当然我不可能对每一条数据都扔进 Excel 里进行拟合。在 C# 中我们可以使用 [Math.Net](https://www.mathdotnet.com/) 进行非线性拟合。

[Math.Net](https://www.mathdotnet.com/) 是一个开源项目，旨在构建和维护涵盖基础数学的工具箱，以满足 .Net 开发人员的高级需求和日常需求。其中 [Math.NET Numerics](https://numerics.mathdotnet.com/)  旨在为科学、工程和日常使用中的数值计算提供方法和算法。涵盖的主题包括特殊函数，线性代数，概率模型，随机数，插值，积分变换等等。

要使用 [Math.NET Numerics](https://numerics.mathdotnet.com/)，首先安装它的 Nuget 包：

```
Install-Package MathNet.Numerics
```

[Math.NET Numerics](https://numerics.mathdotnet.com/) 提供了 `Fit.Polynomial` 函数用作多项式拟合，如以下代码所示，其中 `X` 是 X 轴的数组， `Y` 是 Y 轴的数组， 函数的第三个参数是多项式的阶数，这里用 2 作为阶数。 

``` CS
double[] X = Enumerable.Range(1, 6).Select(r => (double)r).ToArray();
double[] Y = values.ToArray();
double[] parameters = Fit.Polynomial(X, Y, 2);
```

返回的结果是最佳拟合参数的数组 `[p0，p1，p2，…，pk]`，将其带入公式 <CODE>p0 + p1×x + p2×x^2^ + ... + pk×x^k^</CODE> 即可算出对应的拟合数据。完整的代码如下，在这个示例里，我只需要用倒数第9到14个数据，通过 `Fit.Polynomial` 获得一个多项式的方程 （ <CODE>f(x) = p0 + p1×x + p2×x^2^</CODE> ），然后用这个方程计算出后面 8 个点的数据替换原本出错的数据：

``` CS
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
```
![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103132504808-1972362365.png)




替换后的结果如上所示，整体符合前面数据的趋势，使用这组数据进行运算也能得到很好的结果。

## 4.  Microsoft Chart Controls 中的 FinancialFormula

接下来玩玩“新”花样，用古老的 Microsoft Chart Controls 实现相同的功能。

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210108153008813-1791931147.png)




A long time ago in a galaxy far, far away... 微软推出了一套免费又强大的图表控件，它用于 WinForms 和 WebForms 中，可轻松套用各种功能强大的 2D、3D、实时变化的动态图表，头发比较少的 .NET 开发者或多或少都接触过这套图表控件。虽然现在看来多少有些落后了，但它还是很有用啊，而且还不收钱。

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103163918554-569379066.png)

那么，在哪里可以找到这个图表库呢？现在微软的官网也只能找到 for Microsoft .NET Framework 3.5 的[下载](https://www.microsoft.com/en-us/download/details.aspx?id=14422)，找不到更新的版本。幸好 Visual Studio 里就自带了这个图表库，可以直接添加 `System.Windows.Forms.DataVisualization` 的引用：

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103164013349-2071652237.png)

这篇我不会介绍如何做图表，而是讲讲这个图表库中的一样很有趣的东西：[FinancialFormula](https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.forms.datavisualization.charting.dataformula.financialformula?view=netframework-4.8&WT.mc_id=WD-MVP-5003763)。如果只是做简单的财务数据处理，可以用它玩玩。当图表中已有其它序列（Series）的数据，DataManipulator 的 `FinancialFormula` 可以使用大部分常见的金融公式处理这些数据并产生新的数据序列。

例指，`数移动平均线 `(Exponential Moving Average) 是对一段时间内的数据计算所得的平均值，它的输入和输出如下：

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103164412015-22403602.png)



而`蔡金震荡` (Chaikin Oscillator) 指标是指应用于聚散的 3 天指数移动平均线与 10 天指数移动平均线之差，它的输出如下：


![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103164418116-974773912.png)




FinancialFormula 还有很多其它用法，具体可以参考以下两个页面：

[FinancialFormula Enum (System.Windows.Forms.DataVisualization.Charting) Microsoft Docs](https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.forms.datavisualization.charting.financialformula?view=netframework-4.8&WT.mc_id=WD-MVP-5003763)

[Using Financial Formulas](https://origin2.cdn.componentsource.com/sites/default/files/resources/dundas/538236/WinChart2005/Formulas_HowToUseFormulas.html)

## 5. 数据预测

这次我用到的是`预测` (Forecasting) ，它是指使用历史观测值来预测未来值。

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103165509145-174722414.png)



Forecasting公式采用四个可选参数：


- **RegressionType**: 回归类型。使用一个数字来指示特定次数的多元回归，或者使用以下值之一指定不同的回归类型：Linear、Exponential、Logarithmic、Power。默认值为 2，与指定 Linear 等效。

- **Period**: 预测时段。公式会预测此指定的未来天数内的数据变化。默认值为序列长度的一半。

- **ApproxError**： 是否输出近似误差。如果设置为 false，则输出误差序列不包含相应历史数据的数据。默认值为 true。

- **ForecastError**： 是否输出预测误差。如果设置为 false，并且 ApproxError 设置为 true，则输出误差序列将包含所有预测数据点的近似误差。默认值为 true。

输出值有三个序列：

- **Forecast**： 预测测值。

- **UpperError**： 上限误差。

- **LowerError**： 下限误差。


输入参数中回归类型的具体值所代表的公式可以参考以下链接：

[Time Series and Forecasting Formula](https://origin2.cdn.componentsource.com/sites/default/files/resources/dundas/538236/WinChart2005/Forecasting.html)

使用 `FinancialFormula` 的代码十分简单，只需创建一个临时的 `Chart` ,插入原始数据作为一个 `Series` ，然后调用 `DataManipulator.FinancialFormula` 即可，所有代码加起来也就 30 来行：

``` CS
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
```

这里我使用了 `Exponential` （指数函数）作为回归类型，结果如下，看起来重复性很好，但是转折处比较生硬，导致最后在实际计算中不太理想。如果想要理想的结果，应该先尝试找出最合适的回归公式。

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210103164604420-1972719257.png)


## 6. 最后

[Math.Net](https://www.mathdotnet.com/) 是一个强大的项目，这篇文章只介绍了它所有功能的冰山一角。想了解更多可以参考官方文档，或参考博客园上的文章，例如：

[【目录】开源Math.NET基础数学类库使用总目录 - 数据之巅 - 博客园](https://www.cnblogs.com/asxinyu/p/Bolg_Category_For_MathNet.html)

`FinancialFormula` 挺好玩的，但它和图表控件耦合在一起，用起来感觉有点邪门歪道，倒是通过它多少学会了一点财务公式。

话说回来当年微软的控件库都很上心嘛，现在微软都不会出这么良心的图表库了，逼我们买第三方控件。

![](https://img2020.cnblogs.com/blog/38937/202101/38937-20210108160100040-720937680.png)



## 7. 参考


[Math.NET Numerics](https://numerics.mathdotnet.com/)

[Curve Fitting Linear Regression](https://numerics.mathdotnet.com/Regression.html)

[【目录】开源Math.NET基础数学类库使用总目录 - 数据之巅 - 博客园](https://www.cnblogs.com/asxinyu/p/Bolg_Category_For_MathNet.html)

[数据预测与曲线拟合 - 知乎](https://zhuanlan.zhihu.com/p/95277637)

[Time Series and Forecasting Formula](https://origin2.cdn.componentsource.com/sites/default/files/resources/dundas/538236/WinChart2005/Forecasting.html)

[DataManipulator Class (System.Web.UI.DataVisualization.Charting) Microsoft Docs](https://docs.microsoft.com/zh-cn/dotnet/api/system.web.ui.datavisualization.charting.datamanipulator?view=netframework-4.8&WT.mc_id=WD-MVP-5003763)

[DataFormula.FinancialFormula Method (System.Windows.Forms.DataVisualization.Charting) Microsoft Docs](https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.forms.datavisualization.charting.dataformula.financialformula?view=netframework-4.8&WT.mc_id=WD-MVP-5003763)

[FinancialFormula Enum (System.Windows.Forms.DataVisualization.Charting) Microsoft Docs](https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.forms.datavisualization.charting.financialformula?view=netframework-4.8&WT.mc_id=WD-MVP-5003763)

[how to generate graphs using Microsoft Chart Control ](http://www.nullskull.com/q/10352018/how-to-generate-graphs-using-microsoft-chart-control.aspx)
