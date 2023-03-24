using ClinicaDomain;

namespace ClinicaServices;

public interface IUserServices
{
	bool Authenticate(string user, string password);
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
}