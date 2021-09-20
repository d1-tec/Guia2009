using BusinessLogic.Exceptions;
using BusinessLogic.ReservationFolder;
using BusinessLogic.SMSExtract;
using BusinessLogic.SwitchCountryValidator;
using BusinessLogic.UserFolder;
using BusinessLogic.Validators;
using BusinessLogic.Validators.Uruguay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1;

namespace BusinessLogic.ParkItSystem
{
    public class ParkItImplemented : IParkItSystem
    {
        public List<User> RegisteredUsers { get; set; }
        public List<Reservation> ActiveReservations { get; set; }
        public ITimeProvider Provider { get; set; }

        private const int LicensePlateNumbersLength = 4;
        private const int LicensePlateLettersLength = 3;

        public ParkItImplemented(ITimeProvider provider)
        {
            this.RegisteredUsers = new List<User>();
            this.ActiveReservations = new List<Reservation>();
            this.Provider = provider;
        }

        public void RegisterUser(string phoneNumberInput)
        {
            PhoneNumberValidatorImpUru phoneValidator = new PhoneNumberValidatorImpUru();
            phoneValidator.PhoneNumberToEvaluate = phoneNumberInput;
            if (phoneValidator.PhoneNumberIsValid())
            {
                phoneNumberInput = SetStandardFormatPhone(phoneNumberInput);
                if (!ContainsThisUser(phoneNumberInput))
                {
                    AddUser(phoneNumberInput);
                }
                else
                {
                    throw new UserAlreadyRegisteredException(phoneNumberInput);
                }
            }
        }

        private void AddUser(string phoneToBeRegistered)
        {
            User userToBeAdded = new User(phoneToBeRegistered,SelectedCountry.CurrentCountry.CountryName);
            RegisteredUsers.Add(userToBeAdded);
        }

        private string SetStandardFormatPhone(string phoneNumberInput)
        {
            if (phoneNumberInput.Length == 8)
            {
                phoneNumberInput = "0" + phoneNumberInput;
            }
            return phoneNumberInput;
        }

        public User GetUserByNumber(string phoneNumberEntered)
        {
            string originalPhoneInput = phoneNumberEntered;
            phoneNumberEntered = SetStandardFormatPhone(phoneNumberEntered);
            if (this.ContainsThisUser(phoneNumberEntered))
            {
                
                User userFound = RegisteredUsers.FirstOrDefault(x => x.PhoneNumber == phoneNumberEntered);
                return userFound;
            }
            else
            {
                throw new UserNotFoundException(originalPhoneInput);
            }
            
        }

        public bool UsersListIsEmpty()
        {
            return this.AmountOfRegisteredUsers() == 0;
        }

        public int AmountOfRegisteredUsers()
        {
            return this.RegisteredUsers.Count();
        }

        public bool ContainsThisUser(string phoneNumberEntered)
        {
            phoneNumberEntered = SetStandardFormatPhone(phoneNumberEntered);
            User userToSearch = new User(phoneNumberEntered,SelectedCountry.CurrentCountry.CountryName);
            return this.RegisteredUsers.Contains(userToSearch);

        }

        public void AddReservation(string SMSInput, string phoneNumberInput)
        {
            if (this.ContainsThisUser(phoneNumberInput))
            {
                if (SMSIsValid(SMSInput))
                {
                    User reservationUser = this.GetUserByNumber(phoneNumberInput);
                    string[] smsFields = SMSInput.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    SMSExtractDataImpUruguay extractSMS = new SMSExtractDataImpUruguay(smsFields);
                    int minutesReservation = Int32.Parse(extractSMS.ParkingMinutes);
                    reservationUser.PayForParkingIfHasEnoughBalance(minutesReservation);
                    Reservation newReservation = new Reservation(reservationUser.PhoneNumber, SMSInput, Provider);
                    this.ActiveReservations.Add(newReservation);
                }
            }
            else
            {
                throw new UserNotFoundException(phoneNumberInput);
            }

        }

        private bool SMSIsValid(string SMSInput)
        {
            SMSFormatValidatorUru SMSValidator = new SMSFormatValidatorUru(Provider);
            SMSValidator.SMSInput = SMSInput;
            return SMSValidator.SMSIsValid();
        }

        public bool ExistsInReservations(string licensePlate, string timeOfDay)
        {
            DeleteInactiveReservations();
            if (TimeOfDayInputIsValid(timeOfDay) && LicensePlateIsValid(licensePlate))
            {
                return this.LicensePlateHasAReservation(licensePlate, timeOfDay); 
            }          
            return false;
            
        }

        public void DeleteInactiveReservations()
        {
            for (int indexOfReservation = 0; indexOfReservation < ActiveReservations.Count; indexOfReservation++)
            {
                Reservation currentReservation = ActiveReservations[indexOfReservation];
                if (ReservationIsOutOfTime(currentReservation))
                {
                    this.ActiveReservations.Remove(currentReservation);
                    indexOfReservation--;
                }

            }

        }

        private bool TimeOfDayInputIsValid(string timeOfDay)
        {
            ParkingStartTimeValidatorUy validator = new ParkingStartTimeValidatorUy(this.Provider);
            validator.ParkingStartTimeToBeEvaluated = timeOfDay;
            return validator.ParkingStartTimeIsValid();
            
        }

        private bool LicensePlateIsValid(string licensePlate)
        {
            
            CheckLicensePlateValidatorUy check = new CheckLicensePlateValidatorUy();
            check.LicensePlateToEvaluate = licensePlate;
            return check.LicensePlateToCheckIsValid();
            
        }

        

        private bool ReservationIsOutOfTime(Reservation currentReservation)
        {
            string currentTime = this.Provider.Now();
            int hourCurrentTime = GetHourAsAInt(currentTime);
            int minutesCurrentTime = GetMinutesAsAInt(currentTime);
            int endHourReservation = GetHourAsAInt(currentReservation.EndTime);
            int endMinutesReservation = GetMinutesAsAInt(currentReservation.EndTime);
            if (hourCurrentTime > endHourReservation)
            {
                return true;
            }
            else if (hourCurrentTime < endHourReservation)
            {
                return false;
            }
            else
            {
                if (minutesCurrentTime <= endMinutesReservation)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            
        }

        private int GetHourAsAInt(string hour)
        {
            hour = hour.Substring(0, 2);
            int hourAsInt = Int32.Parse(hour);
            return hourAsInt;
        }

        private int GetMinutesAsAInt(string minutes)
        {
            minutes = minutes.Substring(3, 2);
            int minutesAsInt = Int32.Parse(minutes);
            return minutesAsInt;
        }

        private bool LicensePlateHasAReservation(string licensePlate, string timeOfDay)
        {
            for (int indexOfReservation = 0; indexOfReservation < ActiveReservations.Count; indexOfReservation++)
            {
                Reservation currentReservation = ActiveReservations[indexOfReservation];
                if (CurrentReservationHasThisLicensePlate(currentReservation, licensePlate))
                {
                    return IsInTimeRange(currentReservation,timeOfDay);
                }

            }
            return false;
        }

        private bool CurrentReservationHasThisLicensePlate(Reservation currentReservation, string licensePlate)
        {

            return String.Equals(currentReservation.LicensePlate, licensePlate, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsInTimeRange(Reservation currentReservation, string timeOfDay)
        {
            string endTimeReservation = currentReservation.EndTime;
            int hourEndTimeReservation = GetHourAsAInt(endTimeReservation);
            int minutesEndTimeReservation = GetMinutesAsAInt(endTimeReservation);

            int hourTimeOfDay = GetHourAsAInt(timeOfDay);
            int minutesTimeOfDay = GetMinutesAsAInt(timeOfDay);
            if (hourEndTimeReservation > hourTimeOfDay)
            {
                return true;
            }
            else if (hourEndTimeReservation < hourTimeOfDay)
            {
                return false;
            }
            else
            {
                if (minutesEndTimeReservation >= minutesTimeOfDay)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void AddPurchase(Purchase newPurchase)
        {
            throw new NotImplementedException();
        }

        public List<Purchase> GetReportByCountryAndDates(ReportDate newReportDate)
        {
            throw new NotImplementedException();
        }

        public List<Purchase> GetReportByLicensePlate(string licensePlate)
        {
            throw new NotImplementedException();
        }

        public int AmountOfReservations()
        {
            throw new NotImplementedException();
        }
    }
}