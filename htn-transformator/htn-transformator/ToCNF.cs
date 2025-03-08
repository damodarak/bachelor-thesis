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
            d = (new RemoveUnitMethods(d)).Transform();

            // now we need to split large methods with 2 > Compounds and interchange Primitives with new Unit Compounds

            return d;
        }
    }
}
