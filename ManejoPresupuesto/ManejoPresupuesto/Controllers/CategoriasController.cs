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
        public async Task<IActionResult> Index()
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categorias = await repositorioCategoria.Obtener(usuarioId);
            return View(categorias);

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
        public async Task<IActionResult> Editar(int id) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategoria.ObtenerPorId(id, usuarioId);

            if (categoria is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar) 
        {
            if (!ModelState.IsValid) 
            {
                return View(categoriaEditar);
            }

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategoria.ObtenerPorId(categoriaEditar.Id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoriaEditar.UsuarioId = usuarioId;

            await repositorioCategoria.Actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategoria.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategoria.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
       

            await repositorioCategoria.Borrar(id);
            return RedirectToAction("Index");
        }
    }
}
