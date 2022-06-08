using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCategoria
    {
        Task Crear(Categoria categoria);
    }
    public class RepositorioCategoria:IRepositorioCategoria
    {
        private readonly string connectionString;
        public RepositorioCategoria(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria) 
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Categoria(Nombre, TipoOperacionId, UsuarioId)
                                                                Values (@Nombre,@TipoOperacionId, @UsuarioId);
                                                                SELECT SCOPE_IDENTITY()", categoria);

            categoria.Id = id;  
        } 
    }
}
