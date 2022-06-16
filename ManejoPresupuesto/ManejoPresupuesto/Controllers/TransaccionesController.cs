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
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(IServiciosUsuarios serviciosUsuarios, 
            IRepositorioCuentas repositorioCuentas,
            IRepositorioCategoria repositorioCategoria,
            IRepositorioTransacciones repositorioTransacciones,
            IMapper mapper,
            IServicioReportes servicioReportes)
        {
                this.serviceUsuario = serviciosUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategoria = repositorioCategoria;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Crear() 
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();
            var modelo = new TransaccionCrecionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }
        public async Task<IActionResult> Index(int mes, int anio) 
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();

            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, anio, ViewBag);

            return View(modelo);
        }

       
        public IActionResult Mensual()
        {
            return View();
        }
        public async Task<IActionResult> Semanal(int mes, int anio)
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, anio, ViewBag);


            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x =>
                            new ResultadoObtenerPorSemana()
                            {
                                Semana = x.Key,
                                Ingresos = x.Where( x => x.TipoOperacionId == TipoOperacion.Ingreso).Select( x => x.Monto).FirstOrDefault(),
                                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()

                            }).ToList();
            if (anio == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                anio = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(anio, mes, 1);
            var diaDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diaSegmentados = diaDelMes.Chunk(7).ToList();//divido en 7 los dias del mnes

            for (int i = 0; i < diaSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(anio, mes, diaSegmentados[i].First());
                var fechaFin = new DateTime(anio, mes, diaSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);
                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaIFin = fechaFin
                    });
                }
                else 
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaIFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View();
        }
        public IActionResult ExcelReporte()
        {
            return View();
        }
        public IActionResult Calendario()
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
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
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
            modelo.UrlRetorno = urlRetorno;

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
            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");

            }
            else 
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null) 
        {
            var usuarioId = serviceUsuario.ObtenerUsuarioId();

            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTransacciones.Borrar(id);
            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");

            }
            else
            {
                return LocalRedirect(urlRetorno);
            }

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
