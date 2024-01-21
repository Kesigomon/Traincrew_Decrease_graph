using System;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;//グラフ用
using TrainCrew;

namespace Traincrew_Decrease_graph
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Chart chart;
        ChartArea area;
        double[] deceleration = new double[] { 1.09, 1.68, 2.29, 2.83, 3.37, 3.83 };
        double speed { get; set; } = 110;
        double stopAt { get; set; }
        int notch {  get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Topmost = true;
            TrainCrewInput.Init();
            var windowsFormsHost = (WindowsFormsHost)grid1.Children[0];
            chart = (Chart)windowsFormsHost.Child;

            // ChartArea追加
            area = new ChartArea("ChartArea1");
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 1000;

            
            chart.ChartAreas.Add(area);
            System.Windows.Media.CompositionTarget.Rendering += update;


        }

        private void update(object sender, EventArgs e) { 
            // 今ある線を全部クリア
            chart.Series.Clear();

            var state = TrainCrewInput.GetTrainState();
            double maxAxisX = 1000;
            speed = state.Speed;
            notch = Math.Max(state.Bnotch - 1, 0);

            // 停目線を引く
            if (state.nextStopType == "停車" || state.nextStopType == "運転停車")
            {
                Series series = new Series
                {
                    ChartType = SeriesChartType.Spline,
                    MarkerStyle = MarkerStyle.None
                };
                series.Points.AddXY(state.nextStaDistance, 0);
                series.Points.AddXY(state.nextStaDistance, speed);
                chart.Series.Add(series);
                maxAxisX = state.nextStaDistance;
            }
            // 減速曲線を引く
            foreach(double dec in deceleration)
            {
                Series series = new Series
                {
                    ChartType = SeriesChartType.Spline,
                    MarkerStyle = MarkerStyle.None
                };


                for (double x = 0; x <= 1000; x += 1)
                {
                    var y = Math.Sqrt(speed * speed - 36 * dec * x / 5);
                    if (double.IsNaN(y))
                    {
                        // y=0になるxを求める
                        var x_d = 5 * speed * speed / 36 / dec;
                        series.Points.AddXY(x_d, 0);
                        maxAxisX = Math.Max(maxAxisX, x_d);
                        break;
                    }
                    series.Points.AddXY(x, y);
                }

                chart.Series.Add(series);
            }
            area.AxisX.Maximum = Math.Min(1000, 200 * Math.Ceiling(maxAxisX / 200));
            
        }
        
    }
}
