using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class ArgParser
    {
        public string inputFile { get; private set; }
        public string outputFile { get; private set; } = "";
        public TransformationType type { get; private set; }

        public ArgParser(string[] args) 
        {
            if (args.Length != 5 && args.Length != 3)
            {
                throw new Exception("Incorrect argument length!");
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i"  && i != args.Length - 1)
                {
                    inputFile = args[i + 1];
                    i++;
                }
                else if (args[i] == "-o" && i != args.Length - 1)
                {
                    outputFile = args[i + 1];
                    i++;
                }
                else if (args[i] == "--between")
                {
                    type = TransformationType.RemoveBetween;
                }
                else if (args[i] == "--empty")
                {
                    type = TransformationType.RemoveEmptyMethods;
                }
                else if (args[i] == "--tocnf")
                {
                    type = TransformationType.ToCNF;
                }
                else if (args[i] == "--tognf")
                {
                    type = TransformationType.TOGNF;
                }
                else
                {
                    throw new Exception("Incorrect arguments!");
                }               
            }

            if(!File.Exists(inputFile))
            {
                throw new Exception("Incorrect input file!");
            }
        }
    }
}
