﻿Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Threading
Imports MicroLibrary
Imports MicroLibrary.Networking
Imports MicroLibrary.Networking.Serializable

Module Module1

    Public WithEvents client As New Client.TcpClient(MessagePackSerializerEngine)
    Public WithEvents server As New Server.TcpServer(MessagePackSerializerEngine, 4237)

    Private Sub server_OnReceive(sender As Socket, obj As Object, BytesReceived As Integer) Handles server.OnReceive
        Select Case obj.GetType
            Case GetType(Message)
                Dim M As Message = DirectCast(obj, Message)
                Console.WriteLine("Received Message { " & M.Name & " }")
        End Select
    End Sub

    Sub Main()
        ' Start The Server
        server.Listen(1000)

        ' Connect to the server
        client.Connect("127.0.0.1", 4237)

        Console.WriteLine("Press enter to run benchmark..")
        Console.ReadLine()

        Do
            ' Benchmark Work Look
            Console.Clear()
            WorkLoop()
            Console.ReadLine()
        Loop

        server.Close()
    End Sub

    Public Sub WorkLoop()
        Dim MsgData As String = DuplicateString(GenerateCode(1000), 1000)
        Dim SendAmount As Integer = 1000


        For i As Integer = 0 To SendAmount
            Dim MessageID As String = Guid.NewGuid.ToString
            Dim MSG As New Message(MessageID, MsgData)
            Dim start As Long = Stopwatch.GetTimestamp
            Dim List As List(Of String) = New List(Of String)
            client.Send(MSG)
            Console.WriteLine("Sent Message { " & MessageID & " }")
        Next

    End Sub

    Public Sub DoUntilWorked(ByVal action As Action)
        Dim sw As New SpinWait
        Dim worked As Boolean = False
        Do Until worked
            Try
                action.Invoke()
                worked = True
            Catch ex As Exception
                sw.SpinOnce()
            End Try
        Loop
    End Sub

    Public Function DuplicateString(Input As String, Multiples As Integer) As String
        Dim output As String = Nothing
        For i As Integer = 0 To Multiples - 1
            output = Input & output
        Next
        Return output
    End Function

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

End Module
