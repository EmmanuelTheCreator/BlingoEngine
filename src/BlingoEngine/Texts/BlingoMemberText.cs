
using AbstUI.Components;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Texts.FrameworkCommunication;

namespace BlingoEngine.Texts
{
    public class BlingoMemberText : BlingoMemberTextBase<IBlingoFrameworkMemberText>, IBlingoMemberText
    {
        public BlingoMemberText(BlingoCast cast, IBlingoFrameworkMemberText frameworkMember, int numberInCast, IAbstComponentFactory componentFactory, string name = "", string fileName = "", APoint regPoint = default) : base(BlingoMemberType.Text,cast, frameworkMember, numberInCast, componentFactory, name, fileName, regPoint)
        {
        }

        protected override BlingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException();
            //var clone = new BlingoMemberText(_cast, _blingoFrameworkMember, newNumber, Name);
            //clone.Text = Text;
            //return clone;
        }
        /// <summary>
      
       

      
    }

}

 


