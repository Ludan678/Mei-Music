using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mei_Music.Models
{
    public class Song : INotifyPropertyChanged
    {
        public string? Index { get; set; }
        public string? Name { get; set; }

        private double volume = 50.0;
        public double Volume
        {
            get => volume;
            set
            {
                if (volume != value)
                {
                    volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
