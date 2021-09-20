using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio;
using System.Drawing;

namespace Logica
{
    public class ModoAsignacionCantidadCasosAtendidos : TipoDeAsignacion
    {
        public bool AsignarMovilALlamada(Llamada unaLlamada, SistemaLogica sistema)
        {
            List<Movil> listaMovilesLibres = sistema.repositorio.ListaDeMovilesLibres();
            if (listaMovilesLibres.Count > 0)
            {
                List<Movil> listaMovilesQueMenosAtendieron = MovilesQueMenosAtendieronEn12Horas(listaMovilesLibres, sistema);

                if (HayMovilesLibres(listaMovilesQueMenosAtendieron))
                {
                    Movil movilLibreMasCercanoALlamada = MovilMasCercano(listaMovilesQueMenosAtendieron, unaLlamada.Ubicacion);
                    unaLlamada.MovilAsignado = movilLibreMasCercanoALlamada;
                    unaLlamada.EstaAsignadaAUnMovil = true;
                    unaLlamada.FechaYHoraDeAsignacion = DateTime.Now;
                    sistema.repositorio.CambiarDeEstadoLibreONo(movilLibreMasCercanoALlamada);
                    unaLlamada.ModoQueFueAsignada = "Modo por cantidad casos atendidos";
                    sistema.repositorio.VolverCantidadDeCasosAtendidosACero(listaMovilesLibres);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public List<Movil> MovilesQueMenosAtendieronEn12Horas(List<Movil> listaMovilesLibres, SistemaLogica unSistema)
        {
            List<Llamada> llamadasResueltas = unSistema.repositorio.LlamadasResueltas();
            List<Llamada> llamadasUltimas12Horas = LlamadasUltimas12Horas(llamadasResueltas);
            List<Movil> movilesConCantidadDeCasos = CargarMovilesConNumeroDeCasos(llamadasResueltas, listaMovilesLibres);
            return MovilesQueMenosAtendieron(movilesConCantidadDeCasos, unSistema);
        }

        private List<Movil> MovilesQueMenosAtendieron(List<Movil> movilesConCantidadDeCasos, SistemaLogica sistema)
        {
            List<Movil> MovilQueMenosAtendieron = new List<Movil>();
            if(movilesConCantidadDeCasos.Count == 0) {
                movilesConCantidadDeCasos = sistema.repositorio.RetornarListaDeMoviles();
            }
            int menorCantidadAtendida = 0;
            if (movilesConCantidadDeCasos.Count > 0)
            {
                menorCantidadAtendida = movilesConCantidadDeCasos.ElementAt(0).CantidadCasosAtendidosUltimas12Horas;
            }
            for (int posicionLista = 0; posicionLista < movilesConCantidadDeCasos.Count; posicionLista++)
            {
                Movil unMovil = movilesConCantidadDeCasos.ElementAt(posicionLista);
                if (unMovil.CantidadCasosAtendidosUltimas12Horas < menorCantidadAtendida)
                {
                    MovilQueMenosAtendieron = new List<Movil>();
                    MovilQueMenosAtendieron.Add(unMovil);
                    menorCantidadAtendida = unMovil.CantidadCasosAtendidosUltimas12Horas;
                }
                else if (unMovil.CantidadCasosAtendidosUltimas12Horas == menorCantidadAtendida)
                {
                    MovilQueMenosAtendieron.Add(unMovil);
                }
            }
            return MovilQueMenosAtendieron;
        }

        private List<Movil> sumarUnCasoAMovil(Movil unMovil, List<Movil> movilesLibres)
        {
            List<Movil> listaMovilesSumandoUno = movilesLibres;
            for (int posicionLista = 0; posicionLista < movilesLibres.Count; posicionLista++)
            {
                if (movilesLibres.ElementAt(posicionLista).Equals(unMovil))
                {
                    movilesLibres.ElementAt(posicionLista).CantidadCasosAtendidosUltimas12Horas++;
                    listaMovilesSumandoUno = movilesLibres;
                }
            }
            return listaMovilesSumandoUno;
        }

        private List<Movil> CargarMovilesConNumeroDeCasos(List<Llamada> llamadasResueltas, List<Movil> listaMovilesLibres)
        {
            List<Movil> listaMovilesConCantidadDeCasos = new List<Movil>();
            for (int posicionLista = 0; posicionLista < llamadasResueltas.Count; posicionLista++)
            {
                listaMovilesConCantidadDeCasos = sumarUnCasoAMovil(llamadasResueltas.ElementAt(posicionLista).MovilAsignado, listaMovilesLibres);
                listaMovilesLibres = sumarUnCasoAMovil(llamadasResueltas.ElementAt(posicionLista).MovilAsignado, listaMovilesConCantidadDeCasos);
            }
            return listaMovilesConCantidadDeCasos;
        }

        private List<Llamada> LlamadasUltimas12Horas(List<Llamada> llamadasResueltas)
        {
            List<Llamada> llamadasUltimas12Horas = new List<Llamada>();
            DateTime menosDoceHoras = DateTime.Now;
            menosDoceHoras.AddHours(-12);
            for (int posicionLista  = 0; posicionLista < llamadasResueltas.Count; posicionLista++)
            {
                Llamada llamadaResuelta = llamadasResueltas.ElementAt(posicionLista);
                DateTime fechaYHoraDeAsignacion = llamadasResueltas.ElementAt(posicionLista).FechaYHoraDeAsignacion;
                DateTime fechaYHoraDeResolucion = llamadasResueltas.ElementAt(posicionLista).FechaYHoraDeResolucion;
                if (fechaYHoraDeAsignacion >= menosDoceHoras || fechaYHoraDeResolucion >= menosDoceHoras)
                {
                    llamadasUltimas12Horas.Add(llamadaResuelta);
                }
            }
            return llamadasUltimas12Horas;
        }


        private bool HayMovilesLibres(List<Movil> listaMoviles)
        {
            if (listaMoviles.Count > 0)
            {
                return true;
            }
            return false;
        }

        private Movil MovilMasCercano(List<Movil> listaMovilesLibres, PointF ubicacionLlamada)
        {
            Movil movilMasCercano = listaMovilesLibres.ElementAt(0);
            double menorDistancia = DistanciaEntreDosUbicaciones(movilMasCercano.Ubicacion, ubicacionLlamada);

            for (int posicionLista = 0; posicionLista < listaMovilesLibres.Count; posicionLista++)
            {
                double distanciaEntreMoviles = DistanciaEntreDosUbicaciones(listaMovilesLibres.ElementAt(posicionLista).Ubicacion, ubicacionLlamada);

                if (distanciaEntreMoviles < menorDistancia)
                {
                    movilMasCercano = listaMovilesLibres.ElementAt(posicionLista);
                    menorDistancia = distanciaEntreMoviles;
                }
            }
            return movilMasCercano;
        }

        private double DistanciaEntreDosUbicaciones(PointF ubicacionMovil, PointF ubicacionLlamada)
        {
            double restaDeLatitudes = (double)(ubicacionLlamada.X - ubicacionMovil.X);
            double restaDeLongitudes = (double)(ubicacionLlamada.Y - ubicacionMovil.Y);

            return Math.Sqrt(restaDeLatitudes * restaDeLatitudes + restaDeLongitudes * restaDeLongitudes);
        }


        public void AsignarLlamadasEnEspera(SistemaLogica sistema)
        {
            List<Llamada> listaLlamadasEnEspera = sistema.repositorio.LlamadasEnEspera();
            List<Llamada> listaLlamadasMasUrgentes = CargarListaLlamadaConMayorUrgencia(listaLlamadasEnEspera);
            if (listaLlamadasMasUrgentes.Count > 0)
            {
                Llamada llamadaUrgenteMasVieja = CargarLlamadaMasVieja(listaLlamadasMasUrgentes);
                AsignarMovilALlamada(llamadaUrgenteMasVieja, sistema);
            }
        }

        private List<Llamada> CargarListaLlamadaConMayorUrgencia(List<Llamada> listaLlamadasEnEspera)
        {
            List<Llamada> listaLlamadasConMayorUrgencia = new List<Llamada>();

            if (ListaTieneLlamadaConUrgencia(Llamada.Urgencia.Alta, listaLlamadasEnEspera))
            {
                listaLlamadasConMayorUrgencia = AgregarLlamadasALista(Llamada.Urgencia.Alta, listaLlamadasEnEspera);
            }

            else if (ListaTieneLlamadaConUrgencia(Llamada.Urgencia.Media, listaLlamadasEnEspera))
            {
                listaLlamadasConMayorUrgencia = AgregarLlamadasALista(Llamada.Urgencia.Media, listaLlamadasEnEspera);
            }

            else
            {
                listaLlamadasConMayorUrgencia = AgregarLlamadasALista(Llamada.Urgencia.Baja, listaLlamadasEnEspera);
            }
            return listaLlamadasConMayorUrgencia;
        }

        private bool ListaTieneLlamadaConUrgencia(Llamada.Urgencia nivelUrgencia, List<Llamada> listaLlamadas)
        {
            for (int posicionLista = 0; posicionLista < listaLlamadas.Count; posicionLista++)
            {
                if (listaLlamadas.ElementAt(posicionLista).NivelUrgencia == nivelUrgencia)
                {
                    return true;
                }
            }
            return false;
        }

        private Llamada CargarLlamadaMasVieja(List<Llamada> listaLlamadaMayorUrgencia)
        {
            Llamada llamadaMasVieja = listaLlamadaMayorUrgencia.ElementAt(0);

            DateTime fechaMenorDeLlamada = listaLlamadaMayorUrgencia.ElementAt(0).FechaYHoraDeAsignacion;

            for (int llamadaMayor = 0; llamadaMayor < listaLlamadaMayorUrgencia.Count; llamadaMayor++)
            {
                int diferencia = DateTime.Compare(listaLlamadaMayorUrgencia.ElementAt(llamadaMayor).FechaYHoraDeAsignacion, fechaMenorDeLlamada);

                if (diferencia < 0)
                {
                    llamadaMasVieja = listaLlamadaMayorUrgencia.ElementAt(llamadaMayor);
                    fechaMenorDeLlamada = llamadaMasVieja.FechaYHoraDeAsignacion;
                }

            }
            return llamadaMasVieja;
        }

        private List<Llamada> AgregarLlamadasALista(Llamada.Urgencia nivelUrgencia, List<Llamada> listaLlamadasEnEspera)
        {
            List<Llamada> listaLlamadasConMayorUrgencia = new List<Llamada>();
            for (int posicionLista = 0; posicionLista < listaLlamadasEnEspera.Count; posicionLista++)
            {
                if (listaLlamadasEnEspera.ElementAt(posicionLista).NivelUrgencia == nivelUrgencia)
                {
                    listaLlamadasConMayorUrgencia.Add(listaLlamadasEnEspera.ElementAt(posicionLista));
                }
            }

            return listaLlamadasConMayorUrgencia;
        }

        public override string ToString()
        {
            return "Modo por cantidad casos atendidos";
        }
    }
}
   
