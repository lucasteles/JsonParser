using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            string Caminho = @"C:\Users\rmendonca\Documents\GitHub\JsonValidator\Test.json";

            Json Validador = new Json();
            Validador.ValidJson(Caminho);
        }
    }
}
