using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServiciosUsuarios serviceUsuario;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategoria repositorioCategoria;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;

        public TransaccionesController(IServiciosUsuarios serviciosUsuarios, 
            IRepositorioCuentas repositorioCuentas,
            IRepositorioCategoria repositorioCategoria,
            IRepositorioTransacciones repositorioTransacciones,
            IMapper mapper)
        {
                this.serviceUsuario = serviciosUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategoria = repositorioCategoria;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Crear() 
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();
            var modelo = new TransaccionCrecionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }
        public IActionResult Index() 
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCrecionViewModel model) 
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();
            if (!ModelState.IsValid) 
            {
                model.Cuentas = await ObtenerCuentas(usuarioId);
                model.Categorias = await ObtenerCategorias(usuarioId, model.TipoOperacionId);
                return View(model);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(model.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await repositorioCategoria.ObtenerPorId(model.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            model.UsuarioId = usuarioId;
            if (model.TipoOperacionId == TipoOperacion.Gasto) 
            {
                model.Monto *= -1;
            }
            await repositorioTransacciones.Crear(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        { 
            var usuarioId = serviceUsuario.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<TransaccionesActualizacionViewModel>(transaccion);
            modelo.MontoAnterior = modelo.Monto;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }
            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionesActualizacionViewModel modelo)
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();

            if (!ModelState.IsValid) 
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if(cuenta is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategoria.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");

            }
            var transaccion = mapper.Map<Transaccion>(modelo);

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }
            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Borrar(int id) 
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();

            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTransacciones.Borrar(id);
            return RedirectToAction("Index");

        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId) 
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categoria = await repositorioCategoria.Obtener(usuarioId, tipoOperacion);
            return categoria.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));

        }
        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }
    }
}
