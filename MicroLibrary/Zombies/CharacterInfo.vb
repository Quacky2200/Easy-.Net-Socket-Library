Imports System.Net

Namespace Zombies
    <Serializable()>
    Public Class CharacterInfo
        'Identity, name and their IP Address for recognition
        Public Property ID As String = Guid.NewGuid.ToString
        Public Property EndPoint As IPEndPoint
        Public Property Username As String
        'Sound and shooting events happen with these switches
        Public Property isShooting As Boolean = False
        Public Property Reloading As Boolean = False
        Public Property Talking As Boolean = False
        'Character health & death states
        Public Property Health As Integer = 100
        Public Property Dead As Boolean = False
        'Character leaderboard data
        Public Property Points As Integer = 0
        Public Property Kills As Integer = 0
        'Character bullet properties
        'Public Property MouseStatus As MouseButton
        Public Property Gun As String = "Pistol"
        Public Property Bullets As Integer = 750
        Public Property BulletStrength As Integer = 10
        Public Property BulletsInClip As Integer = 15
        'Game begin information
        Public Property isReady As Boolean = False
        Sub New(Username As String)
            Me.Username = Username
        End Sub
    End Class
End Namespace