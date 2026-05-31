using Microsoft.UI.Xaml;

namespace RiskFlow
{
    /// <summary>
    /// Relais permettant de lier des éléments situés dans un DataTemplate (ListView) à un
    /// objet externe (ici le ViewModel), via une ressource statique.
    /// </summary>
    public partial class BindingProxy : DependencyObject
    {
        public object? Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
    }
}
