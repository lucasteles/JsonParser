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
            string Caminho = @"..\..\..\Test.json";

            Json Validador = new Json();
            Validador.ValidJson(Caminho);

            if(Validador.Aceito)
                Console.WriteLine("Aceitou");
            else
                Console.WriteLine("Erro\nLinha:"+Validador.Line.ToString());
            Console.ReadKey();
        }
    }
}
