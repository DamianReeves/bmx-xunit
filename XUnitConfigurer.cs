using System.ComponentModel;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMasterExtensions.XUnit;

[assembly: ExtensionConfigurer(typeof(XUnitConfigurer))]

namespace Inedo.BuildMasterExtensions.XUnit
{
    public sealed class XUnitConfigurer : ExtensionConfigurerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XUnitConfigurer"/> class.
        /// </summary>
        public XUnitConfigurer()
        {
        }

        /// <summary>
        /// Gets or sets the path to xunit-console.exe.
        /// </summary>
        [Persistent]
        [DisplayName("XUnit Console Executable Path")]
        [Description(@"The path to xunit-console.clr4.exe or xunit-console.exe.")]
        public string XUnitConsoleExePath { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Empty;
        }
    }
}
