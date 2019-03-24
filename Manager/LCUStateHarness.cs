using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LCU.Manager
{
    public abstract class LCUStateHarness<TState>
    {
        #region Fields
        protected readonly LCUStateDetails details;

        protected readonly ILogger log;

        protected readonly TState state;
        #endregion

        #region Constructors
        public LCUStateHarness(HttpRequest req, ILogger log, TState state)
        {
            this.details = req.LoadStateDetails();

            this.log = log;

            this.state = state;
        }
        #endregion

        #region API Methods
        public virtual TState Eject()
        {
            return state;
        }

        public virtual async Task<TState> WhenAll(params Task<TState>[] stateActions)
        {
            var states = await stateActions.WhenAll();

            return state;
        }
        #endregion
    }
}