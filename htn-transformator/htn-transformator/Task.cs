using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal abstract class Task
    {
        private static int nextID = 0;
        private static Dictionary<string, TaskName> existingTaskNames = new Dictionary<string, TaskName>();

        public TaskName Name { get; private set; }

        public Task(string name)
        {
            if (existingTaskNames.ContainsKey(name))
            {
                Name = existingTaskNames[name];
            }
            else
            {
                TaskName tn = new TaskName(name, nextID++);
                existingTaskNames.Add(name, tn);

                Name = tn;
            }
        }
    }
}
