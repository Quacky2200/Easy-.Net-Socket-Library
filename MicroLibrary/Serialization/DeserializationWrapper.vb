Imports System.Reflection

Namespace Serialization
    <Serializable>
    Public Class DeserializationWrapper
        Public Shared Property ReflectionFlags As BindingFlags = BindingFlags.Instance Or BindingFlags.[Public]
        Public Sub New()
        End Sub

        Public Sub New(Obj As Object, Protocol As ISerializationProtocol)
            Me.Type = Obj.GetType.AssemblyQualifiedName
            Me.Data = Obj
        End Sub

        Public Property Type As String
        Public Property Data As Object

        Public Function GetInitializedObject(sender As ISerializationProtocol) As Object
            Dim theType As Type = System.Type.GetType(Type)
            Dim instance As Object = Activator.CreateInstance(theType)
            GetPropertyReferences(instance).ForEach(Sub(r) SetProperty(r, sender))
            Return instance
        End Function

        Public Sub SetProperty(Reference As PropertyReference, Protocol As ISerializationProtocol)
            With Reference
                If .PropertyInfo.CanWrite Then
                    Dim v = Protocol.GetPropertyValue( .PropertyInfo.Name, .PropertyIndex, Data)
                    .PropertyInfo.SetValue( .Instance, v, Nothing)
                End If
            End With
        End Sub

        Public Shared Function GetPropertyReferences(target As Object) As List(Of PropertyReference)
            Return GetPropertyReferences(target.GetType, ReflectionFlags)
        End Function

        Public Shared Function GetPropertyReferences(target As Object, ReflectionFlags As BindingFlags) As List(Of PropertyReference)
            Return GetPropertyReferences(target.GetType, ReflectionFlags)
        End Function

        Public Shared Function GetPropertyReferences(Type As Type) As List(Of PropertyReference)
            Return GetPropertyReferences(Type, ReflectionFlags)
        End Function

        Public Shared Function GetPropertyReferences(Type As Type, ReflectionFlags As BindingFlags) As List(Of PropertyReference)
            Dim objProperties As PropertyInfo() = Type.GetProperties(ReflectionFlags)
            Dim instance As Object = Activator.CreateInstance(Type)
            Dim index As Integer = 0
            Dim References As New List(Of PropertyReference)
            For Each p As PropertyInfo In objProperties
                References.Add(New PropertyReference(instance, p, index))
                index += 1
            Next
            Return References
        End Function

        Public Shared Function GetMethodReferences(Type As Type, ReflectionFlags As BindingFlags) As List(Of MethodReference)
            Dim objMethods As MethodInfo() = Type.GetMethods(ReflectionFlags)
            Dim instance As Object = Activator.CreateInstance(Type)
            Dim index As Integer = 0
            Dim References As New List(Of MethodReference)
            For Each m As MethodInfo In objMethods
                References.Add(New MethodReference(instance, m, index))
                index += 1
            Next
            Return References
        End Function

    End Class

    Public Class PropertyReference

        Public Sub New(Instance As Object, PropertyInfo As PropertyInfo, PropertyIndex As Integer)
            Me.Instance = Instance
            Me.PropertyInfo = PropertyInfo
            Me.PropertyIndex = PropertyIndex
        End Sub

        Public Property Instance As Object
        Public Property PropertyInfo As PropertyInfo
        Public Property PropertyIndex As Integer

    End Class

    Public Class MethodReference

        Public Sub New(Instance As Object, MethodInfo As MethodInfo, MethodIndex As Integer)
            Me.Instance = Instance
            Me.MethodInfo = MethodInfo
            Me.MethodIndex = MethodIndex
        End Sub

        Public Property Instance As Object
        Public Property MethodInfo As MethodInfo
        Public Property MethodIndex As Integer

    End Class
End Namespace