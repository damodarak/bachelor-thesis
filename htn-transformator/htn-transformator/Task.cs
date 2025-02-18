using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal abstract class Task
    {
        public static readonly int LeftSideIndex = -1;

        private static int nextTaskNameID = 0;
        private static Dictionary<string, TaskName> existingTaskNames = new Dictionary<string, TaskName>();

        public TaskName TaskName { get; private set; }
        public int TaskIndex { get; private set; }
        public Task(string name, int taskIndex)
        {
            TaskIndex = taskIndex;

            if (existingTaskNames.ContainsKey(name))
            {
                TaskName = existingTaskNames[name];
            }
            else
            {
                TaskName tn = new TaskName(name, nextTaskNameID++);
                existingTaskNames.Add(name, tn);

                TaskName = tn;
            }
        }

        public override string ToString()
        {
            if (TaskIndex == -1) return TaskName.Name;

            return $"{TaskName.Name}#{TaskIndex}";
        }
    }
}
