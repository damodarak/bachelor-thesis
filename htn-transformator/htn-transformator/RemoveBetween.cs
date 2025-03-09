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
        private Dictionary<TaskName, HashSet<PropositionalSymbol>> mandatoryPropSymbols = new(); // IDs of mandatory Propositional Symbols of a TaskName
        private Dictionary<TaskName, TaskName> derivedFrom = new(); // child TaskName -> father TaskName
        private Dictionary<TaskName, HashSet<TaskName>> derivesTo = new(); // father TaskName -> children TaskNames
        private HashSet<TaskName> searched = new();
        public RemoveBetween(PlanningDomain pd) 
        {
            d = pd;
        }
        public PlanningDomain Transform()
        {
            if (!d.IsTotallyOrdered()) throw new Exception("This transformation operates only with totally ordered domains!");

            foreach (Method m in d.Methods)
            {
                List<Task> linearOrdering = m.TaskTotalOrdering();
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

            betweensToBefores(m);

            List<Task> ordering = m.TaskTotalOrdering();
            List<HashSet<PropositionalSymbol>> symbols = symbolsFromBetweensAndNewTaskNames(m, ordering);
            m.ClearBetweens();

            HashSet<TaskName> toBeSearched = appendBeforesAndSwapTasks(m, ordering, symbols);
            copyMethodsAndSearchTask(toBeSearched);
        }
        private void searchCompoundTask(TaskName tn)
        {
            List<Method> searchMethods = Common.MethodsWithHead(d.Methods, tn);

            foreach (Method m in searchMethods)
            {
                betweensToBefores(m);

                List<Task> ordering = m.TaskTotalOrdering();
                List<HashSet<PropositionalSymbol>> neededSymbols = symbolsFromBetweensAndNewTaskNames(m, ordering);
                m.ClearBetweens();

                foreach (var symbols in neededSymbols)
                {
                    symbols.UnionWith(mandatoryPropSymbols[tn]); // mandatoryPropSymbols[tn] is never null here
                }

                HashSet<TaskName> toBeSearched = appendBeforesAndSwapTasks(m, ordering, neededSymbols);
                copyMethodsAndSearchTask(toBeSearched);
            }
        }
        private HashSet<TaskName> appendBeforesAndSwapTasks(Method m, List<Task> ordering, List<HashSet<PropositionalSymbol>> symbols)
        {
            HashSet<TaskName> toBeSearched = new();

            for (int i = 0; i < ordering.Count; i++)
            {
                if (symbols[i].Count == 0) continue;

                if (ordering[i] is PrimitiveTask pt)
                {
                    appendBeforesToPrimitiveTask(m, pt, symbols[i]);
                }
                else
                {
                    CompoundTask? existingCT = existingNewCompoundTaskName(ordering[i], symbols[i]);
                    if (existingCT == null)
                    {
                        CompoundTask newCT = createNewCompoundTask(ordering[i], symbols[i]);
                        toBeSearched.Add(newCT.TaskName);
                        m.SwapTask(ordering[i], newCT);
                    }
                    else
                    {
                        m.SwapTask(ordering[i], existingCT);
                    }
                }
            }

            return toBeSearched;
        }
        private List<HashSet<PropositionalSymbol>> symbolsFromBetweensAndNewTaskNames(Method m, List<Task> ordering)
        {
            // ints are reffering to Propositional symbol IDs
            HashSet<PropositionalSymbol>[] symbols = new HashSet<PropositionalSymbol>[ordering.Count];

            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i] = new(); // we don't want null values
            }

            foreach (BetweenConstraint bc in m.Betweens) // betweens from the method constraints
            {
                int fromIndex = ordering.IndexOf(bc.FromTask);
                int toIndex = ordering.IndexOf(bc.ToTask);

                for (int i = fromIndex + 1; i < toIndex; i++)
                {
                    symbols[i].Add(bc.Symbol);
                }
            }

            for (int i = 0; i < symbols.Length; i++) // symbols from the newly created TaskNames
            {
                if (mandatoryPropSymbols.ContainsKey(ordering[i].TaskName))
                    symbols[i].UnionWith(mandatoryPropSymbols[ordering[i].TaskName]);
            }

            return new List<HashSet<PropositionalSymbol>>(symbols);
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
                    m.RemoveBetweenAt(i);
                    m.AppendBefore(new BeforeConstraint(ps, t));

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
            TaskName oldCompoundTaskName = derivedFrom[newCompoundTask]; // problem because we look at originial task, not actual father
            List<Method> toCopy = Common.MethodsWithHead(d.Methods, oldCompoundTaskName);

            foreach (Method old in toCopy)
            {
                Method copied = new Method(new CompoundTask(newCompoundTask, -1), old);
                d.AppendMethod(copied);
            }
        }
        private CompoundTask createNewCompoundTask(Task father, HashSet<PropositionalSymbol> symbols)
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
        private CompoundTask? existingNewCompoundTaskName(Task t, HashSet<PropositionalSymbol> neededSymbols)
        {
            // Task t could be new TaskName but also original
            if (!derivedFrom.ContainsKey(t.TaskName) && !derivesTo.ContainsKey(t.TaskName)) return null;

            TaskName search;

            if (derivedFrom.ContainsKey(t.TaskName))
                search = derivedFrom[t.TaskName];
            else
                search = t.TaskName;

                foreach (TaskName child in derivesTo[search])
                {
                    if (mandatoryPropSymbols.ContainsKey(child) && mandatoryPropSymbols[child].SetEquals(neededSymbols))
                    {
                        return new CompoundTask(child, nextNewCompoundIndex++);
                    }
                }

            return null;
        }
        private void appendBeforesToPrimitiveTask(Method m, PrimitiveTask pt, HashSet<PropositionalSymbol> symbols)
        {
            if (symbols == null) return;

            int index = m.RightSidePrimitive.IndexOf(pt);
            if (index == -1)
            {
                throw new Exception("Appending state constraints to non-existing Primitive task!");
            }

            foreach(PropositionalSymbol s in symbols)
            {
                BeforeConstraint bc = new BeforeConstraint(s, m.RightSidePrimitive[index]);
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
        private void betweensToBefores(Method m)
        {
            foreach (BetweenConstraint bw in m.Betweens) // in case that all intermedieate tasks are empty in the final plan
            {
                BeforeConstraint bc = new BeforeConstraint(bw.Symbol, bw.ToTask);
                m.AppendBefore(bc);
            }
        }
    }
}
