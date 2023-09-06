using ClassWithNamespaceMover.Binding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ClassWithNamespaceMover {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    private static String envVariable = "ClassWithNamespaceMover";

    /// <summary></summary>
    public MainWindow() {
      InitializeComponent();
      ResetStatus();
      String? folder = Environment.GetEnvironmentVariable(envVariable, EnvironmentVariableTarget.User);
      if (!String.IsNullOrEmpty(folder)) {
        FolderPath = folder;
      }
    }

    private ICommand _runCommand;
    public ICommand RunCommand {
      get {
        if (_runCommand == null) {
          _runCommand = new RelayCommand(
              param => RunAsync()
          );
        }
        return _runCommand;
      }
    }

    private ICommand _clearCommand;
    public ICommand ClearCommand {
      get {
        if (_clearCommand == null) {
          _clearCommand = new RelayCommand(
              param => Clear()
          );
        }
        return _clearCommand;
      }
    }

    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register("StatusText", typeof(String), typeof(MainWindow));
    public String StatusText {
      get { return (String)GetValue(StatusTextProperty); }
      set { SetValue(StatusTextProperty, value); }
    }

    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty StatusForegroundProperty =
        DependencyProperty.Register("StatusForeground", typeof(Brush), typeof(MainWindow), new UIPropertyMetadata());
    public Brush StatusForeground {
      get { return (Brush)GetValue(StatusForegroundProperty); }
      set { SetValue(StatusForegroundProperty, value); }
    }

    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty FolderPathProperty =
        DependencyProperty.Register("FolderPath", typeof(String), typeof(MainWindow));
    public String FolderPath {
      get { return (String)GetValue(FolderPathProperty); }
      set { SetValue(FolderPathProperty, value); }
    }

    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty NewNamespaceNameProperty =
        DependencyProperty.Register("NewNamespace", typeof(String), typeof(MainWindow));
    public String NewNamespace {
      get { return (String)GetValue(NewNamespaceNameProperty); }
      set { SetValue(NewNamespaceNameProperty, value); }
    }

    // The dependency property which will be accessible on the UserControl
    public static readonly DependencyProperty MovedClassNamesProperty =
        DependencyProperty.Register("MovedClassNames", typeof(String), typeof(MainWindow));
    public String MovedClassNames {
      get { return (String)GetValue(MovedClassNamesProperty); }
      set { SetValue(MovedClassNamesProperty, value); }
    }

    private async void HandleChooseOutputFolderPathClick(object sender, RoutedEventArgs e) {
      System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
      folderDlg.SelectedPath = FolderPath;
      if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
        FolderPath = folderDlg.SelectedPath;
        String path = folderDlg.SelectedPath;
        await Task.Run(() => Environment.SetEnvironmentVariable(envVariable, path, EnvironmentVariableTarget.User));
      }
    }

    private async void RunAsync() {
      if (String.IsNullOrWhiteSpace(FolderPath)
        || String.IsNullOrWhiteSpace(NewNamespace)
        || String.IsNullOrWhiteSpace(MovedClassNames)) {
        StatusText = "Alle Felder ausfüllen";
        StatusForeground = Brushes.DarkRed;
        return;
      }

      if (!Directory.Exists(FolderPath)) {
        StatusText = "Ausgwählter Ordner exisitert nicht";
        StatusForeground = Brushes.DarkRed;
        return;
      }
      StatusText = "Vorgang läuft...";
      StatusForeground = Brushes.Black;

      var oldCursor = Cursor;
      try {
        Cursor = Cursors.Wait;

        String folderPath = FolderPath;
        String newNamespace = NewNamespace;
        String movedClassNames = MovedClassNames;
        await Task.Run(() => Moving(folderPath, newNamespace, movedClassNames));
      }
      finally {
        Cursor = oldCursor;
      }
      ResetStatus();
    }

    private static void Moving(String folderPath, String newNamespace, String movedClassNames) {
      Regex nameSpacePattern = new Regex("(namespace( )*[A-Za-z0-9.]*( )*(;|{))");
      Regex usingNewNamespacePattern = new Regex($"(using)( )*({newNamespace})( )*(;)");
      Regex usingPattern = new Regex($"(using)( )*[A-Za-z0-9.]*( )*(;)");
      String classes = String.Join("|", movedClassNames.Split(",", StringSplitOptions.TrimEntries));
      Regex classNamesUsagePattern = new Regex($@"(new||\()( )*[^A-Za-z0-9]({classes})[^A-Za-z0-9]( |.|\()");
      Regex classNamesDefinitionPattern = new Regex($@"(class|interface|enum)( )*({classes})( )*({{|:)");

      DirectoryInfo parentFolder = new DirectoryInfo(folderPath);
      var files = GetFiles(parentFolder, "*.cs");
      foreach (var file in files) {
        try {
          String fileContent = File.ReadAllText(file.FullName, new UTF8Encoding(true));
          var nameSpaceMatches = nameSpacePattern.Match(fileContent); // contains a valid namespace --> must be a valid class, can be subject to add the using
          if (!nameSpaceMatches.Success) {
            continue;
          }
          var newNamespaceMatches = usingNewNamespacePattern.Match(fileContent); // using already present --> not needed again
          if (newNamespaceMatches.Success) {
            continue;
          }
          var containsClass = classNamesUsagePattern.Match(fileContent); // class used in the context of this class
          if (!containsClass.Success) {
            continue;
          }
          var containsClassDefinition = classNamesDefinitionPattern.Match(fileContent); // class should not be defined in the found class --> using only needed in other classes not in the same
          if (containsClassDefinition.Success) {
            continue;
          }

          var fileStartSection = fileContent.Substring(0, nameSpaceMatches.Index);
          var lastUsing = usingPattern.Match(fileStartSection);
          if (lastUsing.Success) {
            fileContent = fileContent.Insert(lastUsing.Index, $"using {newNamespace};{Environment.NewLine}");
          }
          else { // case first using in file
            fileContent = fileContent.Insert(nameSpaceMatches.Index, $"using {newNamespace};{Environment.NewLine}{Environment.NewLine}");
          }
          File.WriteAllText(file.FullName, fileContent, new UTF8Encoding(true));
        }
        catch (Exception ex) {
          Console.WriteLine(ex.ToString());
        }
      }
    }

    private static List<FileInfo> GetFiles(DirectoryInfo parentFolder, String fileSearchPattern) {
      if (parentFolder == null) {
        return new List<FileInfo>();
      }
      List<FileInfo> files = new List<FileInfo>();
      files.AddRange(parentFolder.GetFiles(fileSearchPattern));
      foreach (var folder in parentFolder.GetDirectories()) {
        files.AddRange(GetFiles(folder, fileSearchPattern));
      }
      return files;
    }

    private void Clear() {
      FolderPath = "";
      NewNamespace = "";
      MovedClassNames = "";
      ResetStatus();
    }

    private void ResetStatus() {
      StatusText = "Alle Systeme bereit";
      StatusForeground = Brushes.Black;
    }
  }
}
