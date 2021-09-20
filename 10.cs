using System.Collections.Generic;
using System.Linq;
using Dominio;
using Excepciones;
using Persistencia;

namespace Logica
{
    public class SistemaLogica
    {

        public RepositorioMovilYLlamada repositorio;

        public TipoDeAsignacion modoAsignacion;

        public SistemaLogica(RepositorioMovilYLlamada unRepositorio)
        {
            repositorio = unRepositorio;
            modoAsignacion = new ModoAsignacionPorDefecto();
        }

        public SistemaLogica()
        {
            repositorio = new RepositorioMovilYLlamada();
            modoAsignacion = new ModoAsignacionPorDefecto();
        }

        public void AsignarLlamadasEnEspera()
        {
            modoAsignacion.AsignarLlamadasEnEspera(this);
        }

        public void AsignarMovilALlamada(Llamada unaLlamada)
        {
            modoAsignacion.AsignarMovilALlamada(unaLlamada, this);
        }

        public int EstadisticasRegistroYAsignacionMovil(Movil movilSeleccionado)
        {
            try
            {
                int tiempoMedioEnMinutos = 0;

                List<Llamada> listaLlamados = repositorio.LlamadasResueltasOEnProceso();

                int cantidadLlamadas = listaLlamados.Count;

                for (int posicionLista = 0; posicionLista < listaLlamados.Count; posicionLista++)
                {
                    if (listaLlamados.ElementAt(posicionLista).MovilAsignado.Equals(movilSeleccionado))
                    {
                        Llamada unaLlamada = listaLlamados.ElementAt(posicionLista);
                        tiempoMedioEnMinutos += (int)(unaLlamada.FechaYHoraDeAsignacion - unaLlamada.FechaYHoraDeLlamada).TotalMinutes;
                    }

                }
                return tiempoMedioEnMinutos / cantidadLlamadas;
            }
            catch (ExcepcionGenerica)
            {
                return 0;
            }
        }

        public int EstadisticasAsignacionYResolucionMovil(Movil movilSeleccionado)
        {
            try
            {
                int tiempoMedioEnMinutos = 0;

                List<Llamada> listaLlamados = repositorio.LlamadasResueltas();

                int cantidadLlamadas = listaLlamados.Count;

                for (int posicionLista = 0; posicionLista < listaLlamados.Count; posicionLista++)
                {
                    if (listaLlamados.ElementAt(posicionLista).MovilAsignado.Equals(movilSeleccionado))
                    {
                        Llamada unaLlamada = listaLlamados.ElementAt(posicionLista);
                        tiempoMedioEnMinutos += (int)(unaLlamada.FechaYHoraDeResolucion - unaLlamada.FechaYHoraDeAsignacion).TotalMinutes;
                    }

                }
                return tiempoMedioEnMinutos / cantidadLlamadas;
            }
            catch (ExcepcionGenerica)
            {
                return 0;
            }
        }

        public int EstadisticasRegistroYResolucionMovil(Movil movilSeleccionado)
        {
            try
            {
                int tiempoMedioEnMinutos = 0;

                List<Llamada> listaLlamados = repositorio.LlamadasResueltas();

                int cantidadLlamadas = listaLlamados.Count;

                for (int posicionLista = 0; posicionLista < listaLlamados.Count; posicionLista++)
                {
                    Llamada unaLlamada = listaLlamados.ElementAt(posicionLista);
                    tiempoMedioEnMinutos += (int)(unaLlamada.FechaYHoraDeResolucion - unaLlamada.FechaYHoraDeLlamada).TotalMinutes;
                }
                return tiempoMedioEnMinutos / cantidadLlamadas;
            }
            catch (ExcepcionGenerica)
            {
                return 0;
            }
        }

        public int EstadisticasRegistroYAsignacion(List<Llamada> listaLlamadasACalcularEstadisticas)
        {
            try
            {
                int tiempoMedioEnMinutos = 0;

                List < Llamada > listaLlamados = listaLlamadasACalcularEstadisticas;

                int cantidadLlamadas = listaLlamados.Count;

                for (int posicionLista = 0; posicionLista < listaLlamados.Count; posicionLista++)
                {
                    Llamada unaLlamada = listaLlamados.ElementAt(posicionLista);
                    tiempoMedioEnMinutos += (int)(unaLlamada.FechaYHoraDeAsignacion - unaLlamada.FechaYHoraDeLlamada).TotalMinutes;
                }
                    return tiempoMedioEnMinutos / cantidadLlamadas;
            }
            catch(ExcepcionGenerica) {
                return 0;
            }
        }

        public int EstadisticasAsignacionYResolucion(List<Llamada> listaLlamadasACalcularEstadisticas)
        {
            try
            {
                int tiempoMedioEnMinutos = 0;

                List<Llamada> listaLlamados = listaLlamadasACalcularEstadisticas;

                int cantidadLlamadas = listaLlamados.Count;

                for (int posicionLista = 0; posicionLista < listaLlamados.Count; posicionLista++)
                {
                    Llamada unaLlamada = listaLlamados.ElementAt(posicionLista);
                    tiempoMedioEnMinutos += (int)(unaLlamada.FechaYHoraDeResolucion - unaLlamada.FechaYHoraDeAsignacion).TotalMinutes;
                }
                return tiempoMedioEnMinutos / cantidadLlamadas;
            }
            catch (ExcepcionGenerica)
            {
                return 0;
            }
        }

        public int EstadisticasRegistroYResolucion(List<Llamada> listaLlamadasACalcularEstadisticas)
        {
            try
            {
                int tiempoMedioEnMinutos = 0;

                List<Llamada> listaLlamados = listaLlamadasACalcularEstadisticas;

                int cantidadLlamadas = listaLlamados.Count;

                for (int posicionLista = 0; posicionLista < listaLlamados.Count; posicionLista++)
                {
                    Llamada unaLlamada = listaLlamados.ElementAt(posicionLista);
                    tiempoMedioEnMinutos += (int)(unaLlamada.FechaYHoraDeResolucion - unaLlamada.FechaYHoraDeLlamada).TotalMinutes;
                }
                return tiempoMedioEnMinutos / cantidadLlamadas;
            }
            catch (ExcepcionGenerica)
            {
                return 0;
            }
        }


    }
}