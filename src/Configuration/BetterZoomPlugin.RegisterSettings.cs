namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        private void RegisterSettings()
        {
            var settings = context.Settings("Better Zoom",
                "Zoom further out and in in the workshop and research tree, with fine-tuned zoom steps.");

            settings.AddSlider(ZoomStep, 0.05f, 1.0f, 0.05f, "Zoom Step Size", FormatZoomStep);
            settings.AddSlider(ExtraOut, 0, 5, "Extra Zoom Out");
            settings.AddSlider(ExtraIn, 0, 5, "Extra Zoom In");
        }
    }
}
