Namespace Networking.Serializable
    <Serializable()>
    Public Class Message
        Public Property Message As String
        Public Property Name As String
        Public Property TimeStamp As String = DateTime.Now

        Sub New()
        End Sub
        Sub New(Name As String, Message As String)
            Me._Message = Message
            Me._Name = Name
            Me._TimeStamp = DateTime.Now
        End Sub
    End Class
End Namespace