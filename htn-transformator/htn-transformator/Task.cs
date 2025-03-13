using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace htn_transformator
{
    /// <summary>
    /// Class representing abstract Task. Each task is defined with TaskName and int TaskIndex (this way we can have multiple
    /// tasks with the same TaskName in a single method).
    /// </summary>
    internal abstract class Task
    {
        public static readonly int HeadIndex = -1;
        public TaskName TaskName { get; private set; }
        public int TaskIndex { get; private set; }
        public Task(int id, int taskIndex)
        {
            TaskIndex = taskIndex;
            TaskName = new TaskName(id);
        }
        public Task(string name, int taskIndex)
        {
            TaskIndex = taskIndex;
            TaskName = new TaskName(name);
        }
        public Task(TaskName taskName, int taskIndex)
        {
            TaskName = taskName;
            TaskIndex = taskIndex;
        }
        /// <summary>
        /// New constructor is used in RemoveBetween transformation. To display which Symbols are guaranteed with this CompoundTask.
        /// </summary>
        /// <param name="father"></param>
        /// <param name="symbols"></param>
        /// <param name="index"></param>
        public Task(Task father, HashSet<PropositionalSymbol> symbols, int index)
        {
            TaskIndex = index;

            StringBuilder sb = new StringBuilder();

            sb.Append(father.TaskName.Name);
            sb.Append('{');
            foreach (PropositionalSymbol s in symbols)
            {
                sb.Append($"{s},");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append('}');

            TaskName = new TaskName(sb.ToString());
        }
        public override string ToString()
        {
            if (TaskIndex == -1) return TaskName.Name;

            return $"{TaskName.Name}#{TaskIndex}";
        }
    }
}
