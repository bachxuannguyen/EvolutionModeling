using OxyPlot;
using OxyPlot.WindowsForms;
using System.Windows.Forms;

namespace EvoModeling
{
    public partial class FormPlotEvaluator : Form
    {
        public FormPlotEvaluator(PlotModel modelEvaluator)
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            PlotView viewEvaluator = new PlotView();
            viewEvaluator.Model = modelEvaluator;

            viewEvaluator.Dock = System.Windows.Forms.DockStyle.Right;
            viewEvaluator.Location = new System.Drawing.Point(0, 0);
            viewEvaluator.Size = new System.Drawing.Size(1366, 600);
            viewEvaluator.TabIndex = 0;

            Controls.Add(viewEvaluator);
        }
    }
}
