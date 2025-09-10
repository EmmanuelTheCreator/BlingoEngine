using LingoEngine.Core;
using LingoEngine.Movies;
#pragma warning disable IDE1006 // Naming Styles
namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 3_SpriteManager.ls
    public class SpriteManager : LingoParentScript
    {
        private int pNum;
        private readonly List<int> pDestroyList = new();
        private readonly List<int> pSpriteNums = new();
        private object? pGame;

        public SpriteManager(ILingoMovieEnvironment env) : base(env) { }

        public void Init(int beginningSprite)
        {
            pNum = beginningSprite;
            pDestroyList.Clear();
            pSpriteNums.Clear();
            pGame = null;
        }

        public int Sadd()
        {
            if (pDestroyList.Count == 0) // are there destroyed sprites
            {
                pNum += 1; // create a new one
                // check if we are at the maximum
                if (pNum > 999)
                {
                    var spr = Sprite(1000);
                    _Movie.PuppetSprite(1000, true);
                    spr.Loc = Point(100, 30);
                    spr.SetMember("TomuchSprites");
                    spr.Blend = 100;
                    spr.LocZ = 1000;
                    spr.Blend = 0;
                    spr.Loc = Point(1, -40);
                    pNum -= 1;
                    return 0;
                }
                _Movie.PuppetSprite(pNum, true);
                if (pSpriteNums.Contains(pNum))
                    pNum += 100000;
                pSpriteNums.Add(pNum);
                var sprite = Sprite(pNum);
                sprite.Ink = 36;
                return pNum;
            }
            else // create a new from the destroyed sprite list
            {
                int pNumDestroy = pDestroyList[0];
                _Movie.PuppetSprite(pNumDestroy, true);
                pDestroyList.RemoveAt(0);
                pSpriteNums.Add(pNumDestroy);
                Sprite(pNumDestroy).Ink = 36;
                return pNumDestroy;
            }
        }

        /// <summary>
        ///  destroy a sprite
        /// </summary>
        /// <param name="sprNum">The sprite num</param>
        public void SDestroy(int sprNum)
        {
            if (!pSpriteNums.Contains(sprNum))
                return;
            pSpriteNums.Remove(sprNum);
            pDestroyList.Add(sprNum);
            var spr = Sprite(sprNum);
            spr.SetMember("empty");
            spr.LocZ = sprNum;
            _Movie.PuppetSprite(sprNum, false);
        }

        public IReadOnlyList<int> GetSpriteNums() => pSpriteNums;

        public int CheckSprite(int num) => pSpriteNums.Contains(num) ? 1 : 0;

        public void Destroy()
        {
            foreach (var i in pDestroyList.ToArray())
                SDestroy(i);
            pGame = null;
        }
    }
}
