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
public class ServiciosController : Controller
{
    private readonly IServiciosServices _services;

    public ServiciosController(IServiciosServices serviciosServices)
    {
        _services = serviciosServices;
    }


    // GET: ServiciosController
    public ActionResult Index()
    {
        var servicios = _services.GetAll();
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
            _services.AddServicio(servicio);
        }
        catch
        {
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
        catch
        {
            return View();
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
        catch
        {
            return View();
        }
    }

    // POST: ServiciosControler/Delete/5
    [HttpPost]
    public ActionResult Eliminar(Guid id)
    {
        try
        {
            _services.DeleteServicio(id);
        }
        catch { }
        return RedirectToAction("Index");
    }
}
