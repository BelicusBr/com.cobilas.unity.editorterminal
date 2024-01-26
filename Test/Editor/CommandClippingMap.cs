using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cobilas.Unity.Editor.Terminal {
    public sealed class CommandClippingMap : IDisposable {
        private bool disposedValue;
        private List<string> result;
        private List<KeyValuePair<int, int>> argumentArea;

        public CommandClippingMap(string args) {
            Clip(args);
            foreach (var item in result)
                UnityEngine.Debug.Log(item);
        }

        private void Clip(string args) {
            result = new List<string>();
            argumentArea = new List<KeyValuePair<int, int>>();
            int cursorIndex = 0;
            int breakLoop = 0;
            int open = -1;
            int close = -1;
            int closeArg = -1;
            while (true) {
                cursorIndex = args.IndexOf('"', cursorIndex);
                if (open != -1 && close != -1) {
                    closeArg = args.IndexOf('>', close);
                    open = close = -1;
                }
                if (cursorIndex < 0) break;
                if (open == -1) {
                    if (cursorIndex == 0) cursorIndex = (open = 0) + 1;
                    else if (args[cursorIndex - 1] != '\\')
                        cursorIndex = (open = cursorIndex) + 1;
                } else {
                    if (args[cursorIndex - 1] != '\\') {
                        cursorIndex = (close = cursorIndex) + 1;
                        argumentArea.Add(new KeyValuePair<int, int>(open, close));
                    }
                }
                if (breakLoop > args.Length * 2)
                    break;
                ++breakLoop;
            }

            if (closeArg < 0) return;
            string uet_arg = args.Remove(closeArg);
            string t_arg = args.Remove(0, closeArg + 1);
            int selectIndex = cursorIndex = breakLoop = 0;
            //key > open
            //value > close
            while (true) {
                cursorIndex = uet_arg.IndexOf(' ', cursorIndex);
                bool isArg = false;
                StringBuilder builder = null;
                if (cursorIndex < 0) break;

                foreach (var item in argumentArea) {
                    if (cursorIndex > item.Key && cursorIndex < item.Value) {
                        isArg = true;
                        builder = new StringBuilder();
                        for (int I = item.Key + 1; I < item.Value; I++)
                            builder.Append(uet_arg[I]);

                        selectIndex = cursorIndex = item.Value + 1;
                        result.Add(builder.ToString());
                        break;
                    }
                }
                if (isArg) continue;
                builder = new StringBuilder();
                for (int I = selectIndex; I < cursorIndex; I++)
                    builder.Append(uet_arg[I]);
                
                selectIndex = (cursorIndex += 1);
                result.Add(builder.ToString());
                if (breakLoop > args.Length * 2)
                    break;
                ++breakLoop;
            }
            result.Add($"[t_arg]{t_arg}");
        }

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                }
                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}