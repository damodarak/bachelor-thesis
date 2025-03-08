namespace htn_transformator
{
    internal class ToCNF : ITransformable
    {
        private PlanningDomain d;
        public ToCNF(PlanningDomain pd)
        {
            d = pd;
        }
        public PlanningDomain Transform()
        {
            d = (new RemoveBetween(d)).Transform();
            d = (new RemoveEmptyMethods(d)).Transform();

            return d;
        }
    }
}
