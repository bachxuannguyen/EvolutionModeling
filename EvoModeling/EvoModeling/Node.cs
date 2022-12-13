using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoModeling
{
    public class Node : ICloneable
    {
        public int id;
        public int weight;
        public bool alive;
        public int[] cNode;
        public int[] pNode;
        public int[] aNode;

        public object Clone()
        {
            Node clonedNode = new Node();
            clonedNode.id = id;
            clonedNode.weight = weight;
            clonedNode.alive = alive;
            clonedNode.cNode = cNode;
            clonedNode.pNode = pNode;
            clonedNode.aNode = aNode;
            return clonedNode;
        }
    }
}
