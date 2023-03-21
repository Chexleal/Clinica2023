using clinicaWeb.Models;

namespace ClinicaServices;

public interface IUserServices
{
	bool Authenticate(string user, string password);
	List<Usuario> GetAll();
}
public class UserServices : IUserServices
{
	private readonly ClinicaDbtraumhaContext _dbContext;
	public UserServices(ClinicaDbtraumhaContext dbContext)
	{
		_dbContext = dbContext;
	}

	public bool Authenticate(string user, string password)
	{
		var userItem= _dbContext.Usuarios.FirstOrDefault(x=>x.Password == password && x.UsuarioNombre==user);
		if (userItem == null) return false;
		return true;
	}

	public List<Usuario> GetAll()
	{
		return _dbContext.Usuarios.ToList();
	}
}