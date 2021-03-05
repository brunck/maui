using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class GetHandlersBenchmarker
	{
		ApplicationStub _application;

		Registrar<IFrameworkElement, IViewHandler> _registrar;

		[Params(100_000)]
		public int N { get; set; }

		[GlobalSetup(Target = nameof(GetHandlerUsingDI))]
		public void SetupForDI()
		{
			var startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder();

			startup.Configure(appBuilder);

			appBuilder.Build();

			_application = new ApplicationStub();

			appBuilder.SetServiceProvider(_application);
		}

		[GlobalSetup(Target = nameof(GetHandlerUsingRegistrar))]
		public void SetupForRegistrar()
		{
			_registrar = new Registrar<IFrameworkElement, IViewHandler>();
			_registrar.Register<IButton, ButtonHandler>();
		}

		[Benchmark]
		public void GetHandlerUsingDI()
		{
			for (int i = 0; i < N; i++)
			{
				_application.Context.Handlers.GetHandler<IButton>();
			}
		}

		[Benchmark(Baseline = true)]
		public void GetHandlerUsingRegistrar()
		{
			for (int i = 0; i < N; i++)
			{
				_registrar.GetHandler<IButton>();
			}
		}
	}
}