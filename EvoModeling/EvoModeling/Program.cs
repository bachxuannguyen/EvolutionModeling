using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using OxyPlot.WindowsForms;

namespace EvoModeling
{
    class Program
    {
        static Library lib = new Library();
        static EvoEngine evoEng = new EvoEngine();
        static Tracer[] tracer = new Tracer[] { };
        static int genCount = 0;
        static int deathCurrent = 0;
        static int deathTotal = 0;
        static bool alive = true;
        static int evaluatedGen = 0;

        [STAThread]
        static void Main(string[] args)
        {
            //Thử đồ thị.
            Application.EnableVisualStyles();
            //Application.Run(new FormPlotPreview());

            //Nhập số thế hệ tối đa.
            Console.Write("Max generation [" + lib.maxGen.ToString() + "]: ");
            int maxGen = 0;
            if (!int.TryParse(Console.ReadLine(), out maxGen))
                maxGen = lib.maxGen;

            //Dừng theo chu kỳ.
            int nextPause = 0;
            Console.Write("Pause after [0]: ");
            if (!int.TryParse(Console.ReadLine(), out nextPause) && nextPause >= 1 && nextPause <= maxGen)
                nextPause = 0;

            //Khởi tạo máy mô phỏng.
            Console.WriteLine("Initializing...");
            evoEng.Init();

            //Khởi chạy bộ đếm thời gian.
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Chạy mô phỏng.
            while (genCount < maxGen || !alive)
            {
                stopWatch.Start();

                //CHUYỂN THẾ HỆ.

                if (!alive)
                {
                    evoEng.obj = tracer[genCount - 2].savedObj.Clone() as Object;
                    alive = true;
                }
                else
                {
                    genCount++;
                    deathCurrent = 0;
                    Array.Resize(ref tracer, genCount);
                    tracer[genCount - 1] = new Tracer();
                }           
                Console.WriteLine("///////\r\nGENERATION: " + genCount.ToString());
                

                //PHÂN HÓA.

                Console.WriteLine("---");
                EventLog[] eL = evoEng.doDissocating(genCount);

                //ĐỘT BIẾN.

                Console.WriteLine("---");
                eL = evoEng.doMutating(genCount).Concat(eL).ToArray();

                if (eL.Length > 0)
                {
                    Array.Resize(ref tracer[genCount - 1].eventLog, eL.Length);
                    for (int i = 0; i < eL.Length; i++)
                    {
                        tracer[genCount - 1].eventLog[i] = new EventLog();
                        tracer[genCount - 1].eventLog[i] = eL[i].Clone() as EventLog;
                    }
                }

                //GHI THỐNG KÊ.
                
                Tracer currTracer = new Tracer();
                tracer[genCount - 1].Write(genCount, evoEng.obj, false);

                //HIỆN TỔNG KẾT.

                showGenSummary(false, false, genCount);

                //PHÁT HIỆN BIẾN CỐ.
                
                int incidentCode = evoEng.detectIncident(tracer, genCount);
                if (incidentCode >= 0)
                {
                    //Xử lý chết.
                    Array.Resize(ref evoEng.deathNote, evoEng.deathNote.Length + 1);
                    evoEng.deathNote[evoEng.deathNote.Length - 1] = new DeathNote();
                    evoEng.deathNote[evoEng.deathNote.Length - 1].Write(genCount - 1, incidentCode);
                    deathCurrent++;
                    deathTotal++;
                    alive = false;

                    //Hiển thị biến cố.
                    showIncidentDetail(incidentCode);
                    showDeathNote();
                }

                //Đặc cách chuyển thế hệ tiếp theo.
                if (deathCurrent >= lib.threshold_DeathCount_LetItGo)
                {
                    Array.Resize(ref evoEng.letItGo, evoEng.letItGo.Length + 1);
                    evoEng.letItGo[evoEng.letItGo.Length - 1] = genCount - 1;
                    alive = true;
                }

                stopWatch.Stop();

                //KIỂM TRA ĐIỀU KIỆN DỪNG.

                //Dừng theo chu kỳ.
                if ((genCount == nextPause && alive) || (genCount == maxGen && alive))
                {
                    Console.WriteLine("---");
                    Console.Write("[1] Continue, [2] Object analysis: ");
                    string opCodeStr = Console.ReadLine();
                    int opCode = 0;
                    if (!int.TryParse(opCodeStr, out opCode))
                        opCode = 1;
                    switch (opCode)
                    {
                        case 1:
                            break;
                        case 2:
                            {                              
                                objectAnalysis();
                                break;
                            }
                        default:
                            break;
                    }
                    if (genCount < maxGen)
                    {
                        Console.Write("Pause after [0]: ");
                        if (int.TryParse(Console.ReadLine(), out nextPause) && nextPause >= 1 && nextPause <= maxGen - genCount)
                            nextPause = genCount + nextPause;
                    }
                }
            }

            //Tổng kết.
            showProcSummary();
            Console.WriteLine("---\r\nTime elapsed: " + ((double)stopWatch.ElapsedMilliseconds / 1000).ToString() + "s");

            Console.Write("Save Plot Models [No]: ");
            if (!string.IsNullOrEmpty(Console.ReadLine()))
            {
                string folderName1 = DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Millisecond.ToString("000") + "-" + "summary" + lib.getId_Random(3);
                string folderName2 = DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Millisecond.ToString("000") + "-" + "evaluate" + lib.getId_Random(3);
                System.IO.Directory.CreateDirectory(lib.folderPath + folderName1);
                System.IO.Directory.CreateDirectory(lib.folderPath + folderName2);
                try
                {
                    for (int i = 0; i < genCount; i++)
                    {
                        plot_Summary(i + 1, true, folderName1);
                        plot_Evaluator(i + 1, true, folderName2);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Console.WriteLine("Done.");
            }

            Console.ReadLine();
        }

        static void objectAnalysis()
        {
        tag_0:
            plot_Summary(genCount, false, "");
            Console.Write("---\r\nGen analysis [None]: ");
            string genAnalysisStr = Console.ReadLine();
            int genAnalysis = 0;
            if (!int.TryParse(genAnalysisStr, out genAnalysis))
                goto tag_1;
            else if (genAnalysis <= 0 || genAnalysis > genCount)
                genAnalysis = genCount;

            showGenSummary(false, false, genAnalysis);
            plot_Evaluator(genAnalysis, false, "");

        tag_1:
            Console.Write("Number of evaluated nodes [None]: ");
            string evaluatedNodeCountStr = Console.ReadLine();
            int evalutedNodeCount = 0;
            if (!int.TryParse(evaluatedNodeCountStr, out evalutedNodeCount))
                goto tag_2;
            else if (evalutedNodeCount <= 0 || evalutedNodeCount >= tracer[genAnalysis - 1].savedObj.getAliveNodes().Length)
                goto tag_1;

            evoEng.evaluator = new Evaluator();
            evoEng.evaluator.tracer = tracer[genCount - 1].Clone() as Tracer;
            evoEng.evaluator.evNodeCount = evalutedNodeCount;
            evoEng.evaluator.Execute();
            drawObject(evalutedNodeCount, genCount - 1);

        tag_2:
            Console.Write("Again [Yes]: ");
            if (string.IsNullOrEmpty(Console.ReadLine()))
                goto tag_0;
        }

        static void plot_Summary(int genPlot, bool saveImg, string folderName)
        {
            if (genPlot > genCount)
            {
                Console.WriteLine("x1");
                return;
            }

            //Tổng số nút.
            var line1 = new OxyPlot.Series.LineSeries()
            {
                Title = "Total",
                Color = OxyPlot.OxyColors.Black,
                StrokeThickness = 3,
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Circle
            };

            //Số nút sống.
            var line2 = new OxyPlot.Series.LineSeries()
            {
                Title = "Total Alive",
                Color = OxyPlot.OxyColors.Red,
                StrokeThickness = 3,
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Circle
            };

            //Tỷ lệ nút phân hóa.
            var line3 = new OxyPlot.Series.LineSeries()
            {
                Title = "Ratio Dissociated",
                Color = OxyPlot.OxyColors.Gray,
                StrokeThickness = 3,
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Circle
            };

            //Tỷ lệ nút đột biến.
            var line4 = new OxyPlot.Series.LineSeries()
            {
                Title = "Ratio Mutated",
                Color = OxyPlot.OxyColors.DarkGray,
                StrokeThickness = 3,
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Circle
            };

            //Tổng số nút biến động do phân hóa.
            var line5 = new OxyPlot.Series.LineSeries()
            {
                Title = "Dissociating Score",
                Color = OxyPlot.OxyColors.Blue,
                StrokeThickness = 3,
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Circle
            };

            //Tổng số nút biến động do đột biến.
            var line6 = new OxyPlot.Series.LineSeries()
            {
                Title = "Mutating Score",
                Color = OxyPlot.OxyColors.DarkBlue,
                StrokeThickness = 3,
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Circle
            };

            //Tổng số nút biến động.
            var line7 = new OxyPlot.Series.LineSeries()
            {
                Title = "Total Score",
                Color = OxyPlot.OxyColors.BlueViolet,
                StrokeThickness = 3,
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Circle
            };

            //Số lần chết đối tượng.
            var sSerie1 = new OxyPlot.Series.ScatterSeries()
            {
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Circle,
                MarkerFill = OxyPlot.OxyColors.Red
            };

            //Thế hệ được đặc cách chuyển thế hệ.
            var sSerie2 = new OxyPlot.Series.ScatterSeries()
            {
                MarkerSize = 4,
                MarkerType = OxyPlot.MarkerType.Square,
                MarkerFill = OxyPlot.OxyColors.Green
            };

            //Hệ số khuếch đại cho tỷ lệ phân hóa / đột biến.
            int maxTotal = 0;
            for (int i = 0; i < genPlot; i++)
                if (tracer[i].total > maxTotal)
                    maxTotal = tracer[i].total;
            double alpha = 1.0;
            alpha = (double)maxTotal / 100;

            for (int i = 0; i < genPlot; i++)
            {
                line1.Points.Add(new OxyPlot.DataPoint(tracer[i].generation, tracer[i].total));
                line2.Points.Add(new OxyPlot.DataPoint(tracer[i].generation, tracer[i].totalAlive));
                line3.Points.Add(new OxyPlot.DataPoint(tracer[i].generation, tracer[i].ratio_Dissociation * 100 * alpha));
                line4.Points.Add(new OxyPlot.DataPoint(tracer[i].generation, tracer[i].ratio_Mutation * 100 * alpha));
                line5.Points.Add(new OxyPlot.DataPoint(tracer[i].generation, tracer[i].count_DerivatedByDissociating));
                line6.Points.Add(new OxyPlot.DataPoint(tracer[i].generation, tracer[i].count_DerivatedByMutating));
                line7.Points.Add(new OxyPlot.DataPoint(tracer[i].generation, tracer[i].count_DerivatedTotal));
            }
            if (evoEng.deathNote.Length > 0)
            {
                int[] gen = new int[evoEng.deathNote.Length];
                for (int i = 0; i < evoEng.deathNote.Length; i++)
                    gen[i] = evoEng.deathNote[i].generation;
                gen = gen.Distinct().ToArray();
                int[] count = new int[gen.Length];
                for (int i = 0; i < gen.Length; i++)
                    for (int j = 0; j < evoEng.deathNote.Length; j++)
                        if (evoEng.deathNote[j].generation == gen[i])
                            count[i]++;
                for (int i = 0; i < gen.Length; i++)
                {
                    if (gen[i] <= genPlot)
                    {
                        sSerie1.Points.Add(new OxyPlot.Series.ScatterPoint(gen[i], count[i]));
                        if (evoEng.letItGo.Contains(gen[i]))
                            sSerie2.Points.Add(new OxyPlot.Series.ScatterPoint(gen[i], count[i]));
                    }
                }
            }

            var modelNode = new OxyPlot.PlotModel
            {
                Title = "Alive / Total, Dissociating n Mutating Ratio"
            };
            modelNode.Series.Add(line1);
            modelNode.Series.Add(line2);
            modelNode.Series.Add(line3);
            modelNode.Series.Add(line4);
            modelNode.Series.Add(line5);
            modelNode.Series.Add(line6);
            modelNode.Series.Add(line7);
            modelNode.Series.Add(sSerie1);
            modelNode.Series.Add(sSerie2);

            if (saveImg)
            {
                var pngExporter = new PngExporter { Width = 1200, Height = 800 };
                string fileName = "gen-" + genPlot.ToString() + ".png";           
                pngExporter.ExportToFile(modelNode, lib.folderPath + folderName + "/" + fileName);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.Run(new FormPlotSummary(modelNode));
            }
        }

        static void plot_Weight(int genShowed)
        {
            Node[] aliveNode = tracer[genShowed - 1].savedObj.getAliveNodes();

            //Trọng số.
            var sSerie1 = new OxyPlot.Series.ScatterSeries()
            {
                MarkerSize = 3,
                MarkerType = OxyPlot.MarkerType.Circle,
                MarkerFill = OxyPlot.OxyColors.Red
            };
            for (int i = 0; i < aliveNode.Length; i++)
                sSerie1.Points.Add(new OxyPlot.Series.ScatterPoint(aliveNode[i].id, aliveNode[i].weight));

            //Trọng số (sắp xếp).
            var sSerie2 = new OxyPlot.Series.ScatterSeries()
            {
                MarkerSize = 3,
                MarkerType = OxyPlot.MarkerType.Circle,
                MarkerFill = OxyPlot.OxyColors.Red
            };
            Node[] aliveNodeDesc = aliveNode.OrderByDescending(x => x.weight).ToArray();
            for (int i = 0; i < aliveNodeDesc.Length; i++)
                sSerie2.Points.Add(new OxyPlot.Series.ScatterPoint(i, aliveNodeDesc[i].weight));

            //Nút con.
            var sSerie3 = new OxyPlot.Series.ScatterSeries()
            {
                MarkerSize = 3,
                MarkerType = OxyPlot.MarkerType.Circle,
                MarkerFill = OxyPlot.OxyColors.Green
            };
            for (int i = 0; i < aliveNode.Length; i++)
                sSerie3.Points.Add(new OxyPlot.Series.ScatterPoint(aliveNode[i].id, aliveNode[i].cNode.Length));

            //Nút kế cận.
            var sSerie4 = new OxyPlot.Series.ScatterSeries()
            {
                MarkerSize = 3,
                MarkerType = OxyPlot.MarkerType.Circle,
                MarkerFill = OxyPlot.OxyColors.Blue
            };
            for (int i = 0; i < aliveNode.Length; i++)
                sSerie4.Points.Add(new OxyPlot.Series.ScatterPoint(aliveNode[i].id, aliveNode[i].aNode.Length));

            //Trọng số (sắp xếp + chồng chất).

            var modelWeight = new OxyPlot.PlotModel
            {
                Title = "Weight"
            };
            modelWeight.Series.Add(sSerie1);
            var modelWeightDesc = new OxyPlot.PlotModel
            {
                Title = "Weight (Descending)"
            };
            modelWeightDesc.Series.Add(sSerie2);

            var modelCANode = new OxyPlot.PlotModel
            {
                Title = "Child / Adjacency Node"
            };
            modelCANode.Series.Add(sSerie3);
            modelCANode.Series.Add(sSerie4);

            //Vẽ đồ thị từ Windows Form.
            Application.EnableVisualStyles();
            Application.Run(new FormPlotWeight(modelWeight, modelWeightDesc, modelCANode));
        }

        static void plot_Evaluator(int genShowed, bool saveImg, string folderName)
        {
            Evaluator[] evaluator = new Evaluator[1];
            Object obj = tracer[genShowed - 1].savedObj.Clone() as Object;
            Node[] aliveNode = obj.getAliveNodes();
            aliveNode = aliveNode.OrderByDescending(x => x.weight).ToArray();
            if (aliveNode.Length > 0)
            {
                double[] ratio_P = new double[aliveNode.Length];
                double[] ratio_A = new double[aliveNode.Length];
                double[] ratio_W = new double[aliveNode.Length];
                Array.Resize(ref evaluator, aliveNode.Length);
                for (int i = 1; i < aliveNode.Length; i++)
                {
                    evaluator[i] = new Evaluator();
                    evaluator[i].tracer = tracer[genShowed - 1];
                    evaluator[i].evNodeCount = i;
                    evaluator[i].Execute();
                    ratio_P[i] = evaluator[i].ratio_PNode;
                    ratio_A[i] = evaluator[i].ratio_ANode;
                    ratio_W[i] = evaluator[i].ratio_Weight;
                }

                //ratio_P.
                var sSerie1 = new OxyPlot.Series.ScatterSeries()
                {
                    MarkerSize = 3,
                    MarkerType = OxyPlot.MarkerType.Circle,
                    MarkerFill = OxyPlot.OxyColors.Blue
                };
                //ratio_A.
                var sSerie2 = new OxyPlot.Series.ScatterSeries()
                {
                    MarkerSize = 3,
                    MarkerType = OxyPlot.MarkerType.Circle,
                    MarkerFill = OxyPlot.OxyColors.Green
                };
                //ratio_W.
                var sSerie3 = new OxyPlot.Series.ScatterSeries()
                {
                    MarkerSize = 3,
                    MarkerType = OxyPlot.MarkerType.Circle,
                    MarkerFill = OxyPlot.OxyColors.Red
                };
                for (int i = 1; i < aliveNode.Length; i++)
                {
                    sSerie1.Points.Add(new OxyPlot.Series.ScatterPoint(i, ratio_P[i]));
                    sSerie2.Points.Add(new OxyPlot.Series.ScatterPoint(i, ratio_A[i]));
                    sSerie3.Points.Add(new OxyPlot.Series.ScatterPoint(i, ratio_W[i]));
                }

                var modelEvaluator = new OxyPlot.PlotModel
                {
                    Title = "Evaluator"
                };
                modelEvaluator.Series.Add(sSerie1);
                modelEvaluator.Series.Add(sSerie2);
                modelEvaluator.Series.Add(sSerie3);

                if (saveImg)
                {
                    var pngExporter = new PngExporter { Width = 1200, Height = 800 };
                    string fileName = "gen-" + genShowed.ToString() + ".png";
                    pngExporter.ExportToFile(modelEvaluator, lib.folderPath + folderName + "/" + fileName);
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.Run(new FormPlotEvaluator(modelEvaluator));
                }
            }
        }

        static void showObjectByDescedants(int genShowed)
        {
            Console.WriteLine("---");
            Console.WriteLine("Show object (generation " + genShowed.ToString() + ")");

            //Node[] aliveNodes = evoEng.obj.getAliveNodes();
            Object showObj = tracer[genShowed - 1].savedObj.Clone() as Object;
            Node[] aliveNode = showObj.getAliveNodes();

            if (aliveNode.Length > 0)
            {
                int[] aliveNodeId = new int[aliveNode.Length];
                for (int i = 0; i < aliveNode.Length; i++)
                    aliveNodeId[i] = aliveNode[i].id;
                Node[] rootNodes = new Node[] { };
                rootNodes = rootNodes.Concat(aliveNode.Where(x => x.id == 0 || !aliveNodeId.Contains(x.pNode[0])).Distinct().ToArray()).ToArray();
                Console.WriteLine("Total root node(s): " + rootNodes.Length.ToString());
                if (rootNodes.Length > 0)
                    for (int i = 0; i < rootNodes.Length; i++)
                        showObjectByDescedants_Node(1, rootNodes[i].id, genShowed);
            }
        }

        static void showObjectByDescedants_Node(int depth, int nodeId, int genShowed)
        {
            string dStr = "";
            for (int i = 0; i < depth; i++)
                dStr += "--";

            Console.Write(dStr + nodeId.ToString());

            //Xuất nút kế cận.
            Console.Write(" [");
            if (tracer[genShowed - 1].savedObj.node[nodeId].aNode.Length > 0)
                for (int j = 0; j < tracer[genShowed - 1].savedObj.node[nodeId].aNode.Length; j++)
                    Console.Write(tracer[genShowed - 1].savedObj.node[nodeId].aNode[j].ToString() + ",");
            Console.Write("]");

            Console.Write("\r\n");

            if (tracer[genShowed - 1].savedObj.node[nodeId].cNode.Length > 0)
            {
                for (int i = 0; i < tracer[genShowed - 1].savedObj.node[nodeId].cNode.Length; i++)
                {
                    if (tracer[genShowed - 1].savedObj.node[tracer[genShowed - 1].savedObj.node[nodeId].cNode[i]].alive && tracer[genShowed - 1].savedObj.node[nodeId].cNode[i] != 0)
                        showObjectByDescedants_Node(depth + 1, tracer[genShowed - 1].savedObj.node[nodeId].cNode[i], genShowed);
                }
            }
        }

        static void showLogs(int genShowed)
        {
            Console.WriteLine("---");
            Console.WriteLine("Show log (generation " + genShowed.ToString() + ")");
            Console.WriteLine("Total log entry: " + tracer[tracer.Length - 1].eventLog.Length.ToString());
            if (tracer[tracer.Length - 1].eventLog.Length > 0)
            {
                for (int i = tracer[tracer.Length - 1].eventLog.Length - 1; i >= 0; i--)
                {
                    string s = "";
                    if (tracer[tracer.Length - 1].eventLog[i].accLogEntry.Length > 0)
                        for (int j = 0; j < tracer[tracer.Length - 1].eventLog[i].accLogEntry.Length; j++)
                            s += tracer[tracer.Length - 1].eventLog[i].accLogEntry[j] + "-";
                    Console.WriteLine(tracer[tracer.Length - 1].eventLog[i].id + " [" + tracer[tracer.Length - 1].eventLog[i].eventSource.ToString() + "," + tracer[tracer.Length - 1].eventLog[i].threadInit.ToString() + "][" + tracer[tracer.Length - 1].eventLog[i].nodeId.ToString() + "][" + tracer[tracer.Length - 1].eventLog[i].count_Create.ToString() + "," + tracer[tracer.Length - 1].eventLog[i].count_Delete.ToString() + "," + tracer[tracer.Length - 1].eventLog[i].count_Update.ToString() + "," + tracer[tracer.Length - 1].eventLog[i].count_Total.ToString() + "][" + tracer[tracer.Length - 1].eventLog[i].count_AccCreate.ToString() + "," + tracer[tracer.Length - 1].eventLog[i].count_AccDelete.ToString() + "," + tracer[tracer.Length - 1].eventLog[i].count_AccUpdate.ToString() + "," + tracer[tracer.Length - 1].eventLog[i].count_AccTotal.ToString() + "][" + s + "]");
                }
            }
            showDMThreads();
        }

        static void showIncidentDetail(int incidentCode)
        {
            switch (incidentCode)
            {
                case 0:
                    {
                        Console.WriteLine("---");
                        Console.WriteLine("Incident code 0: Previous " + tracer[genCount - 2].totalAlive.ToString() + " alive(s), current " + tracer[genCount - 1].count_DerivatedTotal.ToString() + " (" + tracer[genCount - 1].count_DerivatedByDissociating.ToString() + "+" + tracer[genCount - 1].count_DerivatedByMutating.ToString() + ") dissociating / mutating event(s)");
                        Console.WriteLine("Threshold allowed " + lib.threshold_EventRatio.ToString() + ", this generation " + ((double)tracer[genCount - 1].count_DerivatedTotal / (double)tracer[genCount - 2].totalAlive).ToString());
                        showDMThreads();
                        break;
                    }
                case 1:
                    {
                        Console.WriteLine("---");
                        Console.WriteLine("Incident code 1: Total alive(s), previous " + tracer[genCount - 2].totalAlive.ToString() + ", current " + tracer[genCount - 1].totalAlive.ToString());
                        Console.WriteLine("Threshold allowed " + (1.0 - lib.threshold_DeadRatio).ToString() + ", this generation " + ((double)tracer[genCount - 1].totalAlive / (double)tracer[genCount - 2].totalAlive).ToString());
                        showDMThreads();
                        break;
                    }
                case 2:
                    {
                        Console.WriteLine("---");
                        Console.WriteLine("Incident code 2: Empty object");
                        showDMThreads();
                        break;
                    }
            } 
        }

        static void showDMThreads()
        {
            Console.WriteLine("---");
            Console.Write("D thread(s): ");
            if (tracer[genCount - 1].eventLog_DThreading.Length > 0)
                Console.Write(tracer[genCount - 1].eventLog_DThreading.Length.ToString() + " thread(s), derivated " + tracer[genCount - 1].count_DerivatedByDissociating.ToString() + ": ");
                for (int i = 0; i < tracer[genCount - 1].eventLog_DThreading.Length; i++)
                    Console.Write("[" + tracer[genCount - 1].eventLog_DThreading[i].id + "][id" + tracer[genCount - 1].eventLog_DThreading[i].nodeId + "-" + tracer[genCount - 1].eventLog_DThreading[i].count_AccTotal + "] ");
            Console.Write("\r\n");
            Console.Write("M thread(s): ");
            if (tracer[genCount - 1].eventLog_MThreading.Length > 0)
                Console.Write(tracer[genCount - 1].eventLog_MThreading.Length.ToString() + " thread(s), derivated " + tracer[genCount - 1].count_DerivatedByMutating.ToString() + ": ");
                for (int i = 0; i < tracer[genCount - 1].eventLog_MThreading.Length; i++)
                    Console.Write("[" + tracer[genCount - 1].eventLog_MThreading[i].id + "][id" + tracer[genCount - 1].eventLog_MThreading[i].nodeId + "-" + tracer[genCount - 1].eventLog_MThreading[i].count_AccTotal + "] ");
            Console.Write("\r\n");
        }

        static void showGenSummary(bool isShowLogs, bool isShowObject, int genShowed)
        {
            Console.WriteLine("---");
            if (isShowLogs)
                showLogs(genShowed);
            if (isShowObject)
                showObjectByDescedants(genShowed);
            Console.WriteLine("Generation " + genShowed.ToString() + " summary: " + tracer[genShowed - 1].totalAlive.ToString() + "/" + tracer[genShowed - 1].total.ToString() + " alive(s)");
            Console.WriteLine("Dissociation ratio " + tracer[genShowed - 1].ratio_Dissociation.ToString("F5") + " derivated " + tracer[genShowed - 1].count_DerivatedByDissociating.ToString() + ", mutation ratio " + tracer[genShowed - 1].ratio_Mutation.ToString("F5") + " derivated " + tracer[genShowed - 1].count_DerivatedByMutating.ToString());
        }

        static void showDeathNote()
        {
            Console.WriteLine("---");
            Console.WriteLine("Death note: " + deathCurrent.ToString() + "/" + deathTotal.ToString());
            int x = tracer[genCount - 2].totalAlive;
            int y = tracer[genCount - 2].savedObj.getAliveNodes().Length;
            if (x != y)
            {
                Console.WriteLine("Tracer mismatch: Generation " + (genCount - 1).ToString() + " " + x.ToString() + " vs " + y.ToString());
                Console.ReadLine();
            }
            else
                Console.WriteLine("Will be restored: Generation " + (genCount - 1).ToString() + ", " + x.ToString() + "/" + tracer[genCount - 2].total.ToString() + " alive(s)");
        }

        static void showProcSummary()
        {
            Console.WriteLine("---\r\nSUMMARY");
            Console.WriteLine("Death total " + deathTotal.ToString());
        }

        static void drawObject(int evaluatedNodeCount, int genShowed)
        {
            //showGenSummary(false, true, genCount - 1);
            FormDrawObject frmShObj = new FormDrawObject();
            frmShObj.evaluator = evoEng.evaluator.Clone() as Evaluator;
            Application.Run(frmShObj);
        }
    }
}
