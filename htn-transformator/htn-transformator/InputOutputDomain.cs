using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class InputOutputDomain
    {
        private string inputFile { get; set; }
        private string outputFile { get; set; }
        public InputOutputDomain(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
        }
        public PlanningDomain LoadDomain()
        {
            throw new NotImplementedException();
        }
        public void StoreDomain()
        {
            throw new NotImplementedException();
        }
    }
}
