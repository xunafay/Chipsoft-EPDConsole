using Microsoft.EntityFrameworkCore;

namespace Chipsoft.Assignments.EPDConsole
{
    public class EPDDbContext : DbContext
    {
        // The following configures EF to create a Sqlite database file in the
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source=epd.db");
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Physician> Physician { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
    }

    public class Patient
    {
        public int PatientId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public Patient(
            string name,
            string surname,
            string street,
            string city,
            string zipCode,
            string country,
            string email,
            string phone
        )
        {
            Name = name;
            Surname = surname;
            Street = street;
            City = city;
            ZipCode = zipCode;
            Country = country;
            Email = email;
            Phone = phone;
        }
    }


    public class Physician
    {
        public Physician(string name, string surname)
        {
            Name = name;
            Surname = surname;
        }

        public int PhysicianId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    }

    public class Appointment
    {
        public int AppointmentId { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }

        public Patient Patient { get; set; } = null!;
        public int PatientId { get; set; }

        public Physician Physician { get; set; } = null!;
        public int PhysicianId { get; set; }

        public Appointment(DateTime date, string? description, int patientId, int physicianId)
        {
            Date = date;
            Description = description;
            PatientId = patientId;
            PhysicianId = physicianId;
        }

        public override string ToString()
        {
            if (Physician == null || Patient == null)
            {
                return $"{Date:dd/MM/yyyy}";
            }
            else
            {
                return $"{Date:dd/MM/yyyy HH:mm} - {Patient.Name} {Patient.Surname} - {Physician.Name} {Physician.Surname} - {Description}";
            }
        }
    }
}
