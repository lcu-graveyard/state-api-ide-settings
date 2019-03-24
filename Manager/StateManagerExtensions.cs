using System;
using System.Threading.Tasks;
using Fathym.Design.Factory;
using LCU.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Http
{
    public static class StateManagerExtensions
    {
        public static TMgr Manage<TState, TMgr>(this HttpRequest req, TState state, ILogger log)
            where TMgr : LCUStateHarness<TState>
        {
            return new ActivatorFactory<TMgr>().Create(req, log, state);
        }

        public static async Task<IActionResult> Manage<TArgs, TState, TMgr>(this HttpRequest req, ILogger log, Func<TMgr, TArgs, Task<TState>> action)
            where TState : class
            where TMgr : LCUStateHarness<TState>
        {
            return await req.WithState<TArgs, TState>(log, async (details, reqData, state, stateMgr) =>
            {
                var mgr = req.Manage<TState, TMgr>(state, log);

                return await action(mgr, reqData);
            });
        }
    }
}
