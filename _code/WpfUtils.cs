using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp {
    static class WpfUtils {
        public static System.Windows.Threading.Dispatcher MainDispatcher { get; set; }

        public static void delayCall(Action action, int delayMS = 10) {
            Task.Delay(delayMS).ContinueWith(o => {
                action.Invoke();
            });
        }

        public static void toMainThread(Action action) {
            MainDispatcher.BeginInvoke(action);
        }

        public static void toMainThread(Action action, int delayMS) {
            Task.Delay(delayMS).ContinueWith(o => {
                MainDispatcher.BeginInvoke(action);
            });
        }

        public static void createBgThread(Action action) {
            Task.Run(action);
        }
    }
}
