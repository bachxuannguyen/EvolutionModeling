using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoModeling
{
    class EvoEngine
    {
        private readonly Random r = new Random();
        private readonly Library lib = new Library();
        public Object obj = new Object();
        public DeathNote[] deathNote = new DeathNote[] { };
        public Evaluator evaluator = new Evaluator();
        public int[] letItGo = new int[] { };

        public bool Init()
        {
            //Khởi tạo đối tượng và nút đầu tiên.
            obj = new Object();
            obj.Init();
            obj.nodeCreate(true, true, new int[] { 0 }, 0);

            return true;
        }

        public EventLog[] doDissocating(int gen)
        {
            EventLog[] accLog = new EventLog[] { };

            //Tính số nút sẽ phân hóa.
            int countDisNode = lib.GetCount_DisNode(obj.getAliveNodes().Length);

            //Xử lý từng nút phân hóa..
            if (countDisNode > 0)
            {
                //Tạo mảng chứa các nút phân hóa.
                int[] disNode = new int[countDisNode];
                for (int i = 0; i < disNode.Length; i++)
                    disNode[i] = -1;

                //Đưa ngẫu nhiên các nút của đối tượng vào mảng các nút phân hóa.
                int[] index = new int[obj.node.Length];
                for (int i = 0; i < index.Length; i++)
                    index[i] = i;
                index = index.Where(x => obj.node[x].alive).ToArray();
                disNode = index.OrderBy(x => r.Next()).Take(countDisNode).ToArray();

                obj.tracer_disRatio = (double)disNode.Length / (double)obj.getAliveNodes().Length;

                //Hiển thị các nút sẽ phân hóa.
                string s = "";
                if (disNode.Length > 0)
                {                
                    for (int i = 0; i < disNode.Length; i++)
                    {
                        s += "[" + obj.node[disNode[i]].id.ToString() + "]";
                    }
                }
                Console.WriteLine("Dissociated node(s) (" + disNode.Length.ToString() + "/" + obj.getAliveNodes().Length.ToString() + "): " + s);

                //Xử lý phân hóa từng nút.
                if (disNode.Length > 0)
                {
                    for (int i = 0; i < disNode.Length; i++)
                    {
                        for (int j = 0; j < lib.GetCount_NewDisNode(); j++)
                        {
                            int[] myPNode = { disNode[i] };
                            accLog = obj.nodeCreate(true, true, myPNode, gen).Concat(accLog).ToArray();
                        }
                    }
                }
                Console.Write("\r\n");
            }

            //Tính lại các trọng số.
            if (obj.node.Length > 0)
                for (int i = 0; i < obj.node.Length; i++)
                    if (obj.node[i].alive)
                        obj.weightCalculate(obj.node[i].id);

            return accLog;
        }

        public EventLog[] doMutating(int gen)
        {
            EventLog[] accLog = new EventLog[] { };

            //Lấy số lượng nút sẽ đột biến.
            int countMutNode = lib.GetCount_MutNode(obj.getAliveNodes().Length);

            //Xử lý từng nút.
            if (countMutNode > 0)
            {
                //Tạo mảng chứa các nút đột biến.
                int[] mutNode = new int[countMutNode];
                for (int i = 0; i < mutNode.Length; i++)
                    mutNode[i] = -1;

                //Đưa ngẫu nhiên các nút của đối tượng vào mảng các nút đột biến.
                int[] index = new int[obj.node.Length];
                for (int i = 0; i < index.Length; i++)
                    index[i] = i;
                index = index.Where(x => obj.node[x].alive).ToArray();
                mutNode = index.OrderBy(x => r.Next()).Take(countMutNode).ToArray();
                if (mutNode.Length > 0)
                    mutNode = mutNode.Where(x => obj.node[x].alive).ToArray();

                obj.tracer_mutRatio = (double)mutNode.Length / (double)obj.getAliveNodes().Length;

                //Hiển thị các nút sẽ đột biến.
                string s = "";
                if (mutNode.Length > 0)
                {
                    for (int i = 0; i < mutNode.Length; i++)
                    {
                        s += "[" + obj.node[mutNode[i]].id.ToString() + "]";
                    }
                }
                Console.WriteLine("Mutated node(s) (" + mutNode.Length.ToString() + "/" + obj.getAliveNodes().Length.ToString() + "): " + s);

                //Xử lý đột biến từng nút.
                if (mutNode.Length > 0)
                {
                    for (int i = 0; i < mutNode.Length; i++)
                    {
                        //Nút ngoại lệ khi lan truyền.
                        int[] exception = { mutNode[i] };
                        //Đột biến là mất hay thay đổi.
                        if (lib.GetType_Mutation(obj.getAliveNodes().Length) == 1)
                        {
                            accLog = obj.nodeDelete(false, true, mutNode[i], 0, exception, gen).Concat(accLog).ToArray();
                        }
                        else
                        {
                            accLog = obj.nodeUpdate(false, true, mutNode[i], 0, exception, gen).Concat(accLog).ToArray();
                        }
                    }
                }
                Console.Write("\r\n");
            }

            //Tính lại các trọng số.
            if (obj.node.Length > 0)
                for (int i = 0; i < obj.node.Length; i++)
                    if (obj.node[i].alive)
                        obj.weightCalculate(obj.node[i].id);

            return accLog;
        }

        public int detectIncident(Tracer[] tracer, int genCount)
        {
            //Quá nhiều biến động do phân hóa và đột biến.
            if (genCount > 1 && tracer[genCount - 2].totalAlive > lib.threshold_MeanNodeCount)
                if ((double)tracer[genCount - 1].count_DerivatedTotal > (double)tracer[genCount - 2].totalAlive * lib.threshold_EventRatio)
                    return 0;

            //Quá nhiều nút chết.
            if (genCount > 1 && tracer[genCount - 2].totalAlive > lib.threshold_MeanNodeCount)
                if ((double)tracer[genCount - 1].totalAlive < ((1.0 - lib.threshold_DeadRatio) * (double)tracer[genCount - 2].totalAlive))
                    return 1;

            //Toàn bộ nút chết.
            if (tracer[genCount - 1].totalAlive == 0)
                return 2;

            return -1;
        }
    }
}
