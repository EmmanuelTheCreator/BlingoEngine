using LingoEngine.Core;
using LingoEngine.VerboseLanguage;

namespace LingoEngine.Demo.TetriGrounds.Core
{
    // its not a requirement, but just for fun, using the verbose language to setup the members
    public class TetriGroundsMembersSetup : LingoVerboseMethods
    {
        public TetriGroundsMembersSetup(ILingoPlayer lingoPlayer)
            :base(lingoPlayer)
        {
            
        }
        public void InitMembers()
        {
            var castData = _player.CastLib("Data");

            // The verbose language
            Set(The().WidthMember.Of.Member("T_data")).To(191);
            Set(The().WidthMember.Of.Member(56,2)).To(473); // text memberLoading

            // Lingo : put round into field "round"
            // var round = 20;
            // Put(round).Into.Field("round");
            // var roundText = Get(The().Text.Of.Member("round"));

            // put the Text of member "Paul Robeson" into member "How Deep"
            //Put(The().Text.Of.Member("Paul Robeson")).Into.Field("How Deep");

            //var test = Not(The().Visibility.Of.Sprite(3));

            //var whichSprite = 3;
            //Set(The().Visibility.Of.Sprite(whichSprite)).Value = Not(The().Visibility.Of.Sprite(whichSprite));

            //castData.Member["T_data"]!.Width = 191;
            castData.Member["T_NewGame"]!.Width = 48;
            castData.Member["T_Score"]!.Width = 99;
            castData.Member["T_OverScreen"]!.Width = 446;
            castData.Member["T_OverScreen2"]!.Width = 446;
            castData.Member["T_InternetScoresNames"]!.Width = 65;
            castData.Member["T_InternetScores"]!.Width = 37;
            castData.Member["T_InternetScoresNamesP"]!.Width = 69;
            castData.Member["T_InternetScoresP"]!.Width = 37;
            castData.Member["T_StartLevel"]!.Width = 26;
            castData.Member["T_StartLines"]!.Width = 26;
            castData.Member["PlayerName"]!.Width = 100;
            castData.Member["T_InputText"]!.Width = 300;
            castData.Member["T_PopupTitle"]!.Width = 300;


        }
    }
}
