using ProjectorRays.Common;
using ProjectorRays.director.Scores.Data;
using System.Collections.Generic;

namespace ProjectorRays.director.Scores;

public interface IRaysScoreFrameParserV2
{
    List<RaySprite> ParseScore(ReadStream stream);
}

