using LingoEngine.Core;
using LingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LingoEngine.VerboseLanguage
{
    public interface ILingoVerbosePutInto
    {
        void Field(string memberName, string? castlibName = null);
        void Field(string memberName, int castlib);
    }
    /// <summary>
    /// put round into field "round"
    /// </summary>
    public interface ILingoVerbosePut
    {
        ILingoVerbosePutInto Into { get; }
        void ToLog();
    }
    /// <inheritdoc/>
    public record LingoVerbosePut : LingoVerboseBase, ILingoVerbosePut, ILingoVerbosePutInto
    {
        private object? _value;

        public LingoVerbosePut(LingoPlayer player, object? value)
            : base(player)
        {
            _value = value;
        }

        public ILingoVerbosePutInto Into => this;

        public void Field(string memberName, string? castlibName = null) => DoOnMember<ILingoMemberTextBase>(memberName, castlibName, field => field.Text = _value?.ToString() ?? "");
       
        public void Field(string memberName, int castlib) => DoOnMember<ILingoMemberTextBase>(memberName, castlib, field => field.Text = _value?.ToString() ?? "");

        public void ToLog() => _player.ServiceProvider.GetRequiredService<ILogger<LingoVerbosePut>>().LogInformation("Put: {Value}", _value?.ToString() ?? "null");


        
    }


   
}



