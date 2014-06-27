using System.Linq;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.XUnit
{
    /// <summary>
    /// Custom editor for the XUnit action.
    /// </summary>
    internal sealed class XUnitActionEditor : ActionEditorBase
    {
        private SourceControlFileFolderPicker _txtExePath;
        private ValidatingTextBox _txtTestFile, _txtGroupName;
        private DropDownList _ddlFrameworkVersion;
        private ValidatingTextBox _txtAdditionalArguments;
        private ValidatingTextBox _txtCustomXmlOutputPath;
        private CheckBox _chkTreatInconclusiveTestsAsFailure;

        /// <summary>
        /// Initializes a new instance of the <see cref="XUnitActionEditor"/> class.
        /// </summary>
        public XUnitActionEditor()
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

            this._txtExePath = new SourceControlFileFolderPicker
            {
                DisplayMode = SourceControlBrowser.DisplayModes.FoldersAndFiles,
                ServerId = this.ServerId,
                DefaultText = "Default for Selected Configuration"
            };

            this._txtGroupName = new ValidatingTextBox
            {
                Text = deployable != null ? deployable.Deployable_Name : string.Empty,
                Width= 300,
                Required = true
            };

            this._txtTestFile = new ValidatingTextBox
            {
                Required = true,
                Width = 300
            };

            this._ddlFrameworkVersion = new DropDownList();
            this._ddlFrameworkVersion.Items.Add(new ListItem(FrameworkVersions.Net20, FrameworkVersions.Net20));
            this._ddlFrameworkVersion.Items.Add(new ListItem(FrameworkVersions.Net40, FrameworkVersions.Net40));
            this._ddlFrameworkVersion.Items.Add(new ListItem("unspecified", ""));
            this._ddlFrameworkVersion.SelectedValue = "";

            this._txtAdditionalArguments = new ValidatingTextBox
            {
                Required = false,
                Width = 300
            };

            this._txtCustomXmlOutputPath = new ValidatingTextBox
            {
                Required = false,
                Width = 300,
                DefaultText = "Managed by BuildMaster"
            };

            this._chkTreatInconclusiveTestsAsFailure = new CheckBox
            {
                Text = "Treat Inconclusive Tests as Failures",
                Checked = true
            };

            this.Controls.Add(
                new FormFieldGroup(
                    "Custom xUnit Executable Path", 
                    "The path to (and including) xunit-console.exe if using a different version of XUnit than the one specified "
                        +"in the NUnit extension configuration.", 
                    false, 
                    new StandardFormField("xUnit Exe Path:", this._txtExePath)
                ),
                new FormFieldGroup(
                    ".NET Framework Version",
                    "The version of .NET which will host the unit test runner.",
                    false,
                    new StandardFormField(".NET Framework Version:", this._ddlFrameworkVersion)
                ),
                new FormFieldGroup(
                    "Test File", 
                    "The path relative to the source directory of the DLL, project file, or NUnit file to test against.", 
                    false, 
                    new StandardFormField("Test File:", this._txtTestFile)
                ),
                new FormFieldGroup(
                    "Custom XML Output Path",
                    "The path relative to the source directory of the NUnit-generated XML output file.",
                    false,
                    new StandardFormField("XML Output Path:", this._txtCustomXmlOutputPath)
                ),
                new FormFieldGroup(
                    "NUnit Options",
                    "Specify any additional options for NUnit here.",
                    false,
                    new StandardFormField("", this._chkTreatInconclusiveTestsAsFailure)
                ),
                new FormFieldGroup(
                    "Group Name", 
                    "The Group name allows you to easily identify the unit test.", 
                    false, 
                    new StandardFormField("Group Name:", this._txtGroupName)
                ),
                new FormFieldGroup(
                    "Additional Arguments",
                    "The additional arguments to pass to the NUnit executable.",
                    true,
                    new StandardFormField("Additional Arguments:", this._txtAdditionalArguments)
                )
            );
        }

        public override void BindToForm(ActionBase extension)
        {
            var xunitAction = (XUnitAppAction)extension;

            this._txtExePath.Text = xunitAction.ExePath;
            this._txtTestFile.Text = xunitAction.TestFile;
            this._txtGroupName.Text = xunitAction.GroupName;
            this._ddlFrameworkVersion.SelectedValue = xunitAction.FrameworkVersion ?? "";
            this._txtAdditionalArguments.Text = xunitAction.AdditionalArguments;
            this._txtCustomXmlOutputPath.Text = xunitAction.CustomXmlOutputPath;
            this._chkTreatInconclusiveTestsAsFailure.Checked = xunitAction.TreatInconclusiveAsFailure;
        }

        public override ActionBase CreateFromForm()
        {
            return new XUnitAppAction
            {
                ExePath = this._txtExePath.Text,
                TestFile = this._txtTestFile.Text,
                GroupName = this._txtGroupName.Text,
                FrameworkVersion = this._ddlFrameworkVersion.SelectedValue,
                AdditionalArguments = this._txtAdditionalArguments.Text,
                CustomXmlOutputPath = this._txtCustomXmlOutputPath.Text,
                TreatInconclusiveAsFailure = this._chkTreatInconclusiveTestsAsFailure.Checked
            };
        }
    }
}
