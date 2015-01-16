Module Helpers

    Public Property PowerUpSounds As New Dictionary(Of Powerup_Type, List(Of String))

    Public Function CheckEnum(ByRef Text As String) As Powerup_Type
        For Each Value As Powerup_Type In [Enum].GetValues(GetType(Powerup_Type)) 'We get each value in the enumeration
            If Text.ToLower.Contains(Value.ToString.ToLower) Then 'detect whether it contains the subject name (allows us to comment on the subject line like //(own)emotions)
                Return Value 'Return the value we want
                Exit Function 'We won't need to search the other lists (esp. if the list's long)
            End If
        Next
        Return Powerup_Type.General 'If we never found a suitable subject, we'll ignore the word/letter
    End Function

    Public Sub GrabVoiceEffects()
        Dim FileCol As IEnumerable(Of String) = System.IO.Directory.EnumerateFiles(System.IO.Directory.GetCurrentDirectory() & "\Sounds\powerups")
        For Each Str As String In FileCol
            If Str.EndsWith(".wav") Then
                If PowerUpSounds.ContainsKey(CheckEnum(Str)) Then
                    PowerUpSounds.Item(CheckEnum(Str)).Add(Str)
                Else
                    PowerUpSounds.Add(CheckEnum(Str), New List(Of String))
                    PowerUpSounds.Item(CheckEnum(Str)).Add(Str)
                End If
            End If
        Next
    End Sub

    Public Function ResponseTimeFromPing(Host As String) As String
        Dim Result As Net.NetworkInformation.PingReply
        Dim SendPing As New Net.NetworkInformation.Ping
        Try
            Result = SendPing.Send(Host)
            If Result.Status = Net.NetworkInformation.IPStatus.Success Then
                Return Result.RoundtripTime & "ms"
            Else
                Return CType(Result.Status, Net.NetworkInformation.IPStatus).ToString
            End If
        Catch ex As Exception
            Debug.Print("Error in MattJames.Networking.General.ResponseTimeFromPing:- {0}", ex.Message)
        End Try
        Return Nothing
    End Function

End Module
