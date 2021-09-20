
using BusinessLogic.Exceptions;
using BusinessLogic.Lists;
using BusinessLogic.Models;
using BusinessLogic.Persistence;
using System;
using System.Collections.Generic;

namespace BusinessLogic.Controllers {
    public class ScoringController {

        private static ScoringController instance;
        private static TeamController teamController;
        private ScoringList scorings;
        private Scoring scoring;

        private ScoringController() {
            scorings = new ScoringList();
            teamController = TeamController.GetInstance();
        }

        public void CreateScoring() {
            scoring = new Scoring();
            scoring.Id = 1;
            scorings.Add(scoring);
        }
        public void Initialize() {
            scorings.LoadAll();
            scoring = scorings[scorings.Count - 1];
        }

        public static ScoringController GetInstance() {
            if (instance == null) {
                instance = new ScoringController();
            }
            return instance;
        }

        private void UpdateScoring() {
            this.scorings.Update(this.scoring);
        }

        public void SetDeleteBoardScore(int score) {
            this.scoring.DeleteBoardScore = score;
            UpdateScoring();
        }

        public void SetAddElementScore(int score) {
            this.scoring.AddElementScore = score;
            UpdateScoring();
        }

        public void SetResolveCommentScore(int score) {
            this.scoring.ResolveCommentScore = score;
            UpdateScoring();
        }

        public void SetCreateBoardScore(int score) {
            this.scoring.CreateBoardScore = score;
            UpdateScoring();
        }

        public void SetAddCommentScore(int score) {
            this.scoring.AddCommentScore = score;
            UpdateScoring();
        }

        public int GetCreateBoardScore() {
            return this.scoring.CreateBoardScore;
        }

        public int GetDeleteBoardScore() {
            return this.scoring.DeleteBoardScore;
        }

        public int GetAddElementScore() {
            return this.scoring.AddElementScore;
        }

        public int GetAddCommentScore() {
            return this.scoring.AddCommentScore;
        }

        public int GetResolveCommentScore() {
            return this.scoring.ResolveCommentScore;
        }

        public void ResetScores(int teamId) {
            teamController.ResetScore(teamId);
        }

        public int GetUserScore(int teamId, int userId) {
            Team team = teamController.FindTeam(teamId);
            foreach (var userScore in team.Scores) {
                if (userScore.User.Id == userId) {
                    return userScore.Score;
                }
            }
            return 0;
        }

        internal void AddCreateBoardPoints(Team team, User owner) {
            teamController.AddPointsToUser(team, owner, scoring.CreateBoardScore);
        }
        internal void AddDeleteBoardPoints(Team team, User owner) {
            teamController.AddPointsToUser(team, owner, scoring.DeleteBoardScore);
        }
        internal void AddElementPoints(Team team, User owner) {
            teamController.AddPointsToUser(team, owner, scoring.AddElementScore);
        }
        internal void AddCommentPoints(Team team, User owner) {
            teamController.AddPointsToUser(team, owner, scoring.AddCommentScore);
        }
        internal void AddCommentResolvedPoints(Team team, User owner) {
            teamController.AddPointsToUser(team, owner, scoring.ResolveCommentScore);
        }

        public List<UserScore> GetTeamScores(Team team) {
            return teamController.GetTeamScores(team);
        }
    }
}