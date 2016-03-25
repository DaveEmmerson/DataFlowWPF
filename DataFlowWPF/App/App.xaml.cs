using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;


namespace DataFlowWPF
{
    public partial class App : Application
    {
        #region Constants

        private const int retryCount = 1;
        private const int retryInterval = 2;

        #endregion Constants

        #region Fields

        private MainWindow _mainWindow;

        #endregion Fields

        public App()
        {
            Startup += App_Startup;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // let's just display a message and carry on regardless... yee haa! :D

            var window = _mainWindow;

            if(window == null || window.Visibility == Visibility.Hidden)
            {
                MessageBox.Show("Exception occurred before window available. Exiting application");
                Current.Shutdown();

            } else {

                MessageBox.Show(e.Exception.StackTrace, e.Exception.Message);

            }

            e.Handled = true;
        }

        private async void App_Startup(object sender, StartupEventArgs e) {

            var log = MEF.GetInstance<ILogger>();
            var mainWindowViewModel = MEF.GetInstance<IMainWindowViewModel>();

            var logTicksAgent = new ActionBlock<TickerEvent>(_ => log.Verbose("Tick"));

            Func<Func<ITicker>, ITicker> createTicker = tickerFactory =>
            {
                log.Verbose("In createTicker");
                var newTicker = tickerFactory();
                newTicker.LinkTo(logTicksAgent);
                mainWindowViewModel.TickerViewModel.LinkTo(newTicker);
                return newTicker;
            };

            var ticker = createTicker(MEF.GetInstance<ITicker>);
            ticker.LinkTo(logTicksAgent);
            mainWindowViewModel.TickerViewModel.LinkTo(ticker);

            log.LinkTo(mainWindowViewModel.LogViewModel);

            _mainWindow = new MainWindow();
            _mainWindow.DataContext = mainWindowViewModel;
            _mainWindow.Show();

            await Monitor(ticker, retryCount, log, () => createTicker(() => new Ticker(log)));
        }

        private static async Task Monitor<T>(T agent, int retryCount, ILogger log, Func<T> recreateAgent) where T : IDisposable, IDataflowBlock
        {
            int retries = retryCount;

            while (retries > -1)
            {
                try
                {

                    await agent.Completion;

                }
                catch (Exception ex)
                {
                    log.Error($"The agent failed: {ex.Message}");
                }

                log.Verbose("Disposing agent");
                agent.Dispose();
                log.Verbose("Disposed.");
                log.Verbose("Creating new agent!");

                try
                {
                    agent = recreateAgent();
                    log.Verbose("Successfully created new agent.");
                    retries = retryCount;
                    continue;
                }
                catch
                {
                    var retryMessage = retries > 0
                        ? $"Will retry a further {retries} times."
                        : $"After trying {retryCount} times, will not retry again.";

                    log.Error($"Failed to create new agent. {retryMessage}");
                }

                if (retries > 0)
                {
                    log.Verbose($"Waiting {retryInterval} seconds before retrying.");
                    await Task.Delay(retryInterval * 1000); // millseconds
                }
                else
                {
                    log.Verbose("Stopping running agent monitor");
                }

                retries--;
            }
        }
    }
}
