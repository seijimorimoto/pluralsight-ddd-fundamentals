using System;

namespace BlazorShared.Models.Appointment
{
  public class ListAppointmentRequest : BaseRequest
  {
    public const string Route = "api/schedule/{ScheduleId}/appointments?date={Date}";
    public Guid ScheduleId { get; set; }
    public DateTimeOffset Date { get; set; }
  }
}
