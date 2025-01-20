using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Chipsoft.Assignments.EPDConsole
{
    public class Program
    {
        /// <summary>
        ///  The maximum amount of days to look ahead for planning an appointment
        /// </summary>
        private const int PLANNING_MAX_LOOK_AHEAD = 56; // 8 weeks
        private static TimeOnly START_TIME = new(8, 0);
        private static TimeOnly END_TIME = new(17, 0);

        //Don't create EF migrations, use the reset db option
        //This deletes and recreates the db, this makes sure all tables exist

        private static void AddPatient()
        {
            AnsiConsole.Clear();
            EPDDbContext dbContext = new();

            var name = AnsiConsole.Ask<string>("[red]Voornaam:[/] ");
            var surname = AnsiConsole.Ask<string>("[red]Achternaam:[/] ");
            var street = AnsiConsole.Ask<string>("[red]Straat:[/] ");
            var zipCode = AnsiConsole.Ask<string>("[red]Postcode:[/] ");
            var city = AnsiConsole.Ask<string>("[red]Plaats:[/] ");
            var country = AnsiConsole.Ask<string>("[red]Land:[/] ", "België");
            var email = AnsiConsole.Ask<string>("[red]Email:[/] ");
            var phone = AnsiConsole.Ask<string>("[red]Telefoon:[/] ");

            var patient = new Patient(name, surname, street, city, zipCode, country, email, phone);
            dbContext.Patients.Add(patient);
            var confirmation = AnsiConsole.Confirm($"Patient [bold green]{name} {surname}[/] toevoegen?");
            if (confirmation)
            {
                dbContext.SaveChanges();
            }
        }

        private static void ShowAppointment()
        {
            AnsiConsole.Clear();
            var now = DateTime.Now;
            var previous = now.AddMonths(-1);
            var next = now.AddMonths(1);
            AnsiConsole.Write(new Columns(
                new Calendar(previous.Year, previous.Month).Culture("nl-BE"),
                new Calendar(now.Year, now.Month).Culture("nl-BE").AddCalendarEvent(now).HighlightStyle(Style.Parse("yellow bold")),
                new Calendar(next.Year, next.Month).Culture("nl-BE")
            ));

            var result = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Kies een optie (space/enter om te selecteren)")
                .AddChoices(new string[] {
                    "1 - Alle afspraken",
                    "2 - Afspraken van patiënt",
                    "3 - Afspraken van arts",
                    "4 - Afspraken van arts vandaag",
                })
            );

            EPDDbContext dbContext = new();

            // Show all appointments
            if (result.StartsWith("1"))
            {
                var appointments = dbContext.Appointments
                    .Include(a => a.Physician)
                    .Include(a => a.Patient)
                    .Where(a => a.Physician.Deleted == null)
                    .Where(a => a.Patient.Deleted == null)
                    .ToList()
                    .GroupBy(a => a.Date.Date)
                    .OrderByDescending(g => g.Key);

                if (!appointments.Any())
                {
                    AnsiConsole.Confirm("[bold red]Geen afspraken gevonden[/]");
                    return;
                }

                var prompt = new SelectionPrompt<Appointment>()
                        .Title("Alle afspraken")
                        .PageSize(14)
                        .UseConverter(a => a.ToString());

                foreach (var group in appointments)
                {
                    prompt.AddChoiceGroup(new(group.Key, "", 0, 0), group);
                }

                AnsiConsole.Prompt(prompt);
            }
            // Show appointments of patient
            else if (result.StartsWith("2"))
            {
                var patients = dbContext.Patients.Where(p => p.Deleted == null).ToList();
                var patient = Util.SearchAndSelect(
                    patients,
                    "Zoek patient",
                    "Kies een patient [red](Ctr+C om te annuleren)[/]",
                    (patient) => $"{patient.Name} {patient.Surname} - {patient.Street} {patient.City}"
                );

                if (patient == null)
                {
                    return;
                }

                var appointments = dbContext.Appointments
                    .Include(a => a.Physician)
                    .Include(a => a.Patient)
                    .Where(a => a.Physician.Deleted == null)
                    .Where(a => a.Patient.Deleted == null)
                    .Where(a => a.PatientId == patient.PatientId)
                    .ToList()
                    .GroupBy(a => a.Date.Date)
                    .OrderByDescending(g => g.Key);

                if (!appointments.Any())
                {
                    AnsiConsole.Confirm("[bold red]Geen afspraken gevonden[/]");
                    return;
                }

                var prompt = new SelectionPrompt<Appointment>()
                        .Title($"Alle afspraken voor [bold green]{patient.Name} {patient.Surname}[/]")
                        .PageSize(14)
                        .UseConverter(a => a.ToString());

                foreach (var group in appointments)
                {
                    prompt.AddChoiceGroup(new(group.Key, "", 0, 0), group);
                }

                AnsiConsole.Prompt(prompt);
            }
            // Show appointments of physician
            else if (result.StartsWith("3"))
            {
                var physicians = dbContext.Physician.Where(p => p.Deleted == null).ToList();
                var physician = Util.SearchAndSelect(
                    physicians,
                    "Zoek arts",
                    "Kies een arts [red](Ctr+C om te annuleren)[/]",
                    (physician) => $"{physician.Name} {physician.Surname}"
                );

                if (physician == null)
                {
                    return;
                }

                var appointments = dbContext.Appointments
                    .Include(a => a.Physician)
                    .Include(a => a.Patient)
                    .Where(a => a.Physician.Deleted == null)
                    .Where(a => a.Patient.Deleted == null)
                    .Where(a => a.PhysicianId == physician.PhysicianId)
                    .ToList()
                    .GroupBy(a => a.Date.Date)
                    .OrderByDescending(g => g.Key);

                if (!appointments.Any())
                {
                    AnsiConsole.Confirm("[bold red]Geen afspraken gevonden[/]");
                    return;
                }

                var prompt = new SelectionPrompt<Appointment>()
                        .Title($"Alle afspraken voor [bold red]{physician.Name} {physician.Surname}[/]")
                        .PageSize(14)
                        .UseConverter(a => a.ToString());

                foreach (var group in appointments)
                {
                    prompt.AddChoiceGroup(new(group.Key, "", 0, 0), group);
                }

                AnsiConsole.Prompt(prompt);
            }
            // Show appointments of physician today
            else if (result.StartsWith("4"))
            {
                var physicians = dbContext.Physician.Where(p => p.Deleted == null).ToList();
                var physician = Util.SearchAndSelect(
                    physicians,
                    "Zoek arts",
                    "Kies een arts [red](Ctr+C om te annuleren)[/]",
                    (physician) => $"{physician.Name} {physician.Surname}"
                );

                if (physician == null)
                {
                    return;
                }

                var appointments = dbContext.Appointments
                    .Include(a => a.Physician)
                    .Include(a => a.Patient)
                    .Where(a => a.Physician.Deleted == null)
                    .Where(a => a.Patient.Deleted == null)
                    .Where(a => a.Date.Date == DateTime.Now.Date)
                    .Where(a => a.PhysicianId == physician.PhysicianId)
                    .ToList();

                if (!appointments.Any())
                {
                    AnsiConsole.Confirm("[bold red]Geen afspraken gevonden[/]");
                    return;
                }

                var prompt = new SelectionPrompt<Appointment>()
                        .Title($"Afspraken van vandaag voor [bold red]{physician.Name} {physician.Surname}[/]")
                        .PageSize(14)
                        .UseConverter(a => a.ToString())
                        .AddChoices(appointments);

                AnsiConsole.Prompt(prompt);
            }
        }

        private static void AddAppointment()
        {
            AnsiConsole.Clear();
            EPDDbContext dbContext = new();
            var physicians = dbContext.Physician.Where(p => p.Deleted == null).ToList();
            var patients = dbContext.Patients.Where(p => p.Deleted == null).ToList();

            var physician = Util.SearchAndSelect(
                physicians,
                "Zoek arts",
                "Kies een arts [red](Ctr+C om te annuleren)[/]",
                (physician) => $"{physician.Name} {physician.Surname}"
            );

            if (physician == null)
            {
                return;
            }

            var patient = Util.SearchAndSelect(
                patients,
                "Zoek patient",
                "Kies een patient [red](Ctr+C om te annuleren)[/]",
                (patient) => $"{patient.Name} {patient.Surname} - {patient.Street} {patient.City}"
            );

            if (patient == null)
            {
                return;
            }

            var description = AnsiConsole.Ask<string>("[red]Beschrijving:[/] ");
            var now = DateTime.Now;

            // Show calendar for current and next month as reference
            var next = now.AddMonths(1);
            AnsiConsole.Write(new Columns(
                new Calendar(now.Year, now.Month).Culture("nl-BE").AddCalendarEvent(now).HighlightStyle(Style.Parse("yellow bold")),
                new Calendar(next.Year, next.Month).Culture("nl-BE")
            ));

            var prompt = new SelectionPrompt<DateTime>()
                .Title($"Kies een datum voor een afspraak bij [bold red]{physician.Surname} {physician.Name}[/] voor [bold green]{patient.Surname} {patient.Name}[/]")
                .PageSize(14)
                .UseConverter((date) => $"{date:dd/MM/yyyy HH:mm} - {Util.WeekdayToStringNL(date)}");

            // Fetch all available time slots and add them grouped by day to the prompt
            for (int i = 0; i < PLANNING_MAX_LOOK_AHEAD; i++)
            {
                prompt.AddChoiceGroup(DateTime.Now.AddDays(i), TimeTools.FindAvailableTimeSlots(
                    dbContext.Appointments.Where(a => a.PhysicianId == physician.PhysicianId).ToList(),
                    now.AddDays(i),
                    START_TIME,
                    END_TIME
                ));
            }

            var date = AnsiConsole.Prompt(prompt);
            var appointment = new Appointment(date, description, patient.PatientId, physician.PhysicianId);
            var confirmed = AnsiConsole.Confirm(
                $"Afspraak op [bold blue]{date:dd/MM/yyyy:HH:mm}[/] voor [bold green]{patient.Surname} {patient.Name}[/] bij [bold red]{physician.Surname} {physician.Name}[/] toevoegen?"
            );

            if (confirmed)
            {
                dbContext.Appointments.Add(appointment);
                dbContext.SaveChanges();
            }
        }

        private static void DeletePhysician()
        {
            AnsiConsole.Clear();
            EPDDbContext dbContext = new();
            var physicians = dbContext.Physician.Where(p => p.Deleted == null).ToList();
            var physician = Util.SearchAndSelect(
                physicians,
                "Zoek arts",
                "Kies een arts om te verwijderen [red](Ctr+C om te annuleren)[/]",
                (physician) => $"{physician.Name} {physician.Surname}"
            );

            if (physician == null)
            {
                return;
            }

            var confirmation = AnsiConsole.Confirm($"Arts [bold red]{physician.Name}[/] verwijderen?");
            if (confirmation)
            {
                physician.Deleted = DateTime.Now;
                dbContext.Physician.Update(physician);
                dbContext.SaveChanges();
                AnsiConsole.MarkupLine($"[bold green]Arts {physician} verwijderd[/]");
            }
        }

        private static void AddPhysician()
        {
            AnsiConsole.Clear();
            EPDDbContext dbContext = new();

            var name = AnsiConsole.Ask<string>("[red]Voornaam:[/] ");
            var surname = AnsiConsole.Ask<string>("[red]Achternaam:[/] ");

            var pyhsician = new Physician(name, surname);
            dbContext.Physician.Add(pyhsician);
            var confirmation = AnsiConsole.Confirm($"Arts [bold green]{name} {surname}[/] toevoegen?");
            if (confirmation)
            {
                dbContext.SaveChanges();
            }
        }

        private static void DeletePatient()
        {
            AnsiConsole.Clear();
            EPDDbContext dbContext = new();
            var patients = dbContext.Patients.Where(p => p.Deleted == null).ToList();
            var patient = Util.SearchAndSelect(
                patients,
                "Zoek patient",
                "Kies een patient om te verwijderen [red](Ctr+C om te annuleren)[/]",
                (patient) => $"{patient.Name} {patient.Surname} - {patient.Street} {patient.City}"
            );

            if (patient == null)
            {
                return;
            }

            var confirmation = AnsiConsole.Confirm($"Patient [bold red]{patient.Name}[/] verwijderen?");
            if (confirmation)
            {
                patient.Deleted = DateTime.Now;
                dbContext.Patients.Update(patient);
                dbContext.SaveChanges();
                AnsiConsole.Confirm($"[bold green]Patient {patient} verwijderd[/]");
            }
        }

        #region FreeCodeForAssignment
        static void Main(string[] args)
        {
            while (ShowMenu())
            {
                //Continue
            }
        }

        public static bool ShowMenu()
        {
            Console.Clear();
            foreach (var line in File.ReadAllLines("logo.txt"))
            {
                AnsiConsole.MarkupLine($"[bold blue]{line}[/]");
            }

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Kies een optie (space/enter om te selecteren)")
                .AddChoices(new[] {
                    "1 - Patient toevoegen",
                    "2 - Patienten verwijderen",
                    "3 - Arts toevoegen",
                    "4 - Arts verwijderen",
                    "5 - Afspraak toevoegen",
                    "6 - Afspraken inzien",
                    "7 - Sluiten",
                    "8 - Reset db"
                }));

            if (int.TryParse(selection.Split(" ")[0], out var option))
            {
                switch (option)
                {
                    case 1:
                        AddPatient();
                        return true;
                    case 2:
                        DeletePatient();
                        return true;
                    case 3:
                        AddPhysician();
                        return true;
                    case 4:
                        DeletePhysician();
                        return true;
                    case 5:
                        AddAppointment();
                        return true;
                    case 6:
                        ShowAppointment();
                        return true;
                    case 7:
                        return false;
                    case 8:
                        EPDDbContext dbContext = new();
                        dbContext.Database.EnsureDeleted();
                        dbContext.Database.EnsureCreated();
#if DEBUG
                        // Seed data
                        var pyhsician = new Physician("John", "Doe");
                        dbContext.Physician.Add(pyhsician);
                        var patient = new Patient("Hannah", "Witvrouwen", "Selderstraat 7/201", "Antwerpen", "2060", "België", "hannah.witvrouwen@gmail.com", "+324 71 79 10 67");
                        dbContext.Patients.Add(patient);
                        dbContext.SaveChanges();
#endif
                        return true;
                    default:
                        return true;
                }
            }
            return true;
        }

        #endregion
    }
}