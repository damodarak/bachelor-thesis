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

            int methodCount = d.Methods.Count; // may change after insertin of new nullified methods

            for (int i = 0; i < methodCount; i++)
            {
                List<Task> ordering = d.Methods[i].TaskOrdering();
                List<int> nullifiableIndices = findNullifiableIndices(d.Methods[i], ordering);

                if (nullifiableIndices.Count != 0)
                {
                    List<List<int>> powerSet = GetPowerSet(nullifiableIndices, ordering.Count);

                    foreach (var indices in powerSet)
                    {
                        nullifieMethodAndAppendToDomain(d.Methods[i], indices, 0, ordering, new List<Tuple<int, PropositionalSymbol>>());
                    }
                }
            }

            for (int i = 0; i < d.Methods.Count; i++)
            {
                if (d.Methods[i].IsEmpty())
                {
                    d.RemoveMethod(d.Methods[i]);
                    i--;
                }
            }

            return d;
        }
        private void nullifieMethodAndAppendToDomain(Method m, List<int> nullTaskIndices, int index, List<Task> ordering, List<Tuple<int, PropositionalSymbol>> insert)
        {
            //append consts
            //shift; RemoveTaskAndShiftConstraints(Task t)
            //delete tasks; RemoveTaskAndShiftConstraints(Task t)

            for (int i = 0; i < nullifies[ordering[nullTaskIndices[index]].TaskName].Count; i++)
            {
                List<Tuple<int, PropositionalSymbol>> copy = new(insert);

                foreach (var symbol in nullifies[ordering[nullTaskIndices[index]].TaskName][i])
                {
                    copy.Add(new Tuple<int, PropositionalSymbol>(nullTaskIndices[index], symbol));
                }

                if (index == nullTaskIndices.Count - 1)
                {
                    //create null method
                    Method nulledMethod = new Method(m.Head, m);
                    var nulledMethodOrdering = nulledMethod.TaskOrdering();

                    foreach (var newBefore in insert)
                    {
                        nulledMethod.AppendBefore(new BeforeConstraint(newBefore.Item2, nulledMethodOrdering[newBefore.Item1]));
                    }

                    for (int j = nullTaskIndices.Count - 1; j >= 0; j--) // nullTaskIndices are in ascending order
                    {
                        nulledMethod.RemoveTaskAndShiftConstraints(nulledMethodOrdering[nullTaskIndices[j]]);
                    }

                    d.AppendMethod(nulledMethod);
                }
                else
                    nullifieMethodAndAppendToDomain(m, nullTaskIndices, index + 1, ordering, copy);
            }
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
        private List<int> findNullifiableIndices(Method m, List<Task> ordering)
        {
            List<int> result = new();

            for (int i = 0; i < ordering.Count; i++)
            {
                if (nullifies.ContainsKey(ordering[i].TaskName)) result.Add(i);
            }

            return result;
        }
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
        private List<List<int>> GetPowerSet(List<int> indices, int methodTaskCount) // ChatGPT & GitHub
        {
            int n = indices.Count;
            int powerSetCount = 1 << n; // 2^n
            List<List<int>> powerSet = new List<List<int>>();

            for (int i = 0; i < powerSetCount; i++)
            {
                List<int> subset = new List<int>();

                for (int j = 0; j < n; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        subset.Add(indices[j]);
                    }
                }

                powerSet.Add(subset);
            }

            powerSet.RemoveAt(0); // we do not want empty set

            if (powerSet[powerSet.Count - 1].Count == methodTaskCount) powerSet.RemoveAt(powerSet.Count - 1); // we do not want to remove all tasks

            return powerSet;
        }
        //private void shiftNullifiedTasksConstraints(Method)
    }
}
