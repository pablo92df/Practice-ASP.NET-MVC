using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Crear(TipoCuenta tipoCuenta) 
        {
            if (!ModelState.IsValid) 
            {//le reenvio el mismo para rellenarle el formulario con lo que envio

                return View(tipoCuenta);
            }
            return View();
        }
    }
}
