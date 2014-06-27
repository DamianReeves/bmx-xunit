using System.Linq;
using System.Globalization;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.NUnit
{
    /// <summary>
    /// Custom editor for the NUnit action.
    /// </summary>
    internal sealed class NUnitActionEditor : ActionEditorBase
    {
        private SourceControlFileFolderPicker txtExePath;
        private ValidatingTextBox txtTestFile, txtGroupName;
        private DropDownList ddlFrameworkVersion;
        private ValidatingTextBox txtAdditionalArguments;
        private ValidatingTextBox txtCustomXmlOutputPath;
        private CheckBox chkTreatInconclusiveTestsAsFailure;

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitActionEditor"/> class.
        /// </summary>
        public NUnitActionEditor()
        {
        }

        /// <summary>
        /// Gets a value indicating whether a textbox to edit the source directory should be displayed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a textbox to edit the source directory should be displayed; otherwise, <c>false</c>.
        /// </value>
        public override bool DisplaySourceDirectory { get { return true; } }

        protected override void CreateChildControls()
        {
            Tables.Deployables_Extended deployable = null;
            if (this.DeployableId > 0) deployable = StoredProcs
                .Applications_GetDeployable(this.DeployableId)
                .Execute()
                .FirstOrDefault();

            this.txtExePath = new SourceControlFileFolderPicker
            {
                DisplayMode = SourceControlBrowser.DisplayModes.FoldersAndFiles,
                ServerId = this.ServerId,
                DefaultText = "Default for Selected Configuration"
            };

            this.txtGroupName = new ValidatingTextBox
            {
                Text = deployable != null ? deployable.Deployable_Name : string.Empty,
                Width= 300,
                Required = true
            };

            this.txtTestFile = new ValidatingTextBox
            {
                Required = true,
                Width = 300
            };

            this.ddlFrameworkVersion = new DropDownList();
            this.ddlFrameworkVersion.Items.Add(new ListItem("2.0.50727", "2.0.50727"));
            this.ddlFrameworkVersion.Items.Add(new ListItem("4.0.30319", "4.0.30319"));
            this.ddlFrameworkVersion.Items.Add(new ListItem("unspecified", ""));
            this.ddlFrameworkVersion.SelectedValue = "";

            this.txtAdditionalArguments = new ValidatingTextBox
            {
                Required = false,
                Width = 300
            };

            this.txtCustomXmlOutputPath = new ValidatingTextBox
            {
                Required = false,
                Width = 300,
                DefaultText = "Managed by BuildMaster"
            };

            this.chkTreatInconclusiveTestsAsFailure = new CheckBox
            {
                Text = "Treat Inconclusive Tests as Failures",
                Checked = true
            };

            this.Controls.Add(
                new FormFieldGroup(
                    "Custom NUnit Executable Path", 
                    "The path to (and including) nunit-console.exe if using a different version of NUnit than the one specified "
                        +"in the NUnit extension configuration.", 
                    false, 
                    new StandardFormField("NUnit Exe Path:", this.txtExePath)
                ),
                new FormFieldGroup(
                    ".NET Framework Version",
                    "The version of .NET which will host the unit test runner.",
                    false,
                    new StandardFormField(".NET Framework Version:", this.ddlFrameworkVersion)
                ),
                new FormFieldGroup(
                    "Test File", 
                    "The path relative to the source directory of the DLL, project file, or NUnit file to test against.", 
                    false, 
                    new StandardFormField("Test File:", this.txtTestFile)
                ),
                new FormFieldGroup(
                    "Custom XML Output Path",
                    "The path relative to the source directory of the NUnit-generated XML output file.",
                    false,
                    new StandardFormField("XML Output Path:", this.txtCustomXmlOutputPath)
                ),
                new FormFieldGroup(
                    "NUnit Options",
                    "Specify any additional options for NUnit here.",
                    false,
                    new StandardFormField("", this.chkTreatInconclusiveTestsAsFailure)
                ),
                new FormFieldGroup(
                    "Group Name", 
                    "The Group name allows you to easily identify the unit test.", 
                    false, 
                    new StandardFormField("Group Name:", this.txtGroupName)
                ),
                new FormFieldGroup(
                    "Additional Arguments",
                    "The additional arguments to pass to the NUnit executable.",
                    true,
                    new StandardFormField("Additional Arguments:", this.txtAdditionalArguments)
                )
            );
        }

        public override void BindToForm(ActionBase extension)
        {
            var nunitAction = (NUnitAppAction)extension;

            this.txtExePath.Text = nunitAction.ExePath;
            this.txtTestFile.Text = nunitAction.TestFile;
            this.txtGroupName.Text = nunitAction.GroupName;
            this.ddlFrameworkVersion.SelectedValue = nunitAction.FrameworkVersion ?? "";
            this.txtAdditionalArguments.Text = nunitAction.AdditionalArguments;
            this.txtCustomXmlOutputPath.Text = nunitAction.CustomXmlOutputPath;
            this.chkTreatInconclusiveTestsAsFailure.Checked = nunitAction.TreatInconclusiveAsFailure;
        }

        public override ActionBase CreateFromForm()
        {
            return new NUnitAppAction
            {
                ExePath = this.txtExePath.Text,
                TestFile = this.txtTestFile.Text,
                GroupName = this.txtGroupName.Text,
                FrameworkVersion = this.ddlFrameworkVersion.SelectedValue,
                AdditionalArguments = this.txtAdditionalArguments.Text,
                CustomXmlOutputPath = this.txtCustomXmlOutputPath.Text,
                TreatInconclusiveAsFailure = this.chkTreatInconclusiveTestsAsFailure.Checked
            };
        }
    }
}
