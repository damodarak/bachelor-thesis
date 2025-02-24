using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal struct TaskName
    {
        private static int nextTaskNameID = 0;
        private static Dictionary<string, int> nameToID = new();
        private static Dictionary<int, string> idToTaName = new();
        public string Name { get { return idToTaName[ID]; } } // to save up memory
        public int ID { get; init; }

        public TaskName(string name)
        {
            if (nameToID.ContainsKey(name))
            {
                ID = nameToID[name];
            }
            else
            {
                ID = nextTaskNameID++;
                nameToID.Add(name, ID);
                idToTaName.Add(ID, name);
            }
        }
        public TaskName(int taskNameID)
        {
            if (!idToTaName.ContainsKey(taskNameID)) throw new Exception("Creating TaskName with non-existing TaskNameID!");

            ID = taskNameID;
        }
    }
}
