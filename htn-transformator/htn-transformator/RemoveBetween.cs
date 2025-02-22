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

        // translation between Task and the set of mandatory IDs of propositional symbols
        private Dictionary<CompoundTask, HashSet<int>> mandatoryPropSymbols = new();
        private Dictionary<CompoundTask, CompoundTask> derivedFrom = new(); // father compound task
        private Dictionary<CompoundTask, HashSet<CompoundTask>> derivesTo = new(); // children compound tasks
        public RemoveBetween(PlanningDomain pd) 
        {
            d = pd;
        }
        public PlanningDomain Transform()
        {
            if (!d.IsTotallyOrdered()) throw new Exception("This transformation operates only with totally ordered domains!");

            ITransformable removeIdenticalUnits = new RemoveIdenticalUnitMethods(d);
            d = removeIdenticalUnits.Transform();

            foreach (var m in d.Methods)
            {
                removeBetweens(m);
            }

            return d;
        }
        private void searchCompoundTask(CompoundTask compoundTask)
        {
            throw new NotImplementedException();
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
        private void removeBetweens(Method m)
        {
            if (m.Betweens.Count == 0) return;

            List<Task> linearOrdering = m.TaskOrdering();
            removeNeighbourBetweens(m, linearOrdering);
            if (containtsIrrationalBetweens(m, linearOrdering))
            {
                throw new Exception("Input planning domains contains irrational between constraint!");
            }

            foreach (BetweenConstraint bw in m.Betweens)
            {
                AfterConstraint ac = new AfterConstraint(bw.Symbol, bw.FromTask);
                BeforeConstraint bc = new BeforeConstraint(bw.Symbol, bw.ToTask);
                m.AppendAfter(ac);
                m.AppendBefore(bc);
            }

            List<HashSet<int>> symbols = symbolsFromBetweens(m, linearOrdering);
            m.Betweens = new List<BetweenConstraint>(); // remove all betweens from the method

            List<CompoundTask> toBeSearched = new();
            for (int i = 0; i < symbols.Count; i++) // == linearOrdering.Count
            {
                if (symbols[i] == null) continue;

                if (linearOrdering[i] is PrimitiveTask pt)
                {
                    appendStateConstraintsToPrimitiveTask(m, pt, symbols[i]);
                }
                else // CompoundTask
                {
                    CompoundTask? existingCT = existingNewCompound((CompoundTask)linearOrdering[i], symbols[i], linearOrdering);
                    if (existingCT == null) // create new, swap in constraints, copy methods
                    {
                        CompoundTask newCT = createNewCompoundTask((CompoundTask)linearOrdering[i], symbols[i]);
                        toBeSearched.Add(newCT);
                        swapCompoundTask(m, (CompoundTask)linearOrdering[i], newCT);
                    }
                    else
                    {
                        swapCompoundTask(m, (CompoundTask)linearOrdering[i], existingCT);
                    }
                }
            }

            foreach (CompoundTask newCT in toBeSearched)
            {
                copyMethods(newCT);
            }

            foreach (CompoundTask ct in toBeSearched)
            {
                searchCompoundTask(ct);
            }
        }
        private void copyMethods(CompoundTask newCT)
        {
            CompoundTask oldCT = derivedFrom[newCT];

            List<Method> toBeAdded = new();

            foreach (Method m in d.Methods)
            {
                if (m.Head.TaskName.ID == oldCT.TaskName.ID)
                {
                    Method copied = new Method(new CompoundTask(newCT.TaskName.ID, -1), m);
                    toBeAdded.Add(copied);
                }
            }

            foreach (Method m in toBeAdded)
            {
                d.AppendMethod(m);
            }
        }
        private CompoundTask createNewCompoundTask(CompoundTask father, HashSet<int> symbols)
        {
            CompoundTask newCT = new CompoundTask(father, symbols, nextNewCompoundIndex++);
            mandatoryPropSymbols[newCT] = symbols;
            derivedFrom[newCT] = father;

            if (!derivesTo.ContainsKey(father))
            {
                derivesTo[father] = new HashSet<CompoundTask>();
                derivesTo[father].Add(newCT);
            }

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
        private CompoundTask? existingNewCompound(CompoundTask t, HashSet<int> neededSymbols, List<Task> ordering)
        {
            int index = ordering.IndexOf(t);

            int id = ordering[index].TaskName.ID;

            if (!derivesTo.ContainsKey((CompoundTask)ordering[index])) return null;

            foreach (CompoundTask child in derivesTo[t])
            {
                if (mandatoryPropSymbols.ContainsKey(child) && mandatoryPropSymbols[child] == neededSymbols)
                {
                    return new CompoundTask(child.TaskName.ID, nextNewCompoundIndex++);
                }
            }

            return null;
        }
        private void appendStateConstraintsToPrimitiveTask(Method m, PrimitiveTask pt, HashSet<int> symbols)
        {
            int index = m.RightSidePrimitive.IndexOf(pt);
            if (index == -1)
            {
                throw new Exception("Appending state constraints to non-existing Primitive task!");
            }

            foreach(int id in symbols)
            {
                BeforeConstraint bc = new BeforeConstraint(new PropositionalSymbol(id), m.RightSidePrimitive[index]);
                m.AppendBefore(bc);
                AfterConstraint ac = new AfterConstraint(new PropositionalSymbol(id), m.RightSidePrimitive[index]);
                m.AppendAfter(ac);
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
