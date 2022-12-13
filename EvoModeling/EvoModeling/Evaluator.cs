using System;
using System.Linq;

namespace EvoModeling
{
    public class Evaluator : ICloneable
    {
        //Vào.
        public Tracer tracer = new Tracer();
        public int evNodeCount = 0;
        //Ra.
        //Mảng nút - tất cả.
        public Node[] node_Total;
        //Mảng nút - còn sống.
        public Node[] node_Alive;
        //Mảng nút - đánh giá.
        public Node[] node_Evaluated;
        //Số nút - tất cả.
        public int nodeCount_Total;
        //Số nút - còn sống.
        public int nodeCount_Alive;
        //Số nút - đánh giá.
        public int nodeCount_Evaluated;
        //Số nút - tỷ lệ.
        public double ratio_Node;
        //Trọng số - tất cả.
        public int weight_Total;
        //Trọng số - số nút đánh giá.
        public int weight_Evaluated;
        //Trọng số - tỷ lệ.
        public double ratio_Weight;
        //Số liên kết cha con thực có.
        public int pNodeCount;
        //Tỷ lệ liên kết cha con thực có.
        public double ratio_PNode;
        //Số liên kết kế cận thực có.
        public int aNodeCount;
        //Tỷ lệ liên kết kế cận thực có.
        public double ratio_ANode;

        public object Clone()
        {
            Evaluator clonedEval = new Evaluator();
            clonedEval.aNodeCount = aNodeCount;
            clonedEval.evNodeCount = evNodeCount;
            clonedEval.nodeCount_Evaluated = nodeCount_Evaluated;
            clonedEval.nodeCount_Alive = nodeCount_Alive;
            clonedEval.pNodeCount = pNodeCount;
            clonedEval.ratio_ANode = ratio_ANode;
            clonedEval.ratio_Node = ratio_Node;
            clonedEval.ratio_PNode = ratio_PNode;
            clonedEval.ratio_Weight = ratio_Weight;
            clonedEval.tracer = tracer;
            clonedEval.weight_Evaluated = weight_Evaluated;
            clonedEval.weight_Total = weight_Total;
            clonedEval.nodeCount_Total = nodeCount_Total;
            Array.Resize(ref clonedEval.node_Alive, nodeCount_Alive);
            Array.Resize(ref clonedEval.node_Evaluated, nodeCount_Evaluated);
            Array.Resize(ref clonedEval.node_Total, nodeCount_Total);
            if (node_Alive.Length > 0)
                for (int i = 0; i < nodeCount_Alive; i++)
                {
                    clonedEval.node_Alive[i] = new Node();
                    clonedEval.node_Alive[i] = node_Alive[i].Clone() as Node;
                }
            if (node_Evaluated.Length > 0)
                for (int i = 0; i < nodeCount_Evaluated; i++)
                {
                    clonedEval.node_Evaluated[i] = new Node();
                    clonedEval.node_Evaluated[i] = node_Evaluated[i].Clone() as Node;
                }
            if (node_Total.Length > 0)
                for (int i = 0; i < nodeCount_Total; i++)
                {
                    clonedEval.node_Total[i] = new Node();
                    clonedEval.node_Total[i] = node_Total[i].Clone() as Node;
                }
            return clonedEval;
        }

        public void Execute()
        {
            if (tracer.generation == 0 || evNodeCount == 0)
                return;
            Object obj = tracer.savedObj.Clone() as Object;

            //Mảng chứa các nút.
            node_Total = obj.node;
            node_Total = node_Total.OrderByDescending(x => x.weight).ToArray();
            node_Alive = obj.getAliveNodes();
            node_Alive = node_Alive.OrderByDescending(x => x.weight).ToArray();
            if (evNodeCount > node_Alive.Length)
                return;
            node_Evaluated = node_Alive.Take(evNodeCount).ToArray();

            //Số lượng các nút.
            nodeCount_Total = obj.node.Length;
            nodeCount_Alive = node_Alive.Length;
            nodeCount_Evaluated = node_Evaluated.Length;
            ratio_Node = (double)nodeCount_Evaluated / nodeCount_Alive;

            //Trọng số.
            for (int i = 0; i < node_Evaluated.Length; i++)
                weight_Evaluated += node_Evaluated[i].weight;
            int[] accWeight = new int[node_Alive.Length];
            accWeight[0] = node_Alive[0].weight;
            for (int i = 1; i < node_Alive.Length; i++)
                accWeight[i] = accWeight[i - 1] + node_Alive[i].weight;
            weight_Total = accWeight[node_Alive.Length - 1];
            ratio_Weight = (double)weight_Evaluated / weight_Total;

            //Tạo mảng chứa ID của nút.
            int[] evaluatedNodeId = new int[node_Evaluated.Length];
            for (int i = 0; i < node_Evaluated.Length; i++)
                evaluatedNodeId[i] = node_Evaluated[i].id;

            //Nút cha.
            for (int i = 0; i < node_Evaluated.Length; i++)
                if (evaluatedNodeId.Contains(node_Evaluated[i].pNode[0]))
                    pNodeCount++;
            ratio_PNode = (double)pNodeCount / node_Evaluated.Length;

            //Nút kế cận.
            int possANode = 0;
            for (int i = 0; i < node_Evaluated.Length; i++)
            {
                possANode += i;
                if (node_Evaluated[i].aNode.Length > 0)
                    for (int j = 0; j < node_Evaluated[i].aNode.Length; j++)
                        if (evaluatedNodeId.Contains(node_Evaluated[i].aNode[j]))
                            aNodeCount++;
            }
            aNodeCount = aNodeCount / 2;
            ratio_ANode = (double)aNodeCount / possANode;
        }
    }
}
