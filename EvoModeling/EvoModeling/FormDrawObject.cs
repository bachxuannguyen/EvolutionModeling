using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvoModeling
{
    public partial class FormDrawObject : Form
    {
        Library lib = new Library();
        public Evaluator evaluator = new Evaluator();

        public FormDrawObject()
        {
            InitializeComponent();
            this.Paint += new PaintEventHandler(this.Show);
        }

        private void Show(object sender, PaintEventArgs e)
        {
            Random r = new Random();

            //Định nghĩa các nét vẽ.
            Pen penPoint = new Pen(Color.DarkRed, 3);
            Pen penPLink = new Pen(Color.Blue, 1);
            Pen penALink = new Pen(Color.Green, 1);
            Font fontNode = new Font(FontFamily.GenericSansSerif, 8.0F, FontStyle.Bold);
            Font fontSummary = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Bold);
            SolidBrush brushNode = new SolidBrush(Color.Black);
            SolidBrush brushSummary = new SolidBrush(Color.Black);
            
            //Kích thước nút vẽ.
            int eW = 3;
            int eH = 3;

            //Bắt đầu vẽ.
            if (evaluator.nodeCount_Evaluated > 0)
            {
                //Gán tọa độ cho từng nút.
                int[] arrX = new int[evaluator.node_Evaluated.Length];
                int[] arrY = new int[evaluator.node_Evaluated.Length];
                int r0 = 0;
                int rN = 0;
                int x0 = this.ClientSize.Width / 2;
                int y0 = this.ClientSize.Height / 2;
                int dX = x0;
                int dY = y0;
                r0 = (int)Math.Sqrt(y0 * y0 + x0 * x0);
                for (int i = 0; i < evaluator.nodeCount_Evaluated; i++)
                {
                    rN = i * r0 / evaluator.node_Evaluated.Length;
                    while (dX >= x0 || dY >= y0)
                    {
                        dY = r.Next(rN);
                        dX = (int)(Math.Sqrt((rN * rN) - (dY * dY)));
                    }

                    int x = r.Next(1, 5);
                    if (x == 1)
                    {
                        arrX[i] = lib.Randomize(x0 - dX, 30, 20, 2 * x0 - 50);
                        arrY[i] = lib.Randomize(y0 - dY, 30, 50, 2 * y0 - 50);
                    }
                    else if (x == 2)
                    {
                        arrX[i] = lib.Randomize(x0 - dX, 30, 20, 2 * x0 - 50);
                        arrY[i] = lib.Randomize(y0 + dY, 30, 50, 2 * y0 - 50);
                    }
                    else if (x == 3)
                    {
                        arrX[i] = lib.Randomize(x0 + dX, 30, 20, 2 * x0 - 50);
                        arrY[i] = lib.Randomize(y0 - dY, 30, 50, 2 * y0 - 50);
                    }
                    else if (x == 4)
                    {
                        arrX[i] = lib.Randomize(x0 + dX, 30, 20, 2 * x0 - 50);
                        arrY[i] = lib.Randomize(y0 + dY, 30, 50, 2 * y0 - 50);
                    }
                    dX = x0;
                    dY = x0;
                }

                //Mảng chứa ID của các nút.
                int[] node_Evaluated_Id = new int[evaluator.nodeCount_Evaluated];
                for (int i = 0; i < evaluator.nodeCount_Evaluated; i++)
                    node_Evaluated_Id[i] = evaluator.node_Evaluated[i].id;

                //Xuất chuỗi.
                string summaryStr = "Generation " + evaluator.tracer.generation.ToString() + ": " + evaluator.node_Alive.Length.ToString() + "/" + evaluator.node_Total.Length.ToString() + " alive(s)";
                summaryStr += "\r\nShow: Node " + evaluator.node_Evaluated.Length.ToString();
                summaryStr += " - Weight " + evaluator.weight_Evaluated.ToString() + " - Parent link " + evaluator.pNodeCount.ToString() + " - Adjacency link " + evaluator.aNodeCount.ToString();
                summaryStr += "\r\nRatio: Node " + evaluator.ratio_Node.ToString("F2");
                summaryStr += " - Weight " + evaluator.ratio_Weight.ToString("F2") + " - Parent link " + evaluator.ratio_PNode.ToString("F2") + " - Adjacencing link " + evaluator.ratio_ANode.ToString("F2");

                e.Graphics.DrawString(summaryStr, fontSummary, brushSummary, 1, 1);

                //Vẽ liên kết kế cận.
                for (int i = 0; i < evaluator.nodeCount_Evaluated; i++)                           
                    if (evaluator.node_Evaluated[i].aNode.Length > 0)
                        for (int j = 0; j < evaluator.node_Evaluated[i].aNode.Length; j++)
                        {
                            if (node_Evaluated_Id.Contains(evaluator.node_Evaluated[i].aNode[j]))
                                e.Graphics.DrawLine(penALink, new Point(arrX[i], arrY[i]), new Point(arrX[Array.IndexOf(node_Evaluated_Id, evaluator.node_Evaluated[i].aNode[j])], arrY[Array.IndexOf(node_Evaluated_Id, evaluator.node_Evaluated[i].aNode[j])]));
                        }
                
                //Vẽ liên kết cha.
                for (int i = 0; i < evaluator.nodeCount_Evaluated; i++)
                    if (node_Evaluated_Id.Contains(evaluator.node_Evaluated[i].pNode[0]))
                        e.Graphics.DrawLine(penPLink, new Point(arrX[i], arrY[i]), new Point(arrX[Array.IndexOf(node_Evaluated_Id, evaluator.node_Evaluated[i].pNode[0])], arrY[Array.IndexOf(node_Evaluated_Id, evaluator.node_Evaluated[i].pNode[0])]));
                
                //Vẽ nút và ghi nhãn.
                for (int i = 0; i < evaluator.nodeCount_Evaluated; i++)
                {
                    e.Graphics.DrawEllipse(penPoint, arrX[i], arrY[i], eW, eH);
                    e.Graphics.DrawString(evaluator.node_Evaluated[i].id.ToString() + "." + evaluator.node_Evaluated[i].cNode.Length + "." + evaluator.node_Evaluated[i].aNode.Length, fontNode, brushNode, arrX[i], arrY[i] + eH + 1);
                }
            }
        }
    }
}
