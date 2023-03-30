using ClinicaDomain;

namespace ClinicaServices;

public interface IUserServices
{
	bool Authenticate(string user, string password);
	List<Usuario> GetAll();
	void AddUser(Usuario usuario);
	List<Usuario> SearchUser(string input);
    Usuario GetUsuario(Guid id);
}
public class UserServices : IUserServices
{
	private readonly ClinicaContext _dbContext;
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
		var userItem= _dbContext.Usuarios.FirstOrDefault(x=>x.Password == password && x.NombreUsuario==user);
		if (userItem == null) return false;
		return true;
	}

	public List<Usuario> GetAll()
	{
		return _dbContext.Usuarios.ToList();
	}

    public Usuario GetUsuario(Guid id)
    {
        return _dbContext.Usuarios.Find(id);
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