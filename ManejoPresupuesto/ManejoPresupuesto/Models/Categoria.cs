using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength:50,ErrorMessage ="No Puede ser Mayor a {1} caracteres")]
        public string Nombre { get; set; }
        public TipoOperacion TipoOperacionId { get; set; }
        public int UsuarioId { get; set; }
    }
}
