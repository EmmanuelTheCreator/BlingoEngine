using BlingoEngine.Sprites;
using System.Reflection;

namespace BlingoEngine.Setup
{
    public static class MovieRegistrationExtensions
    {
        public static IMovieRegistration AddScriptsFromAssembly(this IMovieRegistration registration, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();

            var addBehavior = registration.GetType().GetMethod(nameof(IMovieRegistration.AddBehavior));
            var addParentScript = registration.GetType().GetMethod(nameof(IMovieRegistration.AddParentScript));
            var addMovieScript = registration.GetType().GetMethod(nameof(IMovieRegistration.AddMovieScript));
            if (addBehavior == null || addParentScript == null || addMovieScript == null)
                throw new InvalidOperationException("Registration implementation missing required methods");

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;

                if (typeof(BlingoSpriteBehavior).IsAssignableFrom(type))
                {
                    addBehavior.MakeGenericMethod(type).Invoke(registration, null);
                }
                else if (typeof(Core.BlingoParentScript).IsAssignableFrom(type))
                {
                    addParentScript.MakeGenericMethod(type).Invoke(registration, null);
                }
                else if (typeof(Movies.BlingoMovieScript).IsAssignableFrom(type))
                {
                    addMovieScript.MakeGenericMethod(type).Invoke(registration, null);
                }
            }
            return registration;
        }
    }
}

