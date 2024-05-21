using System.IO;
using System.Windows;
using System.Windows.Media;
using ScottPlot;
using TrainCrew;
using Color = ScottPlot.Color;

namespace Traincrew_Decrease_graph
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        double[] deceleration = [1.09, 1.68, 2.29, 2.83, 3.37, 3.83];
        private Plot _plot;

        public MainWindow()
        {
            InitializeComponent();
            Topmost = true;
            _plot = WpfPlot1.Plot;
            _plot.FigureBackground.Color = new Color(0, 0, 0, 0);
            _plot.Axes.Color(Color.FromHex("#FFFFFF"));
            TrainCrewInput.Init();

            Closing += (sender, e) => { TrainCrewInput.Dispose(); };
            MouseLeftButtonDown += (sender, e) =>
            {
                e.Handled = true; DragMove(); };
            CompositionTarget.Rendering += (_, _ ) =>
            {
                try
                {
                    update();
                }
                catch (Exception e)
                {
                    try
                    {
                        // クラッシュログを出力する 
                        Directory.CreateDirectory("crashlog");
                        File.WriteAllText($"crashlog/{DateTime.Now:yyyyMMddHHmmss}.txt", e.ToString());
                    }
                    catch (Exception)
                    {
                        // クラッシュログの出力に失敗した場合は何もしない
                    }
                    Close();
                }
            };
        }

        private void update()
        {
            // 今ある線を全部クリア
            _plot.Clear();

            var state = TrainCrewInput.GetTrainState();
            var gradient = Math.Atan(state.gradient / 100.0);
            var acceleration = 2.7 * Math.Sin(gradient);
            double maxAxisX = 1000;
            double speed = state.Speed; 
            var notch = Math.Max(state.Bnotch - 1, 0);

            // 停目線を引く
            if (state.nextStopType is "停車" or "運転停車")
            {
                _plot.Add.VerticalLine(state.nextStaDistance);
                maxAxisX = state.nextStaDistance;
            }

            // 減速曲線を引く
            for (var i = 0; i < deceleration.Length; i++)
            {
                var dec = deceleration[i] + acceleration;
                var func = new Func<double, double>(x =>
                {
                    var y = Math.Sqrt(speed * speed - 36 * dec * x / 5);
                    return double.IsNaN(y) ? 0 : y;
                });
                var functionPlot = _plot.Add.Function(func);
                functionPlot.Label = $"B{i + 1}";
                functionPlot.LineWidth = notch == i + 1 ? 5 : 2;
            }

            maxAxisX = maxAxisX switch
            {
                <= 6 => 5,
                <= 40 => 50,
                <= 90 => 100,
                < 1000 => 200 * (Math.Floor(maxAxisX / 200) + 1),
                _ => 1000
            };
            var maxAxisY = speed switch
            {
                <= 9 => 10,
                <= 20 => (Math.Floor(speed / 5) + 1) * 5,
                _ => (Math.Floor(speed / 10) + 1) * 10
            };

            _plot.Axes.SetLimits(0, maxAxisX, 0, maxAxisY);
            WpfPlot1.Refresh();
        }

    }
}