Imports System.Net
Imports System.Net.Sockets

Namespace Networking
    ''' <summary>
    ''' BaseTCPSocket used for ServerTCPSocket and ClientTCPSocket
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class BaseTCPSocket
        Public Event OnConnectionInterrupt(sender As Socket)
        Public Event OnReceive(sender As Socket, obj As Object, BytesReceived As Integer)
        Public Event OnSent(sender As Socket, BytesSent As Integer)
        Public Event OnError(sender As Socket, e As Exception)
        Friend Event OnRelease(sender As Socket, senderIP As IPEndPoint)

        Private _Port As Integer

#Region ".NET 4.5 Task Implementation"

        Friend Function SendTask(sender As Socket, Obj As Object) As Task(Of Integer)
            Dim Data As Byte() = PrepareSend(Protocol.Serialize(Obj))
            Dim t As Task(Of Integer) = Task.Run(Of Integer)(Function() sender.Send(Data, 0, Data.Length, SocketFlags.None))
            t.Wait()
            Return t
        End Function

#End Region

        ''' <summary>
        ''' The main socket that listens to all requests. Using the TCP protocol.
        ''' </summary>
        ''' <remarks>Uses AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp</remarks>
        Public BaseSocket As Socket
        ''' <summary>
        ''' Make a new TCP socket and bind it instantly.
        ''' </summary>
        ''' <param name="Port">The port you wish to bind to</param>
        ''' <param name="Protocol">The protocol to use for serializing and deserializing information</param>
        ''' <remarks></remarks>
        Public Sub New(Protocol As ISerializationProtocol, Optional Port As Integer = 0)
            Me._Port = Port
            BaseSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            BaseSocket.ReceiveTimeout = -1
            BaseSocket.SendTimeout = -1
            Try
                Bind(Port)
            Catch ex As SocketException
                Throw New Exception("Cannot bind to port!")
            Catch ex As Exception
                Throw ex
            End Try
            Me.Protocol = Protocol
        End Sub
        Private Sub Bind(Port As Integer)
            BaseSocket.Bind(New IPEndPoint(IPAddress.Any, Port))
        End Sub
        Public Property Protocol As ISerializationProtocol
        ''' <summary>
        ''' Returns the binded port number
        ''' </summary>
        ''' <value>Integer</value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Port As Integer
            Get
                Return _Port
            End Get
        End Property
        ''' <summary>
        ''' Returns the binded IPEndPoint
        ''' </summary>
        ''' <value>IPEndPoint</value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LocalIPEndPoint As IPEndPoint
            Get
                Return CType(BaseSocket.LocalEndPoint, IPEndPoint)
            End Get
        End Property
        ''' <summary>
        ''' Returns a boolean value if the socket's connected to a remote host
        ''' </summary>
        ''' <value>Boolean</value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Connected As Boolean
            Get
                Return If(Not IsNothing(BaseSocket), BaseSocket.Connected, False)
            End Get
        End Property

        'Friend Sub Receive(sender As Socket)

        '    Do
        '        Dim Buffer As New List(Of Byte)
        '        Try
        '            Dim tmpBuffer(65536) As Byte
        '            Dim ReadBytes As Integer = 1
        '            Do
        '                ReadBytes = sender.Receive(tmpBuffer)
        '                Buffer.AddRange(tmpBuffer.ToList.GetRange(0, ReadBytes))
        '            Loop While ReadBytes > 0

        '            Dim data As Object = Protocol.Deserialize(Buffer.ToArray)
        '            If Not IsNothing(data) Then
        '                Buffer.Clear()
        '                RaiseEvent OnReceive(sender, data, Buffer.Count)
        '            End If

        '        Catch e As SocketException
        '            RaiseEvent OnConnectionInterrupt(sender)
        '            Buffer.Clear()
        '            Exit Do
        '        Catch e As Threading.ThreadAbortException
        '            Buffer.Clear()
        '            Exit Do
        '        Catch e As Exception
        '            RaiseEvent OnError(sender, e)
        '            Buffer.Clear()
        '            Exit Do
        '        End Try

        '        Buffer.Clear()
        '        System.Threading.Thread.Sleep(1)
        '    Loop

        '    RaiseEvent OnConnectionInterrupt(sender)
        '    CloseSocket(sender)
        'End Sub

        '// read the first 2 bytes as message length
        'BeginReceive(msg,0,2,-,-,New AsyncCallback(LengthReceived),-)

        'LengthReceived(ar) {
        '  StateObject so = (StateObject) ar.AsyncState;
        '  Socket s = so.workSocket;
        '  int read = s.EndReceive(ar);
        '  msg_length = GetLengthFromBytes(so.buffer);
        '  BeginReceive(so.buffer,0,msg_length,-,-,New AsyncCallback(DataReceived),-)
        '}

        'DataReceived(ar) {
        '  StateObject so = (StateObject) ar.AsyncState;
        '  Socket s = so.workSocket;
        '  int read = s.EndReceive(ar);
        '  ProcessMessage(so.buffer);
        '  BeginReceive(so.buffer,0,2,-,-,New AsyncCallback(LengthReceived),-)
        '}

        Friend Sub Receive(sender As Socket)
            Try
                ' Create the state object.
                Dim state As New StateObject
                state.workSocket = sender

                ' Get the length of the body data transfer.
                sender.BeginReceive(state.LengthBuffer, 0, StateObject.LengthBufferSize, 0, New AsyncCallback(AddressOf LengthCallback), state)
            Catch ex As SocketException
                RaiseEvent OnConnectionInterrupt(sender)
            Catch ex As Exception
                RaiseEvent OnError(sender, ex)
            End Try

        End Sub 'Receive
        Private Sub LengthCallback(ar As IAsyncResult)
            ' Retrieve the state object and the client socket 
            ' from the asynchronous state object.
            Dim state As StateObject = CType(ar.AsyncState, StateObject)
            Dim client As Socket = state.workSocket

            ' Read the length of the body.
            Dim bytesRead As Integer = client.EndReceive(ar)
            state.TotalBytesRead += bytesRead + 1
            state.TotalBytesToRead = CInt(System.Text.UTF8Encoding.UTF8.GetString(state.LengthBuffer))

            ' Begin receiving the data from the remote device.
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReceiveCallback), state)
        End Sub
        Private Sub ReceiveCallback(ar As IAsyncResult)
            ' Retrieve the state object and the client socket 
            ' from the asynchronous state object.
            Dim state As StateObject = CType(ar.AsyncState, StateObject)
            Dim client As Socket = state.workSocket

            ' Read data from the remote device.
            Dim bytesRead As Integer = client.EndReceive(ar)

            ' Add temporary buffer data to final output buffer.
            state.ObjectData.AddRange(state.buffer)
            state.TotalBytesRead += bytesRead

            ' Check if we need more data or have finished the transfer.
            Dim Difference As Integer = state.TotalBytesToRead - state.TotalBytesRead
            If Difference < StateObject.BufferSize Then
                ' Done After Next Receive.
                client.BeginReceive(state.buffer, 0, Difference + 1, 0, New AsyncCallback(AddressOf FinishReceiveCallback), state)
            Else
                ' Get the rest of the data.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReceiveCallback), state)
            End If

        End Sub 'ReceiveCallback

        Private Sub FinishReceiveCallback(ByVal ar As IAsyncResult)

            ' Retrieve the state object and the client socket 
            ' from the asynchronous state object.
            Dim state As StateObject = CType(ar.AsyncState, StateObject)
            Dim client As Socket = state.workSocket

            ' Read data from the remote device.
            Dim bytesRead As Integer = client.EndReceive(ar)

            ' Add temporary buffer data to final output buffer.
            state.ObjectData.AddRange(state.buffer.ToList.GetRange(0, bytesRead))
            state.TotalBytesRead += bytesRead

            Dim data As Object = Protocol.Deserialize(state.ObjectData.ToArray)
            If Not IsNothing(data) Then
                RaiseEvent OnReceive(client, data, state.TotalBytesRead - 1)
            End If

            ' Clean Up
            state.TotalBytesToRead = 0
            state.TotalBytesRead = 0
            state.buffer = New Byte(StateObject.BufferSize - 1) {}
            state.LengthBuffer = New Byte(StateObject.LengthBufferSize - 1) {}
            state.ObjectData.Clear()
            state = Nothing

            Receive(client)

        End Sub
        Private Sub SendCallBack(ByVal ar As IAsyncResult)
            ' Retrieve the socket from the state object.
            Dim client As Socket = CType(ar.AsyncState, Socket)
            ' Complete sending the data to the remote device.
            Dim bytesSent As Integer = client.EndSend(ar)
            'Console.WriteLine("Sent {0} bytes to server.", bytesSent)
            ' Signal that all bytes have been sent.
            RaiseEvent OnSent(client, bytesSent)
        End Sub
        Friend Sub Send(sender As Socket, Obj As Object)
            Send(sender, Protocol.Serialize(Obj))
        End Sub
        Friend Sub Send(sender As Socket, Bytes() As Byte)
            Try
                Dim Data As Byte() = PrepareSend(Bytes)
                sender.BeginSend(Data, 0, Data.Length, SocketFlags.None, AddressOf SendCallBack, sender)
            Catch e As Exception
                If Not IsNothing(sender) And sender.Connected Then
                    RaiseEvent OnConnectionInterrupt(sender)
                    sender.Close()
                    Exit Sub
                End If
            End Try
        End Sub

        Private Function PrepareSend(Data As Byte()) As Byte()
            Dim LengthString As String = (Data.Length + StateObject.LengthBufferSize).ToString("D9")
            Dim LengthBytes As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(LengthString)
            Dim merged = New Byte(LengthBytes.Length + Data.Length - 1) {}
            LengthBytes.CopyTo(merged, 0)
            Data.CopyTo(merged, LengthBytes.Length)
            Return merged
        End Function
        Private Sub CloseSocket(s As Socket)
            RaiseEvent OnRelease(s, s.RemoteEndPoint)
            s.Close()
        End Sub

        Private Sub BaseTCPSocket_OnError(sender As Socket, e As Exception) Handles Me.OnError

        End Sub
    End Class
    Public Class StateObject
        ' Client socket.
        Public workSocket As Socket = Nothing
        ' Size of receive buffer.
        Public Const BufferSize As Integer = 4096
        Public Const LengthBufferSize As Integer = 9
        Public TotalBytesRead As Integer = 0
        Public TotalBytesToRead As Integer = 0
        ' Receive buffer.
        Public buffer(BufferSize - 1) As Byte
        Public LengthBuffer(LengthBufferSize - 1) As Byte
        Public ObjectData As New List(Of Byte)
    End Class 'StateObject

End Namespace