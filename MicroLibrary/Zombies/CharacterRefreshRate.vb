Imports System.Net

Namespace Zombies
    Public Class CharacterRefreshRate

        <Serializable>
        Public Class CharacterRefreshState
            Public Property EndPoint As IPEndPoint
            Public Property Position As Windows.Point
            Public Property Rotation As Double
            Public Property CursorPosition As Windows.Point
            Public Property MouseStatus As MouseButton
            Sub New(EndPoint As IPEndPoint, Position As Windows.Point, Rotation As Double, CursorPosition As Windows.Point, MouseStatus As MouseButton)
                Me.EndPoint = EndPoint
                Me.Position = Position
                Me.Rotation = Rotation
                Me.CursorPosition = CursorPosition
                Me.MouseStatus = MouseStatus
            End Sub
            Sub New()

            End Sub
        End Class

    End Class
End Namespace
