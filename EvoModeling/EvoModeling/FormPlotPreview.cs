using OxyPlot.WindowsForms;
using System;
using System.Windows.Forms;

namespace EvoModeling
{
    public partial class FormPlotPreview : Form
    {
        public FormPlotPreview()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            //Định nghĩa các nét.
            var line1 = new OxyPlot.Series.LineSeries()
            {
                Title = "y",
                Color = OxyPlot.OxyColors.Blue,
                StrokeThickness = 1,
                MarkerSize = 2,
                MarkerType = OxyPlot.MarkerType.Circle
            };
            var line2 = new OxyPlot.Series.LineSeries()
            {
                Title = "y",
                Color = OxyPlot.OxyColors.Red,
                StrokeThickness = 1,
                MarkerSize = 2,
                MarkerType = OxyPlot.MarkerType.Circle
            };

            int maxX = 10000;

            for (int i = 0; i < maxX; i++)
            {
                line1.Points.Add(new OxyPlot.DataPoint(i, Math.Pow(0.2, i) * 100));
                line2.Points.Add(new OxyPlot.DataPoint(i, 5.0 * Math.Sqrt(i)));
            }

            /*
            //Lũy thừa với cơ số nhỏ hơn 1.
            double alpha = 0.9;
            for (int i = 0; i < maxX; i++)
            {
                line1.Points.Add(new OxyPlot.DataPoint(i, Math.Pow(alpha, i) * i));
            }
            */

            //Tạo plot model.
            var modelNode = new OxyPlot.PlotModel
            {
                Title = "Plot Things"
            };
            modelNode.Series.Add(line1);
            modelNode.Series.Add(line2);

            //Tạo plot view.
            PlotView viewNode = new PlotView();
            viewNode.Model = modelNode;
            viewNode.Dock = System.Windows.Forms.DockStyle.Fill;
            viewNode.Location = new System.Drawing.Point(0, 0);
            viewNode.Size = new System.Drawing.Size(500, 500);
            viewNode.TabIndex = 0;

            Controls.Add(viewNode);
        }
    }
}
