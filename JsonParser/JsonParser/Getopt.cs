using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    public class Getopt
    {
        private Stack<string> options;
        private bool mainValue = false;
        public Getopt(string[] args)
        {
            options =  new Stack<string>();
            for (int i = args.Length-1; i >= 0; i--)
                options.Push(args[i]);
                
        }

        public string Get(out string value)
        {
            var ret = String.Empty;
            value = String.Empty;

            if (options.Count == 0)
                return ret;

            ret = options.Pop();

            if ( ret.StartsWith("-") ){
                ret = ret.Substring(1);
            }

            if (options.Count > 0 && !options.Peek().StartsWith("-"))
            {
                value = options.Pop();
                value = value.Replace("\"", "");
            }
            
            if (!mainValue && ret.Contains("."))
            {
                value = ret;
                ret = "default";
                mainValue = true;
            }

            return ret.ToUpper();
        }
    }
}
