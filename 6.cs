
using Domain;
using Exceptions;
using System;
using System.Collections.Generic;
using Persistence;
using Domain.Enums;

namespace Logic
{
    public class StatisticsLogic
    {
        private PersistenceImp persistence;

        public StatisticsLogic(PersistenceImp onePersistence)
        {
            persistence = onePersistence;
        }

        private IList<Assignation> GetAssignationsByMode(AssignationMode? assignationMode)
        {
            if (assignationMode == null)
            {
                return persistence.GetAssignations();
            } 
            else
            {
                return persistence.FilterAssignationByMode(assignationMode);
            }
        }

        private IList<Assignation> GetAssignations(AssignationMode? assignationMode)
        {
            try
            {
                return GetAssignationsByMode(assignationMode);
            }
            catch (DatabaseException exception)
            {
                throw new GetEntityException(exception.Message, exception);
            }
        }

        private IList<Assignation> GetResolvedAssignations(AssignationMode? assignationMode)
        {
            try
            {
                return GetResolvedAssignationsByMode(assignationMode);
            }
            catch (DatabaseException exception)
            {
                throw new GetEntityException(exception.Message, exception);
            }
        }

        private IList<Assignation> GetResolvedAssignationsByMode(AssignationMode? assignationMode)
        {
            if (assignationMode == null)
            {
                return persistence.GetResolvedAssignations();
            }
            else
            {
                return persistence.FilterResolvedAssignationByMode(assignationMode);
            }
        }

        public string AverageAssignationTime(AssignationMode? assignationMode = null)
        {
            IList<Assignation> assignations = GetAssignations(assignationMode);
            if (assignations.Count == 0)
            {
                throw new EntityNotExistException("No hay asignaciones disponibles para obtener estadistica");
            }

            int days = 0;
            int hours = 0;
            int min = 0;
            int averageDays = 0;
            int averageHours = 0;
            int averageMin = 0;

            foreach (var assignation in assignations)
            {
                TimeSpan difference = assignation.AssignationDate.Subtract(assignation.EmergencyAssigned.EmergencyDate);
                days = days + difference.Days;
                hours = hours + difference.Hours;
                min = min + difference.Minutes;
            }
            averageDays = days / assignations.Count;
            averageHours = hours / assignations.Count;
            averageMin = min / assignations.Count;

            return averageDays + " Dia/s - " + averageHours + " Hora/s - " + averageMin + " Minuto/s";
        }

        public string AverageResolutionTime(AssignationMode? assignationMode = null)
        {
            IList<Assignation> assignations = GetResolvedAssignations(assignationMode);

            if (assignations.Count == 0)
            {
                throw new EntityNotExistException("No hay asignaciones resueltas disponibles para obtener estadistica");
            }

            int days = 0;
            int hours = 0;
            int min = 0;
            int averageDays = 0;
            int averageHours = 0;
            int averageMin = 0;

            foreach (var assignation in assignations)
            {
                TimeSpan difference = assignation.ResolutionDate.Value.Subtract(assignation.AssignationDate);
                days = days + difference.Days;
                hours = hours + difference.Hours;
                min = min + difference.Minutes;
            }
            averageDays = days / assignations.Count;
            averageHours = hours / assignations.Count;
            averageMin = min / assignations.Count;

            return averageDays + " Dia/s - " + averageHours + " Hora/s - " + averageMin + " Minuto/s";
        }

        public string AverageAssistanceTime(AssignationMode? assignationMode = null)
        {
            IList<Assignation> assignations = GetResolvedAssignations(assignationMode);

            if (assignations.Count == 0)
            {
                throw new EntityNotExistException("No hay asignaciones resueltas disponibles para obtener estadistica");
            }

            int days = 0;
            int hours = 0;
            int min = 0;
            int averageDays = 0;
            int averageHours = 0;
            int averageMin = 0;

            foreach (var assignation in assignations)
            {
                TimeSpan difference = assignation.ResolutionDate.Value.Subtract(assignation.EmergencyAssigned.EmergencyDate);
                days = days + difference.Days;
                hours = hours + difference.Hours;
                min = min + difference.Minutes;
            }
            averageDays = days / assignations.Count;
            averageHours = hours / assignations.Count;
            averageMin = min / assignations.Count;

            return averageDays + " Dia/s - " + averageHours + " Hora/s - " + averageMin + " Minuto/s";
        }
    }
 }