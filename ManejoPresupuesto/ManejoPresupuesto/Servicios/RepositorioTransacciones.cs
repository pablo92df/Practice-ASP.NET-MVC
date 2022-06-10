using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo) 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id, t.Monto, t.FechaTransaccion, C.Nombre as Categoria
                                                FROM Transacciones t 
                                                INNER JOIN Categoria c 
                                                on c.Id = t.CategoriaId 
                                                INNER JOIN Cuentas cu 
                                                ON cu.Id = t.CuentaId 
                                                WHERE t.CuentaId = @ CuentaId AND t.UsuarioId = @UsuarioId 
                                                AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", modelo);
        } 
        public async Task Crear(Transaccion transaccion) 
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
                                                        new { 
                                                                transaccion.UsuarioId,
                                                                transaccion.FechaTransaccion,
                                                                transaccion.Monto,
                                                                transaccion.CategoriaId,
                                                                transaccion.CuentaId,
                                                                transaccion.Nota
                                                            },
                                                       commandType: System.Data.CommandType.StoredProcedure);
            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal MontoAnterior, int CuentaAnteriorId)
        {

            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    MontoAnterior,
                    CuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        { 
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT Transacciones.*, cat.TipoOperacionId
                                                                            FROM Transacciones 
                                                                            INNER JOIN Categorias cat 
                                                                            ON cat.Id = Transacciones.CategoriaId 
                                                                            WHERE Transacciones.Id = @Id 
                                                                            AND Transacciones.UsuarioId = @UsuarioId", new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Transacciones_Borrar", new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
