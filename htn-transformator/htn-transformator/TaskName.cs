using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class TaskName
    {
        public string Name { get; private set; }
        public int TaskID { get; private set; }

        public TaskName(string name, int taskID)
        {
            Name = name;
            TaskID = taskID;
        }
    }
}
