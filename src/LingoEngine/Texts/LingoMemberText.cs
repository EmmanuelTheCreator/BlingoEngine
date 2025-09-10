
using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.Texts
{
    public class LingoMemberText : LingoMemberTextBase<ILingoFrameworkMemberText>, ILingoMemberText
    {
        public LingoMemberText(LingoCast cast, ILingoFrameworkMemberText frameworkMember, int numberInCast, IAbstComponentFactory componentFactory, string name = "", string fileName = "", APoint regPoint = default) : base(LingoMemberType.Text,cast, frameworkMember, numberInCast, componentFactory, name, fileName, regPoint)
        {
        }

        protected override LingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException();
            //var clone = new LingoMemberText(_cast, _lingoFrameworkMember, newNumber, Name);
            //clone.Text = Text;
            //return clone;
        }
        /// <summary>
      
       

      
    }

}

 

