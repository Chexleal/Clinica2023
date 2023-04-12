using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class CitasController : Controller
    {
        // GET: CitasController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CitasController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CitasController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CitasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: CitasController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CitasController/Edit/5
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

        // GET: CitasController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CitasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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
    }
}
