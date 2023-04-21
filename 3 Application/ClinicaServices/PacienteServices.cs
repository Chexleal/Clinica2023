using Azure.Core;
using ClinicaDomain;
using PaaS.Framework.Utils.Extensions;

namespace ClinicaServices;

public interface IPacienteServices
{
	int AddPaciente(Paciente paciente);
	Paciente? GetPacienteById(Guid id);
    List<Paciente>? GetAll();
    void UpdatePaciente(Paciente paciente);
    void DeletePaciente(Guid id);
}
public class PacienteServices : IPacienteServices
{
	private readonly ClinicaContext _dbContext;
	public PacienteServices(ClinicaContext dbContext)
	{
		_dbContext = dbContext;
	}

	public int AddPaciente(Paciente paciente)
	{
		//CAMBIAR INT POR ENUM 
		try
		{
			//paciente.IdPaciente = Guid.NewGuid();

			var pacienteExsitente = _dbContext.Pacientes.FirstOrDefault(x => x.IdPaciente == paciente.IdPaciente && !x.EstadoEliminado);
			if (pacienteExsitente is not null) return 2;
            paciente.IdPaciente = $"{paciente.Nombre.Trim().ToLower()}|{paciente.Apellido.Trim().ToLower()}".ToGuid();
            paciente.EstadoEliminado = false;
            paciente.BeforeSaveChanges();
            _dbContext.Pacientes.Add(paciente);
            _dbContext.SaveChanges();
			return 1;
		}
		catch (Exception)
		{
			return 3;
		}
	}

    public Paciente? GetPacienteById(Guid id)
    {
        return _dbContext.Pacientes.FirstOrDefault(p => p.IdPaciente == id);
    }

    public List<Paciente> GetAll()
	{
        return _dbContext.Pacientes.Where(x => !x.EstadoEliminado).ToList();
	}

    public void UpdatePaciente(Paciente paciente)
    {
        var pacienteDB = GetPacienteById(paciente.IdPaciente);
        if (pacienteDB is not null)
        {
            pacienteDB.Dpi = paciente.Dpi;
            pacienteDB.Nombre = paciente.Nombre;
            pacienteDB.Apellido = paciente.Apellido;
            pacienteDB.FechaNacimiento = paciente.FechaNacimiento;
            pacienteDB.Genero = paciente.Genero;
            pacienteDB.Telefono = paciente.Telefono;
            pacienteDB.Correo = paciente.Correo;
            pacienteDB.Direccion = paciente.Direccion;
            pacienteDB.Alergias = paciente.Alergias;
            pacienteDB.EstadoCivil = paciente.EstadoCivil;
            pacienteDB.Profesion = paciente.Profesion;
            pacienteDB.Nacionalidad = paciente.Nacionalidad;
            pacienteDB.Remitido = paciente.Remitido;
            pacienteDB.Antecedentes = paciente.Antecedentes;
            pacienteDB.TipoSange = paciente.TipoSange;
            pacienteDB.BeforeSaveChanges();
            _dbContext.SaveChanges();
        }
    }

    public void DeletePaciente(Guid id)
    {
        var paciente = GetPacienteById(id);
        if (paciente is not null)
        {
            paciente.EstadoEliminado = true;
            _dbContext.SaveChanges();
        }
    }

}