using Azure.Core;
using ClinicaDomain;

namespace ClinicaServices;

public interface IPacienteServices
{
	int Create(Paciente paciente);
	Paciente Get(Guid id);
    List<Paciente> GetAll();
    void Update(Paciente paciente);
    void Delete(Guid id);
	void PermantlyDelete(Guid id);
}
public class PacienteServices : IPacienteServices
{
	private readonly ClinicaContext _dbContext;
	public PacienteServices(ClinicaContext dbContext)
	{
		_dbContext = dbContext;
	}

	public int Create(Paciente paciente)
	{
		//CAMBIAR INT POR ENUM 
		try
		{
			//paciente.IdPaciente = Guid.NewGuid();

			var pacienteExsitente = _dbContext.Pacientes.FirstOrDefault(x => x.IdPaciente == paciente.IdPaciente && !x.EstadoEliminado);
			if (pacienteExsitente is not null) return 2;

			_dbContext.Pacientes.Add(paciente);
			_dbContext.SaveChanges();
			return 1;
		}
		catch (Exception)
		{
			return 3;
		}
	}

	public Paciente Get(Guid id)
	{
		return _dbContext.Pacientes.FirstOrDefault(x => x.IdPaciente == id && !x.EstadoEliminado);
	}
	public List<Paciente> GetAll()
	{
		return _dbContext.Pacientes.Where(x => !x.EstadoEliminado).ToList();
	}

	public void Update(Paciente paciente)
	{
		_dbContext.Pacientes.Update(paciente);
    }

    public void Delete(Guid id)
    {
        var paciente = Get(id);
		paciente.EstadoEliminado = true;
		Update(paciente);
    }
    public void PermantlyDelete(Guid id)
    {
		var paciente= Get(id);
        _dbContext.Pacientes.Remove(paciente);
    }

}