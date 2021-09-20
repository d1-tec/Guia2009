using System;

namespace BusinessLogic
{
	public abstract class Country
	{
		public static bool validateMatricula(string matricula)
		{
			bool esValido = true;
			matricula = matricula.Replace(" ", "");
			Char[] charArray = matricula.ToCharArray();
			int lengthArray = charArray.Length;


			if (lengthArray == 7)
			{
				for (int i = 0; i <= 2; i++)
				{
					if (((int)charArray[i] < 65 || (int)charArray[i] > 90 && (int)charArray[i] < 97) || ((int)charArray[i] < 97 && (int)charArray[i] > 90) || (int)charArray[i] > 122)
					{
						esValido = false;
					}

				}

				for (int j = 3; j <= 6; j++)
				{
					if ((int)charArray[j] < 48 || (int)charArray[j] > 57)
					{
						esValido = false;
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
				throw new BusinessLogicExceptions("Matricula invalida");
			}

		}
		public static DateTime calFinalHour(DateTime dtHoraInicial, int mins)
		{
			string strHoraMin = "10:00";
			string strHoraMax = "18:00";
			DateTime dtHoraMin = DateTime.Parse(strHoraMin);
			DateTime dtHoraMax = DateTime.Parse(strHoraMax);
			DateTime dtHoraFinal = dtHoraInicial.AddMinutes(mins);

			if (dtHoraFinal.Hour > dtHoraMax.Hour)
			{
				TimeSpan diferenciaMins = dtHoraFinal - dtHoraMax;
				int minsHoras = diferenciaMins.Hours;
				minsHoras = (minsHoras * 60);
				minsHoras += diferenciaMins.Minutes;
				mins -= minsHoras;
			}
			return dtHoraInicial.AddMinutes(mins);
		}

		public bool validateHour(DateTime dtHoraIni, int mins)
		{
			bool esValida = true;
			string strHoraMin = "10:00";
			string strHoraMax = "18:00";
			DateTime dtHoraMin = DateTime.Parse(strHoraMin);
			DateTime dtHoraMax = DateTime.Parse(strHoraMax);
			DateTime dtHoraFinal = dtHoraIni.AddMinutes(mins);

			if ((DateTime.Compare(dtHoraIni, dtHoraMin) < 0) || DateTime.Compare(dtHoraMax, dtHoraIni) <= 0)
			{
				esValida = false;
			}
			if (esValida)
			{
				return true;
			}
			else
			{
				throw new BusinessLogicExceptions("La hora ingresada no es válida (Hora valida: 10-18)");
			}

		}

		public bool validateMessageFormat(string message)
		{
			string matriculaRecibida = "";
			int intMins = 0;
			DateTime dtHoraInicial = new DateTime();
			Char[] arrHora = new Char[5];
			message = message.Replace(" ", "");
			Char[] arrCharMensaje = message.ToCharArray();
			int lengthArray = arrCharMensaje.Length;
			bool esValido = true;

			if (lengthArray < 9 || (lengthArray > 10 && lengthArray < 14) || lengthArray > 15)
			{
				esValido = false;
			}
			else
			{
				for (int i = 0; i <= 6; i++)
				{
					matriculaRecibida += arrCharMensaje[i];
				}

				validateMatricula(matriculaRecibida);

				Char[] arrMinutosRecibidos = new Char[3];
				arrMinutosRecibidos[0] = arrCharMensaje[7];
				arrMinutosRecibidos[1] = arrCharMensaje[8];

				if (lengthArray == 15 || lengthArray == 10)
				{
					arrMinutosRecibidos[2] = arrCharMensaje[9];
				}

				string strMinutos = string.Concat(arrMinutosRecibidos);

				intMins = int.Parse(strMinutos);

				this.validateMinutes(intMins);

				if (lengthArray == 9 || lengthArray == 10)
				{
					dtHoraInicial = DateTime.Now;
				}
				 
				string strHoraInicial = "";

				if (lengthArray == 15)
				{
					strHoraInicial = convertArrString(arrCharMensaje, 10, 14);
					dtHoraInicial = DateTime.Parse(strHoraInicial);
				}

				if (lengthArray == 14)
				{
					strHoraInicial = convertArrString(arrCharMensaje, 9, 13);
					dtHoraInicial = DateTime.Parse(strHoraInicial);
				}
				validateHour(dtHoraInicial, intMins);
			}
			if (esValido)
			{
				return true;
			}
			else
			{
				throw new BusinessLogicExceptions("Mensaje incorrecto.Ej: ABC 1234 60 10:00");
			}

		}

		public static string convertArrString(Char[] arrCharLetters, int start, int final)
		{

			string strReturn = "";
			for (int i = start; i <= final; i++)
			{
				strReturn += arrCharLetters[i];
			}

			return strReturn;


		}

		public abstract bool validateMinutes(int mins);
        public abstract bool validateNum(string numero);

	}
}