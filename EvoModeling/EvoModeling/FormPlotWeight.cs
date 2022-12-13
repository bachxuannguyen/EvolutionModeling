using OxyPlot;
using OxyPlot.WindowsForms;
using System.Windows.Forms;

namespace EvoModeling
{
    public partial class FormPlotWeight : Form
    {
        public FormPlotWeight(PlotModel modelWeight, PlotModel modelWeightDesc, PlotModel modelCANode)
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            //WEIGHT.

            PlotView viewWeight = new PlotView();
            viewWeight.Model = modelWeight;

            viewWeight.Dock = System.Windows.Forms.DockStyle.Right;
            viewWeight.Location = new System.Drawing.Point(0, 0);
            viewWeight.Size = new System.Drawing.Size(440, 600);
            viewWeight.TabIndex = 0;

            Controls.Add(viewWeight);

            //WEIGHT DESC.

            PlotView viewWeightDesc = new PlotView();
            viewWeightDesc.Model = modelWeightDesc;

            viewWeightDesc.Dock = System.Windows.Forms.DockStyle.Right;
            viewWeightDesc.Location = new System.Drawing.Point(0, 0);
            viewWeightDesc.Size = new System.Drawing.Size(440, 600);
            viewWeightDesc.TabIndex = 0;

            Controls.Add(viewWeightDesc);

            //CHILD / ADJACENCY NODE.

            PlotView viewCANode = new PlotView();
            viewCANode.Model = modelCANode;

            viewCANode.Dock = System.Windows.Forms.DockStyle.Right;
            viewCANode.Location = new System.Drawing.Point(0, 0);
            viewCANode.Size = new System.Drawing.Size(440, 600);
            viewCANode.TabIndex = 0;

            Controls.Add(viewCANode);
        }
    }

}
