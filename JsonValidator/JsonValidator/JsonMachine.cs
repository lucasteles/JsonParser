using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonValidator
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
        public string CODE = String.Empty;
        private Stack<JsonCollect> FlowControl = new Stack<JsonCollect>();
        private string Message = string.Empty;
        States State = States.INIT;
        bool genInString = false;
        
        int Lines = 0, Cols = 0;

        public JsonMachine(BinaryReader reader)
        {
            Reader = reader;
        }

        public bool Analisys(out string message) {
            var ret = true;
            
            message = string.Empty;

            while (Reader.PeekChar() != -1 && State != States.ERR)
            {
                var c = (char)Reader.ReadChar();

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
                    continue;
                }
               
                gen(c);
               MainTransitionFunction(c);
               
                   
            }

            if (State == States.ERR)
            {
                ret = false;
                message = Message;
            }
            return ret;
        }


        private void ErrMsg(char c)
        {
            Message = String.Format("unexpected token \"{0}\" at {1}:{2}", c.ToString(), Lines + 1, Cols + 1);
            State = States.ERR;
        }

        private void MainTransitionFunction(char c)
        {
            int loop = 1;
            for (int i = 0; i < loop; i++) 
            switch (State){
            case States.INIT:
                switch (c){
                    case JsonTokens.OpenList:
                        State = States.VAL;
                        FlowControl.Push(JsonCollect.Array);
                        break;

                    case JsonTokens.OpenDict:
                        State = States.KEY;
                        FlowControl.Push(JsonCollect.Dictionary);
                        break;

                    default:
                        ErrMsg(c);            
                        break;
               }
               break;
                    
            case States.KEY:
               if (c == JsonTokens.StringDelimiter)
                   State = States.SK1;
               else
                   ErrMsg(c);
               break;

            case States.SK1:
               if (c == JsonTokens.StringDelimiter)
                   State = States.SK2;

               if (c == JsonTokens.StringEscape)
                   State = States.SK3;
                
                break;

           case States.SK2:
                if (c == JsonTokens.DictKeySeparator)
                    State = States.VAL;
                else
                    ErrMsg(c);

                break;
            case States.SK3:
                if ("/\\\"bfnrt".Contains(c))
                    ErrMsg(c);

                break;

            case States.ERR:
                ErrMsg(c);
                break;

            case States.VAL:

                if (c == JsonTokens.OpenList)
                    FlowControl.Push(JsonCollect.Array);

                else if (c == JsonTokens.OpenDict)
                {
                    FlowControl.Push(JsonCollect.Dictionary);
                    State = States.KEY;
                }
                else if (c == JsonTokens.CloseDict)
                {
                    if (FlowControl.Peek() == JsonCollect.Dictionary)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else if (c == JsonTokens.CloseList)
                {
                    if (FlowControl.Peek() == JsonCollect.Array)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else if (c == '0')
                    State = States.N4;

                else if (c == '-')
                    State = States.N1;

                else if (c >= '1' && c <= '9')
                    State = States.N2;

                else if (c == JsonTokens.StringDelimiter)
                    State = States.S1;
                else if (c == 't')
                    State = States.TRUE1;
                else if (c == 'f')
                    State = States.FALSE1;
                else if (c == 'n')
                    State = States.NULL1;
                else
                    ErrMsg(c);

                break;
            case States.AD:

                if (c == JsonTokens.ValueSeparator){
                    State = States.V;
                }
                else if (c == JsonTokens.CloseDict)
                {
                    if (FlowControl.Peek() == JsonCollect.Dictionary)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else if (c == JsonTokens.CloseList)
                {
                    if (FlowControl.Peek() == JsonCollect.Array)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }

                break;
            case States.AD2:
                // nunca entra neste estado, o loop foi simplificado
                break;
            case States.FIN:
                // nunca entra neste estado
                break;
            case States.V:
                loop++;
                if (FlowControl.Peek() == JsonCollect.Array)
                    State = States.VAL;

                else if (FlowControl.Peek() == JsonCollect.Dictionary)
                    State = States.KEY;

                else
                    ErrMsg(c);
                    
                break;
            case States.S1:
                 
                 if (c == JsonTokens.StringEscape)
                    State = States.S3;

                 if (c == JsonTokens.StringDelimiter)
                     State = States.S2;
                break;

            case States.S2:
                if (c == JsonTokens.ValueSeparator)
                    State = States.V;
                else if (c == JsonTokens.CloseDict)
                {
                    if (FlowControl.Peek() == JsonCollect.Dictionary)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else if (c == JsonTokens.CloseList)
                {
                    if (FlowControl.Peek() == JsonCollect.Array)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else
                    ErrMsg(c);
                break;

            case States.S3:
                if ("/\\\"bfnrt".Contains(c))
                    State = States.S1;
                else if (c == 'u')
                    State = States.S4;
                else
                    ErrMsg(c);
                break;

            case States.S4:
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))
                    State = States.S5;
                else
                    ErrMsg(c);
                break;
                    
            case States.S5:
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))
                    State = States.S6;
                else
                    ErrMsg(c);
                break;


            case States.S6:
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))
                    State = States.S7;
                else
                    ErrMsg(c);
                break;

            case States.S7:
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))
                    State = States.S1;
                else
                    ErrMsg(c);
                break;

            case States.N1:
                if (c == '0')
                    State = States.N4;
                else if (c >= '0' && c <= '9')
                    State = States.N2;
                else
                    ErrMsg(c);
                break;

            case States.N2:

                if (c == '.')
                    State = States.N6;
                else if (c == 'e' || c == 'E')
                    State = States.N5;
                else if (c == JsonTokens.ValueSeparator)
                    State = States.V;
                else if (c == JsonTokens.CloseDict)
                {
                    if (FlowControl.Peek() == JsonCollect.Dictionary)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else if (c == JsonTokens.CloseList)
                {
                    if (FlowControl.Peek() == JsonCollect.Array)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else if (c >= '0' && c <= '9')
                    State = States.N2;
                else
                    ErrMsg(c);
                break;

            case States.N3:
                if (c >= '0' && c <= '9')
                    State = States.N2;
                else if (c == JsonTokens.ValueSeparator)
                    State = States.V;
                else if (c == JsonTokens.CloseDict)
                {
                    if (FlowControl.Peek() == JsonCollect.Dictionary)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else if (c == JsonTokens.CloseList)
                {
                    if (FlowControl.Peek() == JsonCollect.Array)
                    {
                        State = States.AD;
                        FlowControl.Pop();
                    }
                    else
                        ErrMsg(c);
                }
                else
                    ErrMsg(c);

                break;

            case States.N4:
                 if (c == '.')
                    State = States.N6;
                else if (c == 'e' || c == 'E')
                    State = States.N5;
                else if (c == JsonTokens.ValueSeparator)
                    State = States.V;
                 else if (c == JsonTokens.CloseDict)
                 {
                     if (FlowControl.Peek() == JsonCollect.Dictionary)
                     {
                         State = States.AD;
                         FlowControl.Pop();
                     }
                     else
                         ErrMsg(c);
                 }
                 else if (c == JsonTokens.CloseList)
                 {
                     if (FlowControl.Peek() == JsonCollect.Array)
                     {
                         State = States.AD;
                         FlowControl.Pop();
                     }
                     else
                         ErrMsg(c);
                 }
                else if (c >= '0' && c <= '9')
                    State = States.N2;
                else
                    ErrMsg(c);
                break;

            case States.N5:
                if (c >= '0' && c <= '9')
                    State = States.N8;
                else if (c == '+' || c == '-')
                    State = States.N7;
                else
                    ErrMsg(c);
                break;

            case States.N6:
                if (c >= '0' && c <= '9')
                    State = States.N3;
                else
                    ErrMsg(c);
                break;

            case States.N7:
                if (c >= '0' && c <= '9')
                    State = States.N8;
                else
                    ErrMsg(c);
                break;

            case States.N8:
                if (c >= '0' && c <= '9')
                    State = States.N8;
                else if (c == ',')
                    State = States.V;
                else
                    ErrMsg(c);
                break;

                case States.NULL4:
                case States.TRUE4:
                case States.FALSE5:
                    if (c == JsonTokens.ValueSeparator)
                        State = States.V;
                    else if (c == JsonTokens.CloseDict)
                    {
                        if (FlowControl.Peek() == JsonCollect.Dictionary)
                        {
                            State = States.AD;
                            FlowControl.Pop();
                        }
                        else
                            ErrMsg(c);
                    }
                    else if (c == JsonTokens.CloseList)
                    {
                        if (FlowControl.Peek() == JsonCollect.Array)
                        {
                            State = States.AD;
                            FlowControl.Pop();
                        }
                        else
                            ErrMsg(c);
                    }
                    else
                        ErrMsg(c);
                        break;
                case States.NULL1:
                    if (c == 'u')
                        State = States.NULL2;
                    else
                        ErrMsg(c);
                    break;
                case States.NULL2:
                    if (c == 'l')
                        State = States.NULL3;
                    else
                        ErrMsg(c);
                    break;
                case States.NULL3:
                    if (c == 'l')
                        State = States.NULL4;
                    else
                        ErrMsg(c);
                    break;

                case States.FALSE1:
                    if (c == 'a')
                        State = States.FALSE2;
                    else
                        ErrMsg(c);
                    break;

                case States.FALSE2:
                    if (c == 'l')
                        State = States.FALSE3;
                    else
                        ErrMsg(c);
                    break;

                case States.FALSE3:
                    if (c == 's')
                        State = States.FALSE4;
                    else
                        ErrMsg(c);
                    break;

                case States.FALSE4:
                    if (c == 'e')
                        State = States.FALSE5;
                    else
                        ErrMsg(c);
                    break;
                case States.TRUE1:
                    if (c == 'r')
                        State = States.TRUE2;
                    else
                        ErrMsg(c);
                    break;

                case States.TRUE2:
                    if (c == 'u')
                        State = States.TRUE3;
                    else
                        ErrMsg(c);
                    break;

                case States.TRUE3:
                    if (c == 'e')
                        State = States.TRUE4;
                    else
                        ErrMsg(c);
                    break;
            default:
                ErrMsg(c);
                break;
	        }
            

        }


        private void gen(char c) { 

            if (c==JsonTokens.StringDelimiter)
                genInString=!genInString;

            if (genInString)
            {
                CODE+=c;
                return;
            }

            if (c == JsonTokens.OpenList)
            {
                CODE += "new List<object>(){\n";
                return;
            }

            if (c == JsonTokens.CloseList)
            {
                CODE += "\n}";
                return;
            }
           
            if (c == JsonTokens.OpenDict)
            {
                CODE += "new Dictionary<string,object>(){\n\t{";
                return;
            }

            if (c == JsonTokens.DictKeySeparator)
            {
                CODE += ",";
                return;
            }



            if (c == JsonTokens.ValueSeparator)
            {
                if (FlowControl.Peek() == JsonCollect.Dictionary)
                    CODE += "},\n\t{";
                else if (FlowControl.Peek() == JsonCollect.Array)
                    CODE += ",";

                return;
            }
            if (c == JsonTokens.CloseDict)
            {
                CODE += "}\n}";
                return;
            }
                
            CODE += c;
        }

        public string GetCode()
        {
            return "var data = \n" + CODE + ";";
        }
    }
}
