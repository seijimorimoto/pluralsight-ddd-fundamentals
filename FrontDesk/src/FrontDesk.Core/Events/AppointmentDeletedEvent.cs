using FrontDesk.Core.ScheduleAggregate;
using PluralsightDdd.SharedKernel;

namespace FrontDesk.Core.Events
{
  public class AppointmentDeletedEvent : BaseDomainEvent
  {
    public AppointmentDeletedEvent(Appointment appointment)
    {
      AppointmentDeleted = appointment;
    }

    public Appointment AppointmentDeleted { get; private set; }
  }
}
