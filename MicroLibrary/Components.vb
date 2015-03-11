Public Class ImprovedSpinWait

    Public Overloads Shared Sub SpinFor(millisecondsTimeout As Double)
        SpinFor(CLng(millisecondsTimeout * TimeSpan.TicksPerMillisecond))
    End Sub

    Public Overloads Shared Sub SpinFor(Ticks As Long)
        Dim s As New Stopwatch
        s.Start()
#If NET20 Or NET30 Or NET35 Then
        Do Until s.Elapsed.Ticks >= Ticks
            Threading.Thread.SpinWait(1)
        Loop
#Else
        Threading.SpinWait.SpinUntil(Function() s.Elapsed.Ticks >= Ticks)
#End If
        s.Stop()
    End Sub

End Class

Public Enum MouseButton As Short
    Up = 0
    Down = 1
End Enum

Enum Powerup_Type As Byte
    Nuke = 0
    Ammo = 1
    MP5 = 2
    Sniper = 3
    Pistol = 4
    Health = 5
    General = 6
End Enum