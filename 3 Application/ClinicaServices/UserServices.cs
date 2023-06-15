using ClinicaDomain;
using System;
using System.Net.Mail;
using System.Net;
using PaaS.Framework.Utils.Extensions;
using ServiceStack;
using ServiceStack.Html;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace ClinicaServices;

public interface IUserServices
{
    Usuario Authenticate(string user, string password);

    bool CheckUserExist(string userName);
    string? CheckAnswer(string userName);
    string? SecurityQuestion(string userName);
    List<Usuario> GetAll();
    void AddUser(Usuario user, List<string> permissionsList);
    Usuario? GetUser(Guid id);
    Usuario? GetUserByName(string userName);
    bool CheckEmails(string email, string emailConfirmed, string username);
    bool CheckNewPassword(string newPassword, string newPasswordConfirmed, Guid idUsuario);
    void DeleteUser(Guid id);
    void UpdateUser(Usuario user, List<string> permissionsListEdit);
    void SetActive(Guid id, bool state);
    void ChangePassword(Guid id, string Password);

    List<RolDetalle> GetPermissions(Guid idUser);
}
public class UserServices : IUserServices
{
    private readonly ClinicaContext _dbContext;
    public UserServices(ClinicaContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddUser(Usuario user, List<string> permissionsList)
    {
        user.IdUsuario = $"{user.NombreUsuario.Trim().ToLower()}".ToGuid();
        user.EstadoEliminado = false;
        user.UsuarioActivo = true;
        user.BeforeChanges();
        _dbContext.Usuarios.Add(user);
        _dbContext.SaveChanges();

        foreach (var permission in permissionsList)
        {
            RolDetalle rolDetalle = new()
            {
                IdRolDetalle = Guid.NewGuid(),
                UsuarioId = user.IdUsuario,
                Permiso = permission,
                Descripcion = permission
            };
            _dbContext.RolDetalles.Add(rolDetalle);
        }
        _dbContext.SaveChanges();
    }

    public Usuario Authenticate(string user, string password)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.Password == password && x.NombreUsuario == user);
        return userItem;
    }

    public List<Usuario> GetAll()
    {
        List<Usuario> result = _dbContext.Usuarios.Where(x =>
        x.EstadoEliminado.Equals(false)).ToList();

        return result;
        //return _dbContext.Usuarios.ToList();
    }

    public bool RecoverAccount(string userEmail, string userName,string respuestaSeg)
    {
        string secPass = "pwqyxudymqccltdr";
        string fromAddress = "traumah_recovery@outlook.com";
        string fromPassword = "thrtohajbvykrmvy";
        string toAddress = userEmail;
        string domain = "clinicamh.azurewebsites.net"; //localhost:7070
        string subject = "Password reset for your account";
        string body = "<html>" +
                 "<body>" +
                 "<h1>Cambio de contraseña</h1>" +
                 "<p>Estimado usuario,</p>" +
                 "<p>Hemos recibido una solicitud para restablecer la contraseña de su cuenta.</p>" +
                 "<p>Para continuar con el proceso de restablecimiento de contraseña, haga clic en el siguiente enlace:</p>" +
                 $"<a href=\"https://{domain}/Home/CheckAnswer?answer={respuestaSeg}&hiddenUsername={userName}\">Cambiar Contraseña</a>" +
                 "<p>Si no ha solicitado el restablecimiento de contraseña, puede ignorar este correo electrónico.</p>" +
                 "<p>Atentamente,</p>" +
                 "<p>El equipo de Traumah</p>" +
                 "</body>" +
               "</html>";

        //clinicamh.azurewebsites.net
        // Configuración del cliente SMTP
        var smtpClient = new SmtpClient("smtp-mail.outlook.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromAddress, fromPassword),
            EnableSsl = true
        };

        // Creación del correo electrónico
        var email = new MailMessage
        {
            From = new MailAddress(fromAddress),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        email.To.Add(toAddress);

        // Envío del correo electrónico
        try
        {
            smtpClient.Send(email);
            Console.WriteLine("Correo electrónico enviado exitosamente.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al enviar correo electrónico: " + ex.Message);
            return false;
        }
    }

    public bool CheckUserExist(string userName)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == userName);
        if (userItem == null) return false;
        return true;
    }

    public string? SecurityQuestion(string userName)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario.ToLower().Trim() == userName.ToLower().Trim());
        if (userItem == null)
        {

            return null;
        }
        else
        {
            var preguntaSegura = userItem.PreguntaSeg.ToString();
            return preguntaSegura;
        }
    }

    public string? CheckAnswer(string answer)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.RespuestaSeg.ToLower().Trim() == answer.ToLower().Trim());
        if (userItem != null) return userItem.RespuestaSeg.ToString();
        return null;
    }

    public bool CheckEmails(string email, string emailConfirmed, string username)
    {
        var emailOnData = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == username && x.Correo == email);
        if(emailOnData != null) {
            var userName = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == emailOnData.NombreUsuario);
            var answer = userName.RespuestaSeg.ToString();

            if (emailOnData != null)
            {
                bool checkedEmail = email.Equals(emailConfirmed);
                if (checkedEmail)
                {
                    RecoverAccount(email, userName.NombreUsuario.ToString(), answer);
                    return true;
                }
                else { return false; }
            }
            else { return false; }
            }
        else
        {
            return false;
        }

    }

    public bool UpdateNewPassword(string newPassword, string userName)
    {
        var user = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == userName);
        if (user != null)
        {
            return true;
        }
        else { return false; }

    }

    public bool CheckNewPassword(string newPassword, string newPasswordConfirmed, Guid idUsuario)
    {
        bool checkedPassword = newPassword.Equals(newPasswordConfirmed);
        var user = _dbContext.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
        
        if (checkedPassword && user != null)
        {
            ChangePassword(user.IdUsuario, newPassword);
            return true;
        }
        else { return false; }
    }

    public Usuario? GetUser(Guid id)
    {
        return _dbContext.Usuarios.FirstOrDefault(p => p.IdUsuario == id);
    }

    public Usuario GetUserByName(string userName)
    {
        return _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario.ToLower().Trim() == userName.ToLower().Trim());
    }

    public void UpdateUser(Usuario user, List<string> permissionsListEdit)
    {
        var userDB = GetUser(user.IdUsuario);
        if (userDB is not null)
        {
            userDB.NombreUsuario = user.NombreUsuario;
            userDB.PreguntaSeg = user.PreguntaSeg;
            userDB.RespuestaSeg = user.RespuestaSeg;
            userDB.Dpi = user.Dpi;
            userDB.Nombre = user.Nombre;
            userDB.Apellido = user.Apellido;
            userDB.FechaNacimiento = user.FechaNacimiento;
            userDB.Telefono = user.Telefono;
            userDB.Correo = user.Correo;
            userDB.EstadoCivil = user.EstadoCivil;
            userDB.Profesion = user.Profesion;
            userDB.Nacionalidad = user.Nacionalidad;
            userDB.Password = user.Password;
            userDB.Antecedentes = user.Antecedentes;
            userDB.TipoSange = user.TipoSange;

            userDB.BeforeChanges();
            var permisionsOld =  _dbContext.RolDetalles.Where(x => x.UsuarioId == userDB.IdUsuario).ToList();
            _dbContext.RolDetalles.RemoveRange(permisionsOld);
            foreach (var permission in permissionsListEdit)
            {
                RolDetalle rolDetalle = new()
                {
                    IdRolDetalle = Guid.NewGuid(),
                    UsuarioId = user.IdUsuario,
                    Permiso = permission,
                    Descripcion = permission
                };
                _dbContext.RolDetalles.Add(rolDetalle);
            }
            _dbContext.SaveChanges();
        }
    }

    public void SetActive(Guid id, bool state)
    {
        var userDB = GetUser(id);
        if (userDB is not null)
        {
            userDB.UsuarioActivo = state;
        }
        _dbContext.SaveChanges();
    }

    public void DeleteUser(Guid id)
    {
        var user = GetUser(id);
        if (user is not null)
        {
            user.EstadoEliminado = true;
            _dbContext.SaveChanges();
        }
    }

    public List<RolDetalle> GetPermissions(Guid idUser)
    {
        var permissions = _dbContext.RolDetalles.Where(x => x.UsuarioId == idUser).ToList();
        return permissions;
    }

    public void ChangePassword(Guid id, string Password)
    {
        var usuario = GetUser(id);

        if (usuario is not null)
        {
            usuario.Password = Password;
            usuario.BeforeChanges();
            _dbContext.SaveChanges();
        }
    }
}
