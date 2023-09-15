namespace KnowledgeMining.Application.Common.Options
{
    public class CustomizationsOptions
    {
        public const string Customizations = "Customizations";

        public bool Enabled { get; set; } = false;
        public string OrganizationName { get; set; } = "Microsoft";
        public string OrganizationLogo { get; set; } = "~/images/logo.png";
        public string OrganizationWebSiteUrl { get; set; } = "https://www.microsoft.com";
    }
}
