using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalmaShared
{
    public abstract class MatchInput : IDisposable
    {
        public enum TouchResultType
        {
            None,
            Undo,
            Redo,
            Field
        }

        public delegate void TouchHandler(TouchResultType resultType, HexCoord hexcoord);
        public abstract event TouchHandler FieldTouched;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            disposedValue = true;
        }

        ~MatchInput()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
