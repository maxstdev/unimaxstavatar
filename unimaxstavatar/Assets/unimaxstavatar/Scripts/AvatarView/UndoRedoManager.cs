using System;
using System.Collections.Generic;
using UnityEngine;

namespace Maxst.Avatar
{
    public class UndoRedoManager
    {
        private Stack<ICommand> undoStack;
        private Stack<ICommand> redoStack;

        private Action ExecuteCommandAction;

        public UndoRedoManager()
        {
            undoStack = new Stack<ICommand>();
            redoStack = new Stack<ICommand>();
        }

        public void SetExecuteCommandAction(Action action)
        {
            ExecuteCommandAction = action;
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
            ExecuteCommandAction?.Invoke();
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                ICommand command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
            }
            else
            {
                Debug.Log("It is not an undo operation.");
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                ICommand command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
            }
            else
            {
                Debug.Log("There is no action to redo.");
            }
        }

        public int UndoCount()
        {
            return undoStack.Count;
        }

        public void ClearUndoStack()
        {
            undoStack.Clear();
        }
    }
}