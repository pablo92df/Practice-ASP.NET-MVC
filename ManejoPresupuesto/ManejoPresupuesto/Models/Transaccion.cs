using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Display(Name = "Fecha Transaccion")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        //public DateTime FechaTransaccion { get; set; } = DateTime.Parse(DateTime.Now.ToString("g"));
        // usando la g es la manera resumida de lo que esta comentado abajo, que tambien tiene la hora
        // public DateTime FechaTransaccion { get; set; } = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd hh:MM tt"));
        public decimal Monto { get; set; }
        [Display(Name = "Categoria")]

        [Range(1, maximum: int.MaxValue, ErrorMessage ="Debe seleccionar una categoria")]
        public int CategoriaId { get; set; }
        [StringLength(maximumLength:1000, ErrorMessage ="La nota no puede ser mayo a {1} catacteres")]
        public string Nota { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una Cuenta")]
        [Display(Name = "Cuenta")]
        public int CuentaId { get; set; }

        [Display(Name = "Tipo Operacion")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;

        public string Cuenta  { get; set; }

        public string Categoria { get; set; }
    }
}
