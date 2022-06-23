using System.Security.Claims;

namespace ManejoPresupuesto.Servicios
{
    public interface IServiciosUsuarios
    {
        int ObtenerUsuarioId();
    }
    public class ServiciosUsuarios : IServiciosUsuarios
    {
        private readonly HttpContext httpContex;

        public ServiciosUsuarios(IHttpContextAccessor httpContextAccessor){
            this.httpContex = httpContextAccessor.HttpContext;
        }
        public int ObtenerUsuarioId() 
        {
            if (httpContex.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContex.User.Claims.Where(x=>x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

                var id = int.Parse(idClaim.Value);
                return id;
            }
            else 
            {
                throw new ApplicationException("El usuario nbo esta autenticado");
            }
           
        }
    }
}
