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
        /// <summary>
        /// Translation between TaskName and the set of mandatory PropositionalSymbols.
        /// </summary>
        private Dictionary<TaskName, HashSet<PropositionalSymbol>> mandatoryPropSymbols = new();
        /// <summary>
        /// Child TaskName -> father TaskName.
        /// </summary>
        private Dictionary<TaskName, TaskName> derivedFrom = new();
        /// <summary>
        ///  Father TaskName -> set of children TaskNames.
        /// </summary>
        private Dictionary<TaskName, HashSet<TaskName>> derivesTo = new();
        /// <summary>
        /// TaskNames that were searched and do not need to be searched again.
        /// </summary>
        private HashSet<TaskName> searched = new();
        public RemoveBetween(PlanningDomain pd) 
        {
            d = pd;
        }
        public PlanningDomain Transform()
        {
            if (!d.IsTotallyOrdered()) throw new Exception("This transformation operates only with totally ordered domains!");

            // betweens that should not be here or easy to remove
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
        /// <summary>
        /// Remove all between constraints from a Method and create new Methods with guaranteed PropositionalSymbols.
        /// </summary>
        /// <param name="m"></param>
        private void removeBetweens(Method m)
        {
            if (m.Betweens.Count == 0) return; // nothing to do

            betweensToBefores(m);

            List<Task> ordering = m.TaskTotalOrdering();
            List<HashSet<PropositionalSymbol>> symbols = symbolsFromBetweensAndNewTaskNames(m, ordering);
            m.ClearBetweens(); // now we can remove all betweens

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
        /// <summary>
        /// Append BeforeConstraints with the PropositionalSymbols symbols to the PrimitiveTasks. And swap CompoundTask with the new
        /// ones that guarantee PropositionalSymbols.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="ordering"></param>
        /// <param name="symbols"></param>
        /// <returns>List of new TaskNames which were created.</returns>
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
                    if (existingCT == null) // New TaskName which has not been searched yet
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
        /// <summary>
        /// For each Task it finds HashSet of PropositionalSymbols that are mentioned in BetweenConstraints.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="ordering"></param>
        /// <returns></returns>
        private List<HashSet<PropositionalSymbol>> symbolsFromBetweensAndNewTaskNames(Method m, List<Task> ordering)
        {
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
        /// <summary>
        /// Remove BetweenConstraints that target Tasks which are next to each other w.r.t. the totall ordering.
        /// These can be easily interchange with a single BeforeConstraint.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="ordering"></param>
        private void removeNeighbourBetweens(Method m, List<Task> ordering)
        {
            for (int i = 0; i < m.Betweens.Count; i++)
            {
                int index = ordering.IndexOf(m.Betweens[i].FromTask);
                if (index != ordering.Count - 1 && m.Betweens[i].ToTask == ordering[index + 1])
                {
                    PropositionalSymbol ps = m.Betweens[i].Symbol;
                    Task t = m.Betweens[i].ToTask;
                    m.RemoveBetweenAt(i);
                    m.AppendBefore(new BeforeConstraint(ps, t));

                    i--;
                }
            }
        }
        /// <summary>
        /// Create new Methods with the toBeSearched TaskNames as a Method.Head.
        /// Search TaskNames (they interact with newly created Methods) which have not been searched yet.
        /// </summary>
        /// <param name="toBeSearched"></param>
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
        /// <summary>
        /// Create new Methods with new TaskNames. New Methods have identical subtasks and constraints as the father TaskName.
        /// </summary>
        /// <param name="newTaskName"></param>
        private void copyMethods(TaskName newTaskName)
        {
            TaskName oldCompoundTaskName = derivedFrom[newTaskName];
            List<Method> toCopy = Common.MethodsWithHead(d.Methods, oldCompoundTaskName);

            foreach (Method old in toCopy)
            {
                Method copied = new Method(new CompoundTask(newTaskName, -1), old);
                d.AppendMethod(copied);
            }
        }
        /// <summary>
        /// Creates new CompounTask with the mandatory symbols.
        /// </summary>
        /// <param name="father"></param>
        /// <param name="symbols"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Tries to find TaskName with the given mandatory symbols.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="neededSymbols"></param>
        /// <returns>CompoundTask with existing TaskName which guaranteees PropositionalSymbols, null otherwise.</returns>
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
                    // if there is a new TaskName with the given mandatory symbols then we recycle this TaskName
                    if (mandatoryPropSymbols.ContainsKey(child) && mandatoryPropSymbols[child].SetEquals(neededSymbols))
                    {
                        return new CompoundTask(child, nextNewCompoundIndex++);
                    }
                }

            return null;
        }
        /// <summary>
        /// Append BeforeConstraints to the Method with the given HashSet of PropositionalSymbols.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="pt"></param>
        /// <param name="symbols"></param>
        /// <exception cref="Exception"></exception>
        private void appendBeforesToPrimitiveTask(Method m, PrimitiveTask pt, HashSet<PropositionalSymbol> symbols)
        {
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="linearOrdering"></param>
        /// <returns>true if a Method contains a BetweenConstraint in which targeted Tasks are in reverse order, false otherwise.</returns>
        private bool containtsIrrationalBetweens(Method m, List<Task> linearOrdering)
        {
            foreach (BetweenConstraint bw in m.Betweens)
            {
                if (linearOrdering.IndexOf(bw.FromTask) > linearOrdering.IndexOf(bw.ToTask))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// For each BetweenConstraint we append a BeforeConstraint that targets the second Task in a BetweenConstraint.
        /// This is a prevention if all Tasks targeted by the BetweenConstraint are decomposed with an empty Method.
        /// </summary>
        /// <param name="m"></param>
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
