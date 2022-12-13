using System;

namespace EvoModeling
{
    class DeathNote : ICloneable
    {
        //public Tracer tracer = new Tracer();
        public int generation = -1;
        public int incidentCode = -1;
        public bool letItGo = false;

        public object Clone()
        {
            DeathNote clonedDeathNote = new DeathNote();
            clonedDeathNote.generation = generation;
            clonedDeathNote.incidentCode = incidentCode;
            return clonedDeathNote;
        }

        public bool Write(int currGen, int currIncidentCode)
        {
            generation = currGen;
            incidentCode = currIncidentCode;
            return true;
        }
    }
}
