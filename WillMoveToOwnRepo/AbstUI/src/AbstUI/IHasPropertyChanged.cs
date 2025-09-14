using System.ComponentModel;

namespace AbstUI;

public interface IHasPropertyChanged
{
    event PropertyChangedEventHandler? PropertyChanged;
}

