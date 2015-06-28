using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JsonValidator
{
    public class Json
    {
        Stack Pilha = new Stack();
        int Estado = 0;
        char Caractere;
        bool Loop;
        public int Line = 1;
        public bool Aceito = false;
        public Json() { 
        
        }

        public void ValidJson(string FileName)
        {
            using (FileStream oFileStream = new FileStream(FileName, FileMode.Open))
            {
                BinaryReader oReader = new BinaryReader(oFileStream);
                Pilha.Push('$');
                Estado = 1;

                while (oReader.PeekChar() != -1)
                {
                    Caractere = oReader.ReadChar();
                    Loop = false;
                    if(Line==3)
                        Loop = false;

                    switch (Estado)
                    {
                        case -1: //ERRO
                            return;
                        case 1:
                            Estado = -1;
                            if (Caractere == '[')
                            {
                                Pilha.Push('A');
                                Estado = 2;
                            }

                            if (Caractere == '{')
                            {
                                Pilha.Push('D');
                                Estado = 3;
                            }
                            Loop = true;
                            break;
                        case 2:
                            Estado = -1;
                            if (Caractere == '[')
                            {
                                Pilha.Push('A');
                                Estado = 2;
                            }
                            if (Caractere == '{')
                            {
                                Pilha.Push('D');
                                Estado = 3;
                            }
                            if (Caractere == '"')
                                Estado = 7;

                            if (Caractere == '\r')
                                Estado = 2;
                            if (Caractere == ' ')
                                Estado = 2;
                            
                            if (Caractere == '\n')
                            {
                                Estado = 2;
                                Line++;
                            }

                            if (Caractere == '}')
                            {
                                char c = (char)Pilha.Pop();

                                if(c == 'D')
                                    Estado = 32;
                            }
                            if (Caractere == ']')
                            {
                                char c = (char)Pilha.Pop();

                                if (c == 'A')
                                    Estado = 32;
                            }

                            if (Caractere == '-')
                                Estado = 14;
                            if (Caractere == '0')
                                Estado = 15;
                            if (Caractere >= '1' && Caractere <= '9')
                                Estado = 16;
                            if (Caractere == 't')
                                Estado = 22;
                            if (Caractere == 'f')
                                Estado = 25;
                            if (Caractere == 'n')
                                Estado = 29;

                            Loop = true;
                            break;
                        case 3:
                            Estado = -1;
                            if (Caractere == '"')
                                Estado = 4;
                            if (Caractere == '\r')
                                Estado = 3;
                            if (Caractere == ' ')
                                Estado = 3;
                            
                            if (Caractere == '\n')
                            {
                                Estado = 3;
                                Line++;
                            }
                            if (Caractere == '}')
                            {
                                char c = (char)Pilha.Pop();

                                if(c == 'D')
                                    Estado = 32;
                            }
                            if (Caractere == ']')
                            {
                                char c = (char)Pilha.Pop();

                                if (c == 'A')
                                    Estado = 32;
                            }
                            Loop = true;
                            break;
                        case 9:
                            Estado = -1;
                            if (Caractere == ',')
                            {
                                char c = (char)Pilha.Pop();

                                if (c == 'A')
                                {
                                    Estado = 2;
                                    Pilha.Push('A');
                                }

                                if (c == 'D')
                                {
                                    Estado = 3;
                                    Pilha.Push('D');
                                }
                            }
                            if (Caractere == '}')
                            {
                                char c = (char)Pilha.Pop();

                                if(c == 'D')
                                    Estado = 32;
                            }
                            if (Caractere == ']')
                            {
                                char c = (char)Pilha.Pop();

                                if (c == 'A')
                                    Estado = 32;
                            }
                            if (Caractere == '\r')
                                Estado = 9;
                            if (Caractere == ' ')
                                Estado = 9;
                            
                            if (Caractere == '\n')
                            {
                                Estado = 9;
                                Line++;
                            }
                            Loop = true;
                            break;
                        case 32:
                            Estado = -1;
                            if (Caractere == ',')
                            {
                                char c = (char)Pilha.Pop();

                                if (c == 'A')
                                {
                                    Estado = 2;
                                    Pilha.Push('A');
                                }

                                if (c == 'D')
                                {
                                    Estado = 3;
                                    Pilha.Push('D');
                                }
                            }
                            if (Caractere == '}')
                            {
                                char c = (char)Pilha.Pop();

                                if(c == 'D')
                                    Estado = 32;
                            }
                            if (Caractere == ']')
                            {
                                char c = (char)Pilha.Pop();

                                if (c == 'A')
                                    Estado = 32;
                            }
                            if (Caractere == '\r')
                                Estado = 32;
                            if (Caractere == ' ')
                                Estado = 32;
                            
                            if (Caractere == '\n')
                            {
                                Estado = 32;
                                Line++;
                            }
                            Loop = true;
                            break;
                        default:
                            break;
                    }
                    // valida a string do obj
                    if (!Loop)
                        ValidNameObj();
                    // valida uma string
                    if (!Loop)
                        ValidString();
                    // valida se é uma numero valido
                    if (!Loop)
                        ValidNumeric();
                    // verifica se está escreto true ou false
                    if (!Loop)
                        ValidBool();
                    // verifica se está escrito null
                    if (!Loop)
                        ValidNull();
                }

                if (Estado == 32)
                {
                    char c = (char)Pilha.Pop();
                    if(c == '$')
                        Aceito = true;
                }
            }
        }

        public void ValidNameObj()
        {
            switch (Estado)
            {
                case 4:
                    Estado = -1;
                    if (Caractere != '"')
                        Estado = 4;
                    if (Caractere == '\\')
                        Estado = 5;
                    if (Caractere == '"')
                        Estado = 6;
                    Loop = true;
                    break;
                case 5:
                    Estado = -1;
                    if (Caractere != '"')
                        Estado = 4;
                    if ("/\\\"bfnrt".Contains(Caractere))
                        Estado = -1;
                    Loop = true;
                    break;
                case 6:
                    Estado = -1;
                    if (Caractere == ':')
                        Estado = 2;
                    Loop = true;
                    break;
                default:
                    break;
            }
        }

        public void ValidString()
        {
            switch (Estado)
            {
                case 7:
                    Estado = 7;

                    if (Caractere == '\\')
                        Estado = 8;
                    if (Caractere == '"')
                        Estado = 9;
                    
                    Loop = true;
                    break;
                case 8:
                    Estado = -1;
                    if ("/\\\"bfnrt".Contains(Caractere))
                        Estado = 7;
                    if (Caractere == 'u')
                        Estado = 10;
                    Loop = true;
                    break;
                case 10:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 11;
                    if (Caractere >= 'A' && Caractere <= 'F')
                        Estado = 11;
                    Loop = true;
                    break;
                case 11:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 12;
                    if (Caractere >= 'A' && Caractere <= 'F')
                        Estado = 12;
                    Loop = true;
                    break;
                case 12:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 13;
                    if (Caractere >= 'A' && Caractere <= 'F')
                        Estado = 13;
                    Loop = true;
                    break;
                case 13:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 7;
                    if (Caractere >= 'A' && Caractere <= 'F')
                        Estado = 7;
                    Loop = true;
                    break;
                default:
                    break;
            }
        }

        public void ValidNumeric()
        {

            switch (Estado)
            {
                case 14:
                    Estado = -1;
                    if (Caractere == '0')
                        Estado = 15;
                    if (Caractere >= '1' && Caractere <= '9')
                        Estado = 16;
                    Loop = true;
                    break;
                case 15:
                    Estado = -1;
                    if (Caractere == ',')
                    {
                        char c = (char)Pilha.Pop();

                        if (c == 'A')
                        {
                            Estado = 2;
                            Pilha.Push('A');
                        }

                        if (c == 'D')
                        {
                            Estado = 3;
                            Pilha.Push('D');
                        }
                    }
                    if (Caractere == '}')
                    {
                        char c = (char)Pilha.Pop();

                        if(c == 'D')
                            Estado = 32;
                    }
                    if (Caractere == ']')
                    {
                        char c = (char)Pilha.Pop();

                        if (c == 'A')
                            Estado = 32;
                    }
                    if (Caractere == '\r')
                        Estado = 9;
                    if (Caractere == ' ')
                        Estado = 9;
                            
                    if (Caractere == '\n')
                    {
                        Estado = 9;
                        Line++;
                    }
                    break;
                case 16:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 16;
                    if (Caractere == '.')
                        Estado = 17;
                    if (Caractere == 'e' && Caractere == 'E')
                        Estado = 19;

                    if (Caractere == ',')
                    {
                        char c = (char)Pilha.Pop();

                        if (c == 'A')
                        {
                            Estado = 2;
                            Pilha.Push('A');
                        }

                        if (c == 'D')
                        {
                            Estado = 3;
                            Pilha.Push('D');
                        }
                    }
                    if (Caractere == '}')
                    {
                        char c = (char)Pilha.Pop();

                        if(c == 'D')
                            Estado = 32;
                    }
                    if (Caractere == ']')
                    {
                        char c = (char)Pilha.Pop();

                        if (c == 'A')
                            Estado = 32;
                    }
                    if (Caractere == '\r')
                        Estado = 9;
                    if (Caractere == ' ')
                        Estado = 9;
                            
                    if (Caractere == '\n')
                    {
                        Estado = 9;
                        Line++;
                    }
                    Loop = true;
                    break;
                case 17:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 18;
                    Loop = true;
                    break;
                case 18:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 18;
                    if (Caractere == 'e' && Caractere == 'E')
                        Estado = 19;

                    if (Caractere == ',')
                    {
                        char c = (char)Pilha.Pop();

                        if (c == 'A')
                        {
                            Estado = 2;
                            Pilha.Push('A');
                        }

                        if (c == 'D')
                        {
                            Estado = 3;
                            Pilha.Push('D');
                        }
                    }
                    if (Caractere == '}')
                    {
                        char c = (char)Pilha.Pop();

                        if(c == 'D')
                            Estado = 32;
                    }
                    if (Caractere == ']')
                    {
                        char c = (char)Pilha.Pop();

                        if (c == 'A')
                            Estado = 32;
                    }
                    if (Caractere == '\r')
                        Estado = 9;
                    if (Caractere == ' ')
                        Estado = 9;
                            
                    if (Caractere == '\n')
                    {
                        Estado = 9;
                        Line++;
                    }
                    Loop = true;
                    break;
                case 19:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 21;
                    if (Caractere == '+' && Caractere == '-')
                        Estado = 20;
                    Loop = true;
                    break;
                case 20:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 21;
                    Loop = true;
                    break;
                case 21:
                    Estado = -1;
                    if (Caractere >= '0' && Caractere <= '9')
                        Estado = 21;
                    if (Caractere == ',')
                    {
                        char c = (char)Pilha.Pop();

                        if (c == 'A')
                        {
                            Estado = 2;
                            Pilha.Push('A');
                        }

                        if (c == 'D')
                        {
                            Estado = 3;
                            Pilha.Push('D');
                        }
                    }
                    if (Caractere == '}')
                    {
                        char c = (char)Pilha.Pop();

                        if(c == 'D')
                            Estado = 32;
                    }
                    if (Caractere == ']')
                    {
                        char c = (char)Pilha.Pop();

                        if (c == 'A')
                            Estado = 32;
                    }
                    if (Caractere == '\r')
                        Estado = 9;
                    if (Caractere == ' ')
                        Estado = 9;
                            
                    if (Caractere == '\n')
                    {
                        Estado = 9;
                        Line++;
                    }
                    Loop = true;
                    break;
                default:
                    break;
            }
        }

        public void ValidBool()
        {
            
            switch (Estado)
            {
                case 22:
                    Estado = -1;
                    if (Caractere == 'r')
                        Estado = 23;
                    Loop = true;
                    break;
                case 23:
                    Estado = -1;
                    if (Caractere == 'u')
                        Estado = 24;
                    Loop = true;
                    break;
                case 24:
                    Estado = -1;
                    if (Caractere == 'e')
                        Estado = 9;
                    Loop = true;
                    break;
                case 25:
                    Estado = -1;
                    if (Caractere == 'a')
                        Estado = 26;
                    Loop = true;
                    break;
                case 26:
                    Estado = -1;
                    if (Caractere == 'l')
                        Estado = 27;
                    Loop = true;
                    break;
                case 27:
                    Estado = -1;
                    if (Caractere == 's')
                        Estado = 28;
                    Loop = true;
                    break;
                case 28:
                    Estado = -1;
                    if (Caractere == 'e')
                        Estado = 9;
                    Loop = true;
                    break;
                default:
                    break;
            }
        }

        public void ValidNull()
        {
            switch (Estado)
            {
                case 29:
                    Estado = -1;
                    if (Caractere == 'u')
                        Estado = 30;
                    break;
                case 30:
                    Estado = -1;
                    if (Caractere == 'l')
                        Estado = 31;
                    break;
                case 31:
                    Estado = -1;
                    if (Caractere == 'l')
                        Estado = 9;
                    break;
                default:
                    break;
            }
        }
    }
}
