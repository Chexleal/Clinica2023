using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface IUserServices
{
	bool Authenticate(string user, string password);
	List<Usuario> GetAll();
	void AddUser(Usuario user);
	List<Usuario> SearchUser(string input);
    Usuario GetUser(Guid id);
    void DeleteUser(Guid id);
    void UpdateUser(Usuario user);
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
			_dbContext.Usuarios.Add(user);
			_dbContext.SaveChanges();
    }

    public void UpdateUser(Usuario user) {
        if (user != null)
        {
            var userDB = GetUser(user.IdUsuario);
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
            _dbContext.Usuarios.Update(userDB);
            _dbContext.SaveChanges();
        }
    }

    public void DeleteUser(Guid id)
    {
        var user = GetUser(id);
        if (user != null)
        {
            user.EstadoEliminado = true;
            _dbContext.SaveChanges();
        }
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

    public Usuario GetUser(Guid id)
    {
        //return _dbContext.Usuarios.Find(id);
        return _dbContext.Usuarios.FirstOrDefault(p => p.IdUsuario == id);
    }

    public List<Usuario> SearchUser(string input)
    {
        List<Usuario> result = new List<Usuario>();
        if (string.IsNullOrEmpty(input)) { return null; }
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
                result.Add(GetUser(user.IdUsuario));
        }

        return result;
    }
}