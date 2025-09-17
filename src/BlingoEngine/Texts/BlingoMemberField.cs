
using AbstUI.Components;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Texts.FrameworkCommunication;

namespace BlingoEngine.Texts
{

    public class BlingoMemberField : BlingoMemberTextBase<IBlingoFrameworkMemberField>, IBlingoMemberField
    {
      
        private bool _isFocused;


        #region Properties
       


        #endregion
        public bool IsFocused => _isFocused;


        public BlingoMemberField(BlingoCast cast, IBlingoFrameworkMemberField frameworkMember, int numberInCast, IAbstComponentFactory componentFactory, string name = "", string fileName = "", APoint regPoint = default) : base(BlingoMemberType.Field, cast, frameworkMember, numberInCast, componentFactory, name, fileName, regPoint)
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




