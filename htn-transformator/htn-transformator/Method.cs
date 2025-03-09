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
        private Dictionary<Task, List<Task>> proceedingTasks = new();
        private List<OrderConstraint> orderings { get; set; } = new();
        private List<CompoundTask> rightSideCompound { get; set; } = new();
        private List<PrimitiveTask> rightSidePrimitive { get; set; } = new();
        private List<BeforeConstraint> befores { get; set; } = new();
        private List<AfterConstraint> afters { get; set; } = new();
        private List<BetweenConstraint> betweens { get; set; } = new();
        public CompoundTask Head { get; private set; }
        public ReadOnlyCollection<CompoundTask> RightSideCompound => rightSideCompound.AsReadOnly();
        public ReadOnlyCollection<PrimitiveTask> RightSidePrimitive => rightSidePrimitive.AsReadOnly();
        public ReadOnlyCollection<OrderConstraint> Orderings => orderings.AsReadOnly();
        public ReadOnlyCollection<BeforeConstraint> Befores => befores.AsReadOnly();
        public ReadOnlyCollection<AfterConstraint> Afters => afters.AsReadOnly();
        public ReadOnlyCollection<BetweenConstraint> Betweens => betweens.AsReadOnly();
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
                rightSidePrimitive.Add(newPT);
            }
            foreach (Task ct in copy.RightSideCompound)
            {
                CompoundTask newCT = new CompoundTask(ct.TaskName.ID, ct.TaskIndex);
                taskTranslation[ct] = newCT;
                rightSideCompound.Add(newCT);
            }
            foreach (OrderConstraint oc in copy.orderings)
            {
                AppendOrderingConstraint(new OrderConstraint(taskTranslation[oc.first], taskTranslation[oc.second]));
            }
            foreach (BeforeConstraint bc in copy.befores)
            {
                AppendBefore(new BeforeConstraint(bc.Symbol, taskTranslation[bc.Task]));
            }
            foreach (AfterConstraint ac in copy.afters)
            {
                AppendAfter(new AfterConstraint(ac.Symbol, taskTranslation[ac.Task]));
            }
            foreach(BetweenConstraint bw in copy.betweens)
            {
                AppendBetween(new BetweenConstraint(bw.Symbol, taskTranslation[bw.FromTask], taskTranslation[bw.ToTask]));
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
        public void AppendTask(Task t)
        {
            if (t is PrimitiveTask pt)
                rightSidePrimitive.Add(pt);
            else
                rightSideCompound.Add((CompoundTask)t);
        }
        public void RemoveTask(Task t)
        {
            proceedingTasks.Remove(t);

            foreach (var item in proceedingTasks)
            {
                item.Value.Remove(t);
            }

            if (t is CompoundTask ct)
                rightSideCompound.Remove(ct);
            else
                rightSidePrimitive.Remove((PrimitiveTask)t);

            for (int i = 0; i < orderings.Count; i++)
            {
                if (orderings[i].first == t || orderings[i].second == t)
                {
                   orderings.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < befores.Count; i++)
            {
                if (befores[i].Task == t)
                {
                    befores.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < afters.Count; i++)
            {
                if (afters[i].Task == t)
                {
                    afters.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < betweens.Count; i++)
            {
                if (betweens[i].FromTask == t || betweens[i].ToTask == t)
                {
                    betweens.RemoveAt(i);
                    i--;
                }
            }
        }
        public void RemoveBeforeAt(int index)
        {
            befores.RemoveAt(index);
        }
        public void RemoveAfterAt(int index)
        {
            afters.RemoveAt(index);
        }
        public void RemoveBetweenAt(int index)
        {
            betweens.RemoveAt(index);
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

            bool fromFound = false;
            bool toFound = false;

            foreach (var task in rightSideCompound)
            {
                if (con.first == task)
                {
                    fromFound = true;
                }
                if (con.second == task)
                {
                    toFound = true;
                }
            }

            foreach (var task in rightSidePrimitive)
            {
                if (con.first == task)
                {
                    fromFound = true;
                }
                if (con.second == task)
                {
                    toFound = true;
                }
            }

            if (fromFound && toFound) orderings.Add(con);
            else throw new Exception("Ordering insertion failed! Target tasks not found!");

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
                    insertedBetween.Symbol == bc.Symbol)
                {
                    return;
                }
            }

            bool fromFound = false;
            bool toFound = false;

            foreach (var task in rightSideCompound)
            {
                if (bc.FromTask == task)
                {
                    fromFound = true;
                }
                if (bc.ToTask == task)
                {
                    toFound = true;
                }
            }

            foreach (var task in rightSidePrimitive)
            {
                if (bc.FromTask == task)
                {
                    fromFound = true;
                }
                if (bc.ToTask == task)
                {
                    toFound = true;
                }
            }

            if (fromFound && toFound) betweens.Add(bc);
            else throw new Exception("Between insertion failed! Target tasks not found!");

        }
        public void ClearBetweens()
        {
            betweens.Clear();
        }
        public void AppendAfter(AfterConstraint ac) // todo check also befores and vica verse
        {
            foreach (AfterConstraint insertedAfter in Afters)
            {
                if (insertedAfter.Task == ac.Task && insertedAfter.Symbol == ac.Symbol) return;
            }

            foreach (var task in rightSideCompound)
            {
                if (ac.Task == task)
                {
                    afters.Add(ac);
                    return;
                }
            }

            foreach (var task in rightSidePrimitive)
            {
                if (ac.Task == task)
                {
                    afters.Add(ac);
                    return;
                }
            }

            throw new Exception("After insertion failed! Target task not found!");
        }
        public void RemoveTaskAndShiftConstraints(Task t)
        {
            List<Task> ordering = TaskOrdering();

            int index = ordering.IndexOf(t);

            if (index == -1) throw new Exception("Task to remove was not found in a method!");
            if (ordering.Count < 2) throw new Exception("Cannot shift constraints because there is only one task left!");

            List<BeforeConstraint> toDeleteBefore = new();
            List<AfterConstraint> toDeleteAfter = new();

            foreach (var before in befores)
            {
                if (before.Task == t) toDeleteBefore.Add(before);
            }
            foreach (var after in afters)
            {
                if (after.Task == t) toDeleteAfter.Add(after);
            }

            int toShiftIndex;
            if (index == 0)
            {
                toShiftIndex = 1;

                foreach (var before in toDeleteBefore)
                {
                    AppendBefore(new BeforeConstraint(before.Symbol, ordering[toShiftIndex]));
                }
                foreach (var after in toDeleteAfter)
                {
                    AppendBefore(new BeforeConstraint(after.Symbol, ordering[toShiftIndex])); // after becomes before because we shift forwards
                }
            }
            else
            {
                toShiftIndex = index - 1;

                foreach (var before in toDeleteBefore)
                {
                    AppendAfter(new AfterConstraint(before.Symbol, ordering[toShiftIndex]));
                }
                foreach (var after in toDeleteAfter)
                {
                    AppendAfter(new AfterConstraint(after.Symbol, ordering[toShiftIndex])); // before becomes after because we shift backwards
                }
            }

            RemoveTask(t);
        }
        public void AppendBefore(BeforeConstraint bc)
        {
            foreach (BeforeConstraint insertedBefore in Befores)
            {
                if (insertedBefore.Task == bc.Task && insertedBefore.Symbol == bc.Symbol) return;
            }

            foreach (var task in rightSideCompound)
            {
                if (bc.Task == task)
                {
                    befores.Add(bc);
                    return;
                }
            }

            foreach (var task in rightSidePrimitive)
            {
                if (bc.Task == task)
                {
                    befores.Add(bc);
                    return;
                }
            }

            throw new Exception("Before insertion failed! Target task not found!");
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
                foreach (Task pt in rightSidePrimitive)
                {
                    sb.Append($"{pt},");
                }

                foreach (Task ct in rightSideCompound)
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
                foreach (Constraint c in orderings)
                {
                    sb.Append($"{c},");
                }
            }

            foreach (Constraint c in befores)
            {
                sb.Append($"{c},");
            }
            foreach (Constraint c in afters)
            {
                sb.Append($"{c},");
            }
            foreach (Constraint c in betweens)
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
        public bool isUnit()
        {
            return rightSidePrimitive.Count == 0 && rightSideCompound.Count == 1;
        }
        public void SwapTask(Task from, Task to)
        {
            AppendTask(to);

            List<OrderConstraint> insertOrders = new();
            for (int i = 0; i < orderings.Count; i++)
            {
                OrderConstraint oc = orderings[i];
                if (oc.first == from)
                {
                    insertOrders.Add(new OrderConstraint(to, oc.second));
                }
                else if (oc.second == from)
                {
                    insertOrders.Add(new OrderConstraint(oc.first, to));
                }
            }
            foreach (OrderConstraint oc in insertOrders)
            {
                AppendOrderingConstraint(oc);
            }

            List<BeforeConstraint> insertBefores = new();
            for (int i = 0; i < befores.Count; i++)
            {
                BeforeConstraint bc = befores[i];
                if (bc.Task == from)
                {
                    insertBefores.Add(new BeforeConstraint(bc.Symbol, to));
                    RemoveBeforeAt(i);
                    i--;
                }
            }
            foreach (BeforeConstraint bc in insertBefores)
            {
                AppendBefore(bc);
            }

            List<AfterConstraint> insertAfters = new();
            for (int i = 0; i < afters.Count; i++)
            {
                AfterConstraint ac = afters[i];
                if (ac.Task == from)
                {
                    insertAfters.Add(new AfterConstraint(ac.Symbol, to));
                    RemoveAfterAt(i);
                    i--;
                }
            }
            foreach (AfterConstraint ac in insertAfters)
            {
                AppendAfter(ac);
            }

            List<BetweenConstraint> insertBetweens = new();
            for (int i = 0; i < betweens.Count; i++)
            {
                BetweenConstraint bw = betweens[i];
                if (bw.FromTask == from)
                {
                    insertBetweens.Add(new BetweenConstraint(bw.Symbol, to, bw.ToTask));
                    RemoveBetweenAt(i);
                    i--;
                }
                else if (bw.ToTask == from)
                {
                    insertBetweens.Add(new BetweenConstraint(bw.Symbol, bw.FromTask, to));
                    RemoveBetweenAt(i);
                    i--;
                }
            }
            foreach (BetweenConstraint bw in insertBetweens)
            {
                AppendBetween(bw);
            }

            RemoveTask(from);
        }
    }
}
