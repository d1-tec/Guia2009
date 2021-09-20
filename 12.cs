using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.SMSExtract;
using BusinessLogic.SwitchCountryValidator;
using BusinessLogic.UserFolder;
using BusinessLogic.Validators;
using WindowsFormsApp1;

namespace BusinessLogic.ReservationFolder
{
    public class Reservation : SharedNumberValidator
    {
        [Key]
        public int ReservationID { get; set; }
        [Required]
        public string LicensePlate { get; set; }
        [Required]
        public int Minutes { get; set; }
        [Required]
        public string StartTime { get; set; }
        [Required]
        public string EndTime { get; set; }
        [Required]
        public string CountryName{ get; set; }
        
        public string PhoneNumber { get; set; }

        [NotMapped]
        public SwitchExtractDataSMS SMSReservation { get; set; }
        [NotMapped]
        public ITimeProvider Provider { get; set; }
        [NotMapped]
        private string[] SMSFields;
        [NotMapped]
        public int MaxHourValidForReservation;
        [NotMapped]
        public int MaxMinutesHourForReservation;

        public Reservation()
        {

        }
        

        public Reservation(string phoneNumberUser, string SMSInput, ITimeProvider provider)
        {
            this.Provider = provider;
            this.PhoneNumber = phoneNumberUser;
            this.CountryName = SelectedCountry.CurrentCountry.CountryName;
            this.MaxHourValidForReservation = SelectedCountry.CurrentCountry.TimeLimitHours;
            this.MaxMinutesHourForReservation = SelectedCountry.CurrentCountry.TimeLimitMinutes;
            GenerateReservation(SMSInput);
        }

        private void GenerateReservation(string SMSInput)
        {
            this.SMSFields = SMSInput.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);           
            SMSReservation = new SwitchExtractDataSMS(SMSFields);
            GetReservationWithThisSMSFields();

        }

        private void GetReservationWithThisSMSFields()
        {
            this.LicensePlate = this.SMSReservation.LicensePlateLetters + this.SMSReservation.LicensePlateNumbers;
            this.LicensePlate = this.LicensePlate.ToUpper();
            this.Minutes = Int32.Parse(this.SMSReservation.ParkingMinutes);
            if (this.SMSReservation.StartTimeParking == null)
            {
                this.StartTime = GetCurrentTime();
            }
            else
            {
                this.StartTime = this.SMSReservation.StartTimeParking;
            }
            SetEndTimeReservation();
        }

        private string GetCurrentTime()
        {
            return this.Provider.Now();
        }

        private void SetEndTimeReservation()
        {
            int minutesOfHourReservation = ConvertStringToInt(this.StartTime,3, 2);
            int hourOfReservation = ConvertStringToInt(this.StartTime, 0, 2);
            int totalMinutes = minutesOfHourReservation + this.Minutes;
            while (totalMinutes >= 60 && hourOfReservation < this.MaxHourValidForReservation)
            {
                totalMinutes = totalMinutes - 60;
                hourOfReservation = hourOfReservation + 1;
            }
            minutesOfHourReservation = totalMinutes;

            if (hourOfReservation >= MaxHourValidForReservation && minutesOfHourReservation >= MaxMinutesHourForReservation)
            {
                
                this.EndTime = SelectedCountry.CurrentCountry.TimeLimitReservation;
                UpdateReservationMinutes();
            }
            else {

                string hourEndTimeReservation = hourOfReservation.ToString();
                string minutesEndTimeReservation = minutesOfHourReservation.ToString();

                if (minutesEndTimeReservation.Length == 1)
                {
                    minutesEndTimeReservation = "0" + minutesEndTimeReservation;
                }
                string endTimeReservation = hourEndTimeReservation + ":" + minutesEndTimeReservation;
                this.EndTime = endTimeReservation;
            }
                       
        }

        private int GetMinutesHourOfReservationAsAInt()
        {
            int minutesOfHourReservation = Int32.Parse(this.StartTime.Substring(3, 2));
            return minutesOfHourReservation;
        }

        private int GetHourOfReservationAsAInt()
        {
            int hourOfReservation = Int32.Parse(this.StartTime.Substring(0, 2));
            return hourOfReservation;
        }

        private void UpdateReservationMinutes()
        {
            int minutesOfHourReservation = GetMinutesHourOfReservationAsAInt();
            int hourOfReservation = GetHourOfReservationAsAInt();
            int amountOfHours = MaxHourValidForReservation - hourOfReservation;
            if (amountOfHours == 1)
            {
                this.Minutes = 60 - minutesOfHourReservation;
            }
            else
            {
                this.Minutes = amountOfHours * 60;
                if (minutesOfHourReservation > 0)
                {
                    this.Minutes = this.Minutes - minutesOfHourReservation;
                }
                
            }

        }

        public bool IsOutOfTime(string currentTime)
        {
            
            int hourCurrentTime = ConvertStringToInt(currentTime, 0, 2);
            int minutesCurrentTime = ConvertStringToInt(currentTime, 3, 2);
            int endHourReservation = ConvertStringToInt(this.EndTime, 0, 2);
            int endMinutesReservation = ConvertStringToInt(this.EndTime, 3, 2);
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

        public bool IsActiveOnThatTime(string timeOfDay)
        {
            string endTimeReservation = this.EndTime;

            int hourTimeOfDay = ConvertStringToInt(timeOfDay, 0, 2);
            int minutesTimeOfDay = ConvertStringToInt(timeOfDay, 3, 2);
            if (TimeOfDayIsEarlierThanStartTimeReservation(hourTimeOfDay, minutesTimeOfDay))
            {
                return false;
            }
            else
            {
                int hourEndTimeReservation = ConvertStringToInt(endTimeReservation, 0, 2);
                int minutesEndTimeReservation = ConvertStringToInt(endTimeReservation, 3, 2);
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
            
        }

        private bool TimeOfDayIsEarlierThanStartTimeReservation(int hourTimeOfDay, int minutesTimeOfDay)
        {
            string startTimeReservation = this.StartTime;
            int hourStartTimeReservation = ConvertStringToInt(startTimeReservation, 0, 2);
            int minutesStartTimeReservation = ConvertStringToInt(startTimeReservation, 3, 2);
            if (hourTimeOfDay < hourStartTimeReservation)
            {
                return true;
            }
            else if (hourTimeOfDay > hourStartTimeReservation)
            {
                return false;
            }
            else
            {
                if (minutesTimeOfDay >= minutesStartTimeReservation)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            
        }


    }
}