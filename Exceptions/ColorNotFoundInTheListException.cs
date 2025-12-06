namespace assecor_assesment_api.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested color is not found in the valid color list.
    /// </summary>
    public class ColorNotFoundInTheListException : Exception
    {
        public string RequestedColor { get; }

        public ColorNotFoundInTheListException(string color)
            : base($"Color '{color}' is not valid. Valid colors are: Blau, Grün, Violett, Rot, Gelb, Türkis, Weiß")
        {
            RequestedColor = color;
        }
    }
}
