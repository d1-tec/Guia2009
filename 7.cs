using System;
using Dominio;
using Excepciones;
using Persistencia;

namespace Logica
{
    public class LlamadaLogica
    {

        public RepositorioMovilYLlamada repositorio;

        public LlamadaLogica(RepositorioMovilYLlamada unRepositorio) {
            repositorio = unRepositorio;
        }

        public LlamadaLogica()
        {
            repositorio = new RepositorioMovilYLlamada();
        }

        public bool FechaYHoraValida(DateTime fecha) {
            bool fechaYHoraEsValida = false;
            if (fecha.Year <= DateTime.MaxValue.Year && fecha.Year >= DateTime.MinValue.Year)
            {
                if (fecha.Month >= 1 && fecha.Month <= 12)
                {
                    if (DateTime.DaysInMonth(fecha.Year, fecha.Month) >= fecha.Day && fecha.Day >= 1)
                    {
                        if (fecha.Hour >= 00 && fecha.Hour <= 23 && fecha.Minute >= 00 && fecha.Minute <= 59)
                        {
                            fechaYHoraEsValida = true;
                        }
                    }
                }
            }
            return fechaYHoraEsValida;
        }

        public void UbicacionValida(float latitud, float longitud)
        {
            try
            {
                LatitudCorrecta(latitud);
                LongitudCorrecta(longitud);
            }
            catch (ExcepcionLlamada e)
            {
                throw new ExcepcionLlamada(e.Message);
            }
        }

        private void LatitudCorrecta(float unaLatitud)
        {
            if (unaLatitud >= -90 && unaLatitud <= 90)
            {
                CantidadDecimalesCorrecta(unaLatitud);
            }
            else
            {
                throw new ExcepcionLlamada("Latitud tiene un valor incorrecto");
            }
        }

        private void CantidadDecimalesCorrecta(float unFloat)
        {
            string numero = unFloat.ToString();

            bool cantidadDeDecimales = (numero.Substring(numero.IndexOf(".") + 1).Length <= 5);

            if (!cantidadDeDecimales)
            {
                throw new ExcepcionLlamada("Cantidad de decimales en ubicación no es correcta, deberian ser maximo 5");
            }
        }

        private void LongitudCorrecta(float unaLongitud)
        {
            if (unaLongitud >= -180 && unaLongitud <= 180)
            {
                CantidadDecimalesCorrecta(unaLongitud);
            }
            else
            {
                throw new ExcepcionLlamada("Longitud tiene un valor incorrecto");
            }
        }

        public void TextoNoVacio(String unTexto)
        {
            if (String.IsNullOrEmpty(unTexto))
            {
                throw new ExcepcionLlamada("Texto vacío");
            }
        }

        public void RegistrarYValidarDatosLlamada(Llamada unaLlamada)
        {
            try
            {
                TextoNoVacio(unaLlamada.Direccion);
                UbicacionValida(unaLlamada.Ubicacion.X, unaLlamada.Ubicacion.Y);
                FechaYHoraValida(unaLlamada.FechaYHoraDeLlamada);
                repositorio.RegistrarLlamada(unaLlamada);
            }
            catch (ExcepcionLlamada error)
            {
                throw new ExcepcionLlamada(error.Message);
            }
        }

    }
}