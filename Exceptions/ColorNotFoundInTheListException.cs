namespace assecor_assesment_api.Exceptions
{
    /// <summary>
    /// Thrown when a requested color is not found in the color list (not in enum range 1-7).
    /// </summary>
    public class ColorNotFoundInTheListException : Exception
    {
        public int RequestedColor { get; }

        public ColorNotFoundInTheListException(int color)
            : base($"Color with ID {color} does not exist in the enumerated color list. Valid colors are: 1 (Blau), 2 (Grün), 3 (Violett), 4 (Rot), 5 (Gelb), 6 (Türkis), 7 (Weiß).")
        {
            RequestedColor = color;
        }
    }
}
