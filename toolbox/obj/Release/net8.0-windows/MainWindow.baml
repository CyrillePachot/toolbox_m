   M S B A M L   `   `   `  ����   toolbox    toolbox.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � Title$  
MainWindow��$	��800��$
��1000��.��� -    �Name$ GrdMain��$��10q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $��* �.+��� $��0��$��0��
���   �$��2q� �� CornerRadius$ 5�� �� Padding$ 5q�$��5q�._���  �� Orientation$ 
Horizontal=�$��Stretch=�.+��� -    ��Name$ BtnOpenFiles��$��5q�$��5q�.��Open File(s)�� $��1��$��0��
���   �$��2q�$ 5��$ 5q�$��5q�._��� $ Vertical=�.+��� -   $ 
BtnCompare��$��5q�$��5q�.��	Compare�� $��
Field Name��$
��Bold"� �� Padding$ 5q�$��5q��� -   	 ��Name$	 TxtBxFieldNameEdit��$��5q�$��5q��� $��Field Value��$
��Bold"�$ 5q�$��5q��� -   $	 TxtBxFieldValueEdit��$��5q�$��5q��� -   $ 
BtnReplace��$��5q�$��5q�.��	Replace�� $��Field Parent��$
��Bold"�$ 5q�$��5q��� -   $	 TxtBxFieldParentEdit��$��5q�$��5q��� -   $ BtnAdd��$��5q�$��5q�.��Add�� $��0��$��1��$��2��
���   �$��2q�$ 5��$ 5q�$��5q�._��� $ Vertical=�.+�� $��Stretch=� �� $��* ��� $
��Auto �.+��� $��0��$��Files��$
��Bold"�$ 5q�$��5q��� -	   $ BtnClose��$��1��$��5q�$��5q�.��Close�� -
   
 ��Name$
 TabCtrlFiles��$��5q�$��5q��� -    ��Name$ StkPnlSelectedCompare��$y�	Collapsed=�.+��� $ 
Horizontal=�.+��� $��Field parent��$
��Bold"�$ 5q�$��5q��� -   $	 TxtBxFieldParentFiles�� �� AcceptsReturn$
 True�� �� 
IsReadOnly$
 True��$��5q�$��5q��� $ 
Horizontal=�.+��� $��
Field Name��$
��Bold"�$ 5q�$��5q��� -   $	 TxtBxFieldNameFiles��$
 True��$
 True��$��5q�$��5q��� $��Line Number��$
��Bold"�$ 5q�$��5q��� -   $	 TxtBxLineNumberFiles��$
 True��$
 True��$��5q�$��5q��� -   $ StkPnlValuesFiles��$ Vertical=��� -   $ BtnSave��$��5q�$��5q�.��Save�� $��2��$��0��$��2��
���   �$��2q�$ 5��$ 5q�$��5q�._��� $ Vertical=�.+�� $��Stretch=� �� $��* ��� $
��Auto �.+��� $��Results��$
��Bold"�$��0��$ 5q�$��5q��� $ 
Horizontal=�$��1��.+��� -   $ BtnCopy��$��5q�$��5q�.��Copy�� -   $ 	BtnExport��$��5q�$��5q�.��Export�� -   $ BtnClear��$��5q�$��5q�.��Clear�� -   $ 	BtnReduce��$��5q�$��5q�$y�Visible=�.��Reduce�� -   $ 	BtnExpand��$��5q�$��5q�$y�	Collapsed=�.��Expand�� -   $	 TxtBxResult��$
 True��$
 True��" �� VerticalScrollBarVisibility$
 Auto=� �� TextWrapping$
 Wrap=�$	��400��$��5q�$��5q�