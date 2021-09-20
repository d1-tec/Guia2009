using BusinessLogic.Data;
using BusinessLogic.Exceptions;
using BusinessLogic.Lists;
using BusinessLogic.Models;
using BusinessLogic.Persistence;
using BusinessLogic.Utils;
using System;
using System.Collections.Generic;

namespace BusinessLogic.Controllers {
    public class TeamController {

        public const int MIN_ID = 1;

        private int lastId = TeamController.MIN_ID - 1;
        private TeamList Teams { get; set; }

        private static TeamController instance = null;
        public static TeamController GetInstance() {
            if (instance == null) {
                instance = new TeamController();
            }
            return instance;
        }
        private TeamController() {
            this.Teams = new TeamList();
        }

        public void Initialize() {
            Teams.LoadAll();
        }

        public List<Model> GetModelList() {
            return new List<Model>(Teams);
        }
        public List<Team> GetList() {
            return new List<Team>(Teams);
        }

        private int GetNewId() {
            this.lastId++;
            return this.lastId;
        }

        private bool ExceptionHasErrors(TeamInvalidException exception) {
            return exception.Fields.Count > 0;
        }
        private void ThrowExceptionIfHasError(TeamInvalidException exception) {
            if (ExceptionHasErrors(exception)) {
                throw exception;
            }
        }

        public void ResetScore(int teamId) {
            Team team = this.FindTeam(teamId);
            if (team == null) {
                throw new ScoringInvalidException();
            }
            team.ResetScore();
        }

        private void ValidateIsAdministrator() {
            SessionController sessionController = SessionController.GetInstance();
            if (!sessionController.IsAdministrator()) {
                throw new ForbiddenException();
            }
        }
        private void ValidateRequiredAttributes(Team team, TeamInvalidException exception) {
            if (StringUtils.IsEmpty(team.Name)) {
                exception.Fields.Add(TeamInvalidException.FIELD_NAME, TeamInvalidException.ERROR_REQUIRED);
            }
        }
        private void ValidateFormatAttributes(Team team, TeamInvalidException exception) {
            if (team.Size <= 0) {
                exception.Fields.Add(TeamInvalidException.FIELD_SIZE, TeamInvalidException.ERROR_FORMAT);
            }
        }

        internal void AddPointsToUser(Team team, User user, int points) {
            List<UserScore> scores = team.Scores;
            bool userFound = false;
            for (int i = 0; i < scores.Count && !userFound; i++) {
                if (scores[i].User.Equals(user)) {
                    scores[i].Score += points;
                    userFound = true;
                }
            }
            if (!userFound) {
                UserScore newUserScore = new UserScore();
                newUserScore.User = user;
                newUserScore.Score = points;
                scores.Add(newUserScore);
            }
        }

        public List<Team> GetMyTeams() {
            User user = SessionController.GetInstance().GetLoggedInUser();
            List<Team> myTeams = new List<Team>();
            foreach (Team team in this.Teams) {
                if (team.UserExists(user)) {
                    myTeams.Add(team);
                }
            }
            return myTeams;
        }

        private static void ValidateMinimumUsers(Team team, TeamInvalidException exception) {
            if (team.Users.Count < 1) {
                exception.Fields.Add(TeamInvalidException.FIELD_USERS, TeamInvalidException.ERROR_REQUIRED);
            }
        }

        internal List<UserScore> GetTeamScores(Team team) {
            return team.Scores;
        }

        private static void ValidateNoDuplicateUsers(Team team, TeamInvalidException exception) {
            foreach (User user in team.Users) {
                List<User> foundUsers = team.Users.FindAll(compared => user.Equals(compared));
                if (foundUsers.Count > 1) {
                    exception.Fields.Add(TeamInvalidException.FIELD_USERS, TeamInvalidException.ERROR_DUPLICATE);
                    return;
                }
            }
        }
        private static void ValidateUsersExist(Team team, TeamInvalidException exception) {
            foreach (User user in team.Users) {
                User foundUser = UserController.GetInstance().GetUser(user.Id);
                if (foundUser == null) {
                    exception.Fields.Add(TeamInvalidException.FIELD_USERS, TeamInvalidException.ERROR_NOT_EXISTS);
                    return;
                }
            }
        }
        private void ValidateUsers(Team team, TeamInvalidException exception) {
            ValidateMinimumUsers(team, exception);
            ValidateNoDuplicateUsers(team, exception);
            ValidateUsersExist(team, exception);
        }
        private void ValidateTeamExists(Team team, TeamInvalidException exception) {
            Team foundTeam = this.Teams.Find(teamInList => team.Equals(teamInList));
            if (foundTeam == null) {
                exception.Fields.Add(TeamInvalidException.FIELD_ID, TeamInvalidException.ERROR_NOT_EXISTS);
            }
        }
        private void ValidateTeamUserNotExists(Team team, User user, TeamInvalidException exception) {
            if (team.UserExists(user)) {
                exception.Fields.Add(TeamInvalidException.FIELD_USERS, TeamInvalidException.ERROR_DUPLICATE);
            }
        }
        private void ValidateTeamIsNotFull(Team team, TeamInvalidException exception) {
            if (team.IsFull()) {
                exception.Fields.Add(TeamInvalidException.FIELD_USERS, TeamInvalidException.ERROR_FULL);
            }
        }
        private void ValidateTeamUserExists(Team team, User user, TeamInvalidException exception) {
            if (!team.UserExists(user)) {
                exception.Fields.Add(TeamInvalidException.FIELD_USERS, TeamInvalidException.ERROR_NOT_EXISTS);
            }
        }
        private void ValidateTeamUserNotMinimumNumber(Team team, User user, TeamInvalidException exception) {
            if (!team.HasMoreThanMinimumUsers()) {
                exception.Fields.Add(TeamInvalidException.FIELD_USERS, TeamInvalidException.ERROR_FORMAT);
            }
        }

        private void ValidateTeamForAddition(Team team) {
            TeamInvalidException exception = new TeamInvalidException();
            ValidateRequiredAttributes(team, exception);
            ValidateFormatAttributes(team, exception);
            ValidateUsers(team, exception);
            ThrowExceptionIfHasError(exception);
        }
        private void ValidateTeamForDeletion(Team team) {
            TeamInvalidException exception = new TeamInvalidException();
            ValidateTeamExists(team, exception);
            ThrowExceptionIfHasError(exception);
        }
        private void ValidateTeamForUpdate(Team team) {
            TeamInvalidException exception = new TeamInvalidException();
            ValidateTeamExists(team, exception);
            ValidateRequiredAttributes(team, exception);
            ValidateFormatAttributes(team, exception);
            ThrowExceptionIfHasError(exception);
        }
        private void ValidateTeamUserForAddition(Team team, User user) {
            TeamInvalidException exception = new TeamInvalidException();
            ValidateTeamUserNotExists(team, user, exception);
            ValidateTeamIsNotFull(team, exception);
            ThrowExceptionIfHasError(exception);
        }
        private void ValidateTeamUserForDeletion(Team team, User user) {
            TeamInvalidException exception = new TeamInvalidException();
            ValidateTeamUserExists(team, user, exception);
            if (!ExceptionHasErrors(exception)) {
                ValidateTeamUserNotMinimumNumber(team, user, exception);
            }
            ThrowExceptionIfHasError(exception);
        }

        public void AddTeam(Team team) {
            ValidateIsAdministrator();
            ValidateTeamForAddition(team);
            team.Id = GetNewId();
            this.Teams.Add(team);

            Database.Instance.Add("Teams", team);
        }
        public void DeleteTeam(Team team) {
            ValidateIsAdministrator();
            ValidateTeamForDeletion(team);
            int numberRemoved = this.Teams.RemoveAll(teamInList => teamInList.Equals(team));
            if (numberRemoved > 0) {
                team.Id = 0;
            }
        }
        public void UpdateTeam(Team team) {
            ValidateIsAdministrator();
            ValidateTeamForUpdate(team);
            Team foundTeam = this.Teams.Find(teamInList => teamInList.Equals(team));
            foundTeam.CopyFrom(team);
            Teams.Update(foundTeam);
        }
        public Team FindTeam(int id) {
            return this.Teams.Find(team => team.Id == id);
        }
        public void AddTeamUser(Team team, User user) {
            ValidateIsAdministrator();
            ValidateTeamUserForAddition(team, user);
            Team foundTeam = this.Teams.Find(teamInList => teamInList.Equals(team));
            team.AddUser(user);
        }
        public bool DeleteTeamUser(Team team, User user) {
            ValidateIsAdministrator();
            ValidateTeamUserForDeletion(team, user);
            return team.DeleteUser(user);
        }
        
    }
}