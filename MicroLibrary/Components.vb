
Public Class TimeTracker
#Region "Properties"
    Private Property _StartedRecording As DateTime
    Public ReadOnly Property StartedAt As DateTime
        Get
            Return _StartedRecording
        End Get
    End Property
    Private Property _StoppedRecording As DateTime
    Public ReadOnly Property StoppedAt As DateTime
        Get
            Return _StoppedRecording
        End Get
    End Property
    Private Property TrackedTime As TimeSpan
    Public ReadOnly Property Result As String
        Get
            Return If(TrackedTime.Hours > 0, TrackedTime.Hours & "Hrs ", Nothing) &
                    If(TrackedTime.Minutes > 0, TrackedTime.Minutes & "Mns ", Nothing) &
                    If(TrackedTime.Seconds > 0, TrackedTime.Seconds & "Scs ", Nothing) &
                    If(TrackedTime.Milliseconds > 0, TrackedTime.Milliseconds & "Ms ", Nothing)
        End Get
    End Property
#End Region
    Public Sub [Start]()
        _StartedRecording = DateTime.Now
    End Sub
    Public Function [Stop]()
        _StoppedRecording = DateTime.Now
        TrackedTime = If(StartedAt.TimeOfDay > StoppedAt.TimeOfDay, StartedAt.TimeOfDay - StoppedAt.TimeOfDay, StoppedAt.TimeOfDay - StartedAt.TimeOfDay)
        Return Result
    End Function
End Class

Public Class RunOnceTimer
    Private RunTimer As Timers.Timer
    Sub New(interval As Integer, EventHandler As Action)
        RunTimer = New Timers.Timer(interval)
        AddHandler RunTimer.Elapsed, Sub()
                                         EventHandler.Invoke()
                                         RunTimer.Stop()
                                     End Sub
        RunTimer.Start()
    End Sub
    Sub Stop_RunOnceTimer()
        RunTimer.Stop()
    End Sub
    Sub Resume_RunOnceTimer()
        RunTimer.Start()
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