using Azure.Core;
using ClinicaDomain;
using Microsoft.EntityFrameworkCore;
using PaaS.Framework.Utils.Extensions;

namespace ClinicaServices;

public interface IPacienteServices
{
	int AddPaciente(Paciente paciente);
	Paciente? GetPacienteById(Guid id);
    List<Paciente>? GetAll();
	void UpdatePaciente(Paciente paciente);
	void UpdateAntecedentes(Guid pacienteId, string? antecedentes);
	void DeletePaciente(Guid id);
    List<Consulta>? GetConsultasFiltradas(Guid servicioId);
    PagedResult<Paciente> GetPaginated(int start, int length, string search, int sortColumn, string sortDir);
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

    public PagedResult<Paciente> GetPaginated(int start, int length, string search, int sortColumn, string sortDir)
    {
        var query = _dbContext.Pacientes.AsNoTracking().Where(x => !x.EstadoEliminado);
        var total = query.Count();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var tokens = search.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var term = token;
                query = query.Where(x =>
                    EF.Functions.Collate(x.Nombre, "Latin1_General_CI_AI").Contains(term) ||
                    EF.Functions.Collate(x.Apellido, "Latin1_General_CI_AI").Contains(term) ||
                    EF.Functions.Collate(x.Dpi, "Latin1_General_CI_AI").Contains(term) ||
                    EF.Functions.Collate(x.Telefono, "Latin1_General_CI_AI").Contains(term) ||
                    EF.Functions.Collate(x.Correo, "Latin1_General_CI_AI").Contains(term) ||
                    x.NoRegistro.ToString().Contains(term));
            }
        }

        var totalFiltered = query.Count();

        var asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        query = sortColumn switch
        {
            1 => asc ? query.OrderBy(x => x.NoRegistro) : query.OrderByDescending(x => x.NoRegistro),
            2 => asc ? query.OrderBy(x => x.Nombre) : query.OrderByDescending(x => x.Nombre),
            3 => asc ? query.OrderBy(x => x.Apellido) : query.OrderByDescending(x => x.Apellido),
            4 => asc ? query.OrderBy(x => x.Dpi) : query.OrderByDescending(x => x.Dpi),
            5 => asc ? query.OrderByDescending(x => x.FechaNacimiento) : query.OrderBy(x => x.FechaNacimiento),
            6 => asc ? query.OrderBy(x => x.Telefono) : query.OrderByDescending(x => x.Telefono),
            7 => asc ? query.OrderBy(x => x.Correo) : query.OrderByDescending(x => x.Correo),
            _ => query.OrderBy(x => x.NoRegistro)
        };

        var data = length > 0 ? query.Skip(start).Take(length).ToList() : query.ToList();

        return new PagedResult<Paciente> { Total = total, TotalFiltered = totalFiltered, Data = data };
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
            pacienteDB.NoRegistro = paciente.NoRegistro;
            pacienteDB.BeforeSaveChanges();
            _dbContext.SaveChanges();
        }
    }

    public void UpdateAntecedentes(Guid pacienteId, string? antecedentes)
    {
        var paciente = GetPacienteById(pacienteId);
        if (paciente is not null)
        {
            paciente.Antecedentes = antecedentes ?? string.Empty;
            paciente.BeforeSaveChanges();
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

    public List<Consulta>? GetConsultasFiltradas(Guid servicioId)
    {
        List<Consulta> consultas = new();

        //foreach (var detalle in )
        //{

        //}

        return consultas;
    }
}
