using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cobilas.Unity.Editor.Terminal {
    [Serializable]
    public sealed class TerminalCommand : IDisposable {
        private string alias;
        private List<TerminalCommand> TerminalCommands;
        private bool disposedValue;
        private Action<string> callInArgument;

        public TerminalCommand(string alias, Action<string> callInArgument, List<TerminalCommand> TerminalCommands) {
            this.alias = alias;
            this.TerminalCommands = TerminalCommands;
            this.callInArgument = callInArgument;
        }

        public TerminalCommand(string alias, List<TerminalCommand> TerminalCommands) : this(alias, (Action<string>)null, TerminalCommands) {}

        public TerminalCommand(string alias, Action<string> callInArgument) : this(alias, callInArgument, new List<TerminalCommand>()) {}

        public TerminalCommand(string alias) : this(alias, new List<TerminalCommand>()) {}

        public bool Invok(int indexTarget, List<string> args) {
            if (disposedValue)
                throw new ObjectDisposedException("The object has already been discarded!");
            if (alias == "{arg}") {
                callInArgument?.Invoke(args[indexTarget]);
                return true;
            }
            if (alias == "{arg>}") {
                callInArgument?.Invoke(args[indexTarget]);
                return true;
            }
            List<string> al = new List<string>(alias.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries));
            if (al.Contains(args[indexTarget])) {
                callInArgument?.Invoke(args[indexTarget]);
                foreach (var item in TerminalCommands)
                    if (item.Invok(indexTarget + 1, args))
                        return true;
                return true;
            }
            return false;
        }

        public bool CheckCommandIntegrity(int indexTarget, List<string> args) {
            if (alias == "{arg>}" || alias == "{arg}") {
                foreach (var item in TerminalCommands)
                    if (item.Invok(indexTarget + 1, args))
                        return true;
                return true;
            }
            List<string> al = new List<string>(alias.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries));
            if (al.Contains(args[indexTarget])) {
                foreach (var item in TerminalCommands)
                    if (item.Invok(indexTarget + 1, args))
                        return true;
                return true;
            }
            return false;
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    alias = (string)null;
                    foreach (var item in TerminalCommands)
                        item.Dispose();
                    TerminalCommands.Clear();
                    TerminalCommands.Capacity = 0;
                    TerminalCommands = (List<TerminalCommand>)null;
                    callInArgument = (Action<string>)null;
                }
                disposedValue = true;
            }
        }

        public static List<string> CommandDismemberer(string command) {
            List<string> res = new List<string>();
            int indexTarget = 0;
            int open = -1;
            int close = -1;
            while (close == -1) {
                indexTarget = command.IndexOf('"', indexTarget);
                if (indexTarget < 0) break;
                if (open == -1) {
                    if (indexTarget == 0) indexTarget = (open = 0) + 1;
                    else if (command[indexTarget - 1] != '\\')
                        indexTarget = (open = indexTarget) + 1;
                } else {
                    if (command[indexTarget - 1] != '\\')
                        indexTarget = (close = indexTarget) + 1;
                }
            }
            return res;
        }
    }
}