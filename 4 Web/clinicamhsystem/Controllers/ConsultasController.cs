using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class ConsultasController : Controller
    {
        // GET: ConsultasController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ConsultasController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ConsultasController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ConsultasController/Create
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

        // GET: ConsultasController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ConsultasController/Edit/5
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

        // GET: ConsultasController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ConsultasController/Delete/5
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
