using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio;

namespace Logica
{
    public class ModoAsignacionPorDefecto : TipoDeAsignacion
    {

        public bool AsignarMovilALlamada(Llamada unaLlamada, SistemaLogica sistema)
        {
            List<Movil> listaMovilesLibres = sistema.repositorio.ListaDeMovilesLibres();

            if (HayMovilesLibres(listaMovilesLibres))
            {
                Movil movilLibreMasCercanoALlamada = MovilMasCercano(listaMovilesLibres, unaLlamada.Ubicacion);
                unaLlamada.MovilAsignado = movilLibreMasCercanoALlamada;
                unaLlamada.EstaAsignadaAUnMovil = true;
                unaLlamada.FechaYHoraDeAsignacion = DateTime.Now;
                unaLlamada.ModoQueFueAsignada = "Modo por defecto";
                sistema.repositorio.CambiarDeEstadoLibreONo(movilLibreMasCercanoALlamada);
                return true;
            }
            else
            {
                return false;
            }
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
            return "Modo por defecto";
        }
    }


}