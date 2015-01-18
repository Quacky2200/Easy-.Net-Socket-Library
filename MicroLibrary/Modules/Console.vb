Imports System.Threading

Public Module ConsoleTricks
    Function GetResponse(str As String, ClearConsole As Boolean) As String
        If ClearConsole Then Console.Clear()
        Console.WriteLine(str)
        Return Console.ReadLine
    End Function
    Function GetResponse(str As String) As String
        Return GetResponse(str, False)
    End Function
    Sub WaitSec(int As Integer)
        Dim NewSecond As Byte = DateAndTime.TimeOfDay.Second + 5
        Dim sw As New SpinWait
        Do Until DateAndTime.TimeOfDay.Second >= NewSecond
            sw.SpinOnce()
        Loop
    End Sub
End Module