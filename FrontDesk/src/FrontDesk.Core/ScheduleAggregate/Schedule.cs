using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using FrontDesk.Core.Events;
using PluralsightDdd.SharedKernel;
using PluralsightDdd.SharedKernel.Interfaces;

namespace FrontDesk.Core.ScheduleAggregate
{
  public class Schedule : BaseEntity<Guid>, IAggregateRoot
  {
    public Schedule(Guid id,
      DateTimeOffsetRange dateRange,
      int clinicId)
    {
      Id = Guard.Against.Default(id, nameof(id));
      DateRange = dateRange;
      ClinicId = Guard.Against.NegativeOrZero(clinicId, nameof(clinicId));
    }

    private Schedule(Guid id, int clinicId) // used by EF
    {
      Id = id;
      ClinicId = clinicId;
    }

    public int ClinicId { get; private set; }
    private readonly List<Appointment> _appointments = new List<Appointment>();
    public IEnumerable<Appointment> Appointments => _appointments.AsReadOnly();

    public DateTimeOffsetRange DateRange { get; private set; }

    public void AddNewAppointment(Appointment appointment)
    {
      Guard.Against.Null(appointment, nameof(appointment));
      Guard.Against.Default(appointment.Id, nameof(appointment.Id));
      Guard.Against.DuplicateAppointment(_appointments, appointment, nameof(appointment));

      _appointments.Add(appointment);

      MarkConflictingAppointments();

      var appointmentScheduledEvent = new AppointmentScheduledEvent(appointment);
      Events.Add(appointmentScheduledEvent);
    }

    public void DeleteAppointment(Appointment appointment)
    {
      Guard.Against.Null(appointment, nameof(appointment));
      var appointmentToDelete = _appointments
                                .Where(a => a.Id == appointment.Id)
                                .FirstOrDefault();

      if (appointmentToDelete != null)
      {
        _appointments.Remove(appointmentToDelete);
      }

      MarkConflictingAppointments();

      var appointmentDeletedEvent = new AppointmentDeletedEvent(appointment);
      Events.Add(appointmentDeletedEvent);
    }

    private void MarkConflictingAppointments()
    {
      foreach (var appointment in _appointments)
      {
        // Same patient cannot have two appointments at same time
        var appointmentsConflictingBySamePatient = _appointments
            .Where(a => a.PatientId == appointment.PatientId &&
            a.TimeRange.Overlaps(appointment.TimeRange) &&
            a != appointment)
            .ToList();

        // Same room cannot have two appointments at same time
        var appointmentsConflictingBySameRoom = _appointments
          .Where(a => a.RoomId == appointment.RoomId &&
          a.TimeRange.Overlaps(appointment.TimeRange) &&
          a != appointment)
          .ToList();

        // Same doctor cannot have two appointments at same time
        var appointmentsConflictingBySameDoctor = _appointments
          .Where(a => a.DoctorId == appointment.DoctorId &&
          a.TimeRange.Overlaps(appointment.TimeRange) &&
          a != appointment)
          .ToList();

        appointmentsConflictingBySamePatient.ForEach(a => a.IsPotentiallyConflicting = true);
        appointmentsConflictingBySameRoom.ForEach(a => a.IsPotentiallyConflicting = true);
        appointmentsConflictingBySameDoctor.ForEach(a => a.IsPotentiallyConflicting = true);

        appointment.IsPotentiallyConflicting = appointmentsConflictingBySamePatient.Any()
          || appointmentsConflictingBySameRoom.Any()
          || appointmentsConflictingBySameDoctor.Any();
      }
    }

    /// <summary>
    /// Call any time this schedule's appointments are updated directly
    /// </summary>
    public void AppointmentUpdatedHandler()
    {
      MarkConflictingAppointments();
    }
  }
}
