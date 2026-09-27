using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace clinicaWeb.Controllers;
[SecurityFilter("Servicios")]
public class ServiciosController(IServiciosServices serviciosServices) : ErrorHandlingController
{


    // GET: ServiciosController
    public ActionResult Index()
    {
        var servicios = serviciosServices.GetAll();
        return View(new ServiciosViewModel { Servicios = servicios});
    }

    // GET: ServiciosController/Details/5
    public ActionResult Details(int id)
    {
        return View();
    }

    // GET: ServiciosController/Create
    [HttpPost]
    public ActionResult Create(MotivoCobro servicio)
    {
        try
        {
            serviciosServices.AddServicio(servicio);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return RedirectToAction("Index");
    }

    // POST: ServiciosController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult CreateP(IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            return View("Error");
        }
    }

    // GET: ServiciosController/Edit/5
    public ActionResult Edit(int id)
    {
        return View();
    }

    // POST: ServiciosController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            return View("Error");
        }
    }

    // POST: ServiciosControler/Delete/5
    [HttpPost]
    public ActionResult Eliminar(Guid id)
    {
        try
        {
            serviciosServices.DeleteServicio(id);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Actualizar(Guid id, string descripcion, decimal? precioSugerido)
    {
        try
        {
            serviciosServices.UpdateServicio(id, descripcion, precioSugerido ?? 0);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return RedirectToAction("Index");
    }
}
