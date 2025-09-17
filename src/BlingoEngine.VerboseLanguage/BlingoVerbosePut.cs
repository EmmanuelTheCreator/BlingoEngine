using BlingoEngine.Core;
using BlingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.VerboseLanguage
{
    public interface IBlingoVerbosePutInto
    {
        void Field(string memberName, string? castlibName = null);
        void Field(string memberName, int castlib);
    }
    /// <summary>
    /// put round into field "round"
    /// </summary>
    public interface IBlingoVerbosePut
    {
        IBlingoVerbosePutInto Into { get; }
        void ToLog();
    }
    /// <inheritdoc/>
    public record BlingoVerbosePut : BlingoVerboseBase, IBlingoVerbosePut, IBlingoVerbosePutInto
    {
        private object? _value;

        public BlingoVerbosePut(BlingoPlayer player, object? value)
            : base(player)
        {
            _value = value;
        }

        public IBlingoVerbosePutInto Into => this;

        public void Field(string memberName, string? castlibName = null) => DoOnMember<IBlingoMemberTextBase>(memberName, castlibName, field => field.Text = _value?.ToString() ?? "");
       
        public void Field(string memberName, int castlib) => DoOnMember<IBlingoMemberTextBase>(memberName, castlib, field => field.Text = _value?.ToString() ?? "");

        public void ToLog() => _player.ServiceProvider.GetRequiredService<ILogger<BlingoVerbosePut>>().LogInformation("Put: {Value}", _value?.ToString() ?? "null");


        
    }


   
}




