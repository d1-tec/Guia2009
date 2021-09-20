using BusinessLogic.Exceptions;
using BusinessLogic.Lists;
using BusinessLogic.Models;
using BusinessLogic.Persistence;
using System;
using System.Collections.Generic;

namespace BusinessLogic.Controllers {
    public class BoardController {
        private static BoardList boards;
        private static BoardController instance = null;
        private static ScoringController scoringController;
        private UserController userController;
        private int idCounter;
        private int idElementCounter;
        private const int MIN_ID = 1;
        private const string TYPE_ADMINISTRATOR = "Administrator";
        private const string TYPE_USER = "User";

        private BoardController() {
            userController = UserController.GetInstance();
            scoringController = ScoringController.GetInstance();
            boards = new BoardList();
            idCounter = MIN_ID - 1;
            idElementCounter = MIN_ID - 1;
        }

        public static BoardController GetInstance() {
            if (instance == null) {
                instance = new BoardController();
            }
            return instance;
        }

        public void Initialize() {
            boards.LoadAll();
        }

        public List<Model> GetModelList() {
            return new List<Model>(boards);
        }
        public List<Board> GetList() {
            return new List<Board>(boards);
        }

        public bool IsLoggedInUserOwner(Board board) {
            bool isOwner = true;
            try {
                ValidateLoggedUserIsOwner(board);
            } catch (BoardInvalidException) {
                isOwner = false;
            }
            return isOwner;
        }
        public bool IsUserMember(Board board, User user) {
            return board.Team.Users.Find(userInList => userInList.Equals(user)) != null;
        }
        public bool IsLoggedInUserMember(Board board) {
            User user = SessionController.GetInstance().GetLoggedInUser();
            return IsUserMember(board, user);
        }
        public bool IsLoggedInUserInBoard(Board board) {
            return IsLoggedInUserOwner(board) || IsLoggedInUserMember(board);
        }
        public List<Board> GetListForLoggedInUser() {
            SessionController sesionController = SessionController.GetInstance();
            List<Board> list;
            if (sesionController.IsAdministrator()) {
                list = GetList();
            } else {
                list = new List<Board>();
                foreach (Board board in boards) {
                    if (IsLoggedInUserInBoard(board)) {
                        list.Add(board);
                    }
                }
            }
            return list;
        }
        public List<Model> GetModelListForLoggedInUser() {
            return new List<Model>(GetListForLoggedInUser());
        }

        private void ValidateFields(Board board) {
            BoardInvalidException exception = new BoardInvalidException();
            if (board.Name.Length == 0) {
                exception.Fields.Add(BoardInvalidException.ERROR_REQUIRED, BoardInvalidException.FIELD_NAME);
            }
            if (board.Owner == null) {
                exception.Fields.Add(BoardInvalidException.ERROR_REQUIRED, BoardInvalidException.FIELD_OWNER);
            }
            ThrowExceptionIfHasError(exception);
        }
        private void ThrowExceptionIfHasError(BoardInvalidException exception) {
            if (exception.Fields.Count > 0) {
                throw exception;
            }
        }

        private void ValidateIsAdministrator() {
            if (!SessionController.GetInstance().IsAdministrator()) {
                throw new ForbiddenException();
            }
        }

        private void AddBoard(Board board) {
            board.Id = idCounter + 1;
            idCounter++;
            if (board.Team == null) {
                BoardInvalidException noTeamException = new BoardInvalidException();
                noTeamException.Fields.Add(BoardInvalidException.ERROR_REQUIRED, BoardInvalidException.FIELD_TEAM);
                throw noTeamException;
            }
            scoringController.AddCreateBoardPoints(board.Team, board.Owner);
            boards.Add(board);
        }

        public void CreateBoard(Board board) {
            ValidateIsAdministrator();
            ValidateFields(board);
            AddBoard(board);
        }

        public int GetBoardCount() {
            return boards.Count;
        }

        private void ValidateBoardExists(Board board) {
            if (GetBoard(board.Id) == null) {
                BoardInvalidException exception = new BoardInvalidException();
                exception.Fields.Add(BoardInvalidException.FIELD_BOARD, BoardInvalidException.ERROR_NOT_EXISTS);
                throw exception;
            }
        }

        public void DeleteBoard(Board board) {

            ValidateBoardExists(board);
            ValidateLoggedInUserPermissions(board);
            scoringController.AddDeleteBoardPoints(board.Team, board.Owner);
            boards.Remove(board);
        }

        private void ValidateLoggedInUserPermissions(Board board) {
            Exception exception = null;
            bool userHasPermitions = false;
            try {
                ValidateIsAdministrator();
                userHasPermitions = true;
            } catch (Exception e) {
                exception = e;
            }
            try {
                ValidateLoggedUserIsOwner(board);
                userHasPermitions = true;
            } catch (Exception e) {
                exception = e;
            }
            if (!userHasPermitions) {
                throw exception;
            }
        }

        private void ValidateLoggedUserIsOwner(Board board) {
            User loggedInUser = SessionController.GetInstance().GetLoggedInUser();
            if (!board.Owner.Equals(loggedInUser)) {
                BoardInvalidException exception = new BoardInvalidException();
                exception.Fields.Add(BoardInvalidException.FIELD_BOARD, BoardInvalidException.ERROR_NOT_ENOUGH_PERMISSION);
                throw exception;
            }
        }

        private Board GetBoard(Board id) {
            return boards.Find(brd => brd.Id.Equals(id));
        }

        public void UpdateBoard(Board board) {
            ValidateLoggedInUserPermissions(board);
            ValidateFields(board);
            ValidateBoardExists(board);
            Board existingBoard = GetBoard(board.Id);
            existingBoard.CopyFrom(board);
            boards.Update(existingBoard);
        }

        public Board GetBoard(int id) {
            return boards.Find(brd => brd.Id.Equals(id));
        }

        public void AddBoardElement(Board board, BoardElement element) {
            element.ValidateElement();
            element.Id = idElementCounter + 1;
            idElementCounter++;
            scoringController.AddElementPoints(board.Team, board.Owner);
            board.AddElement(element);
        }
        public void AddBoardElement(Board board, BoardElement element, User user) {
            element.ValidateElement();
            element.Id = idElementCounter + 1;
            idElementCounter++;
            scoringController.AddElementPoints(board.Team, user);
            board.AddElement(element);
        }

        public void UpdateBoardElement(Board board, BoardElement newElement) {
            ValidateBoardExists(board);
            board.UpdateElement(newElement);
        }

        public BoardElement GetElementFromBoard(Board board, int elementId) {
            ValidateBoardExists(board);
            board.ValidateElementExists(elementId);
            return board.GetElement(elementId);
        }

        public int GetElementCount(Board board) {
            ValidateBoardExists(board);
            return board.GetElementCount();
        }

        public void RemoveBoardElement(Board board, BoardElement element) {
            ValidateBoardExists(board);
            board.ValidateElementExists(element.Id);
            boards.Find(brd => brd.Equals(board)).RemoveElement(element);
            board.RemoveElement(element);
        }
        public Board GetBoardOfElement(BoardElement element) {
            if (element.Id > 0) {
                return boards.Find(board => (board.GetElement(element.Id) != null));
            }
            return null;
        }

        public bool IsBoardElementValid(BoardElement element) {
            if (element != null && element.Id > 0) {
                return GetBoardOfElement(element) != null;
            }
            return false;

        }
    }
}