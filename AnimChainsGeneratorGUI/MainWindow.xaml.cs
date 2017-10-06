using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Xml.Serialization;
using FlatRedBall.Content.AnimationChain;
using System.Drawing;

namespace AnimChainsGenerator
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region    --- INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            // else
            field = value;

            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion --- INotifyPropertyChanged implementation END

        private ProjectForParams _ProjectForParams = new ProjectForParams();
        public ProjectForParams ProjectForParams
        {
            get { return _ProjectForParams; }
            private set { SetField(ref _ProjectForParams, value, nameof(ProjectForParams)); }
        }


        private ProjectForImgSequences _ProjectForImgSequences = new ProjectForImgSequences();
        public ProjectForImgSequences ProjectForImgSequences
        {
            get { return _ProjectForImgSequences; }
            private set { SetField(ref _ProjectForImgSequences, value, nameof(ProjectForImgSequences)); }
        }




        public MainWindow()
        {
            //ContentRendered += This_ContentRendered;

            DataContext = this;

            InitializeComponent();
        }

        private void This_ContentRendered(object sender, EventArgs e)
        { }




        #region    -- GenerateFromParameters
        private bool ContainsDuplicates<T, TValue>(IEnumerable<T> source, Func<T, TValue> selector)
        {
            var hashSet = new HashSet<TValue>();
            foreach (var t in source)
            {
                if (!hashSet.Add(selector(t)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool DuplicateAnimDefs()
        {
            var hashSet = new HashSet<string>();
            foreach (var animDef in _ProjectForParams.AnimDefinitons)
            {
                if (!hashSet.Add(animDef.AnimName))
                {
                    return true;
                }
            }
            return false;
        }


        private void ButLoadProjectFile_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog SaveFileDialog
            var dialog = new OpenFileDialog
            {
                DereferenceLinks = true, // default is false
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "Open project file",
                Filter = "AnimChainsGenerator project (*.achpx)|*.achpx",
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            //&& !String.IsNullOrWhiteSpace(dialog.FileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ProjectForParams));

                using (var stream = File.OpenRead(dialog.FileName))
                {
                    ProjectForParams = serializer.Deserialize(stream) as ProjectForParams;
                }
            }
        }

        private void ButAddAnimDef_Click(object sender, RoutedEventArgs e)
        {
            _ProjectForParams.AnimDefinitons.Add(new AnimDef { CellXstartIndex = 1, CellYstartIndex = 1 });
        }

        private void ButRemoveAnimDef_Click(object sender, RoutedEventArgs e)
        {
            _ProjectForParams.AnimDefinitons.Remove(
                (AnimDef)(sender as Button).Tag
            );
        }

        private void ButGenerateFromParameters_Click(object sender, RoutedEventArgs e)
        {
            #region    -- Error checking
            if (_ProjectForParams.AnimDefinitons.Count == 0)
            {
                MessageBox.Show("Not Anims defined.");
                return;
            }

            if (_ProjectForParams.SheetFilePath == null)
            {
                MessageBox.Show("Enter SpriteSheet image file.");
                return;
            }

            if (!File.Exists(_ProjectForParams.SheetFilePath))
            {
                MessageBox.Show("SpriteSheet image file not found.");
                return;
            }

            if (_ProjectForParams.OutputAchxFilePath == null)
            {
                MessageBox.Show("Enter Achx output directory.");
                return;
            }

            string achxOutputDir = Path.GetDirectoryName(_ProjectForParams.OutputAchxFilePath);

            if (!Directory.Exists(achxOutputDir))
            {
                MessageBox.Show("Achx output directory doesn't exist.");
                return;
            }


            if (DuplicateAnimDefs())
            {
                MessageBox.Show("Two animations can't have a same name.");
                return;
            }
            #endregion -- Error checking END

            string achxOutputFileNameWOExt = Path.GetFileNameWithoutExtension(_ProjectForParams.OutputAchxFilePath);

            // Save project
            if (CheckBoxSaveProject.IsChecked.Value)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ProjectForParams));
                using (TextWriter writer = new StreamWriter(Path.Combine(achxOutputDir, achxOutputFileNameWOExt + ".achpx")))
                {
                    serializer.Serialize(
                        writer,
                        _ProjectForParams
                    );
                }
            }

            // Remove whitespace
            // Convert AnimDefs to zero based indexing
            var zeroIndexedAnimDefinitons = new AnimDef[_ProjectForParams.AnimDefinitons.Count];
            AnimDef animDefClone;
            for (int i = 0; i < zeroIndexedAnimDefinitons.Length; i++)
            {
                // AnimNames error check
                if (String.IsNullOrWhiteSpace(_ProjectForParams.AnimDefinitons[i].AnimName))
                {
                    MessageBox.Show("All animations must have name defined.\nName can't be all whitespaces.");
                    return;
                }

                // Remove whitespace
                _ProjectForParams.AnimDefinitons[i].AnimName = _ProjectForParams.AnimDefinitons[i].AnimName.Trim();

                // Convert AnimDefs to zero based indexing
                animDefClone = AnimDef.Clone(_ProjectForParams.AnimDefinitons[i]);
                animDefClone.CellXstartIndex -= 1;
                animDefClone.CellYstartIndex -= 1;
                zeroIndexedAnimDefinitons[i] = animDefClone;
            }

            // Create data
            var animChainList = Generator.GenerateFromParameters(
                    // sheet cell size
                    _ProjectForParams.SheetCellSize,
                    // sheet file name only (wo path)
                    Path.GetFileName(_ProjectForParams.SheetFilePath),
                    // 
                    _ProjectForParams.Rotations,
                    //
                    new Offset<float> { X = (float)_ProjectForParams.FramesOffset.X, Y = (float)_ProjectForParams.FramesOffset.Y },
                    // 
                    zeroIndexedAnimDefinitons
                );

            // Save achx
            Generator.SaveAchx(
                // AnimChainListSave generated by Generator.Generate()
                animChainList,
                // - achx
                // output path
                achxOutputDir,
                // output file name wo extension 
                achxOutputFileNameWOExt,
                // path to sprite sheet 
                Path.GetDirectoryName(_ProjectForParams.SheetFilePath)
            );

            MessageBox.Show("Achx generation successful");

            // Run achx
            if (CheckBoxOpenAchx.IsChecked.Value)
            {
                System.Diagnostics.Process.Start(
                    Path.Combine(achxOutputDir, achxOutputFileNameWOExt + ".achx")
                );
            }
        }
        #endregion -- GenerateFromParameters END



        #region    --- GenerateFromImgSequences

        private void ButGenerateFromImgSequences_Click(object sender, RoutedEventArgs e)
        {
            #region    -- Error checking
            if (String.IsNullOrWhiteSpace(_ProjectForImgSequences.InputDir))
            {
                MessageBox.Show("Enter input directory containing sprite image sequences.");
                return;
            }
            if (!Directory.Exists(_ProjectForImgSequences.InputDir))
            {
                MessageBox.Show("Input directory not found.");
                return;
            }

            if (String.IsNullOrWhiteSpace(_ProjectForImgSequences.OutputDir))
            {
                MessageBox.Show("Enter output directory for saving result file.");
                return;
            }
            if (!Directory.Exists(_ProjectForImgSequences.OutputDir))
            {
                MessageBox.Show("Output directory not found.");
                return;
            }

            if (String.IsNullOrWhiteSpace(_ProjectForImgSequences.SpriteSheetFileName))
            {
                MessageBox.Show("Enter result sprite sheet file name.");
                return;
            }

            if (String.IsNullOrWhiteSpace(_ProjectForImgSequences.AchxFileName))
            {
                MessageBox.Show("Enter result achx file name.");
                return;
            }
            #endregion -- Error checking END


            // -- Generate

            AnimationChainListSave animListSave;
            Bitmap spriteSheet;

            try
            {
                Generator.GenerateFromImgSequences(
                    _ProjectForImgSequences.InputDir,
                    _ProjectForImgSequences.SpriteSheetFileName,

                    out animListSave, out spriteSheet
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating:\n\n" + ex.Message);
                return;
            }



            // -- Save files

            string resultAchxFilePath;

            try
            {
                spriteSheet.Save(
                    Path.Combine(_ProjectForImgSequences.OutputDir, _ProjectForImgSequences.SpriteSheetFileName + ".png")
                );

                resultAchxFilePath = Path.Combine(_ProjectForImgSequences.OutputDir, _ProjectForImgSequences.AchxFileName + ".achx");

                animListSave.Save( resultAchxFilePath );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving generated data to files:\n\n" + ex.Message);
                return;
            }



            MessageBox.Show("Achx generation successful");

            // -- Run achx
            if (_ProjectForImgSequences.OpenCreated)
            {
                System.Diagnostics.Process.Start(
                    resultAchxFilePath
                );
            }
        }

        #endregion --- GenerateFromImgSequences END

        
    }
}
