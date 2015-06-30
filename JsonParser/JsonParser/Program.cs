using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    class Program
    {

       static string JsonFile = String.Empty;
       static string GenerateFile = String.Empty;

       static void Main(string[] args)
       {
           readArgs(args);

           if (JsonFile == String.Empty)
               return;

           // valida arquivo
           if (!File.Exists(JsonFile))
           {
               Console.WriteLine("File  \"{0}\" not found", JsonFile);
               exit();
               return;
           }
           
           using (var fileStream = new FileStream(JsonFile, FileMode.Open))
           using (var binaryReader = new BinaryReader(fileStream))
           {
               var machine = new JsonMachine(binaryReader);
               string err;
               if (!machine.Analisys(out err))
               {
                   Console.Write("Error: ");
                   Console.Write(err);
               }
               else
               {
                   Console.Write("valid file");
                   if (!String.IsNullOrEmpty(GenerateFile))
                       using (StreamWriter outfile = new StreamWriter(GenerateFile))
                           outfile.Write(machine.GetCode());

               }

               exit();
             }
}
       


        private static void readArgs(string[] args)
        {
            /// inicia leitura do arquivo
            var opt = new Getopt(args);

            string value, o;
            while ((o = opt.Get(out value)) != String.Empty)
                switch (o)
                {

                    case "DEFAULT":
                        JsonFile = value;
                        break;

                    case "O":
                        GenerateFile = value;
                        break;
                    case "H":
                        DumpUsage();
                        break;
                    default:
                        Console.WriteLine("Unexpected command argument \"{0}\"\n\n", o.ToLower());
                        DumpUsage();
                                  
                        break;
                }

            
        }

        private static void DumpUsage()
        {
            Console.WriteLine(
                @"
Validate only
$ JsonParser <file.json>


Generate CSharp equivalent
$ JsonParser <file.json> -o <out.cs>

                "
                );
            exit();
        }

        private static void exit()
        {
#if DEBUG
            Console.ReadKey();
#endif

#if RELEASE
             Environment.Exit(0);
#endif
        }
    }
}
