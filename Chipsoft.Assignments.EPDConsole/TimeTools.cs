namespace Chipsoft.Assignments.EPDConsole
{
    public class TimeTools
    {
        public static List<DateTime> FindAvailableTimeSlots(
        List<Appointment> appointments,
        DateTime day,
        TimeOnly startTime,
        TimeOnly endTime)
        {
            var availableSlots = new HashSet<DateTime>();
            DateTime rangeStart = day.Date.Add(startTime.ToTimeSpan());
            DateTime rangeEnd = day.Date.Add(endTime.ToTimeSpan());

            for (DateTime time = rangeStart; time < rangeEnd; time = time.AddMinutes(15))
            {
                availableSlots.Add(time);
            }

            // Remove slots occupied by appointments
            foreach (var appointment in appointments)
            {
                availableSlots.Remove(appointment.Date);
            }

            // Return sorted available slots
            return availableSlots.OrderBy(slot => slot).ToList();
        }
    }
}

