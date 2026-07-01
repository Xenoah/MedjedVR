using System;
using System.Threading.Tasks;

namespace Basis.Utils
{
    /// <summary>
    /// エラーHandlersの責務をまとめるクラスです。
    /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class ErrorHandlers
    {
        /// <summary>
        /// AttachGlobalHandlersを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void AttachGlobalHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// CurrentDomainUnhandledExceptionを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                BNL.LogError($"Fatal exception: {exception.Message}");
                BNL.LogError($"Stack trace: {exception.StackTrace}");
            }
            else
            {
                BNL.LogError("An unknown fatal exception occurred.");
            }
        }

        /// <summary>
        /// TaskSchedulerUnobservedTaskExceptionを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            foreach (var exception in e.Exception.InnerExceptions)
            {
                BNL.LogError($"Unobserved task exception: {exception.Message}");
                BNL.LogError($"Stack trace: {exception.StackTrace}");
            }

            e.SetObserved();
        }
    }
}
