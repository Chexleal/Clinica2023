using ClinicaDomain;
using System;
using System.Net.Mail;
using System.Net;
using PaaS.Framework.Utils.Extensions;
using ServiceStack;
using ServiceStack.Html;

namespace ClinicaServices;

public interface IUserServices
{
    Usuario Authenticate(string user, string password);

    bool CheckUserExist(string userName);
    string? CheckAnswer(string userName);
    string? SecurityQuestion(string userName);
    List<Usuario> GetAll();
    void AddUser(Usuario user, List<string> permissionsList);
    List<Usuario> SearchUser(string input);
    Usuario? GetUser(Guid id);
    Usuario? GetUserByName(string userName);
    bool CheckEmails(string email, string emailConfirmed, string username);
    bool CheckNewPassword(string newPassword, string newPasswordConfirmed, string userName);
    void DeleteUser(Guid id);
    void UpdateUser(Usuario user);
    void SetActive(Guid id, bool state);

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

        foreach(var permission in permissionsList)
        {
            RolDetalle rolDetalle = new()
            {
                IdRolDetalle = Guid.NewGuid(),
                IdUsuario = user.IdUsuario,
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

    public bool RecoverAccount(string userEmail, string userName)
    {
        string secPass = "pwqyxudymqccltdr";
        // Send an email to the user with instructions to reset their password
        string fromAddress = "traumah_recovery@outlook.com";
        string fromPassword = "thrtohajbvykrmvy";
        string toAddress = userEmail;
        string subject = "Password reset for your account";
        string body = "<html>" +
                         "<body>" +
                         "<h1>Hello world!</h1>" +
                         "<p>This is an HTML email.</p>" +
                         $"<a href=\"https://localhost:7070/Home/NuevaClave/?userName={userName}\">link text</a>" +
                         "</body>" +
                       "</html>";


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
        // Check if the email is registered in the database
        // Return true if it exists, false otherwise
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
        var emailOnData = _dbContext.Usuarios.FirstOrDefault(x =>x.NombreUsuario == username && x.Correo == email);
        var userName = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == emailOnData.NombreUsuario);

        if (emailOnData != null)
        {
            bool checkedEmail = email.Equals(emailConfirmed);
            if (checkedEmail)
            {
                RecoverAccount(email, userName.NombreUsuario.ToString());
                return true;
            }
            else { return false; }
        }
        else { return false; }

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

    public bool CheckNewPassword(string newPassword, string newPasswordConfirmed, string userName)
    {
        bool checkedPassword = newPassword.Equals(newPasswordConfirmed);
        var user = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == userName);
        if (checkedPassword && user != null)
        {
            return true;
        }
        else { return false; }
    }

    public Usuario? GetUser(Guid id)
    {
        //return _dbContext.Usuarios.Find(id);
        return _dbContext.Usuarios.FirstOrDefault(p => p.IdUsuario == id);
    }

    public Usuario GetUserByName(string userName)
    {
        return _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario.ToLower().Trim() == userName.ToLower().Trim());
    }

    public void UpdateUser(Usuario user)
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
            userDB.Antecedentes = user.Antecedentes;
            userDB.TipoSange = user.TipoSange;

            userDB.BeforeChanges();
            _dbContext.SaveChanges();
        }
    }

    public void SetActive(Guid id,bool state)
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

    public List<Usuario> SearchUser(string input)
    {

        if (input.IsNullOrEmpty()) return null;
        
        //DE ESTA FORMA NO SE NECESITA IR POR TODOS LOS USUARIOS, SINO SE FILTRAN DIRECTOS EN DB, LO CUAL LO HACE MAS RÁPIDO
        List<Usuario> result = _dbContext.Usuarios.Where(x => 
        x.NombreUsuario.Contains(input) ||
        x.NombreUsuario.Contains(input) ||
        x.PreguntaSeg.Contains(input) ||
        x.RespuestaSeg.Contains(input) ||
        x.Dpi.Contains(input) ||
        x.Nombre.Contains(input) ||
        x.Apellido.Contains(input) ||
        x.FechaNacimiento.ToString().Contains(input) ||
        x.Telefono.ToString().Contains(input) ||
        x.EstadoCivil.Contains(input) ||
        x.Profesion.Contains(input) ||
        x.Nacionalidad.Contains(input) ||
        x.Antecedentes.Contains(input) ||
        x.TipoSange.Contains(input) ||
        x.Password.Contains(input)).ToList();

        //      foreach (var user in _dbContext.Usuarios.ToList())
        //{
        //          if (user.NombreUsuario.Contains(input) ||
        //              user.NombreUsuario.Contains(input) ||
        //              user.PreguntaSeg.Contains(input) ||
        //              user.RespuestaSeg.Contains(input) ||
        //              user.Dpi.Contains(input) ||
        //              user.Nombre.Contains(input) ||
        //              user.Apellido.Contains(input) ||
        //              user.FechaNacimiento.ToString().Contains(input) ||
        //              user.Telefono.ToString().Contains(input) ||
        //              user.EstadoCivil.Contains(input) ||
        //              user.Profesion.Contains(input) ||
        //              user.Nacionalidad.Contains(input) ||
        //              user.Remitido.Contains(input) ||
        //              user.Antecedentes.Contains(input) ||
        //              user.TipoSange.Contains(input) ||
        //              user.NoRegistro.ToString().Contains(input) ||
        //              user.Password.Contains(input)
        //              )
        //              result.Add(GetUser(user.IdUsuario));
        //      }

        return result;
    }

    public List<RolDetalle> GetPermissions(Guid idUser)
    {
        var permissions = _dbContext.RolDetalles.Where(x => x.IdUsuario==idUser).ToList();
        return permissions;
    }
}
