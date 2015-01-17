Namespace Networking.Serializable
    <Serializable>
    Public Enum CommandType As Short
        Disconnect = -1
        Broadcast = 0
        Connect = 1
        Join = 2
        JoinSuccessful = 3
        ReadyOff = 4
        ReadyOn = 5
        StartGame = 6
        ResyncCharacters = 7
        ReadyUp = 8
        Other = 9
        AddName = 20
    End Enum
End Namespace