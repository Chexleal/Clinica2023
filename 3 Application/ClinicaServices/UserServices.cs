using ClinicaDomain;
using System.Net.Mail;
using System.Net;

namespace ClinicaServices;

public interface IUserServices
{
	bool Authenticate(string user, string password);

    bool CheckUserExist(string userName);

    string? CheckAnswer(string userName);

    string? SecurityQuestion(string userName);

    List<Usuario> GetAll();
}
public class UserServices : IUserServices
{
	private readonly ClinicaContext _dbContext;
	public UserServices(ClinicaContext dbContext)
	{
		_dbContext = dbContext;
	}

	public bool Authenticate(string user, string password)
	{
		var userItem= _dbContext.Usuarios.FirstOrDefault(x=>x.Password == password && x.NombreUsuario==user);
		if (userItem == null) return false;
		return true;
	}

	public List<Usuario> GetAll()
	{
		return _dbContext.Usuarios.ToList();
	}

    public bool RecoverAccount(string email)
    {
        // Check if the email is registered in the database
        if (!CheckUserExist(email))
        {
            return false;
        }

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
            using (MailMessage mail = new MailMessage(fromAddress, toAddress, subject, body))
            {
                mail.IsBodyHtml = false;
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
}