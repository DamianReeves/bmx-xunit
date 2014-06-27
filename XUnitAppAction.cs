using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Actions.Testing;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.XUnit
{
    /// <summary>
    /// Action that runs NUnit unit tests on a specified project, assembly, or NUnit file.
    /// </summary>
    [ActionProperties(
        "Execute xUnit Tests",
        "Runs xUnit unit tests on a specified project, assembly, or xUnit file.")]
    [Tag(Tags.UnitTests)]
    [CustomEditor(typeof(XUnitActionEditor))]
    [RequiresInterface(typeof(IFileOperationsExecuter))]
    public sealed class XUnitAppAction : UnitTestActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XUnitAppAction"/> class.
        /// </summary>
        public XUnitAppAction()
        {
            this.TreatInconclusiveAsFailure = true;
        }

        /// <summary>
        /// Gets or sets the test runner exe path
        /// </summary>
        [Persistent]
        public string ExePath { get; set; }

        /// <summary>
        /// Gets or sets the file nunit will test against (could be dll, proj, or config file based on test runner)
        /// </summary>
        [Persistent]
        public string TestFile { get; set; }

        /// <summary>
        /// Gets or sets the .NET Framework version to run against.
        /// </summary>
        [Persistent]
        public string FrameworkVersion { get; set; }

        /// <summary>
        /// Gets or sets the additional arguments.
        /// </summary>
        [Persistent]
        public string AdditionalArguments { get; set; }

        /// <summary>
        /// Gets or sets the path of the output XML generated by NUnit.
        /// </summary>
        [Persistent]
        public string CustomXmlOutputPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to treat inconclusive tests as failures.
        /// </summary>
        [Persistent]
        public bool TreatInconclusiveAsFailure { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <remarks>
        /// This should return a user-friendly string describing what the Action does
        /// and the state of its important persistent properties.
        /// </remarks>
        public override string ToString()
        {
            return string.Format("Run xUnit Unit Tests on {0}{1}", this.TestFile, Util.ConcatNE(" with the additional arguments: ", this.AdditionalArguments));
        }

        /// <summary>
        /// Runs a unit test against the target specified in the action.
        /// After the test is run, use the <see cref="M:RecordResult" /> method
        /// to save the test results to the database.
        /// </summary>
        protected override void RunTests()
        {
            var doc = new XmlDocument();

            var agent = this.Context.Agent;
            {
                var fileOps = agent.GetService<IFileOperationsExecuter>();

                string xunitExePath = this.GetXUnitExePath(fileOps);
                string tmpFileName = this.GetXmlOutputPath(fileOps);

                LogInformation("XUnitExePath = '{0}'", xunitExePath);
                LogInformation("TestResults Path = '{0}'", tmpFileName);

                if (File.Exists(xunitExePath))
                {
                    throw new FileNotFoundException("The xunit runner could not be.", xunitExePath);
                }

                // For now we are using the nunit flag so we can use the same xml handling as the nunit extension
                this.ExecuteCommandLine(
                    xunitExePath,
                    string.Format("\"{0}\" /nunit:\"{1}\" {2}", this.TestFile, tmpFileName, this.AdditionalArguments),
                    this.Context.SourceDirectory
                );

                LogDebug("Reading test results...", xunitExePath);
                using (var stream = new MemoryStream(fileOps.ReadFileBytes(tmpFileName), false))
                {
                    doc.Load(stream);
                }
            }

            var testStart = DateTime.Parse(doc.SelectSingleNode("//test-results").Attributes["time"].Value);

            var nodeList = doc.SelectNodes("//test-case");

            foreach (XmlNode node in nodeList)
            {
                string testName = node.Attributes["name"].Value;

                // skip tests that weren't actually run
                if (string.Equals(node.Attributes["executed"].Value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    LogInformation(String.Format("XUnit Test: {0} (skipped)", testName));
                    continue;
                }

                bool nodeResult = node.Attributes["success"].Value.Equals("True", StringComparison.OrdinalIgnoreCase) || 
                    (!this.TreatInconclusiveAsFailure && node.Attributes["result"].Value.Equals("inconclusive", StringComparison.OrdinalIgnoreCase));

                var numberStyles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign |NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent; 
                double testLength = 0;
                if (!double.TryParse(node.Attributes["time"].Value, numberStyles, CultureInfo.InvariantCulture, out testLength))
                {
                    this.LogWarning("Error parsing " + node.Attributes["time"].Value + " as a number.");
                };

                this.LogInformation(string.Format("XUnit Test: {0}, Result: {1}, Test Length: {2} secs",
                    testName,
                    nodeResult,
                    testLength));

                this.RecordResult(
                    testName,
                    nodeResult,
                    node.OuterXml,
                    testStart,
                    testStart.AddSeconds(testLength)
                );

                testStart = testStart.AddSeconds(testLength);
            }
        }

        private string GetXUnitExePath(IFileOperationsExecuter fileOps)
        {
            if (!string.IsNullOrWhiteSpace(this.ExePath))
                return fileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, this.ExePath);

            var configurer = (XUnitConfigurer)this.GetExtensionConfigurer();
            if (string.IsNullOrWhiteSpace(configurer.XUnitConsoleExePath))
            {
                string exePath;
                switch (FrameworkVersion)
                {
                    case FrameworkVersions.Net20:
                        exePath = "xunit.console.exe";
                        break;
                    default:
                        exePath = "xunit.console.clr4.exe";
                        break;
                }

                var dirName = Path.GetDirectoryName(GetType().Assembly.Location) ?? ".";
                exePath = Path.Combine(dirName, @"tools\xunit.runners\tools\", exePath);
                return fileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, exePath);
            }

            return fileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, configurer.XUnitConsoleExePath);
        }

        private string GetXmlOutputPath(IFileOperationsExecuter fileOps)
        {
            if (string.IsNullOrWhiteSpace(this.CustomXmlOutputPath))
                return fileOps.CombinePath(this.Context.TempDirectory, Guid.NewGuid().ToString() + ".xml");

            return fileOps.CombinePath(this.Context.SourceDirectory, this.CustomXmlOutputPath);
        }
    }
}
