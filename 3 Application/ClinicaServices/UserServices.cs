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
    bool Authenticate(string user, string password);

    bool CheckUserExist(string userName);
    string? CheckAnswer(string userName);
    string? SecurityQuestion(string userName);
    List<Usuario> GetAll();
    void AddUser(Usuario user);
    List<Usuario> SearchUser(string input);
    Usuario GetUser(Guid id);
    void DeleteUser(Guid id);
    void UpdateUser(Usuario user);
    void SetActive(Guid id, bool state);
}
public class UserServices : IUserServices
{
    private readonly ClinicaContext _dbContext;
    public UserServices(ClinicaContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddUser(Usuario user)
    {
        user.IdUsuario = $"{user.NombreUsuario.Trim().ToLower()}".ToGuid();
        user.EstadoEliminado = false;
        user.UsuarioActivo = true;
        _dbContext.Usuarios.Add(user);
        _dbContext.SaveChanges();
    }

    public bool Authenticate(string user, string password)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.Password == password && x.NombreUsuario == user);
        if (userItem is null) return false;
        return true;
    }

    public List<Usuario> GetAll()
    {
        List<Usuario> result = _dbContext.Usuarios.Where(x =>
        x.EstadoEliminado.Equals(false)).ToList();

        return result;
        //return _dbContext.Usuarios.ToList();
    }

    public bool RecoverAccount(string email)
    {
        // Check if the email is registered in the database
        if (!CheckUserExist(email)) return false;

        // Generate a new password and update the user's account with it
        string newPassword = GeneratePassword();
        UpdatePassword(email, newPassword);

        // Send an email to the user with instructions to reset their password
        string fromAddress = "emrivera2001@gmail.com";
        string fromPassword = "fjgfbuvketepyacx";
        string toAddress = email;
        string subject = "Password reset for your account";
        string body = "Hello,\n\n" +
                      "We have received a request to reset the password for your account. " +
                      "Your new password is: " + newPassword + "\n\n" +
                      "Please use this password to login to your account, and then reset your password to a new value.\n\n" +
                      "Thank you,\n" +
                      "Your Website Team";

        try
        {
            using (MailMessage mail = new(fromAddress, toAddress, subject, body))
            {
                mail.IsBodyHtml = false;
                using (SmtpClient smtp = new("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                    smtp.Send(mail);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur while sending the email
            Console.WriteLine("Error sending email: " + ex.Message);
            return false;
        }
    }

    public bool CheckUserExist(string userName)
    {
        // Check if the email is registered in the database
        // Return true if it exists, false otherwise
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == userName);
        if (userItem is null) return false;
        return true;
    }

    public string? SecurityQuestion(string userName)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario.ToLower().Trim() == userName.ToLower().Trim());
        if (userItem is null)
            return null;
        else
        {
            var preguntaSegura = userItem.PreguntaSeg.ToString();
            return preguntaSegura;
        }
    }

    public string? CheckAnswer(string answer)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.RespuestaSeg.ToLower().Trim() == answer.ToLower().Trim());
        if (userItem is not null) return userItem.RespuestaSeg.ToString();
        return null;
    }

    public string GeneratePassword()
    {
        // Generate a new random password and return it as a string
        return "testing";
    }

    public bool UpdatePassword(string email, string newPassword)
    {
        // Update the user's account with the new password
        // Return true if successful, false otherwise
        return true;
    }

    public Usuario GetUser(Guid id)
    {
        //return _dbContext.Usuarios.Find(id);
        return _dbContext.Usuarios.FirstOrDefault(p => p.IdUsuario == id);
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
            userDB.Remitido = user.Remitido;
            userDB.Antecedentes = user.Antecedentes;
            userDB.TipoSange = user.TipoSange;
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
        x.Remitido.Contains(input) ||
        x.Antecedentes.Contains(input) ||
        x.TipoSange.Contains(input) ||
        x.NoRegistro.ToString().Contains(input) ||
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
}
