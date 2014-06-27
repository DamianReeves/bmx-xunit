using System.ComponentModel;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Configurers.Extension;

[assembly: ExtensionConfigurer(typeof(Inedo.BuildMasterExtensions.NUnit.NUnitConfigurer))]

namespace Inedo.BuildMasterExtensions.NUnit
{
    public sealed class NUnitConfigurer : ExtensionConfigurerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitConfigurer"/> class.
        /// </summary>
        public NUnitConfigurer()
        {
        }

        /// <summary>
        /// Gets or sets the path to nunit-console.exe.
        /// </summary>
        [Persistent]
        [DisplayName("NUnit Console Executable Path")]
        [Description(@"The path to nunit-console.exe, typically: <br /><br />"
            + @"""C:\Program Files (x86)\NUnit 2.X.X\bin\nunit-console.exe")]
        public string NUnitConsoleExePath { get; set; }

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
