using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive.Concurrency;
using Ty.Services;

namespace Ty
{
    public class MyCoolObservableExceptionHandler : IObserver<Exception>
    {

        public void OnNext(Exception value)
        {
            if (Debugger.IsAttached) Debugger.Break();
            var box = TyApp.ServiceProvider.GetRequiredService<IMessageBoxManager>();
            box.Alert.Handle(new AlertInfo(value.Message)).Subscribe();
        }

        public void OnError(Exception error)
        {
            if (Debugger.IsAttached) Debugger.Break();

            RxApp.MainThreadScheduler.Schedule(() => { throw error; });
        }

        public void OnCompleted()
        {
            if (Debugger.IsAttached) Debugger.Break();
            RxApp.MainThreadScheduler.Schedule(() => { throw new NotImplementedException(); });
        }
    }
}
