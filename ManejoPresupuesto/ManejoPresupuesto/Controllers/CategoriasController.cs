using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriasController:Controller
    {
        private readonly IRepositorioCategoria repositorioCategoria;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public CategoriasController(IRepositorioCategoria repositorioCategoria,IServiciosUsuarios serviciosUsuarios)
        {
            this.repositorioCategoria = repositorioCategoria;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpGet]
        public IActionResult Crear()
        { 
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria) 
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }
                var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            categoria.UsuarioId = usuarioId;
            await repositorioCategoria.Crear(categoria);
            return RedirectToAction("Index");
        }
    }
}
