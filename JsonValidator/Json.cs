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

        public Json() { 
        
        }

        public void ValidJson(string FileName)
        {
            using (FileStream oFileStream = new FileStream(FileName, FileMode.Open))
            {
                BinaryReader oReader = new BinaryReader(oFileStream);

                while (oReader.PeekChar() != -1)
                {
                    Caractere = oReader.ReadChar();

                    switch (Estado)
                    {
                        case -1: //ERRO
                            return;

                        case 0:
                            Pilha.Push('$');
                            Estado = 1;
                            break;
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
                            break;
                        case 2:
                            Estado = -1;
                            if (Caractere == '[')
                                Estado = 2;
                            if (Caractere == '{')
                                Estado = 2;

                            if (Caractere == '"')
                                Estado = 2;
                            if (Caractere == '-')
                                Estado = 2;
                            if (Caractere == '0')
                                Estado = 2;
                            if (Caractere >= '1' && Caractere <= '9')
                                Estado = 2;
                            if (Caractere == 't')
                                Estado = 2;
                            if (Caractere == 'f')
                                Estado = 2;
                            if (Caractere == 'n')
                                Estado = 2;
                            break;
                        case 3:
                            Estado = -1;
                            if (Caractere == '"')
                                Estado = 4;
                            break;
                        default:
                            break;
                    }
                    // valida a string do obj
                    ValidNameObj();
                    // valida uma string
                    ValidString();
                    // valida se é uma numero valido
                    ValidNumeric();
                    // verifica se está escreto true ou false
                    ValidBool();
                    // verifica se está escrito null
                    ValidNull();
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
                    break;
                case 5:
                    Estado = -1;
                    break;
                case 6:
                    Estado = -1;
                    if (Caractere == ':')
                        Estado = 2;
                    break;
                default:
                    break;
            }
        }

        public void ValidString()
        {
            switch (Estado)
            {
                default:
                    break;
            }
        }

        public void ValidNumeric()
        {

            switch (Estado)
            {
                default:
                    break;
            }
        }

        public void ValidBool()
        {

            switch (Estado)
            {
                default:
                    break;
            }
        }

        public void ValidNull()
        {
            switch (Estado)
            {
                default:
                    break;
            }
        }
    }
}
