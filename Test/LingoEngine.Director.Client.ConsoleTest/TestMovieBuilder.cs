using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Shapes;
using LingoEngine.Texts;

namespace LingoEngine.Director.Client.ConsoleTest;

public class TestCastBuilder : ILingoCastLibBuilder
{
    public Task BuildAsync(ILingoCastLibsContainer castLibs)
    {
        var cast = castLibs.AddCast("TestCast");

        var text1 = (LingoMemberText)cast.Add(LingoMemberType.Text, 1, "Greeting");
        text1.Text = "Hello from member 1";
        text1.Width = 120;
        text1.Height = 20;

        var text2 = (LingoMemberText)cast.Add(LingoMemberType.Text, 2, "Info");
        text2.Text = "Second text member";
        text2.Width = 120;
        text2.Height = 20;

        var shape = (LingoMemberShape)cast.Add(LingoMemberType.Shape, 3, "Box");
        shape.ShapeType = LingoShapeType.Rectangle;
        shape.Width = 80;
        shape.Height = 40;
        shape.FillColor = new AColor(0, 255, 0, 255);
        shape.StrokeColor = new AColor(0, 0, 0, 255);
        shape.StrokeWidth = 1;
        shape.Filled = true;

        return Task.CompletedTask;
    }
}

public class TestScoreBuilder : ILingoScoreBuilder
{
    public Task BuildAsync(ILingoMovie movie)
    {
        movie.AddSprite(1, 1, 60, 50, 50, s =>
        {
            s.Member = movie.CastLib.GetMember(1, 1);
        });
        movie.AddSprite(2, 1, 60, 200, 50, s =>
        {
            s.Member = movie.CastLib.GetMember(2, 1);
        });
        movie.AddSprite(3, 1, 60, 50, 150, s =>
        {
            s.Member = movie.CastLib.GetMember(3, 1);
        });
        movie.AddSprite(4, 1, 60, 200, 150, s =>
        {
            s.Member = movie.CastLib.GetMember(1, 1);
        });
        movie.AddSprite(5, 1, 60, 350, 150, s =>
        {
            s.Member = movie.CastLib.GetMember(2, 1);
        });
        return Task.CompletedTask;
    }
}

public class TestMovieBuilder : ILingoMovieBuilder
{
    public async Task<ILingoMovie> BuildAsync(ILingoPlayer player)
    {
        await player.LoadAsync<TestCastBuilder>();
        var movie = player.NewMovie("TestMovie");
        await new TestScoreBuilder().BuildAsync(movie);
        return movie;
    }
}
