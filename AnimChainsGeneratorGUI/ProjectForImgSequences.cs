using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsGenerator
{
    public class ProjectForImgSequences : INotifyPropertyChanged
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


        private string _InputDir;
        public string InputDir
        {
            get { return _InputDir; }
            set { SetField(ref _InputDir, value, nameof(InputDir)); }
        }

        private string _OutputDir;
        public string OutputDir
        {
            get { return _OutputDir; }
            set { SetField(ref _OutputDir, value, nameof(OutputDir)); }
        }

        private string _SpriteSheetFileName;
        public string SpriteSheetFileName
        {
            get { return _SpriteSheetFileName; }
            set { SetField(ref _SpriteSheetFileName, value, nameof(SpriteSheetFileName)); }
        }

        private string _AchxFileName;
        public string AchxFileName
        {
            get { return _AchxFileName; }
            set { SetField(ref _AchxFileName, value, nameof(AchxFileName)); }
        }


        private int _MaxSpriteSheetWidth = 4096;
        public int MaxSpriteSheetWidth
        {
            get { return _MaxSpriteSheetWidth; }
            set { SetField(ref _MaxSpriteSheetWidth, value, nameof(MaxSpriteSheetWidth)); }
        }


        private bool _OpenCreated;
        public bool OpenCreated
        {
            get { return _OpenCreated; }
            set { SetField(ref _OpenCreated, value, nameof(OpenCreated)); }
        }
    }
}
