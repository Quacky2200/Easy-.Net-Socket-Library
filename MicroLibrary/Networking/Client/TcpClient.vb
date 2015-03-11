Imports System.Net
Imports System.Net.Sockets
Imports MicroLibrary.Serialization

Namespace Networking.Client
    ''' <summary>
    ''' An easy to use, multithreaded TCP client
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TcpClient
        Inherits BaseTCPSocket
        Public Event OnConnected(Sender As Socket)
        ''' <summary>
        ''' Make a TCP client, binded to a port (if specified). Using IPAddress.Any
        ''' </summary>
        ''' <param name="Port">(Optional) Bind to the specified port</param>
        ''' <remarks>See bind: http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.bind(v=vs.110).aspx 
        ''' See IPEndPoint: http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.bind(v=vs.110).aspx 
        ''' </remarks>
        Sub New(Protocol As ISerializationProtocol, Optional Port As Integer = 0)
            MyBase.New(Protocol, Port)
        End Sub


        ''' <summary>
        ''' Try to connect to the host and port specified
        ''' </summary>
        ''' <param name="Host">The host you intend to try and connect to (e.g. localhost, 127.0.0.1 etc..)</param>
        ''' <param name="Port">The port the host uses</param>
        ''' <remarks></remarks>
        Public Sub Connect(Host As String, Port As Integer)
            Dim hostEntry As IPHostEntry = Dns.GetHostEntry(Host)
            Dim address As IPAddress
            For Each address In ResolveAllIPv4FromHostEntry(hostEntry)
                Dim endPoint As New IPEndPoint(address, Port)
                Try
                    BaseSocket.Connect(endPoint)
                Catch
                End Try
                If BaseSocket.Connected Then
                    Receive(BaseSocket)
                    RaiseEvent OnConnected(BaseSocket)
                    Exit For
                End If
            Next address
            If Not BaseSocket.Connected Then
                Throw New Exception("Cannot connect!")
            End If
        End Sub

        Private Function ResolveAllIPv4FromHostEntry(HostEntry As IPHostEntry) As IPAddress()
            ' Loop through the AddressList to obtain the supported AddressFamily. This is to avoid 
            ' an exception that occurs when the host host IP Address is not compatible with the address family 
            ' (typical in the IPv6 case). 
            Dim IPv4Addresses As New List(Of IPAddress)
            For Each IP As IPAddress In HostEntry.AddressList
                If Not IP.ToString.Contains(":") Then IPv4Addresses.Add(IP)
            Next
            Return IPv4Addresses.ToArray
        End Function

        Public Overloads Sub Send(Obj As Object)
            Send(BaseSocket, Obj)
        End Sub

    End Class
End Namespace