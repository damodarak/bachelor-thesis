using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class Method
    {
        public CompoundTask LeftSide { get; private set; }
        public List<CompoundTask> RightSideCompound { get; set; } = new List<CompoundTask>();
        public List<PrimitiveTask> RightSidePrimitive { get; set; } = new List<PrimitiveTask>();
        public List<OrderConstraint> Orderings { get; set; } = new List<OrderConstraint>();
        public List<BeforeConstraint> Befores { get; set; } = new List<BeforeConstraint>();
        public List<AfterConstraint> Afters { get; set; } = new List<AfterConstraint>();
        public List<BetweenConstraint> Betweens { get; set; } = new List<BetweenConstraint>();
        public Method(CompoundTask leftSide)
        {
            LeftSide = leftSide;
        }
        public bool IsTotallyOrdered()
        {
            int count = TaskCount();
            return Orderings.Count == ((count - 1) * count) / 2;
        }
        public bool IsEmpty()
        {
            return TaskCount() == 0;
        }
        public int TaskCount()
        {
            return RightSideCompound.Count + RightSidePrimitive.Count;
        }
        public int ConstraintsCount()
        {
            return Befores.Count + Afters.Count + Betweens.Count + Betweens.Count + Orderings.Count;
        }
        /// <summary>
        /// In case of totally ordered domains, returns linear ordering of tasks, otherwise the ordering is undefined
        /// </summary>
        /// <returns></returns>
        public Task[] TaskOrdering()
        {
            Task[] ordered = new Task[TaskCount()];

            int j = 0;
            for (; j < RightSideCompound.Count; j++)
            {
                ordered[j] = RightSideCompound[j];
            }

            for (int i = 0; i < RightSidePrimitive.Count; i++)
            {
                ordered[j + i] = RightSidePrimitive[i];
            }

            // Bubble sort
            for (int k = 0; k < ordered.Length; k++)
            {
                for (int l = 0; l < ordered.Length - 1; l++)
                {
                    if (isSmaller(ordered[l + 1], ordered[l]))
                    {
                        // swap
                        Task smaller = ordered[l + 1];
                        ordered[l + 1] = ordered[l];
                        ordered[l] = smaller;
                    }
                }
            }

            return ordered;
        }
        private bool isSmaller(Task left, Task righ)
        {
            foreach (OrderConstraint oc in Orderings)
            {
                if (oc.first == left && oc.second == righ)
                {
                    return true;
                }
                else if (oc.first == righ && oc.second == left)
                {
                    return false;
                }
            }

            return false;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{LeftSide}-->(");

            foreach (Task pt in RightSidePrimitive)
            {
                sb.Append($"{pt},");
            }

            foreach (Task ct in RightSideCompound)
            {
                sb.Append($"{ct},");
            }

            if (!IsEmpty())
            {
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append(");[");

            foreach (Constraint c in Orderings)
            {
                sb.Append($"{c},");
            }
            foreach (Constraint c in Befores)
            {
                sb.Append($"{c},");
            }
            foreach (Constraint c in Afters)
            {
                sb.Append($"{c},");
            }
            foreach (Constraint c in Betweens)
            {
                sb.Append($"{c},");
            }

            if (ConstraintsCount() > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append("]");

            return sb.ToString();
        }
    }
}
