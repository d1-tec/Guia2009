using System;

namespace BusinessLogic
{
    public class Uruguay : Country
    {
        public  override bool validateNum(string numero)
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

            if (lengthArray == 8 || lengthArray == 9)
            {
                if (lengthArray == 9)
                {
                    if (charArray[0] == '0')
                    {
                        p = 2;
                    }
                    else
                    {
                        esValido = false;
                    }
                }
                if (lengthArray == 8)
                {
                    p = 1;

                }
                if (esValido)
                {
                    for (int i = p; i < lengthArray; i++)
                    {
                        if ((int)charArray[i] < 48 || (int)charArray[i] > 57)
                        {
                            esValido = false;
                        }

                    }
                }
            }
            else
            {
                esValido = false;
            }
            if (esValido)
            {
                return true;
            }
            else
            {
                throw new BusinessLogicExceptions("Formato invalido");
            }
        }

		public override bool validateMinutes(int mins)
		{
			bool esMult = true;

			if (mins <= 0)
			{
				esMult = false;
			}
			else
			{
				if (mins % 30 != 0)
				{
					esMult = false;
				}

				else
				{
					esMult = true;
				}

			}
			if (esMult)
			{
				return true;
			}
			else
			{
				throw new BusinessLogicExceptions("Los minutos ingresados deben ser multiplos de 30 y mayores que cero");
			}
		}
	}
}