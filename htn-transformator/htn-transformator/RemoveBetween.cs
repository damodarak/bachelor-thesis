using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class RemoveBetween : ITransformable
    {
        private PlanningDomain d;
        private HashSet<CompoundTask> newCompoundTasks = new HashSet<CompoundTask>();

        // translation between TaskNameIDs and the set of mandatory IDs of propositional symbols
        private Dictionary<int, HashSet<int>> mandatoryPropSymbols = new Dictionary<int, HashSet<int>>();
        public RemoveBetween(PlanningDomain pd) 
        {
            d = pd;
        }
        public PlanningDomain Transform()
        {
            if (!d.IsTotallyOrdered()) throw new Exception("This transformation operates only with totally ordered domains!");

            foreach (var m in d.Methods)
            {
                List<Task> linearOrdering = m.TaskOrdering();
                removeNeighbourBetweens(m, linearOrdering);

                if (m.Betweens.Count > 0)
                {
                    List<HashSet<int>> symbols = symbolsFromBetweens(m, linearOrdering);
                }
            }


            throw new NotImplementedException();
        }
        private void searchCompoundTask(CompoundTask compoundTask)
        {
            throw new NotImplementedException();
        }
        private List<HashSet<int>> symbolsFromBetweens(Method m, List<Task> ordering)
        {
            // ints are reffering to Propositional symbol IDs
            List<HashSet<int>> symbols = new List<HashSet<int>>(ordering.Count);

            foreach (BetweenConstraint bc in m.Betweens)
            {
                int fromIndex = ordering.IndexOf(bc.FromTask);
                int toIndex = ordering.IndexOf(bc.ToTask);

            }

            return null;
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
    }
}
