﻿using ClinicaDomain;

namespace clinicaWeb.Models
{
    public class CitasViewModel
    {
        public List<Paciente> Pacientes { get; set; }
        public List<Cita> Citas { get; set; }
        public Paciente? Paciente { get; set; }

        public Usuario? Usuario { get; set; }
        //public List<(string, Cita)> Eventos = new List<(string, Cita)>();

        //string[,] arreglo_enteros = new string[3, 3];
        //public List<>
    }
}
