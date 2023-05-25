using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace clinicaWeb.Extensions
{
    public enum NotificationType
    {
        Success,
        Errror,
        Info,
        Question,
        Warning
    }

    public class NotificationController : Controller
    {
        public void BasicNotification(string msj, NotificationType type, string tittle = "")
        {
            TempData["notification"] = $"Swal.fire('{tittle}','{msj}','{type.ToString().ToLower()}'";
        }

        public void DefaultNotification()
        {
            TempData["notification"] = $"Swal.fire(\r\n  'The Internet?',\r\n  'That thing is still around?',\r\n  'question'\r\n)";
        }
    }
}
