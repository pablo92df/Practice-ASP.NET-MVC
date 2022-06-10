namespace ManejoPresupuesto.Models
{
    public class ReporteTransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public class TransaccionesPorFecha 
        {
            public DateTime FechaTransaccion { get; set; }
            public IEnumerable<Transaccion> Transacciones { get; set; }

            public decimal BalanceDeposito => Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Sum(x=>x.Monto);
        }
    }
}
