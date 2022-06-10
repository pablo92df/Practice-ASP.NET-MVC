namespace ManejoPresupuesto.Models
{
    public class TransaccionesActualizacionViewModel:TransaccionCrecionViewModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }
    }
}
