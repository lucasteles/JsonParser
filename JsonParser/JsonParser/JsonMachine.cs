using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonParser
{
    
    public struct JsonTokens
    {
        public const char OpenList = '[';
        public const char CloseList = ']';
        public const char OpenDict = '{';
        public const char CloseDict = '}';
        public const char DictKeySeparator = ':';
        public const char ValueSeparator = ',';
        public const char StringDelimiter = '"';
        public const char StringEscape = '\\';
    }

    public class JsonMachine
    {
        private BinaryReader Reader;
        public StringBuilder GeneratedCode = new StringBuilder();
        private Stack<int[]> FlowControlNum = new Stack<int[]>();


        bool genInString = false;
        int tabControl = 1;


        int Lines = 0, Cols = 0;
        Automaton<States> Machine = new Automaton<States>();


        public JsonMachine(BinaryReader reader)
        {
            Reader = reader;

            DefineMachine();

        }

        public bool Analisys(out string message) {
            var ret = true;
            
            message = string.Empty;
            var loop = true;
            char c = '\0';
            while (Reader.PeekChar() != -1 )
            {
                c = (char)Reader.ReadChar();

                if (c == '\r')
                    continue;

                Cols++;
                if (c == '\n')
                {
                    Cols = 0;
                    Lines++;
                    continue;
                }
                if (c == ' ' || c == '\t')
                {
                    if (genInString)
                        gen(c);
                    else
                        continue;
                }
               
               gen(c);
               loop = Machine.Read(c);
               
               if (!loop)
               {
                   message = ErrMsg(c);
                   break;
               }
            }

            
            if (!Machine.Valid())
            {
                ret = false;

                if (message.Equals(String.Empty))
                {
                    if (Machine.StackLenght() == 0)
                    {
                        message = String.Format("Expected token after {0}", c);
                    }
                    else
                    {

                        if ((JsonCollect)Machine.Peek() == JsonCollect.Array)
                            message = String.Format("Expected end of array \"]\" of {0}:{1}", FlowControlNum.Peek()[0], FlowControlNum.Peek()[1]);

                        if ((JsonCollect)Machine.Peek() == JsonCollect.Dictionary)
                            message = String.Format("Expected end of dictionary \"}}\" of {0}:{1}", FlowControlNum.Peek()[0], FlowControlNum.Peek()[1]);
                    }
                }
            }

           
            return ret;
        }
    
        private string ErrMsg(char c)
        {
            return String.Format("unexpected token \"{0}\" at {1}:{2}", c.ToString(), Lines + 1, Cols );
        }


        private void gen(char c)
        {

            if (c == JsonTokens.StringDelimiter)
                genInString = !genInString;

            if (genInString)
            {
                GeneratedCode.Append(c);
                return;
            }

            var tabs = new String('\t', tabControl);



            if (c == JsonTokens.OpenList)
            {
                GeneratedCode.Append("\n");
                GeneratedCode.Append(tabs);
                GeneratedCode.Append("new List<object>(){\n\t");
                GeneratedCode.Append(tabs);
                tabControl++;
                return;
            }

            if (c == JsonTokens.CloseList)
            {
                GeneratedCode.Append("\n");
                GeneratedCode.Append(tabs.Substring(1));
                GeneratedCode.Append("}");
                tabControl--;
                return;
            }

            if (c == JsonTokens.OpenDict)
            {
                tabControl++;
                GeneratedCode.Append("new Dictionary<string,object>(){\n" + tabs + "\t{");

                return;
            }

            if (c == JsonTokens.DictKeySeparator)
            {
                GeneratedCode.Append(",");
                return;
            }


            if (c == JsonTokens.ValueSeparator)
            {
                if (Machine.StackLenght() > 0 && (JsonCollect)Machine.Peek() == JsonCollect.Dictionary)
                    GeneratedCode.Append(" },\n" + tabs + "{");
                else if (Machine.StackLenght() > 0 && (JsonCollect)Machine.Peek() == JsonCollect.Array)
                    GeneratedCode.Append(",\n" + tabs);

                return;
            }
            if (c == JsonTokens.CloseDict)
            {
                tabControl--;
                GeneratedCode.Append("}\n");
                GeneratedCode.Append(tabs.Substring(1));
                GeneratedCode.Append("}");
                return;
            }

            GeneratedCode.Append(c);
        }
        public string GetCode()
        {
            return "var data = " + GeneratedCode.ToString() + ";";
        }


   
        private void DefineMachine()
        {

            var digits = new Regex("^[0-9]+$");

            Machine
             .StartIn(States.INIT)
             .EndIn(States.AD)
             .When(States.INIT)
                 .On(JsonTokens.OpenList, States.VAL, null, JsonCollect.Array)
                 .On(JsonTokens.OpenDict, States.KEY, null, JsonCollect.Dictionary)
             .When(States.KEY)
                .On(JsonTokens.StringDelimiter, States.SK1)
             .When(States.SK1)
                .On(JsonTokens.StringDelimiter, States.SK2)
                .On(JsonTokens.StringEscape, States.SK3)
                .On(e=>true, States.SK1)
             .When(States.SK2)
                .On(JsonTokens.DictKeySeparator, States.VAL)
             .When(States.SK3)
                .On(e => "/\\\"bfnrt".Contains(e), States.ERR)
                .On(e => true, States.SK3)
             .When(States.VAL)
                .On(JsonTokens.OpenList, States.VAL, null, JsonCollect.Array)
                .On(JsonTokens.OpenDict, States.KEY, null, JsonCollect.Dictionary)
                .On(JsonTokens.CloseDict, States.AD, JsonCollect.Dictionary, null)
                .On(JsonTokens.CloseList, States.AD, JsonCollect.Array, null)
                .On('0', States.N4)
                .On('-', States.N1)
                .On(c => c >= '1' && c <= '9', States.N2)
                .On(JsonTokens.StringDelimiter, States.S1)
                .On('t', States.TRUE1)
                .On('f', States.FALSE1)
                .On('n', States.NULL1)
             .When(States.AD)
                .On(JsonTokens.ValueSeparator, States.V)
                .On(JsonTokens.CloseDict, States.AD, JsonCollect.Dictionary, null)
                .On(JsonTokens.CloseList, States.AD, JsonCollect.Array, null)
             .When(States.V)
                .On(States.KEY, JsonCollect.Dictionary, JsonCollect.Dictionary)
                .On(States.VAL, JsonCollect.Array, JsonCollect.Array)
            .When(States.S1)
                .On(JsonTokens.StringEscape, States.S3)
                .On(JsonTokens.StringDelimiter, States.S2)
                .On(e => true, States.S1)
             .When(States.S2)
                .On(JsonTokens.ValueSeparator, States.V)
                .On(JsonTokens.CloseDict, States.AD, JsonCollect.Dictionary, null)
                .On(JsonTokens.CloseList, States.AD, JsonCollect.Array, null)
             .When(States.S3)
                .On(e => "/\\\"bfnrt".Contains(e), States.S1)
                .On('u', States.S4)
              .When(States.S4)
                .On(c => (c >= '0' && c <= '9'), States.S5)
              .When(States.S5)
                .On(c => (c >= '0' && c <= '9'), States.S6)
              .When(States.S6)
                .On(c => (c >= '0' && c <= '9'), States.S7)
              .When(States.S7)
                .On(c => (c >= '0' && c <= '9'), States.S1)
              .When(States.N1)
                .On('0', States.N4)
                .On(digits, States.N2)
              .When(States.N2)
                .On('.', States.N6)
                .On(c=>"eE".Contains(c), States.N5)
                .On(JsonTokens.ValueSeparator, States.V)
                .On(JsonTokens.CloseDict, States.AD, JsonCollect.Dictionary, null)
                .On(JsonTokens.CloseList, States.AD, JsonCollect.Array, null)
                .On(digits,  States.N2)
              .When(States.N3)  
                .On(digits, States.N2)
                .On(JsonTokens.ValueSeparator, States.V)
                .On(JsonTokens.CloseDict, States.AD, JsonCollect.Dictionary, null)
                .On(JsonTokens.CloseList, States.AD, JsonCollect.Array, null)
              .When(States.N4) 
                .On('.', States.N6)
                .On(c=>"eE".Contains(c), States.N5)
                .On(JsonTokens.ValueSeparator, States.V)
                .On(JsonTokens.CloseDict, States.AD, JsonCollect.Dictionary, null)
                .On(JsonTokens.CloseList, States.AD, JsonCollect.Array, null)
                .On(digits,  States.N2)
              .When(States.N5) 
                 .On(digits,  States.N8)
                 .On(c=>"-+".Contains(c), States.N7)
              .When(States.N6) 
                 .On(digits,  States.N3)
              .When(States.N7) 
                 .On(digits,  States.N8)
              .When(States.N8) 
                 .On(digits,  States.N8)
                 .On(JsonTokens.ValueSeparator, States.V)
                 .On(JsonTokens.CloseDict, States.AD, JsonCollect.Dictionary, null)
                 .On(JsonTokens.CloseList, States.AD, JsonCollect.Array, null)
              .When(States.NULL4, States.TRUE4, States.FALSE5)
                 .On(JsonTokens.ValueSeparator, States.V)
                 .On(JsonTokens.CloseDict, States.AD, JsonCollect.Dictionary, null)
                 .On(JsonTokens.CloseList, States.AD, JsonCollect.Array, null)
               .When(States.NULL1)
                  .On('u', States.NULL2)
               .When(States.NULL2)
                  .On('l', States.NULL3)
               .When(States.NULL3)
                  .On('l', States.NULL4)
               .When(States.FALSE1)
                  .On('a', States.FALSE2)
               .When(States.FALSE2)
                  .On('l', States.FALSE3)
               .When(States.FALSE3)
                  .On('s', States.FALSE4)
               .When(States.FALSE4)
                  .On('e', States.FALSE5)
               .When(States.TRUE1)
                  .On('r', States.TRUE2)
               .When(States.TRUE2)
                  .On('u', States.TRUE3)
               .When(States.TRUE3)
                  .On('e', States.TRUE4);
               

            Machine.OnStackPush += (e, o) => FlowControlNum.Push(new int[]{ Lines+1, Cols  });
            Machine.OnStackPop += (e, o) => FlowControlNum.Pop();
        }
    }
}
