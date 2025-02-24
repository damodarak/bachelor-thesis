using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class RemoveBetween : ITransformable
    {
        private int nextNewCompoundIndex = 1;
        private PlanningDomain d;
        // translation between TaskName and the set of mandatory IDs of propositional symbols
        private Dictionary<TaskName, HashSet<int>> mandatoryPropSymbols = new();
        private Dictionary<TaskName, TaskName> derivedFrom = new(); // father TaskName
        private Dictionary<TaskName, HashSet<TaskName>> derivesTo = new(); // children TaskNames
        private HashSet<TaskName> searched = new();
        public RemoveBetween(PlanningDomain pd) 
        {
            d = pd;
        }
        public PlanningDomain Transform()
        {
            if (!d.IsTotallyOrdered()) throw new Exception("This transformation operates only with totally ordered domains!");

            ITransformable removeIdenticalUnits = new RemoveIdenticalUnitMethods(d);
            d = removeIdenticalUnits.Transform();

            foreach (Method m in d.Methods)
            {
                List<Task> linearOrdering = m.TaskOrdering();
                removeNeighbourBetweens(m, linearOrdering);
                if (containtsIrrationalBetweens(m, linearOrdering))
                {
                    throw new Exception("Input planning domains contains irrational between constraint!");
                }
            }

            int methodCount = d.Methods.Count; // new methods are appended to the end of a list

            for (int i = 0; i < methodCount; i++)
            {
                removeBetweens(d.Methods[i]);
            }

            return d;
        }
        private void removeBetweens(Method m)
        {
            if (m.Betweens.Count == 0) return;

            List<Task> linearOrdering = m.TaskOrdering();

            foreach (BetweenConstraint bw in m.Betweens) // in case that all intermedieate tasks are empty
            {
                BeforeConstraint bc = new BeforeConstraint(bw.Symbol, bw.ToTask);
                m.AppendBefore(bc);
            }

            List<HashSet<int>> symbols = symbolsFromBetweens(m, linearOrdering);
            m.Betweens = new List<BetweenConstraint>(); // remove all betweens from the method

            HashSet<TaskName> toBeSearched = new();
            for (int i = 0; i < symbols.Count; i++) // == linearOrdering.Count
            {
                if (symbols[i] == null) continue;

                if (linearOrdering[i] is PrimitiveTask pt)
                {
                    appendBeforesToPrimitiveTask(m, pt, symbols[i]);
                }
                else // CompoundTask
                {
                    CompoundTask? existingCT = existingNewCompoundTaskName(linearOrdering[i], symbols[i], linearOrdering);
                    if (existingCT == null) // create new, swap in constraints, copy methods
                    {
                        CompoundTask newCT = createNewCompoundTask(linearOrdering[i], symbols[i]);
                        toBeSearched.Add(newCT.TaskName);
                        swapCompoundTask(m, (CompoundTask)linearOrdering[i], newCT);
                    }
                    else
                    {
                        swapCompoundTask(m, (CompoundTask)linearOrdering[i], existingCT);
                    }
                }
            }

            copyMethodsAndSearchTask(toBeSearched);
        }
        private void searchCompoundTask(TaskName tn)
        {
            List<Method> searchMethods = methodsWithHead(tn);
            HashSet<TaskName> toBeSearched = new();

            foreach (Method m in searchMethods)
            {
                List<Task> ordering = m.TaskOrdering();
                List<HashSet<int>> methodSymbols = symbolsFromBetweens(m, ordering);
                m.Betweens = new List<BetweenConstraint>();

                for (int i = 0; i < ordering.Count; i++)
                {
                    if (ordering[i] is PrimitiveTask pt)
                    {
                        appendBeforesToPrimitiveTask(m, pt, methodSymbols[i]);
                    }
                    else
                    {
                        HashSet<int> neededSymbols = mandatoryPropSymbols[tn]; // this is never null here
                        neededSymbols.UnionWith(methodSymbols[i]);

                        CompoundTask? existingCT = existingNewCompoundTaskName(ordering[i], neededSymbols, ordering);
                        if (existingCT == null)
                        {
                            CompoundTask newCT = createNewCompoundTask(ordering[i], neededSymbols);
                            toBeSearched.Add(newCT.TaskName);
                            swapCompoundTask(m, (CompoundTask)ordering[i], newCT);
                        }
                        else
                        {
                            swapCompoundTask(m, (CompoundTask)ordering[i], existingCT);
                        }
                    }
                }
            }

            copyMethodsAndSearchTask(toBeSearched);
        }
        private HashSet<TaskName> removeBetweensAndSwapTasks()
        {
            return null;
        }
        private List<HashSet<int>> symbolsFromBetweens(Method m, List<Task> ordering)
        {
            // ints are reffering to Propositional symbol IDs
            HashSet<int>[] symbols = new HashSet<int>[ordering.Count];

            foreach (BetweenConstraint bc in m.Betweens)
            {
                int fromIndex = ordering.IndexOf(bc.FromTask);
                int toIndex = ordering.IndexOf(bc.ToTask);

                for (int i = fromIndex + 1; i < toIndex; i++)
                {
                    if (symbols[i] == null) symbols[i] = new HashSet<int>();

                    symbols[i].Add(bc.Symbol.PropID);
                }
            }

            return new List<HashSet<int>>(symbols);
        }
        private void removeNeighbourBetweens(Method m, List<Task> ordering)
        {
            for (int i = 0; i < m.Betweens.Count; i++)
            {
                int index = ordering.IndexOf(m.Betweens[i].FromTask);
                if (index != -1 && index != ordering.Count - 1 && m.Betweens[i].ToTask == ordering[index + 1])
                {
                    PropositionalSymbol ps = m.Betweens[i].Symbol;
                    Task t = m.Betweens[i].ToTask;
                    m.Betweens.RemoveAt(i);
                    m.Befores.Add(new BeforeConstraint(ps, t));

                    i--;
                }
            }
        }
        private void copyMethodsAndSearchTask(HashSet<TaskName> toBeSearched)
        {
            foreach (TaskName newCompoundTaskName in toBeSearched)
            {
                copyMethods(newCompoundTaskName);
            }

            foreach (TaskName newCompoundTaskName in toBeSearched)
            {
                if (!searched.Contains(newCompoundTaskName))
                {
                    searched.Add(newCompoundTaskName);
                    searchCompoundTask(newCompoundTaskName); // don't search if it was already searched in recursion, TaskName + symbols
                }
            }
        }
        private void copyMethods(TaskName newCompoundTask)
        {
            TaskName oldCompoundTaskName = derivedFrom[newCompoundTask];
            List<Method> toCopy = methodsWithHead(oldCompoundTaskName);

            foreach (Method old in toCopy)
            {
                Method copied = new Method(new CompoundTask(newCompoundTask, -1), old);
                d.AppendMethod(copied);
            }
        }
        private List<Method> methodsWithHead(TaskName searchedHead)
        {
            List<Method> result = new();

            foreach (Method m in d.Methods)
            {
                if (m.Head.TaskName == searchedHead)
                    result.Add(m);
            }

            return result;
        }
        private CompoundTask createNewCompoundTask(Task father, HashSet<int> symbols)
        {
            CompoundTask newCT = new CompoundTask(father, symbols, nextNewCompoundIndex++);
            mandatoryPropSymbols[newCT.TaskName] = symbols;
            derivedFrom[newCT.TaskName] = father.TaskName;

            if (!derivesTo.ContainsKey(father.TaskName))
            {
                derivesTo[father.TaskName] = new HashSet<TaskName>();
            }
            derivesTo[father.TaskName].Add(newCT.TaskName);

            return newCT;
        }
        private void swapCompoundTask(Method m, CompoundTask oldCT, CompoundTask newCT)
        {
            m.RightSideCompound.Remove(oldCT);
            m.RightSideCompound.Add(newCT);

            List<OrderConstraint> insertOrders = new();
            for (int i = 0; i < m.Orderings.Count; i++)
            {
                OrderConstraint oc = m.Orderings[i];
                if (oc.first == oldCT)
                {
                    insertOrders.Add(new OrderConstraint(newCT, oc.second));
                    m.Orderings.RemoveAt(i);
                    i--;
                }
                else if (oc.second == oldCT)
                {
                    insertOrders.Add(new OrderConstraint(oc.first, newCT));
                    m.Orderings.RemoveAt(i);
                    i--;
                }
            }
            m.Orderings.AddRange(insertOrders);

            List<BeforeConstraint> insertBefores = new();
            for (int i = 0; i < m.Befores.Count; i++)
            {
                BeforeConstraint bc = m.Befores[i];
                if (bc.Task == oldCT)
                {
                    insertBefores.Add(new BeforeConstraint(bc.Symbol, newCT));
                    m.Befores.RemoveAt(i);
                    i--;
                }
            }
            m.Befores.AddRange(insertBefores);

            List<AfterConstraint> insertAfters = new();
            for (int i = 0; i < m.Afters.Count; i++)
            {
                AfterConstraint ac = m.Afters[i];
                if (ac.Task == oldCT)
                {
                    insertAfters.Add(new AfterConstraint(ac.Symbol, newCT));
                    m.Afters.RemoveAt(i);
                    i--;
                }
            }
            m.Afters.AddRange(insertAfters);

            List<BetweenConstraint> insertBetweens = new();
            for (int i = 0; i < m.Betweens.Count; i++)
            {
                BetweenConstraint bw = m.Betweens[i];
                if (bw.FromTask == oldCT)
                {
                    insertBetweens.Add(new BetweenConstraint(bw.Symbol, newCT, bw.ToTask));
                    m.Betweens.RemoveAt(i);
                    i--;
                }
                else if (bw.ToTask == oldCT)
                {
                    insertBetweens.Add(new BetweenConstraint(bw.Symbol, bw.FromTask, newCT));
                    m.Betweens.RemoveAt(i);
                    i--;
                }
            }
            m.Betweens.AddRange(insertBetweens);
        }
        private CompoundTask? existingNewCompoundTaskName(Task t, HashSet<int> neededSymbols, List<Task> ordering)
        {
            int index = ordering.IndexOf(t);

            if (!derivesTo.ContainsKey(ordering[index].TaskName)) return null;

            foreach (TaskName tn in derivesTo[ordering[index].TaskName])
            {
                if (mandatoryPropSymbols.ContainsKey(tn) && mandatoryPropSymbols[tn].SetEquals(neededSymbols))
                {
                    return new CompoundTask(tn, nextNewCompoundIndex++);
                }
            }

            return null;
        }
        private void appendBeforesToPrimitiveTask(Method m, PrimitiveTask pt, HashSet<int> symbols)
        {
            if (symbols == null) return;

            int index = m.RightSidePrimitive.IndexOf(pt);
            if (index == -1)
            {
                throw new Exception("Appending state constraints to non-existing Primitive task!");
            }

            foreach(int id in symbols)
            {
                BeforeConstraint bc = new BeforeConstraint(new PropositionalSymbol(id), m.RightSidePrimitive[index]);
                m.AppendBefore(bc);
            }
        }
        private bool containtsIrrationalBetweens(Method m, List<Task> linearOrdering)
        {
            foreach (BetweenConstraint bw in m.Betweens)
            {
                if (linearOrdering.IndexOf(bw.FromTask) > linearOrdering.IndexOf(bw.ToTask))
                    return true;
            }

            return false;
        }
    }
}
