using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Represents PlanningDomain with the set of methods
    /// </summary>
    internal class PlanningDomain
    {
        private List<Method> methods = new List<Method>();
        public ReadOnlyCollection<Method> Methods => methods.AsReadOnly();
        public PlanningDomain() { }
        public void AppendMethod(Method m)
        {
            if (m.IsEmpty() && m.ConstraintsCount() > 0)
            {
                throw new Exception("Empty methods cannot have any constraints!");
            }

            if (m.isUnit() &&
                m.Head.TaskName == m.RightSideCompound[0].TaskName)
            {
                return; // we can discard these unit methods despite possible state constraints
            }

            methods.Add(m);
        }
        public void RemoveMethod(Method m)
        {
            if (!methods.Remove(m)) throw new Exception("Deletion of method failed! Method not found!");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if all methods are totally ordered, false otherwise</returns>
        public bool IsTotallyOrdered()
        {
            foreach (Method m in methods)
            {
                if (!m.IsTotallyOrdered()) return false;
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if a method with no subtasks exists, false otherwise</returns>
        public bool ContaintsEmptyMethod()
        {
            foreach (Method m in methods)
            {
                if (m.IsEmpty()) return true;
            }

            return false;
        }
    }
}
