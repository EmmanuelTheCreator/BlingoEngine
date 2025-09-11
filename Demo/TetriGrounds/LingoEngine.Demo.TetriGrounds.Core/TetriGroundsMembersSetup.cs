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
            Set(the => the.WidthMember.Of.Member("T_data")).To(191);
            Set(the => the.WidthMember.Of.Member(56,2)).To(473); // text memberLoading

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


        }
    }
}
