using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace htn_transformator
{
    internal class Method
    {
        private Dictionary<Task, List<Task>> proceedingTasks = new(); // still memory leak, when the task is remove it is in other lists
        private List<OrderConstraint> orderings { get; set; } = new();
        public CompoundTask Head { get; private set; }
        public List<CompoundTask> RightSideCompound { get; set; } = new();
        public List<PrimitiveTask> RightSidePrimitive { get; set; } = new();
        public ReadOnlyCollection<OrderConstraint> Orderings => orderings.AsReadOnly();
        public List<BeforeConstraint> Befores { get; set; } = new();
        public List<AfterConstraint> Afters { get; set; } = new();
        public List<BetweenConstraint> Betweens { get; set; } = new();
        public Method(CompoundTask head)
        {
            if (head.TaskIndex != -1) throw new Exception("Head of the method must have index == -1!");

            Head = head;
        }
        public Method(CompoundTask head, Method copy)
        {
            if (head.TaskIndex != -1) throw new Exception("Head of the method must have index == -1!");

            Head = head;

            Dictionary<Task, Task> taskTranslation = new();

            foreach (Task pt in copy.RightSidePrimitive)
            {
                PrimitiveTask newPT = new PrimitiveTask(pt.TaskName.ID, pt.TaskIndex);
                taskTranslation[pt] = newPT;
                RightSidePrimitive.Add(newPT);
            }
            foreach (Task ct in copy.RightSideCompound)
            {
                CompoundTask newCT = new CompoundTask(ct.TaskName.ID, ct.TaskIndex);
                taskTranslation[ct] = newCT;
                RightSideCompound.Add(newCT);
            }
            foreach (OrderConstraint oc in copy.Orderings)
            {
                AppendOrderingConstraint(new OrderConstraint(taskTranslation[oc.first], taskTranslation[oc.second]));
            }
            foreach (BeforeConstraint bc in copy.Befores)
            {
                Befores.Add(new BeforeConstraint(bc.Symbol, taskTranslation[bc.Task])); // check how the symbol are copied
            }
            foreach (AfterConstraint ac in copy.Afters)
            {
                Afters.Add(new AfterConstraint(ac.Symbol, taskTranslation[ac.Task]));
            }
            foreach(BetweenConstraint bw in copy.Betweens)
            {
                Betweens.Add(new BetweenConstraint(bw.Symbol, taskTranslation[bw.FromTask], taskTranslation[bw.ToTask]));
            }
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
        public void RemoveTask(Task t)
        {
            proceedingTasks.Remove(t);

            if (t is CompoundTask ct)
                RightSideCompound.Remove(ct);
            else
                RightSidePrimitive.Remove((PrimitiveTask)t);

            for (int i = 0; i < Orderings.Count; i++)
            {
                if (Orderings[i].first == t || Orderings[i].second == t)
                {
                   orderings.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < Befores.Count; i++)
            {
                if (Befores[i].Task == t)
                {
                    Befores.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < Afters.Count; i++)
            {
                if (Afters[i].Task == t)
                {
                    Afters.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < Betweens.Count; i++)
            {
                if (Betweens[i].FromTask == t || Betweens[i].ToTask == t)
                {
                    Betweens.RemoveAt(i);
                    i--;
                }
            }
        }
        public void AppendOrderingConstraint(OrderConstraint con)
        {
            if (con.first == con.second) return;

            if (proceedingTasks.ContainsKey(con.second) && proceedingTasks[con.second].Contains(con.first))
            {
                throw new Exception("The input domain contains two conflicting ordering contratins!");
            }

            if (proceedingTasks.ContainsKey(con.first) && proceedingTasks[con.first].Contains(con.second))
            {
                throw new Exception("Identical ordering constraint is inserted for the second time!");
            }

            orderings.Add(con);

            if (!proceedingTasks.ContainsKey(con.first))
            {
                proceedingTasks[con.first] = new List<Task>();
            }

            proceedingTasks[con.first].Add(con.second);
        }
        /// <summary>
        /// In case of totally ordered domains, returns linear ordering of tasks, otherwise the ordering is undefined.
        /// </summary>
        /// <returns></returns>
        public List<Task> TaskOrdering()
        {
            List<Task> ordered = new List<Task>(TaskCount());

            for (int i = 0; i < RightSideCompound.Count; i++)
            {
                ordered.Add(RightSideCompound[i]);
            }

            for (int i = 0; i < RightSidePrimitive.Count; i++)
            {
                ordered.Add(RightSidePrimitive[i]);
            }

            // Bubble sort
            for (int k = 0; k < ordered.Count; k++)
            {
                for (int l = 0; l < ordered.Count - 1; l++)
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
        public void AppendBetween(BetweenConstraint bc)
        {
            if (bc.FromTask == bc.ToTask) return;

            foreach (BetweenConstraint insertedBetween in Betweens)
            {
                if (insertedBetween.FromTask == bc.FromTask &&
                    insertedBetween.ToTask == bc.ToTask &&
                    insertedBetween.Symbol.PropID == bc.Symbol.PropID)
                {
                    return;
                }
            }

            Betweens.Add(bc);
        }
        public void AppendAfter(AfterConstraint ac) // todo check also befores and vica verse
        {
            foreach (AfterConstraint insertedAfter in Afters)
            {
                if (insertedAfter.Task == ac.Task && insertedAfter.Symbol.PropID == ac.Symbol.PropID) return;
            }

            Afters.Add(ac);
        }
        public void AppendBefore(BeforeConstraint bc)
        {
            foreach (BeforeConstraint insertedBefore in Befores)
            {
                if (insertedBefore.Task == bc.Task && insertedBefore.Symbol.PropID == bc.Symbol.PropID) return;
            }

            Befores.Add(bc);
        }
        /// <summary>
        /// Comparison between tasks with respect to ordering
        /// </summary>
        /// <param name="left">expected smaller</param>
        /// <param name="righ">expected bigged</param>
        /// <returns>true if the left task comes before the right task; false otherwise</returns>
        private bool isSmaller(Task left, Task righ)
        {
            return proceedingTasks.ContainsKey(left) && proceedingTasks[left].Contains(righ);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Head}-->(");

            List<Task> taskOrder = TaskOrdering();

            if (IsTotallyOrdered())
            {
                foreach (Task t in taskOrder)
                {
                    sb.Append($"{t},");
                }
            }
            else
            {
                foreach (Task pt in RightSidePrimitive)
                {
                    sb.Append($"{pt},");
                }

                foreach (Task ct in RightSideCompound)
                {
                    sb.Append($"{ct},");
                }
            }

            if (!IsEmpty())
            {
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append(");[");

            if (IsTotallyOrdered() && TaskCount() > 1)
            {
                foreach (Task t in taskOrder)
                {
                    sb.Append($"{t}<");
                }
                if (!IsEmpty())
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                sb.Append(',');
            }
            else
            {
                foreach (Constraint c in Orderings)
                {
                    sb.Append($"{c},");
                }
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

            sb.Append(']');

            return sb.ToString();
        }
    }
}
