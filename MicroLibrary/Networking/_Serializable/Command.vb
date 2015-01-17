﻿Namespace Networking.Serializable
    <Serializable>
    Public Class Command
        Private _Command As CommandType
        Private _CommandAttachment As Object

        Public ReadOnly Property Command As CommandType
            Get
                Return _Command
            End Get
        End Property

        Public ReadOnly Property CommandAttachment As Object
            Get
                Return _CommandAttachment
            End Get
        End Property

        Sub New()
        End Sub
        Sub New(Command As CommandType, Optional CommandAttachment As Object = Nothing)
            _Command = Command
            _CommandAttachment = CommandAttachment
        End Sub


    End Class
End Namespace