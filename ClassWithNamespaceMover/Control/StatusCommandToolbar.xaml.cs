using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClassWithNamespaceMover.Control {
  /// <summary>
  /// Interaction logic for StatusCommandToolbar.xaml
  /// </summary>
  public partial class StatusCommandToolbar : UserControl {   
    
    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register("StatusText", typeof(String), typeof(StatusCommandToolbar), new UIPropertyMetadata());
    public String StatusText {
      get { return (String)GetValue(StatusTextProperty); }
      set { SetValue(StatusTextProperty, value); }
    }

    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty StatusForegroundProperty =
        DependencyProperty.Register("StatusForeground", typeof(Brush), typeof(StatusCommandToolbar), new UIPropertyMetadata());
    public Brush StatusForeground {
      get { return (Brush)GetValue(StatusForegroundProperty); }
      set { SetValue(StatusForegroundProperty, value); }
    }

    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty RunCommandProperty =
        DependencyProperty.Register("RunCommand", typeof(ICommand), typeof(StatusCommandToolbar), new UIPropertyMetadata());
    public ICommand RunCommand {
      get { return (ICommand)GetValue(RunCommandProperty); }
      set { SetValue(RunCommandProperty, value); }
    }

    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty ClearCommandProperty =
        DependencyProperty.Register("ClearCommand", typeof(ICommand), typeof(StatusCommandToolbar), new UIPropertyMetadata());
    public ICommand ClearCommand {
      get { return (ICommand)GetValue(ClearCommandProperty); }
      set { SetValue(ClearCommandProperty, value); }
    }


    public StatusCommandToolbar() {
      InitializeComponent();
    }
  }
}
