using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
  public enum States
  {
      LOAD,
      INIT,
      KEY,
      SK1,
      SK2,
      SK3,
      ERR,
      VAL,
      AD,
      AD2,
      FIN,
      V,
      S1,
      S2,
      S3,
      S4,
      S5,
      S6,
      S7,
      N1,
      N2,
      N3,
      N4,
      N5,
      N6,
      N7,
      N8,
      TRUE1,
      TRUE2,
      TRUE3,
      TRUE4,
      FALSE1,
      FALSE2,
      FALSE3,
      FALSE4,
      FALSE5,
      NULL1,
      NULL2,
      NULL3,
      NULL4
  }
}
