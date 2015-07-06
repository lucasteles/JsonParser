using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using JsonParser;

namespace JsonParser.Test
{
    [TestClass]
    public class UnitTest1
    {

        


        [TestMethod]
        public void TestValidJson()
        {
            string validJason = JsonConvert.SerializeObject(
                    new Dictionary<string,List<object>> { 
                        {"vai1", new List<object>(){1,"teste1"}},
                        {"vai2", new List<object>(){2,"teste2"}},
                        {"vai3", new List<object>(){3,"teste3"}}
                });
            
            string outmsg;

            Assert.AreEqual(true, TestJson(validJason,out outmsg),"Valid json considered invalid");

        }


        [TestMethod]
        public void TestInvalidJson()
        {
            string validJason = "{\"name\":\"go horse\", \"value\" : 1.23},1";
            
            string outmsg;
            var result = TestJson(validJason,out outmsg);

            Assert.AreEqual(false, result, "invalid json considered valid");

        }


        [TestMethod]
        public void TestGenerate()
        {
            
            string validJason = JsonConvert.SerializeObject(new Dictionary<string, object>(){
		                    {"name","go  horse" },
		                    {"value",1.23}});

            string outmsg;
            TestJson(validJason, out outmsg);
            bool equal= true;

            try
            {
                var code = "var data = new Dictionary<string,object>(){\n\t\t{\"name\",\"go    horse\" },\n\t\t{\"value\",1.23}\n\t};";
                equal = code.Equals(outmsg);

                

            }
            catch 
            {
                equal = false;
            }


            Assert.AreEqual(true, equal, "generated code is invalid");

        }

        private bool TestJson(string json, out string code)
        {
            using (var fileStream = GenerateStreamFromString(json))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var machine = new JsonMachine(binaryReader);
                string err;
                if (!machine.Analisys(out err))
                {
                    code = "Error" + err;
                    return false;
                }
                else
                {
                    code = machine.GetCode();
                    return true;
                }
            }

        }

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }



    }
}
