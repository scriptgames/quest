﻿Public Class EditorControl
    Implements IElementEditorControl
    Implements IAdjustableHeightControl

    ' This class needs to give the option to have the contained control underneath the caption,
    ' or to the right at some position.

    ' We probably need some events to set a requested size for this CaptionControl when the contained control changes size.

    Private Const k_maxCaptionWidth As Integer = 100

    Private WithEvents m_editorControl As IElementEditorControl
    Private WithEvents m_adjustableHeightEditorControl As IAdjustableHeightControl
    Private m_editorControlIsCheckbox As Boolean
    Private m_checkBoxControl As CheckBoxControl
    Private m_attribute As String
    Private m_controller As EditorController
    Private m_hasFixedWidth As Boolean = False
    Private m_controlUnderCaption As Boolean = False
    Private m_definition As IEditorControl
    Private m_expand As Boolean

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        Dim createType As Type = controller.GetControlType(controlData.ControlType)

        If createType Is Nothing Then
            Throw New Exception(String.Format("Unable to create control of type '{0}'", controlData.ControlType))
        End If

        m_editorControl = DirectCast(Activator.CreateInstance(createType), IElementEditorControl)
        m_editorControlIsCheckbox = TypeOf m_editorControl Is CheckBoxControl
        If TypeOf m_editorControl Is IAdjustableHeightControl Then
            m_adjustableHeightEditorControl = DirectCast(m_editorControl, IAdjustableHeightControl)
        End If
        m_editorControl.Control.Parent = Control
        m_editorControl.Control.Top = 0
        If m_editorControlIsCheckbox Then
            lblCaption.Visible = False
            m_editorControl.Control.Left = 0
            m_checkBoxControl = DirectCast(m_editorControl, CheckBoxControl)
        Else
            lblCaption.Visible = True
            m_editorControl.Control.Left = 50
            m_checkBoxControl = Nothing
        End If
        m_editorControl.Controller = m_controller
        Me.Height = m_editorControl.Control.Height

        m_attribute = controlData.Attribute
        m_editorControl.Initialise(controller, controlData)

    End Sub

    Public Property Caption() As String
        Get
            Return lblCaption.Text
        End Get
        Set(value As String)
            If Not m_editorControlIsCheckbox Then
                lblCaption.Text = value + ":"
                If lblCaption.Width > k_maxCaptionWidth Then PlaceControlUnderCaption()
            Else
                m_checkBoxControl.SetCaption(value)
            End If
        End Set
    End Property

    Private Sub PlaceControlUnderCaption()
        Const leftPadding As Integer = 30
        Const paddingUnderCaption As Integer = 5
        lblCaption.Top = 0
        m_editorControl.Control.Left = leftPadding
        m_editorControl.Control.Top = lblCaption.Top + lblCaption.Height + paddingUnderCaption
        m_editorControl.Control.Width = Me.Width - leftPadding
        Me.Height = m_editorControl.Control.Top + m_editorControl.Control.Height
        m_editorControl.Control.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right
        m_controlUnderCaption = True
    End Sub

    Public ReadOnly Property Control() As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public ReadOnly Property CaptionTextWidth() As Integer
        Get
            If m_controlUnderCaption Then Return 0
            Return lblCaption.Width
        End Get
    End Property

    Public Sub SetCaptionWidth(width As Integer)
        If m_editorControlIsCheckbox Then Return
        If m_controlUnderCaption Then Return

        m_editorControl.Control.Left = width
        If Not m_hasFixedWidth Then
            m_editorControl.Control.Width = Me.Width - width
            m_editorControl.Control.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right
        Else
            m_editorControl.Control.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom
        End If
    End Sub

    Public Sub SetControlHeight(height As Integer)
        If Not m_controlUnderCaption Then
            m_editorControl.Control.Height = height
            Me.Height = m_editorControl.Control.Height
        Else
            m_editorControl.Control.Height = height - lblCaption.Height
            Me.Height = lblCaption.Height + m_editorControl.Control.Height
        End If
    End Sub

    Public Sub SetFixedControlWidth(width As Integer)
        m_editorControl.Control.Width = width
        m_hasFixedWidth = True
    End Sub

    Public Property Value() As Object Implements IElementEditorControl.Value
        Get
            Return m_editorControl.Value
        End Get
        Set(value As Object)
            m_editorControl.Value = value
        End Set
    End Property

    Private Sub m_editorControl_Dirty(sender As Object, args As DataModifiedEventArgs) Handles m_editorControl.Dirty
        RaiseEvent Dirty(Me, args)
    End Sub

    Public Property Controller() As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
        End Set
    End Property

    Public Sub SaveData(data As IEditorData)
        Save(data)
    End Sub

    Public ReadOnly Property AttributeName() As String Implements IElementEditorControl.AttributeName
        Get
            Return m_attribute
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
        m_editorControl.Save(data)
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        m_editorControl.Populate(data)
    End Sub

    Private Sub m_editorControl_RequestParentElementEditorSave() Handles m_editorControl.RequestParentElementEditorSave
        RaiseEvent RequestParentElementEditorSave()
    End Sub

    Public Event HeightChanged(sender As Object, newHeight As Integer) Implements IAdjustableHeightControl.HeightChanged

    Private Sub m_adjustableHeightEditorControl_HeightChanged(sender As Object, newHeight As Integer) Handles m_adjustableHeightEditorControl.HeightChanged
        If m_expand Then Return
        Me.Height = m_editorControl.Control.Top + newHeight
        RaiseEvent HeightChanged(Me, newHeight)
    End Sub

    Public Property Definition As IEditorControl
        Get
            Return m_definition
        End Get
        Set(value As IEditorControl)
            m_definition = value
        End Set
    End Property

    ' If Expand is set, this control automatically takes up all the remaining space in the parent ElementEditor,
    ' so we don't want it to try to resize itself in response to changes in its contents.
    Public Property Expand As Boolean
        Get
            Return m_expand
        End Get
        Set(value As Boolean)
            m_expand = value
        End Set
    End Property
End Class
