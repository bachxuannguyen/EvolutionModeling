using OxyPlot;
using OxyPlot.WindowsForms;
using System.Windows.Forms;

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
