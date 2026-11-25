using System;
using System.Collections.Generic;

namespace SimuladorEnrutamientoGPS
{
    public class SolucionadorRutas
    {
        public List<Nodo>? RutaEncontrada { get; private set; }

        public bool BuscarRutaBFS(GrafoMapa mapa, Nodo inicio, Nodo meta)
        {
            if (mapa == null || inicio == null || meta == null)
            {
                throw new ArgumentNullException("El mapa, el nodo de inicio y la meta no pueden ser nulos.");
            }

            RutaEncontrada = new List<Nodo>();

            // Validar que el inicio y la meta sean transitables.
            if (inicio.EsPared || meta.EsPared)
            {
                return false;
            }

            // ESTRUCTURAS APLICADAS SEGÚN DISEÑO TEÓRICO:
            // 1. Cola (FIFO) para exploración por niveles (ruta más corta).
            // 2. Tablas Hash (HashSet y Dictionary) para consultas O(1) de visitados y trazabilidad.
            Queue<Nodo> colaFrontera = new Queue<Nodo>();
            HashSet<Nodo> visitados = new HashSet<Nodo>();
            Dictionary<Nodo, Nodo?> tablaPadres = new Dictionary<Nodo, Nodo?>();

            colaFrontera.Enqueue(inicio);
            visitados.Add(inicio);
            tablaPadres[inicio] = null;

            while (colaFrontera.Count > 0)
            {
                Nodo actual = colaFrontera.Dequeue();

                // Si llegamos a la meta, reconstruimos el camino.
                if (ReferenceEquals(actual, meta))
                {
                    RutaEncontrada = ReconstruirCamino(tablaPadres, meta);
                    return true;
                }

                List<Nodo> vecinos = ObtenerVecinosAccesibles(mapa, actual);

                foreach (Nodo vecino in vecinos)
                {
                    if (!vecino.EsPared && !visitados.Contains(vecino))
                    {
                        visitados.Add(vecino);
                        tablaPadres[vecino] = actual;
                        colaFrontera.Enqueue(vecino);
                    }
                }
            }

            // No se encontró una ruta.
            RutaEncontrada.Clear();
            return false;
        }

        private List<Nodo> ObtenerVecinosAccesibles(GrafoMapa mapa, Nodo nodo)
        {
            List<Nodo> vecinos = new List<Nodo>();

            int x = nodo.X;
            int y = nodo.Y;

            if (x - 1 >= 0 && !mapa.MatrizNodos[x - 1, y].EsPared)
            {
                vecinos.Add(mapa.MatrizNodos[x - 1, y]);
            }

            if (x + 1 < mapa.Filas && !mapa.MatrizNodos[x + 1, y].EsPared)
            {
                vecinos.Add(mapa.MatrizNodos[x + 1, y]);
            }

            if (y - 1 >= 0 && !mapa.MatrizNodos[x, y - 1].EsPared)
            {
                vecinos.Add(mapa.MatrizNodos[x, y - 1]);
            }

            if (y + 1 < mapa.Columnas && !mapa.MatrizNodos[x, y + 1].EsPared)
            {
                vecinos.Add(mapa.MatrizNodos[x, y + 1]);
            }

            return vecinos;
        }

        private List<Nodo> ReconstruirCamino(Dictionary<Nodo, Nodo?> padres, Nodo meta)
        {
            List<Nodo> camino = new List<Nodo>();
            Nodo? actual = meta;

            while (actual != null)
            {
                camino.Add(actual);
                actual = padres[actual];
            }

            camino.Reverse();
            return camino;
        }
    }
}