using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoModeling
{
    public class Tracer : ICloneable
    {
        public Library lib = new Library();

        public EventLog[] eventLog = new EventLog[] { };
        public EventLog[] eventLog_DThreading = new EventLog[] { };
        public EventLog[] eventLog_MThreading = new EventLog[] { };

        public Object savedObj = new Object();

        public int generation;
        public int total;
        public int totalAlive;
        public double ratio_Dissociation;
        public double ratio_Mutation;

        public int count_DerivatedByDissociating;
        public int count_DerivatedByMutating;
        public int count_DerivatedTotal;

        public object Clone()
        {
            Tracer clonedTracer = new Tracer();
            Array.Resize(ref clonedTracer.eventLog, eventLog.Length);
            Array.Resize(ref clonedTracer.eventLog_DThreading, eventLog_DThreading.Length);
            Array.Resize(ref clonedTracer.eventLog_MThreading, eventLog_MThreading.Length);
            if (eventLog.Length > 0)
            {
                for (int i = 0; i < eventLog.Length; i++)
                {
                    clonedTracer.eventLog[i] = new EventLog();
                    clonedTracer.eventLog[i] = eventLog[i].Clone() as EventLog;
                }
            }
            if (eventLog_DThreading.Length > 0)
            {
                for (int i = 0; i < eventLog_DThreading.Length; i++)
                {
                    clonedTracer.eventLog_DThreading[i] = new EventLog();
                    clonedTracer.eventLog_DThreading[i] = eventLog_DThreading[i].Clone() as EventLog;
                }
            }
            if (eventLog_MThreading.Length > 0)
            {
                for (int i = 0; i < eventLog_MThreading.Length; i++)
                {
                    clonedTracer.eventLog_MThreading[i] = new EventLog();
                    clonedTracer.eventLog_MThreading[i] = eventLog_MThreading[i].Clone() as EventLog;
                }
            }
            clonedTracer.savedObj = savedObj.Clone() as Object;
            clonedTracer.generation = generation;
            clonedTracer.total = total;
            clonedTracer.totalAlive = totalAlive;
            clonedTracer.ratio_Dissociation = ratio_Dissociation;
            clonedTracer.ratio_Mutation = ratio_Mutation;
            clonedTracer.count_DerivatedByDissociating = count_DerivatedByDissociating;
            clonedTracer.count_DerivatedByMutating = count_DerivatedByMutating;
            clonedTracer.count_DerivatedTotal = count_DerivatedTotal;
            return clonedTracer;
        }

        public bool Write(int gen, Object currObj, bool more)
        {
            //Cơ bản.
            savedObj = currObj.Clone() as Object;
            generation = gen;
            total = savedObj.node.Length;
            totalAlive = savedObj.getAliveNodes().Length;
            ratio_Dissociation = savedObj.tracer_disRatio;
            ratio_Mutation = savedObj.tracer_mutRatio;

            //Phân luồng sự kiện.
            if (eventLog.Length > 0)
            {
                //Gán điểm và các luồng sự kiện.
                count_DerivatedByDissociating = eventLog.Where(x => x.eventSource).ToArray().Length;
                count_DerivatedByMutating = eventLog.Where(x => !x.eventSource).ToArray().Length;
                count_DerivatedTotal = count_DerivatedByDissociating + count_DerivatedByMutating;

                eventLog_DThreading = eventLog.Where(x => x.eventSource && x.threadInit).ToArray().OrderByDescending(y => y.count_AccTotal).ToArray();
                eventLog_MThreading = eventLog.Where(x => !x.eventSource && x.threadInit).ToArray().OrderByDescending(y => y.count_AccTotal).ToArray();
            }

            //Nâng cao. Chỉ xét nút còn sống.
            if (more && totalAlive > 0)
            {
                //Lấy các nút còn sống.
                Node[] node = savedObj.getAliveNodes();
            }

            return true;
        }
    }
}
