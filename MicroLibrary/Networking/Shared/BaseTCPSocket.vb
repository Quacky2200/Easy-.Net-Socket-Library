Imports System.Net
Imports System.Net.Sockets

Namespace Networking
    ''' <summary>
    ''' BaseTCPSocket used for ServerTCPSocket and ClientTCPSocket
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class BaseTCPSocket

        Private MessageCache As New Dictionary(Of String, DeserializationWrapper)

        Public Event OnConnectionInterrupt(sender As Socket)
        Public Event OnReceive(sender As Socket, obj As Object, BytesReceived As Integer)
        Public Event OnSent(sender As Socket, BytesSent As Integer)
        Public Event OnError(sender As Socket, e As Exception)
        Friend Event OnRelease(sender As Socket, senderIP As IPEndPoint)
        'Public Event OnPacketFailure()

        Public Property isPacketFailureRecoveryEnabled As Boolean = True

        Private SendCache As Object() = New Object(10) {}
        Private CacheIndex As Integer = -1

        Private _Port As Integer

        <Serializable>
        Friend Class ObjectResendRequest
            Public Property ID As String
            Sub New()

            End Sub
            Sub New(ID As String)
                Me.ID = ID
            End Sub
        End Class

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
            _Port = Port
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

        Friend Sub Receive(sender As Socket)
            Try
                ' Create the state object.
                Dim state As New StateObject
                state.workSocket = sender

                ' Get the length of the body data transfer. 
                sender.BeginReceive(state.PaddingBuffer, 0, StateObject.PaddingBufferSize, 0, New AsyncCallback(AddressOf PaddingCallback), state)
            Catch ex As SocketException
                RaiseEvent OnConnectionInterrupt(sender)
            Catch ex As Exception
                RaiseEvent OnError(sender, ex)
            End Try

        End Sub 'Receive
        Private Sub PaddingCallback(ar As IAsyncResult)
            ' Retrieve the state object and the client socket 
            ' from the asynchronous state object.
            Dim state As StateObject = CType(ar.AsyncState, StateObject)
            Dim client As Socket = state.workSocket

            ' Read the length of the body.
            Dim bytesRead As Integer = client.EndReceive(ar)
            state.TotalBytesRead += bytesRead

            Dim StringData As String = Text.Encoding.UTF8.GetString(state.PaddingBuffer)
            Dim BodyLength As Integer = CInt(StringData.Remove(10))
            Dim MessageID As String = StringData.Remove(0, 10)
            state.TotalBytesToRead = BodyLength
            state.ID = MessageID

            ' Begin receiving the data from the remote device.
            DoRecursiveReceive(state)
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

            DoRecursiveReceive(state)

        End Sub 'ReceiveCallback

        Private Sub DoRecursiveReceive(state As StateObject)
            Dim client As Socket = state.workSocket
            ' Check if we need more data or have finished the transfer.
            Dim Difference As Integer = state.TotalBytesToRead - state.TotalBytesRead

            If Difference < 0 Then
                ' Message corrupt
                If isPacketFailureRecoveryEnabled Then
                    ' Attempt To Recover
                    Send(client, New ObjectResendRequest(state.ID))
                Else
                    RaiseEvent OnError(client, New SocketException(SocketError.NoRecovery))
                End If

            ElseIf Difference <= StateObject.BufferSize Then
                ' Done After Next Receive.
                client.BeginReceive(state.buffer, 0, Difference, 0, New AsyncCallback(AddressOf FinishReceiveCallback), state)
            Else
                ' Get the rest of the data.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReceiveCallback), state)
            End If
        End Sub

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
                RaiseEvent OnReceive(client, data, state.TotalBytesRead)
            End If

            FinishReceiveClean(state)

        End Sub
        Private Sub FinishReceiveClean(ByRef State As StateObject)
            ' Clean Up
            Dim client As Socket = State.workSocket
            State.TotalBytesToRead = 0
            State.TotalBytesRead = 0
            State.buffer = New Byte(StateObject.BufferSize - 1) {}
            State.PaddingBuffer = New Byte(StateObject.PaddingBufferSize - 1) {}
            State.ObjectData.Clear()
            State = Nothing

            Receive(client)
        End Sub
        Private Sub UpdateRecoveryCache(MessageID As String, Data As Byte())

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
                Dim RAW As Object() = PrepareSend(Bytes)
                Dim ObjectData As Byte() = RAW(0)
                Dim MessageID As String = RAW(1)
                If isPacketFailureRecoveryEnabled Then UpdateRecoveryCache(MessageID, ObjectData)
                sender.BeginSend(ObjectData, 0, ObjectData.Length, SocketFlags.None, AddressOf SendCallBack, sender)
            Catch e As Exception
                If Not IsNothing(sender) And sender.Connected Then
                    RaiseEvent OnConnectionInterrupt(sender)
                    sender.Close()
                    Exit Sub
                End If
            End Try
        End Sub

        Private Function PrepareSend(Data As Byte()) As Object()
            Dim PaddingInfo As String = GetPaddingInformation(Data)
            Dim MessageID As String = PaddingInfo.Substring(10, 32)
            Dim DataLength As String = PaddingInfo.Substring(0, 10)
            Dim LengthData As Byte() = Text.Encoding.UTF8.GetBytes(PaddingInfo)
            Return {CombineByteArrays({LengthData, Data}), MessageID}
        End Function

        Private Function GetPaddingInformation(Data As Byte()) As String
            Return CInt(Data.Length + StateObject.PaddingBufferSize).ToString("D10") +
                   (Guid.NewGuid.ToString.Replace("-", ""))
        End Function

        Private Sub CloseSocket(s As Socket)
            RaiseEvent OnRelease(s, s.RemoteEndPoint)
            s.Close()
        End Sub

        'Private Sub BaseTCPSocket_OnError(sender As Socket, e As Exception) Handles Me.OnError

        'End Sub
    End Class
    Public Class StateObject
        ' Client socket.
        Public workSocket As Socket = Nothing
        ' Size of receive buffer.
        Public Const BufferSize As Integer = 4096
        Public Const PaddingBufferSize As Integer = 42 ' 32 + 10 (GUID + int)
        Public TotalBytesRead As Integer = 0
        Public TotalBytesToRead As Integer = 0
        ' Receive buffer.
        Public buffer(BufferSize - 1) As Byte
        Public PaddingBuffer(PaddingBufferSize - 1) As Byte
        Public ObjectData As New List(Of Byte)
        'The GUID of the data we're getting
        Public ID As String
    End Class 'StateObject

End Namespace