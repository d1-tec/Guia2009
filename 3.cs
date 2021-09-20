using BusinessLogic.Exceptions;
using BusinessLogic.Models;
using BusinessLogic.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Controllers {
    public class SessionController {

        private User User { get; set; }

        private static SessionController instance = null;
        public static SessionController GetInstance() {
            if (instance == null) {
                instance = new SessionController();
            }
            return instance;
        }
        private SessionController() {
        }

        private bool ExceptionHasErrors(SessionInvalidException exception) {
            return exception.Fields.Count > 0;
        }
        private void ThrowExceptionIfHasError(SessionInvalidException exception) {
            if (ExceptionHasErrors(exception)) {
                throw exception;
            }
        }

        private void ValidateSessionNotExists(SessionInvalidException exception) {
            if (IsLoggedIn()) {
                exception.Fields.Add(SessionInvalidException.FIELD_SESSION, SessionInvalidException.ERROR_EXISTS);
            }
        }
        private void ValidateCredentials(string email, string password, SessionInvalidException exception) {
            if (!StringUtils.IsValidEmail(email)) {
                exception.Fields.Add(SessionInvalidException.FIELD_EMAIL, SessionInvalidException.ERROR_INVALID);
            }
            if (!StringUtils.IsValidPassword(password)) {
                exception.Fields.Add(SessionInvalidException.FIELD_PASSWORD, SessionInvalidException.ERROR_INVALID);
            }
        }
        private User ValidateCredentialsExist(string email, string password, SessionInvalidException exception) {
            User user = UserController.GetInstance().GetUserByCredentials(email, password);
            if (user == null) {
                exception.Fields.Add(SessionInvalidException.FIELD_USER, SessionInvalidException.ERROR_NOT_EXISTS);
            }
            return user;
        }
        private void ValidateSessionExists(SessionInvalidException exception) {
            if (!IsLoggedIn()) {
                exception.Fields.Add(SessionInvalidException.FIELD_SESSION, SessionInvalidException.ERROR_NOT_EXISTS);
            }
        }

        private void ValidateLogIn(User user) {
            SessionInvalidException exception = new SessionInvalidException();
            ValidateSessionNotExists(exception);
            ThrowExceptionIfHasError(exception);
        }
        private User GetValidUserWithCredentials(string email, string password) {
            SessionInvalidException exception = new SessionInvalidException();
            ValidateCredentials(email, password, exception);
            User user = ValidateCredentialsExist(email, password, exception);
            ThrowExceptionIfHasError(exception);
            return user;
        }
        private void ValidateLogOut() {
            SessionInvalidException exception = new SessionInvalidException();
            ValidateSessionExists(exception);
            ThrowExceptionIfHasError(exception);
        }

        public User GetLoggedInUser() {
            return this.User;
        }
        public bool IsLoggedIn() {
            return this.User != null;
        }
        public bool IsAdministrator() {
            return IsLoggedIn() && this.User.IsAdministrator();
        }
        public bool IsCollaborator() {
            return IsLoggedIn() && this.User.IsCollaborator();
        }
        public bool IsCollaboratorStrict() {
            return IsLoggedIn() && this.User.IsCollaboratorStrict();
        }

        public void LogInUser(User user) {
            ValidateLogIn(user);
            this.User = user;
        }
        public User LogIn(string email, string password) {
            User user = GetValidUserWithCredentials(email, password);
            LogInUser(user);
            return user;
        }
        public void LogOut() {
            ValidateLogOut();
            this.User = null;
        }

    }
}