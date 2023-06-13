using ClinicaDomain;

namespace clinicaWeb.Models
{
    public class UsuariosViewModel
    {
        public List<Usuario> Usuarios { get; set; }
        public List<string> Permisos { get; set; }
        public Usuario Usuario { get; set; }
        //public List<Paciente> Pacientes { get; set; }
        //public List<Consulta> Consultas { get; set; }
    }
}
