using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    class RemoveEmptyMethods : ITransformable
    {
        private PlanningDomain d;
        private Dictionary<TaskName, List<HashSet<PropositionalSymbol>>> nullifies = new(); // TaskName -> set of sets of symbols that can be nullified using empty methods
        private HashSet<TaskName> toBeSearched = new();
        private Dictionary<TaskName, HashSet<Method>> containsInSubtasks = new();
        public RemoveEmptyMethods(PlanningDomain d) { this.d = d; }
        public PlanningDomain Transform()
        {
            if (!d.ContaintsEmptyMethod()) return d;

            d = (new RemoveBetween(d)).Transform();
            // now we know that this is totally ordered domain and does not contain any betweens

            containsInInit();
            nullifiesBase();

            while (toBeSearched.Count != 0)
            {
                var enumerator = toBeSearched.GetEnumerator();
                enumerator.MoveNext();
                TaskName toSearch = enumerator.Current;
                toBeSearched.Remove(toSearch);
                searchNullifiedTaskName(toSearch);
            }

            return null;
        }
        private void searchNullifiedTaskName(TaskName tn)
        {
            if (!containsInSubtasks.ContainsKey(tn)) return;

            foreach (Method m in containsInSubtasks[tn])
            {
                if (m.RightSidePrimitive.Count != 0) continue;
                if (!nullifiable(m)) continue;

                HashSet<PropositionalSymbol> symbols = symbolsFromConstraints(m);
                createNewNullifiedSymbols(m, 0, symbols);
            }
        }
        private void createNewNullifiedSymbols(Method m, int index, HashSet<PropositionalSymbol> symbols)
        {
            HashSet<PropositionalSymbol> symbolsCopy;
            int length = nullifies[m.RightSideCompound[index].TaskName].Count; // may be increased in the lowest level of recursion
            for (int i = 0; i < length; i++)
            {
                symbolsCopy = new(symbols);
                symbolsCopy.UnionWith(nullifies[m.RightSideCompound[index].TaskName][i]);

                if (index == m.RightSideCompound.Count - 1)
                {
                    if (!nullifies.ContainsKey(m.Head.TaskName)) nullifies[m.Head.TaskName] = new();

                    if (!containtsSet(nullifies[m.Head.TaskName], symbolsCopy))
                    {
                        nullifies[m.Head.TaskName].Add(symbolsCopy);
                        toBeSearched.Add(m.Head.TaskName);
                    }
                    else 
                        return;
                    
                }
                else
                {
                    createNewNullifiedSymbols(m, index + 1, symbolsCopy);
                }
            }
        }
        private HashSet<PropositionalSymbol> symbolsFromConstraints(Method m)
        {
            HashSet<PropositionalSymbol> symbols = new();

            foreach (var before in m.Befores)
            {
                symbols.Add(before.Symbol);
            }
            foreach (var after in m.Afters)
            {
                symbols.Add(after.Symbol);
            }

            return symbols;
        }
        private bool containtsSet<T>(IEnumerable<HashSet<T>> where, HashSet<T> lookingFor)
        {
            foreach (var set in where)
            {
                if (set.SetEquals(lookingFor)) return true;
            }

            return false;
        }
        private bool nullifiable(Method m)
        {
            foreach (CompoundTask ct in m.RightSideCompound)
            {
                if (!nullifiable(ct.TaskName)) return false;
            }

            return m.RightSidePrimitive.Count == 0; // all CompoundTasks are nullifiable, but there cannot be any PrimitiveTasks
        }
        private bool nullifiable(TaskName tn) { return nullifies.ContainsKey(tn); }
        private void nullifiesBase()
        {
            foreach (Method m in d.Methods)
            {
                if (m.IsEmpty())
                {
                    TaskName headName = m.Head.TaskName;

                    toBeSearched.Add(headName);

                    if (!nullifies.ContainsKey(headName))
                        nullifies[headName] = new();

                    nullifies[headName].Add(new HashSet<PropositionalSymbol>()); // empty set
                }
            }
        }
        private void containsInInit()
        {
            foreach (Method m in d.Methods)
            {
                foreach (Task t in m.RightSideCompound)
                {
                    if (!containsInSubtasks.ContainsKey(t.TaskName))
                    {
                        containsInSubtasks[t.TaskName] = new HashSet<Method>();
                    }

                    containsInSubtasks[t.TaskName].Add(m);
                }
            }
        }
    }
}
