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
    public partial class FormPlotSummary : Form
    {
        Library lib = new Library();

        public FormPlotSummary(PlotModel modelNode)
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            //NODES.

            PlotView viewNode = new PlotView();
            viewNode.Model = modelNode;

            viewNode.Dock = System.Windows.Forms.DockStyle.Left;
            viewNode.Location = new System.Drawing.Point(0, 0);
            viewNode.Size = new System.Drawing.Size(1300, 600);
            viewNode.TabIndex = 0;

            Controls.Add(viewNode);
        }
    }
}
