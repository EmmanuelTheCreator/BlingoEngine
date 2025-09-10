using LingoEngine.Bitmaps;
using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;

#pragma warning disable IDE1006 // Naming Styles
namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 28_MousePointer.ls
    public class MousePointer : LingoParentScript, IHasStepFrameEvent
    {
        private int myNum;
        private float myOldX;
        private float myOldY;
        private int myAnimateNum;
        private int myStartMember;
        private int myNumberMembers;
        private int myDir;
        private List<ILingoMember> myMembers = new();   

        public MousePointer(ILingoMovieEnvironment env) : base(env) { }

        public void Init(int num)
        {
            myNum = num;
            myStartMember = 80;
            myNumberMembers = 5;
            myAnimateNum = 0;
            myDir = 1;
            for (int i = 0; i <= 5; i++)
            {
                myMembers.Add(_Movie.GetMember<LingoMemberBitmap>("mouse000" + i)!);
            }
            ShowMouse();
        }

        public void StepFrame() => Refresh();

        private void Refresh()
        {
            bool changed = false;
            if (myOldX != _Mouse.MouseH)
            {
                myOldX = _Mouse.MouseH;
                changed = true;
            }
            if (myOldY != _Mouse.MouseV)
            {
                myOldY = _Mouse.MouseV;
                changed = true;
            }
            if (changed)
            {
                Sprite(myNum).LocH = _Mouse.MouseH + 17;
                Sprite(myNum).LocV = _Mouse.MouseV + 10;
                if (myDir == 1)
                {
                    if (myAnimateNum < myNumberMembers)
                        myAnimateNum += 1;
                    else
                        myDir = -1;
                }
                else
                {
                    if (myAnimateNum > 0)
                        myAnimateNum -= 1;
                    else
                        myDir = 1;
                }
                Sprite(myNum).SetMember(myMembers[myAnimateNum]);
            }
        }

        public void ShowMouse()
        {
            _Movie.PuppetSprite(myNum, true);
            Sprite(myNum).LocZ = 1000000;
            Sprite(myNum).SetMember(myMembers[0]);
            _Movie.ActorList.Add(this);
            Sprite(myNum).Visibility = true;
        }
        public void HideMouse()
        {
            //_Movie.PuppetSprite(myNum, false);
            _Movie.ActorList.Remove(this);
            Sprite(myNum).Visibility = false;
        }

        public void Mouse_Over() { }
        public void Mouse_Restore() { }

        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}
