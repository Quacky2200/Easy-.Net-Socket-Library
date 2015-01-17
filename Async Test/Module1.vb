Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Threading
Imports MicroLibrary
Imports MicroLibrary.Networking
Imports MicroLibrary.Networking.Serializable

Module Module1

    Dim Benchmarks As New Dictionary(Of String, Long)
    Dim PrettyPrintDictionary As New Dictionary(Of String, List(Of String))

    Dim mEngine As New JsonSerializerEngine
    Public WithEvents client As New Networking.Client.TcpClient(mEngine)

    Public WithEvents server As New Networking.Server.TcpServer(mEngine, 4237)

    Private Sub server_OnReceive(sender As Socket, obj As Object, BytesReceived As Integer) Handles server.OnReceive
        Select Case obj.GetType
            Case GetType(Message)
                Dim M As Message = DirectCast(obj, Message)
                Dim MessageID As String = M.Name
                Dim sw As New Threading.SpinWait
                Do Until Benchmarks.ContainsKey(M.Name)
                    sw.SpinOnce()
                Loop
                Dim Elapsed As TimeSpan = TimeSpan.FromTicks(Stopwatch.GetTimestamp - Benchmarks(M.Name))

                Do Until PrettyPrintDictionary(MessageID).Count = 2
                    sw.SpinOnce()
                Loop
                PrettyPrintDictionary(MessageID).Add(String.Format("    {0} Received, Elapsed {1}", BytesReceived, Elapsed.ToString))
                PrettyPrintDictionary(MessageID).Add(String.Format("    '{0}'", M.Message))
                PrettyPrintDictionary(MessageID).Add("]")
                PrettyPrint(MessageID)
        End Select
    End Sub

    Sub Main()
        server.Listen(1000)
        client.Connect("127.0.0.1", 4237)

        Console.WriteLine("Press enter to run benchmark..")
        Console.ReadLine()

        Do
            Console.Clear()
            WorkLoop()
            Console.ReadLine()
        Loop

        server.Close()
    End Sub

    Public Sub WorkLoop()
        Dim MsgData As String = DuplicateString(GenerateCode(1000), 1000)
        Dim SendAmount As Integer = 100000


        For i As Integer = 0 To SendAmount
            Dim MessageID As String = Guid.NewGuid.ToString
            Dim MSG As New Message(MessageID, MsgData)
            Dim start As Long = Stopwatch.GetTimestamp
            Dim List As List(Of String) = New List(Of String)
            DoUntilWorked(Sub() PrettyPrintDictionary.Add(MessageID, List))
            DoUntilWorked(Sub() Benchmarks.Add(MessageID, start))
            client.Send(MSG)
            Dim elapsed As TimeSpan = TimeSpan.FromTicks(Stopwatch.GetTimestamp - start)

            List.Add(MessageID & ": [")
            List.Add("    Message Sent!")

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

    Public Sub PrettyPrint(MessageID As String)
        Dim AllBlocks As String = Nothing
        Dim Block As String = String.Join(vbNewLine, PrettyPrintDictionary(MessageID).ToArray) & vbNewLine
        AllBlocks = AllBlocks & String.Join(vbNewLine, {AllBlocks, Block})
        Console.WriteLine(AllBlocks)
        PrettyPrintDictionary.Remove(MessageID)
        Benchmarks.Remove(MessageID)
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
