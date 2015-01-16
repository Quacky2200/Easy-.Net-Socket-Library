Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports MattJamesLibrary
Imports MattJamesLibrary.Networking
Imports MattJamesLibrary.Networking.Serializable
Imports ProjectZ.Shared

Module Module1

    Dim s As New Stopwatch

    Dim mEngine As New JsonSerializerEngine
    Public WithEvents client As New Networking.Client.ClientTCPSocket(mEngine)

    Public WithEvents server As New Networking.Server.ServerTCPSocket(mEngine, 4237)

    Private Sub server_OnReceive(sender As Socket, obj As Object, BytesReceived As Integer) Handles server.OnReceive
        Select Case obj.GetType
            Case GetType(Message)
                s.Stop()
                Dim M As Message = DirectCast(obj, Message)
                Console.WriteLine(String.Format("Receive Time {1}.. ( {0} Bytes Received )", BytesReceived, s.Elapsed.ToString))
                s.Reset()
        End Select
    End Sub

    Sub Main()
        server.Listen(1000)

        s.Start()

        client.Connect("127.0.0.1", 4237)
        s.Stop()
        Console.WriteLine(String.Format("Initial Connection Time {0}", {s.Elapsed.ToString}))
        s.Reset()
        Console.ReadLine()
        Do
            Console.Clear()
            WorkLoop()
            Console.ReadLine()
        Loop

        server.Close()
    End Sub

    Private Sub WorkLoop()

        s.Start()
        Dim MSG As New Message(My.Computer.Name, GenerateCode(40005))
        s.Stop()
        Console.WriteLine(String.Format("Message Initialization Time {0}", s.Elapsed.ToString))
        s.Reset()

        For i As Integer = 0 To 0
            doSend(MSG)
            System.Threading.Thread.Sleep(15)
        Next

    End Sub

    Private Async Sub doSend(msg As Message)
        s.Start()
        Dim BytesSent As Integer = Await client.SendTask(msg)
        Console.WriteLine(String.Format("Send Time {1} ( {0} Bytes Sent )", BytesSent, s.Elapsed.ToString))
    End Sub

    Public Function GenerateCode(Optional ByVal intnamelength As Integer = 10) As String
        Dim intrnd As Object
        Dim intstep As Object
        Dim strname As Object
        Dim intlength As Object
        Dim strinputstring As Object
        strinputstring = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        intlength = Len(strinputstring)
        Randomize()
        strname = ""
        For intstep = 1 To intnamelength
            intrnd = Int((intlength * Rnd()) + 1)
            strname = strname & Mid(strinputstring, intrnd, 1)
        Next
        Return strname
    End Function

    Private Sub server_OnError(sender As Socket, e As Exception) Handles server.OnError

    End Sub

    Private Sub server_OnConnectionInterrupt(sender As Socket) Handles server.OnConnectionInterrupt

    End Sub
End Module
