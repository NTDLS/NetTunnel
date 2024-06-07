namespace NetTunnel.UI.Helpers
{
    internal class FormValidationHelpers
    {
        internal static int GetAndValidateInteger(TextBox textBox, int minValue, int maxValue, string message)
        {
            message = message.Replace("[min]", $"{minValue:n0}", StringComparison.InvariantCultureIgnoreCase);
            message = message.Replace("[max]", $"{maxValue:n0}", StringComparison.InvariantCultureIgnoreCase);

            if (int.TryParse(textBox.Text.Replace(",", ""), out var value))
            {
                if (value < minValue || value > maxValue)
                {
                    throw new Exception(message);
                }
                return value;
            }
            throw new Exception(message);
        }

        internal static double GetAndValidateDouble(TextBox textBox, double minValue, double maxValue, string message)
        {
            message = message.Replace("[min]", $"{minValue:n0}", StringComparison.InvariantCultureIgnoreCase);
            message = message.Replace("[max]", $"{maxValue:n0}", StringComparison.InvariantCultureIgnoreCase);

            if (double.TryParse(textBox.Text.Replace(",", ""), out var value))
            {
                if (value < minValue || value > maxValue)
                {
                    throw new Exception(message);
                }
                return value;
            }
            throw new Exception(message);
        }
    }
}
