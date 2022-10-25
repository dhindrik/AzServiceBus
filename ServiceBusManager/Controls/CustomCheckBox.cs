using System.Runtime.CompilerServices;

namespace ServiceBusManager.Controls
{
	public class CustomCheckBox : CheckBox
	{
        public static BindableProperty ToggleCommandProperty = BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(CustomCheckBox));

        public CustomCheckBox()
		{
		}

        public ICommand? ToggleCommand
        {
            get => GetValue(ToggleCommandProperty) as ICommand;
            set => SetValue(ToggleCommandProperty, value);
        }


        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(propertyName == nameof(IsChecked) && ToggleCommand != null && ToggleCommand.CanExecute(BindingContext))
            {
                ToggleCommand.Execute(BindingContext);
            }
        }
    }
}

