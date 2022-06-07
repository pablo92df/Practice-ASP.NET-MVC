using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    //inyeccion de dependencia
    //nuestras clases deben depender de abstracciones y no de tipos concretos
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string Nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int uduarioId);
        Task<TipoCuenta> ObtenerPorId(int Id, int UsuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }
    public class RepositorioTipoCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTipoCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta) 
        {//SELECT SCOPE_IDENTITY me trae el ID del registro recien creado
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar",
                                                            new { UsuarioId = tipoCuenta.UsuarioId,
                                                                Nombre = tipoCuenta.Nombre},
                                                                commandType: System.Data.CommandType.StoredProcedure);
            //uso un storeprocedure para que agrege el numero de orden
            //DAPPER relacoina los nombres del objeto en este caso tipoCuenta con los values de la consulta
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string Nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            //si no existe devuelve 0 porque en el firstOrDefaut puse int en <int> entonces devuelve el valor por defecto
            //si existe devuelve 1
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1
                                                                          FROM TiposCuentas
                                                                          WHERE Nombre = @Nombre AND UsuarioId= @UsuarioId;",
                                                                          new { Nombre, usuarioId }) ;
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int UsuarioId) 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden 
                                                            FROM TiposCuentas 
                                                            WHERE UsuarioId = @UsuarioId
                                                            ORDER BY Orden", new { UsuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            //EXECUTe ejecuta un query que no retorna nada
            await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre WHERE Id= @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int Id, int UsuarioId) 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden 
                                                                            FROM TiposCuentas
                                                                            WHERE Id = @Id AND UsuarioId = @UsuarioId",
                                                                            new {Id, UsuarioId });
        }

        public async Task Borrar(int id) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE TiposCuentas WHERE Id= @Id", new {id});
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados) 
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden Where Id=@Id;";
            using var connection = new SqlConnection(connectionString);
            //se ejecuta por cada tipo cuenta que reciba, si vienen 5 se ejecuta 5 veces y asi
            await connection.ExecuteAsync(query, tipoCuentasOrdenados);
        }
    }
}
