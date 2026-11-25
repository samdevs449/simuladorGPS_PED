using SimuladorEnrutamientoGPS;

// Banco de pruebas de Persona 2 (QA). Ejecuta casos sobre el código real del proyecto.
// Los resultados se calculan durante la ejecución; no están escritos de antemano.

var resultados = new List<(string Id, string Nombre, bool Paso, string Detalle)>();

Ejecutar("TC-01", "Ruta directa disponible", () =>
{
    var mapa = CrearMapa(5, 5, (1, 1), (1, 2), (1, 3));
    var inicio = mapa.MatrizNodos[1, 1];
    var meta = mapa.MatrizNodos[1, 3];
    var solver = new SolucionadorRutas();
    bool ok = solver.BuscarRutaBFS(mapa, inicio, meta);
    bool paso = ok && solver.RutaEncontrada is { Count: 3 };
    return (paso, $"Esperado: ruta de 3 nodos. Obtenido: encontrado={ok}, nodos={solver.RutaEncontrada?.Count ?? 0}.");
});

Ejecutar("TC-02", "Meta inaccesible", () =>
{
    var mapa = CrearMapa(5, 5, (1, 1), (1, 2), (2, 1)); // (3,3) queda bloqueada
    var solver = new SolucionadorRutas();
    bool ok = solver.BuscarRutaBFS(mapa, mapa.MatrizNodos[1, 1], mapa.MatrizNodos[3, 3]);
    bool paso = !ok && solver.RutaEncontrada is { Count: 0 };
    return (paso, $"Esperado: sin ruta y lista vacía. Obtenido: encontrado={ok}, nodos={solver.RutaEncontrada?.Count ?? 0}.");
});

Ejecutar("TC-03", "Inicio igual a meta", () =>
{
    var mapa = CrearMapa(5, 5, (1, 1));
    var nodo = mapa.MatrizNodos[1, 1];
    var solver = new SolucionadorRutas();
    bool ok = solver.BuscarRutaBFS(mapa, nodo, nodo);
    bool paso = ok && solver.RutaEncontrada is { Count: 1 } && ReferenceEquals(solver.RutaEncontrada[0], nodo);
    return (paso, $"Esperado: ruta de un solo nodo. Obtenido: encontrado={ok}, nodos={solver.RutaEncontrada?.Count ?? 0}.");
});


Ejecutar("TC-04", "Dos búsquedas consecutivas", () =>
{
    var mapa = CrearMapa(5, 5, (1, 1), (1, 2), (1, 3), (2, 3), (3, 3));

    var inicio = mapa.MatrizNodos[1, 1];
    var meta = mapa.MatrizNodos[3, 3];
    var solver = new SolucionadorRutas();

    bool primera = solver.BuscarRutaBFS(mapa, inicio, meta);
    int nodosPrimera = solver.RutaEncontrada?.Count ?? 0;

    bool segunda = solver.BuscarRutaBFS(mapa, inicio, meta);
    int nodosSegunda = solver.RutaEncontrada?.Count ?? 0;

    bool paso = primera && segunda && nodosPrimera > 0 && nodosSegunda > 0;

    return (
        paso,
        $"Primera={primera} ({nodosPrimera} nodos); Segunda={segunda} ({nodosSegunda} nodos)."
    );
});


Ejecutar("TC-05", "Integridad de la ruta", () =>
{
    var mapa = CrearMapa(5, 5, (1, 1), (1, 2), (1, 3), (2, 3), (3, 3));
    var inicio = mapa.MatrizNodos[1, 1];
    var meta = mapa.MatrizNodos[3, 3];
    var solver = new SolucionadorRutas();
    bool ok = solver.BuscarRutaBFS(mapa, inicio, meta);
    var ruta = solver.RutaEncontrada;
    if (!ok || ruta is null || ruta.Count == 0)
        return (false, "No se encontró una ruta para validar.");
    if (!ReferenceEquals(ruta[0], inicio) || !ReferenceEquals(ruta[^1], meta))
        return (false, "El inicio o la meta no coinciden con los extremos de la ruta.");
    for (int i = 1; i < ruta.Count; i++)
    {
        int distancia = Math.Abs(ruta[i].X - ruta[i - 1].X) + Math.Abs(ruta[i].Y - ruta[i - 1].Y);
        if (distancia != 1 || ruta[i].EsPared)
            return (false, $"Movimiento no válido entre posiciones {i - 1} y {i}.");
    }
    return (true, $"Ruta válida: {ruta.Count} nodos y {ruta.Count - 1} movimientos.");
});
Ejecutar("TC-06", "Validación de dimensiones inválidas en GrafoMapa (QA-02)", () =>
{
    bool falloPequeno = false;
    bool falloPar = false;

    try { _ = new GrafoMapa(3, 3); } catch (ArgumentException) { falloPequeno = true; }
    try { _ = new GrafoMapa(6, 6); } catch (ArgumentException) { falloPar = true; }

    bool paso = falloPequeno && falloPar;
    return (paso, $"Rechazó 3x3={falloPequeno}, Rechazó 6x6={falloPar}.");
});

Ejecutar("TC-07", "Generación reproducible mediante semilla (QA-03)", () =>
{
    var mapaA = new GrafoMapa(9, 9, semilla: 42);
    mapaA.GenerarLaberintoDFS();

    var mapaB = new GrafoMapa(9, 9, semilla: 42);
    mapaB.GenerarLaberintoDFS();

    bool identicos = mapaA.ObtenerMapaTexto() == mapaB.ObtenerMapaTexto();
    return (identicos, $"Mapas con semilla 42 idénticos={identicos}.");
});

Ejecutar("TC-08", "Conectividad garantizada en laberinto DFS generado", () =>
{
    var mapa = new GrafoMapa(11, 11, semilla: 100);
    mapa.GenerarLaberintoDFS();

    var inicio = mapa.MatrizNodos[1, 1];
    var meta = mapa.MatrizNodos[9, 9];
    var solver = new SolucionadorRutas();

    bool ok = solver.BuscarRutaBFS(mapa, inicio, meta);
    bool paso = ok && solver.RutaEncontrada is { Count: > 1 };
    return (paso, $"Ruta encontrada en laberinto 11x11={ok}, pasos={solver.RutaEncontrada?.Count ?? 0}.");
});

Console.WriteLine();
Console.WriteLine("========== RESULTADOS DE QA ==========");
foreach (var r in resultados)
    Console.WriteLine($"{r.Id} | {(r.Paso ? "PASS" : "FAIL")} | {r.Nombre}\n   {r.Detalle}");
Console.WriteLine("--------------------------------------");
Console.WriteLine($"Total: {resultados.Count} | PASS: {resultados.Count(r => r.Paso)} | FAIL: {resultados.Count(r => !r.Paso)}");
Console.WriteLine("Guarda una captura de esta consola como evidencia. No marques como aprobada una prueba que muestre FAIL.");

void Ejecutar(string id, string nombre, Func<(bool Paso, string Detalle)> prueba)
{
    try
    {
        var resultado = prueba();
        resultados.Add((id, nombre, resultado.Paso, resultado.Detalle));
    }
    catch (Exception ex)
    {
        resultados.Add((id, nombre, false, $"Excepción no controlada: {ex.GetType().Name}: {ex.Message}"));
    }
}

GrafoMapa CrearMapa(int filas, int columnas, params (int x, int y)[] transitables)
{
    var mapa = new GrafoMapa(filas, columnas);
    foreach (var (x, y) in transitables)
        mapa.MatrizNodos[x, y].EsPared = false;
    return mapa;
}
