using System.IO;

namespace BlingoEngine.IO.Legacy.Tests.Cast;

internal sealed class TrackingStream : MemoryStream
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }
    }

   

