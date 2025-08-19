using System;
namespace AbstUI.Commands
{
    public interface IHistoryManager
    {
        void Push(Action undoAction, Action redoAction);
        void Undo();
        void Redo();
        bool CanUndo { get; }
        bool CanRedo { get; }
    }
}
