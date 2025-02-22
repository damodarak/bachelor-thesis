using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace htn_transformator
{
    internal abstract class Task
    {
        public static readonly int LeftSideIndex = -1;

        private static int nextTaskNameID = 0;
        private static Dictionary<string, TaskName> nameToTaskName = new Dictionary<string, TaskName>();
        private static Dictionary<int, TaskName> idToTaskName = new Dictionary<int, TaskName>();

        public TaskName TaskName { get; private set; }
        public int TaskIndex { get; private set; }
        public Task(int id, int taskIndex)
        {
            TaskIndex = taskIndex;
            TaskName = idToTaskName[id];
        }
        public Task(string name, int taskIndex)
        {
            TaskIndex = taskIndex;

            if (nameToTaskName.ContainsKey(name))
            {
                TaskName = nameToTaskName[name];
            }
            else
            {
                TaskName tn = new TaskName(name, nextTaskNameID++);
                nameToTaskName.Add(name, tn);
                idToTaskName.Add(tn.ID, tn);

                TaskName = tn;
            }
        }
        public Task(Task father, HashSet<int> symbols, int index)
        {
            TaskIndex = index;

            StringBuilder sb = new StringBuilder();

            sb.Append(father.TaskName.Name);
            sb.Append('{');
            foreach (int s in symbols)
            {
                sb.Append($"{PropositionalSymbol.GetName(s)},");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append('}');

            TaskName tn = new TaskName(sb.ToString(), nextTaskNameID++);
            nameToTaskName.Add(tn.Name, tn);
            idToTaskName.Add(tn.ID, tn);

            TaskName = tn;
        }
        public override string ToString()
        {
            if (TaskIndex == -1) return TaskName.Name;

            return $"{TaskName.Name}#{TaskIndex}";
        }
    }
}
