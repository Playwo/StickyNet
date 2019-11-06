using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreServer
{
    public static partial class Extensions
    {
        private class StateObject
        {
            public Socket Socket = null;
            public int BufferSize;
            public byte[] Buffer;
            public StringBuilder StringBuilder;
            public TaskCompletionSource<object> ReceiveTaskSource;
            public Task ReceiveTask => ReceiveTaskSource.Task;

            public StateObject(Socket socket, int bufferSize)
            {
                Socket = socket;
                BufferSize = bufferSize;
                Buffer = new byte[BufferSize];
                StringBuilder = new StringBuilder();
                ReceiveTaskSource = new TaskCompletionSource<object>();
            }
        }

        public static async Task<string> ReceiveAsync(this TcpSession session, int timeout)
        {
            var state = new StateObject(session.Socket, 256);

            var timeoutTask = Task.Delay(timeout);

            var callback = new AsyncCallback(ReceiveCallback);
            session.Socket.BeginReceive(state.Buffer, 0, state.BufferSize, 0, callback, state);

            var finishedFirst = await Task.WhenAny(timeoutTask, state.ReceiveTask);

            return finishedFirst == timeoutTask
                ? null
                : state.StringBuilder.ToString();
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject) ar.AsyncState;

            int bytesRead = state.Socket.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.StringBuilder.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                state.Socket.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                state.ReceiveTaskSource.SetResult(null); //Signal that all data is received
            }
        }
    }
}
