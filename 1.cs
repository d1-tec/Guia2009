using BusinessLogic.Exceptions;
using BusinessLogic.Lists;
using BusinessLogic.Models;
using BusinessLogic.Persistence;
using BusinessLogic.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Controllers {
    public class CommentController {

        public const int MIN_ID = 1;

        private int lastId = TeamController.MIN_ID - 1;

        public CommentList Comments { get; set; }

        private static CommentController instance = null;
        private static ScoringController scoringController;
        public static CommentController GetInstance() {
            if (instance == null) {
                instance = new CommentController();
            }
            return instance;
        }
        private CommentController() {
            this.Comments = new CommentList();
            scoringController = ScoringController.GetInstance();
        }

        public void Initialize() {
            Comments.LoadAll();
        }

        public List<Model> GetModelList() {
            return new List<Model>(Comments);
        }
        public List<Comment> GetList() {
            return new List<Comment>(this.Comments);
        }

        private int GetNewId() {
            this.lastId++;
            return this.lastId;
        }

        private bool ExceptionHasErrors(CommentInvalidException exception) {
            return exception.Fields.Count > 0;
        }
        private void ThrowExceptionIfHasError(CommentInvalidException exception) {
            if (ExceptionHasErrors(exception)) {
                throw exception;
            }
        }
        private void ValidateIsCollaborator() {
            SessionController sessionController = SessionController.GetInstance();
            if (!sessionController.IsCollaborator()) {
                throw new ForbiddenException();
            }
        }
        private void ValidateRequiredAttributes(Comment comment, CommentInvalidException exception) {
            if (StringUtils.IsEmpty(comment.Text)) {
                exception.Fields.Add(CommentInvalidException.FIELD_TEXT, CommentInvalidException.ERROR_REQUIRED);
            }
        }
        private void ValidateElementExists(Comment comment, CommentInvalidException exception) {
            BoardController boardController = BoardController.GetInstance();
            if (!boardController.IsBoardElementValid(comment.BoardElement)) {
                exception.Fields.Add(CommentInvalidException.FIELD_ELEMENT, CommentInvalidException.ERROR_NOT_EXISTS);
            }
        }
        private void ValidateUserExists(User user, CommentInvalidException exception, string field) {
            UserController userController = UserController.GetInstance();
            if (user == null || userController.GetUser(user.Id) == null) {
                exception.Fields.Add(field, CommentInvalidException.ERROR_NOT_EXISTS);
            }
        }
        private void ValidateCreatorExists(Comment comment, CommentInvalidException exception) {
            ValidateUserExists(comment.Creator, exception, CommentInvalidException.FIELD_CREATOR);
        }
        private void ValidateResolverExists(User resolver, CommentInvalidException exception) {
            ValidateUserExists(resolver, exception, CommentInvalidException.FIELD_RESOLVER);
        }
        private void ValidateCommentExists(Comment comment, CommentInvalidException exception) {
            if (comment.Id < 1 || FindComment(comment.Id) == null) {
                exception.Fields.Add(CommentInvalidException.FIELD_COMMENT, CommentInvalidException.ERROR_NOT_EXISTS);
            }
        }

        private void ValidateCommentForAddition(Comment comment) {
            CommentInvalidException exception = new CommentInvalidException();
            ValidateRequiredAttributes(comment, exception);
            ValidateElementExists(comment, exception);
            ValidateCreatorExists(comment, exception);
            ThrowExceptionIfHasError(exception);
        }
        private void ValidateCommentForResolution(Comment comment, User resolver, DateTime dateResolved) {
            CommentInvalidException exception = new CommentInvalidException();
            ValidateCommentExists(comment, exception);
            ValidateResolverExists(resolver, exception);
            ThrowExceptionIfHasError(exception);
        }

        public Comment FindComment(int id) {
            return this.Comments.Find(comment => comment.Id == id);
        }
        public void CreateComment(Comment comment) {
            ValidateIsCollaborator();
            ValidateCommentForAddition(comment);
            comment.Id = GetNewId();
            this.Comments.Add(comment);
        }
        public void CreateComment(Team team, Comment comment) {
            ValidateIsCollaborator();
            ValidateCommentForAddition(comment);
            comment.Id = GetNewId();
            scoringController.AddCommentPoints(team, comment.Creator);
            this.Comments.Add(comment);
        }
        public void ResolveComment(Comment comment, User resolver, DateTime dateResolved) {
            ValidateIsCollaborator();
            ValidateCommentForResolution(comment, resolver, dateResolved);
            Comment foundComment = FindComment(comment.Id);
            comment.Resolve(resolver, dateResolved);
            foundComment.Resolve(resolver, dateResolved);
            Comments.Update(foundComment);
        }
        public void ResolveComment(Team team, Comment comment, User resolver, DateTime dateResolved) {
            ValidateIsCollaborator();
            ValidateCommentForResolution(comment, resolver, dateResolved);
            Comment foundComment = FindComment(comment.Id);
            scoringController.AddCommentResolvedPoints(team, resolver);
            comment.Resolve(resolver, dateResolved);
            foundComment.Resolve(resolver, dateResolved);
            Comments.Update(foundComment);
        }
    }
}

