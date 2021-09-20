using BusinessLogic;
using System;

namespace BusinessLogicTest
{
    public class Argentina : Country
    {
        public override bool validateMinutes(int mins)
        {
            if (mins > 0)
                return true;
            else
                return false;
        }

        public override bool validateNum(string numero)
        {
            if (String.IsNullOrEmpty(numero))
            {
                throw new BusinessLogicExceptions("No puede ingrear un numero vacio");
            }
            bool esValido = true;
            numero = numero.Replace(" ", "");
            Char[] charArray = numero.ToCharArray();
            int lengthArray = charArray.Length;
            int p = 0;
            bool last = false;

            for(int i = 0; i < lengthArray; i++)
            {
                if ((int)charArray[i] == 45)
                {
                    if (last)
                    {
                        throw new BusinessLogicExceptions("El numero no puede contener dos guiones seguidos");
                    }
                    else
                    {
                        last = true;
                    }
                } else
                {
                    last = false;
                }
            }
        }

            /* Los números de móvil deben ser
secuencias de dígitos de largo 6, 7 u 8, no pueden contener espacios y
opcionalmente pueden tener uno o más guiones “-” en su interior (en cualquier
posición menos la primera y la última). Algunos ejemplos: “123456”, “1234567”,
“12345678”, “123-4567”, “1-2345678”.*/
        }
    }
}