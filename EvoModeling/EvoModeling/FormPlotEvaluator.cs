using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

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
