using ClinicaDomain;
using System.Net.Mail;
using System.Net;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
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
  	void AddUser(Usuario usuario);
	List<Usuario> SearchUser(string input);
    Usuario GetUsuario(Guid id);

    Usuario GetUserByName(string userName);

    bool CheckEmails(string email, string emailConfirmed);

    bool UpdatePassword(string newPassword, string newPasswordConfirmed, string userName);

}
public class UserServices : IUserServices
{
    private readonly ClinicaContext _dbContext;

    private readonly PacienteServices _pacienteServices;
    public UserServices(ClinicaContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddUser(Usuario usuario)
    {
        _dbContext.Usuarios.Add(usuario);
        _dbContext.SaveChanges();
    }

    public bool Authenticate(string user, string password)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.Password == password && x.NombreUsuario == user);
        if (userItem == null) return false;
        return true;
    }

    public List<Usuario> GetAll()
    {
        return _dbContext.Usuarios.ToList();
    }


    public bool RecoverAccount(string email, string userName)
    {  

        // Send an email to the user with instructions to reset their password
        string fromAddress = "emrivera2001@gmail.com";
        string fromPassword = "vmdmrvksvawwkkzp";
        string toAddress = email;
        string subject = "Password reset for your account";
        string body = "<html>" +
                         "<body>" +
                         "<h1>Hello world!</h1>" +
                         "<p>This is an HTML email.</p>" +
                         $"<a href=\"https://localhost:7070/Home/NuevaClave/?userName={userName}\">link text</a>" +
                         "</body>" +
                       "</html>";


        try
        {
            using (MailMessage mail = new MailMessage(fromAddress, toAddress, subject, body))
            {
                mail.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
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
        if (userItem == null) return false;
        return true;
    }

    public string? SecurityQuestion(string userName)
    {
        var userItem = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario.ToLower().Trim() == userName.ToLower().Trim());
        if (userItem == null) {

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

    public bool CheckEmails(string email, string emailConfirmed)
    {
        var emailOnData = _dbContext.Usuarios.FirstOrDefault(x=> x.Correo == email) ;
        var userName = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == emailOnData.NombreUsuario);

        if (emailOnData != null)
        {
            bool checkedEmail = email.Equals(emailConfirmed);
            if (checkedEmail)
            {
                RecoverAccount(email,userName.NombreUsuario.ToString());
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

    public bool UpdatePassword(string newPassword, string newPasswordConfirmed, string userName)
    {
        bool checkedPassword = newPassword.Equals(newPasswordConfirmed);
        var user = _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario == userName);
        if (checkedPassword && user != null)
        {
            return true;
        }
        else { return false; }
    }

    public Usuario GetUsuario(Guid id)
    {
        return _dbContext.Usuarios.Find(id);
    }

    public Usuario? GetUserByName(string userName)
    {
        return _dbContext.Usuarios.FirstOrDefault(x => x.NombreUsuario.ToLower().Trim() == userName.ToLower().Trim());
    }

    public List<Usuario> SearchUser(string input)
        {
            List<Usuario> result = new List<Usuario>();
            //List<Guid> guids = new List<Guid>();
            foreach (var user in _dbContext.Usuarios.ToList())
            {
                if (user.NombreUsuario.Contains(input) ||
                    user.NombreUsuario.Contains(input) ||
                    user.PreguntaSeg.Contains(input) ||
                    user.RespuestaSeg.Contains(input) ||
                    user.Dpi.Contains(input) ||
                    user.Nombre.Contains(input) ||
                    user.Apellido.Contains(input) ||
                    user.FechaNacimiento.ToString().Contains(input) ||
                    user.Telefono.ToString().Contains(input) ||
                    user.EstadoCivil.Contains(input) ||
                    user.Profesion.Contains(input) ||
                    user.Nacionalidad.Contains(input) ||
                    user.Remitido.Contains(input) ||
                    user.Antecedentes.Contains(input) ||
                    user.TipoSange.Contains(input) ||
                    user.NoRegistro.Contains(input) ||
                    user.Password.Contains(input)
                    )
                    result.Add(_dbContext.Usuarios.Find(user.IdUsuario));
                //guids.Add(user.IdUsuario);
            }



            return result;
        }
}
